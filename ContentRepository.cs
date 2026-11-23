using Dapper;
using HandyConnect.Api.Models;

namespace HandyConnect.Api.Data;

/// <summary>Reference data (trades, governorates, FAQ) and simple inbound content (contact, newsletter).</summary>
public sealed class ContentRepository(IDbConnectionFactory factory)
{
    public async Task<IReadOnlyList<TradeDto>> GetTradesAsync(CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return (await db.QueryAsync<TradeDto>(new CommandDefinition("""
            SELECT t.id AS Id, t.slug AS Slug, t.name_fr AS NameFr, t.name_ar AS NameAr, t.icon AS Icon,
                   (SELECT COUNT(*) FROM artisan_profiles ap
                    WHERE ap.trade_id = t.id AND ap.subscription_ends_at > UTC_TIMESTAMP()) AS ArtisanCount
            FROM trades t ORDER BY t.id
            """, cancellationToken: ct))).ToList();
    }

    public async Task<IReadOnlyList<GovernorateDto>> GetGovernoratesAsync(CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return (await db.QueryAsync<GovernorateDto>(new CommandDefinition(
            "SELECT id AS Id, name_fr AS NameFr, name_ar AS NameAr FROM governorates ORDER BY id",
            cancellationToken: ct))).ToList();
    }

    public async Task<StatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return await db.QuerySingleAsync<StatsDto>(new CommandDefinition("""
            SELECT
              (SELECT COUNT(*) FROM artisan_profiles WHERE subscription_ends_at > UTC_TIMESTAMP()) AS Artisans,
              (SELECT COUNT(*) FROM reviews WHERE status = 'approved') AS Reviews,
              (SELECT COALESCE(ROUND(AVG(rating), 1), 0) FROM reviews WHERE status = 'approved') AS AverageRating
            """, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<FaqDto>> GetFaqAsync(CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return (await db.QueryAsync<FaqDto>(new CommandDefinition(
            "SELECT id AS Id, question_fr AS QuestionFr, answer_fr AS AnswerFr, question_ar AS QuestionAr, " +
            "answer_ar AS AnswerAr, keywords AS Keywords FROM faq_entries ORDER BY sort_order",
            cancellationToken: ct))).ToList();
    }

    public async Task AddContactMessageAsync(ContactRequest m, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        await db.ExecuteAsync(new CommandDefinition(
            "INSERT INTO contact_messages (name, email, subject, message) VALUES (@Name, @Email, @Subject, @Message)",
            m, cancellationToken: ct));
    }

    /// <summary>Idempotent: subscribing twice with the same e-mail is not an error (and leaks nothing).</summary>
    public async Task SubscribeAsync(NewsletterRequest m, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        await db.ExecuteAsync(new CommandDefinition(
            "INSERT INTO newsletter_subscribers (name, email) VALUES (@Name, @Email) " +
            "ON DUPLICATE KEY UPDATE name = VALUES(name)",
            new { m.Name, Email = m.Email.Trim().ToLowerInvariant() }, cancellationToken: ct));
    }
}
