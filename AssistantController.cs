using HandyConnect.Api.Models;
using HandyConnect.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HandyConnect.Api.Controllers;

[ApiController]
[Route("api/assistant")]
public sealed class AssistantController(AssistantService assistant) : ControllerBase
{
    [HttpPost("chat")]
    [EnableRateLimiting("assistant")]
    public async Task<AssistantReply> Chat(ChatRequest request, CancellationToken ct) =>
        await assistant.AskAsync(request, ct);
}
