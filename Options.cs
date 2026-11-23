namespace HandyConnect.Api.Options;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "HandyConnect";
    public string Audience { get; set; } = "HandyConnect";
    public string Key { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 120;
}

public sealed class AnthropicOptions
{
    /// <summary>Leave empty to run the assistant in offline (rule-based) mode.</summary>
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "claude-sonnet-5-5";
    public int MaxTokens { get; set; } = 600;
}

public sealed class SubscriptionOptions
{
    /// <summary>Days of visibility granted at artisan sign-up (payment is out of scope).</summary>
    public int TrialDays { get; set; } = 30;
}
