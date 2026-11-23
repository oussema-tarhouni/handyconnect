using HandyConnect.Api.Data;
using HandyConnect.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HandyConnect.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class PublicContentController(ContentRepository content) : ControllerBase
{
    /// <summary>Trades, governorates and live statistics used by every page.</summary>
    [HttpGet("meta")]
    public async Task<MetaDto> Meta(CancellationToken ct) =>
        new(await content.GetTradesAsync(ct), await content.GetGovernoratesAsync(ct), await content.GetStatsAsync(ct));

    [HttpGet("faq")]
    public async Task<IReadOnlyList<FaqDto>> Faq(CancellationToken ct) => await content.GetFaqAsync(ct);

    [HttpPost("contact")]
    [EnableRateLimiting("forms")]
    public async Task<IActionResult> Contact(ContactRequest request, CancellationToken ct)
    {
        await content.AddContactMessageAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("newsletter")]
    [EnableRateLimiting("forms")]
    public async Task<IActionResult> Newsletter(NewsletterRequest request, CancellationToken ct)
    {
        await content.SubscribeAsync(request, ct);
        return NoContent();
    }
}
