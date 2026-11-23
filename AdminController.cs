using HandyConnect.Api.Common;
using HandyConnect.Api.Data;
using HandyConnect.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HandyConnect.Api.Controllers;

/// <summary>Review moderation (administrators only).</summary>
[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin")]
public sealed class AdminController(ReviewRepository reviews) : ControllerBase
{
    [HttpGet("reviews")]
    public async Task<ActionResult<IReadOnlyList<AdminReviewDto>>> Reviews([FromQuery] string status = "pending", CancellationToken ct = default)
    {
        if (status is not ("pending" or "approved" or "rejected")) return BadRequest(ApiError.InvalidInput);
        return Ok(await reviews.ListByStatusAsync(status, ct));
    }

    [HttpPatch("reviews/{id:int}")]
    public async Task<IActionResult> SetStatus(int id, ReviewStatusRequest request, CancellationToken ct) =>
        await reviews.SetStatusAsync(id, request.Status, ct) ? NoContent() : NotFound(ApiError.NotFound);
}
