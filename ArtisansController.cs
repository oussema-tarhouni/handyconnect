using HandyConnect.Api.Common;
using HandyConnect.Api.Data;
using HandyConnect.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace HandyConnect.Api.Controllers;

[ApiController]
[Route("api/artisans")]
public sealed class ArtisansController(ArtisanRepository artisans, ReviewRepository reviews) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ArtisanDto>>> Search([FromQuery] ArtisanQuery query, CancellationToken ct) =>
        await artisans.SearchAsync(query, ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var artisan = await artisans.GetAsync(id, ct);
        return artisan is null ? NotFound(ApiError.NotFound) : Ok(artisan);
    }

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> Reviews(int id, CancellationToken ct) =>
        Ok(await reviews.ListApprovedAsync(id, ct: ct));

    /// <summary>Reveals the phone number to a signed-in user and records the contact.</summary>
    [Authorize]
    [HttpPost("{id:int}/contact")]
    public async Task<IActionResult> Contact(int id, CancellationToken ct)
    {
        var phone = await artisans.GetPhoneAsync(id, ct);
        if (phone is null) return NotFound(ApiError.NotFound);

        var userId = User.UserId();
        if (userId != id) await artisans.LogContactAsync(userId, id, ct);
        return Ok(new { phone });
    }

    [Authorize(Roles = "client")]
    [HttpPost("{id:int}/reviews")]
    public async Task<IActionResult> AddReview(int id, ReviewRequest request, CancellationToken ct)
    {
        if (await artisans.GetAsync(id, ct) is null) return NotFound(ApiError.NotFound);

        var clientId = User.UserId();
        if (!await reviews.HasContactedAsync(clientId, id, ct)) return StatusCode(403, ApiError.ContactRequired);
        if (await reviews.ExistsAsync(clientId, id, ct)) return Conflict(ApiError.AlreadyReviewed);

        try
        {
            await reviews.AddAsync(clientId, id, request.Rating, request.Comment?.Trim() ?? "", ct);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Conflict(ApiError.AlreadyReviewed);
        }

        // Published only after moderation.
        return StatusCode(StatusCodes.Status201Created, new { status = "pending" });
    }
}
