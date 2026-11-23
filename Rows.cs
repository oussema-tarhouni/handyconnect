namespace HandyConnect.Api.Models;

/// <summary>Database row of the users table.</summary>
public sealed class UserRow
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string Phone { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "client";
    public int? GovernorateId { get; set; }
}

public sealed class ArtisanProfileData
{
    public int TradeId { get; set; }
    public string Bio { get; set; } = "";
    public string Zone { get; set; } = "";
    public int ExperienceYears { get; set; }
    public DateTime SubscriptionEndsAt { get; set; }
    public List<string> Specialties { get; set; } = new();
}

public sealed class SpecialtyRow
{
    public int ArtisanId { get; set; }
    public string Label { get; set; } = "";
}
