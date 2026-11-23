using System.ComponentModel.DataAnnotations;

namespace HandyConnect.Api.Models;

public sealed class RegisterRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required]
    public string Phone { get; set; } = "";

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [Range(1, 24)]
    public int GovernorateId { get; set; }

    [Required, StringLength(72, MinimumLength = 8)]
    public string Password { get; set; } = "";

    [RegularExpression("client|artisan")]
    public string Role { get; set; } = "client";

    // Artisan-only fields (validated in the controller when Role == "artisan").
    public int? TradeId { get; set; }
    [StringLength(255)] public string? Specialties { get; set; }
    [StringLength(150)] public string? Zone { get; set; }
    [Range(0, 60)] public int? ExperienceYears { get; set; }
    [StringLength(500)] public string? Bio { get; set; }
}

public sealed class LoginRequest
{
    [Required] public string Identifier { get; set; } = "";   // email or phone
    [Required] public string Password { get; set; } = "";
}

public sealed class ArtisanQuery
{
    public string? Trade { get; set; }
    public int? Gov { get; set; }
    public string? Q { get; set; }
    public decimal MinRating { get; set; }
    public int MinExperience { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public sealed class ReviewRequest
{
    [Range(1, 5)] public int Rating { get; set; }
    [StringLength(500)] public string? Comment { get; set; }
}

public sealed class ReviewStatusRequest
{
    [Required, RegularExpression("approved|rejected|pending")]
    public string Status { get; set; } = "";
}

public sealed class ContactRequest
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Required, StringLength(150)] public string Subject { get; set; } = "";
    [Required, StringLength(2000, MinimumLength = 5)] public string Message { get; set; } = "";
}

public sealed class NewsletterRequest
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
}

public sealed class ChatTurn
{
    [Required, RegularExpression("user|assistant")] public string Role { get; set; } = "user";
    [Required, StringLength(1500)] public string Content { get; set; } = "";
}

public sealed class ChatRequest
{
    [Required, StringLength(500, MinimumLength = 1)] public string Message { get; set; } = "";
    [RegularExpression("fr|ar")] public string Lang { get; set; } = "fr";
    [MaxLength(10)] public List<ChatTurn>? History { get; set; }
}
