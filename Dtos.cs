namespace HandyConnect.Api.Models;

public sealed class ArtisanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string TradeSlug { get; set; } = "";
    public string TradeFr { get; set; } = "";
    public string TradeAr { get; set; } = "";
    public string TradeIcon { get; set; } = "";
    public int GovernorateId { get; set; }
    public string Governorate { get; set; } = "";
    public string GovernorateAr { get; set; } = "";
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string Bio { get; set; } = "";
    public string Zone { get; set; } = "";
    public int MemberSince { get; set; }
    public List<string> Specialties { get; set; } = new();
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public sealed class ReviewDto
{
    public int Id { get; set; }
    public string Author { get; set; } = "";
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public sealed class AdminReviewDto
{
    public int Id { get; set; }
    public string ArtisanName { get; set; } = "";
    public string ClientName { get; set; } = "";
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public sealed class TradeDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string NameFr { get; set; } = "";
    public string NameAr { get; set; } = "";
    public string Icon { get; set; } = "";
    public int ArtisanCount { get; set; }
}

public sealed class GovernorateDto
{
    public int Id { get; set; }
    public string NameFr { get; set; } = "";
    public string NameAr { get; set; } = "";
}

public sealed class StatsDto
{
    public int Artisans { get; set; }
    public int Reviews { get; set; }
    public decimal AverageRating { get; set; }
}

public sealed record MetaDto(
    IReadOnlyList<TradeDto> Trades,
    IReadOnlyList<GovernorateDto> Governorates,
    StatsDto Stats);

public sealed class FaqDto
{
    public int Id { get; set; }
    public string QuestionFr { get; set; } = "";
    public string AnswerFr { get; set; } = "";
    public string QuestionAr { get; set; } = "";
    public string AnswerAr { get; set; } = "";
    public string Keywords { get; set; } = "";
}

public sealed record AuthUserDto(int Id, string FullName, string Role);
public sealed record AuthResponse(string Token, AuthUserDto User);
