using Dapper;
using HandyConnect.Api.Models;

namespace HandyConnect.Api.Data;

public sealed class ReviewRepository(IDbConnectionFactory factory)
{
    public async Task<IReadOnlyList<ReviewDto>> ListApprovedAsync(int artisanId, int limit = 20, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        var rows = (await db.QueryAsync<ReviewDto>(new CommandDefinition(
            "SELECT r.id AS Id, u.full_name AS Author, r.rating AS Rating, r.comment AS Comment, r.created_at AS CreatedAt " +
            "FROM reviews r JOIN users u ON u.id = r.client_id " +
            "WHERE r.artisan_id = @artisanId AND r.status = 'approved' ORDER BY r.created_at DESC LIMIT @limit",
            new { artisanId, limit }, cancellationToken: ct))).ToList();

        // Privacy: only expose "First L."
        foreach (var review in rows) review.Author = ShortName(review.Author);
        return rows;
    }

    public async Task<bool> HasContactedAsync(int clientId, int artisanId, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return await db.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM contact_logs WHERE client_id = @clientId AND artisan_id = @artisanId",
            new { clientId, artisanId }, cancellationToken: ct)) > 0;
    }

    public async Task<bool> ExistsAsync(int clientId, int artisanId, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return await db.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM reviews WHERE client_id = @clientId AND artisan_id = @artisanId",
            new { clientId, artisanId }, cancellationToken: ct)) > 0;
    }

    /// <summary>New reviews start as "pending" until an administrator approves them.</summary>
    public async Task AddAsync(int clientId, int artisanId, int rating, string comment, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        await db.ExecuteAsync(new CommandDefinition(
            "INSERT INTO reviews (artisan_id, client_id, rating, comment) VALUES (@artisanId, @clientId, @rating, @comment)",
            new { artisanId, clientId, rating, comment }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<AdminReviewDto>> ListByStatusAsync(string status, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return (await db.QueryAsync<AdminReviewDto>(new CommandDefinition(
            "SELECT r.id AS Id, a.full_name AS ArtisanName, c.full_name AS ClientName, r.rating AS Rating, " +
            "r.comment AS Comment, r.status AS Status, r.created_at AS CreatedAt " +
            "FROM reviews r JOIN users a ON a.id = r.artisan_id JOIN users c ON c.id = r.client_id " +
            "WHERE r.status = @status ORDER BY r.created_at LIMIT 200",
            new { status }, cancellationToken: ct))).ToList();
    }

    public async Task<bool> SetStatusAsync(int id, string status, CancellationToken ct = default)
    {
        await using var db = await factory.OpenAsync(ct);
        return await db.ExecuteAsync(new CommandDefinition(
            "UPDATE reviews SET status = @status WHERE id = @id", new { id, status }, cancellationToken: ct)) > 0;
    }

    private static string ShortName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length < 2 ? fullName : $"{parts[0]} {parts[^1][0]}.";
    }
}
