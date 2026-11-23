using Dapper;
using HandyConnect.Api.Models;

namespace HandyConnect.Api.Data;

public sealed class UserRepository(IDbConnectionFactory factory)
{
    private const string Columns =
        "id AS Id, full_name AS FullName, email AS Email, phone AS Phone, " +
        "password_hash AS PasswordHash, role AS Role, governorate_id AS GovernorateId";

    public async Task<UserRow?> FindByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return await db.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(
            $"SELECT {Columns} FROM users WHERE id = @id", new { id }, cancellationToken: ct));
    }

    /// <summary>Finds a user by e-mail or by normalised phone.</summary>
    public async Task<UserRow?> FindByIdentifierAsync(string? email, string? phone, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return await db.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(
            $"SELECT {Columns} FROM users WHERE (@email IS NOT NULL AND email = @email) OR (@phone IS NOT NULL AND phone = @phone) LIMIT 1",
            new { email, phone }, cancellationToken: ct));
    }

    public async Task<bool> ExistsAsync(string? email, string phone, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        var count = await db.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM users WHERE phone = @phone OR (@email IS NOT NULL AND email = @email)",
            new { email, phone }, cancellationToken: ct));
        return count > 0;
    }

    /// <summary>Creates the user (and the artisan profile + specialties) in one transaction.</summary>
    public async Task<int> CreateAsync(UserRow user, ArtisanProfileData? artisan, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);

        var id = (int)await db.ExecuteScalarAsync<long>(new CommandDefinition(
            "INSERT INTO users (full_name, email, phone, password_hash, role, governorate_id) " +
            "VALUES (@FullName, @Email, @Phone, @PasswordHash, @Role, @GovernorateId); SELECT LAST_INSERT_ID();",
            user, tx, cancellationToken: ct));

        if (artisan is not null)
        {
            await db.ExecuteAsync(new CommandDefinition(
                "INSERT INTO artisan_profiles (user_id, trade_id, bio, zone, experience_years, subscription_ends_at) " +
                "VALUES (@UserId, @TradeId, @Bio, @Zone, @ExperienceYears, @SubscriptionEndsAt)",
                new
                {
                    UserId = id,
                    artisan.TradeId,
                    artisan.Bio,
                    artisan.Zone,
                    artisan.ExperienceYears,
                    artisan.SubscriptionEndsAt
                }, tx, cancellationToken: ct));

            if (artisan.Specialties.Count > 0)
            {
                await db.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO artisan_specialties (artisan_id, label) VALUES (@ArtisanId, @Label)",
                    artisan.Specialties.Select(label => new { ArtisanId = id, Label = label }),
                    tx, cancellationToken: ct));
            }
        }

        await tx.CommitAsync(ct);
        return id;
    }
}
