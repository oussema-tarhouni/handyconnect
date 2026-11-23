using Dapper;
using HandyConnect.Api.Models;

namespace HandyConnect.Api.Data;

public sealed class ArtisanRepository(IDbConnectionFactory factory)
{
    // A profile is public only while its subscription is active.
    private const string Select = """
        SELECT u.id AS Id, u.full_name AS Name,
               t.slug AS TradeSlug, t.name_fr AS TradeFr, t.name_ar AS TradeAr, t.icon AS TradeIcon,
               g.id AS GovernorateId, g.name_fr AS Governorate, g.name_ar AS GovernorateAr,
               ap.experience_years AS ExperienceYears, ap.bio AS Bio, ap.zone AS Zone,
               YEAR(u.created_at) AS MemberSince,
               COALESCE(rv.avg_rating, 0) AS Rating, COALESCE(rv.cnt, 0) AS ReviewCount
        """;

    private const string From = """
        FROM artisan_profiles ap
        JOIN users u ON u.id = ap.user_id
        JOIN trades t ON t.id = ap.trade_id
        JOIN governorates g ON g.id = u.governorate_id
        LEFT JOIN (
            SELECT artisan_id, ROUND(AVG(rating), 1) AS avg_rating, COUNT(*) AS cnt
            FROM reviews WHERE status = 'approved' GROUP BY artisan_id
        ) rv ON rv.artisan_id = ap.user_id
        WHERE ap.subscription_ends_at > UTC_TIMESTAMP()
        """;

    private const string Filters = """
         AND (@Trade IS NULL OR t.slug = @Trade)
         AND (@Gov IS NULL OR g.id = @Gov)
         AND ap.experience_years >= @MinExperience
         AND COALESCE(rv.avg_rating, 0) >= @MinRating
         AND (@QLike IS NULL
              OR u.full_name LIKE @QLike
              OR t.name_fr LIKE @QLike
              OR ap.bio LIKE @QLike
              OR ap.zone LIKE @QLike
              OR EXISTS (SELECT 1 FROM artisan_specialties s
                         WHERE s.artisan_id = ap.user_id AND s.label LIKE @QLike))
        """;

    public async Task<PagedResult<ArtisanDto>> SearchAsync(ArtisanQuery q, CancellationToken ct = default)
    {
        var page = Math.Max(q.Page, 1);
        var pageSize = Math.Clamp(q.PageSize, 1, 50);

        var parameters = new
        {
            Trade = string.IsNullOrWhiteSpace(q.Trade) ? null : q.Trade.Trim().ToLowerInvariant(),
            Gov = q.Gov is > 0 ? q.Gov : null,
            MinExperience = Math.Max(q.MinExperience, 0),
            MinRating = Math.Clamp(q.MinRating, 0m, 5m),
            QLike = BuildLike(q.Q),
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        await using var db = await factory.OpenAsync(ct);

        var total = await db.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) " + From + Filters, parameters, cancellationToken: ct));

        var items = (await db.QueryAsync<ArtisanDto>(new CommandDefinition(
            Select + " " + From + Filters +
            " ORDER BY Rating DESC, ReviewCount DESC, u.id LIMIT @Limit OFFSET @Offset",
            parameters, cancellationToken: ct))).ToList();

        return new PagedResult<ArtisanDto>(items, (int)total, page, pageSize);
    }

    public async Task<ArtisanDto?> GetAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        var artisan = await db.QuerySingleOrDefaultAsync<ArtisanDto>(new CommandDefinition(
            Select + " " + From + " AND u.id = @id", new { id }, cancellationToken: ct));
        if (artisan is not null) await AttachSpecialtiesAsync(db, new[] { artisan }, ct);
        return artisan;
    }

    public async Task AttachSpecialtiesAsync(IReadOnlyCollection<ArtisanDto> artisans, CancellationToken ct = default)
    {
        if (artisans.Count == 0) return;
        await using var db = await factory.OpenAsync(ct);
        await AttachSpecialtiesAsync(db, artisans, ct);
    }

    private static async Task AttachSpecialtiesAsync(
        System.Data.Common.DbConnection db, IReadOnlyCollection<ArtisanDto> artisans, CancellationToken ct)
    {
        var ids = artisans.Select(a => a.Id).ToArray();
        var rows = await db.QueryAsync<SpecialtyRow>(new CommandDefinition(
            "SELECT artisan_id AS ArtisanId, label AS Label FROM artisan_specialties " +
            "WHERE artisan_id IN @ids ORDER BY id", new { ids }, cancellationToken: ct));

        var byArtisan = rows.ToLookup(r => r.ArtisanId, r => r.Label);
        foreach (var artisan in artisans) artisan.Specialties = byArtisan[artisan.Id].ToList();
    }

    /// <summary>Phone number of an artisan whose subscription is active (null otherwise).</summary>
    public async Task<string?> GetPhoneAsync(int artisanId, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return await db.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
            "SELECT u.phone FROM users u JOIN artisan_profiles ap ON ap.user_id = u.id " +
            "WHERE u.id = @artisanId AND ap.subscription_ends_at > UTC_TIMESTAMP()",
            new { artisanId }, cancellationToken: ct));
    }

    public async Task LogContactAsync(int clientId, int artisanId, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        await db.ExecuteAsync(new CommandDefinition(
            "INSERT IGNORE INTO contact_logs (client_id, artisan_id) VALUES (@clientId, @artisanId)",
            new { clientId, artisanId }, cancellationToken: ct));
    }

    private static string? BuildLike(string? q)
    {
        if (string.IsNullOrWhiteSpace(q)) return null;
        var escaped = q.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        return $"%{escaped}%";
    }
}
