using System.Text;
using System.Threading.RateLimiting;
using Dapper;
using HandyConnect.Api.Data;
using HandyConnect.Api.Options;
using HandyConnect.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ---- Configuration -------------------------------------------------------
builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"));
builder.Services.Configure<AnthropicOptions>(config.GetSection("Anthropic"));
builder.Services.Configure<SubscriptionOptions>(config.GetSection("Subscription"));

var jwt = config.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters (set it with the Jwt__Key environment variable).");

// ---- Data access ---------------------------------------------------------
DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ArtisanRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<ContentRepository>();
builder.Services.AddTransient<DbSeeder>();

// ---- Services ------------------------------------------------------------
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHttpClient<AssistantService>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ---- Security ------------------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep our short claim names: uid, name, role
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

static RateLimitPartition<string> PerIp(HttpContext ctx, int permits, TimeSpan window) =>
    RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = window });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", ctx => PerIp(ctx, 10, TimeSpan.FromMinutes(1)));
    options.AddPolicy("forms", ctx => PerIp(ctx, 10, TimeSpan.FromMinutes(1)));
    options.AddPolicy("assistant", ctx => PerIp(ctx, 20, TimeSpan.FromMinutes(1)));
});

// ---- API -----------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// The API also serves the static frontend, so everything runs on one origin (no CORS needed).
var frontendPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, config["Frontend:Path"] ?? "../../frontend"));
if (Directory.Exists(frontendPath))
{
    var files = new PhysicalFileProvider(frontendPath);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = files });
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (config.GetValue<bool>("Seed:Demo"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
}

app.Run();
