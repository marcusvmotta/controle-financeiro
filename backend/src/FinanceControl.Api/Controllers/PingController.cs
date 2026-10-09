using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.Api.Controllers;

/// <summary>
/// Endpoint "hello world" do M0: prova que o frontend consegue falar com a API.
/// </summary>
[ApiController]
[Route("api/ping")]
[AllowAnonymous]
public sealed class PingController(TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PingResponse>(StatusCodes.Status200OK)]
    public ActionResult<PingResponse> Get() =>
        Ok(new PingResponse("pong", timeProvider.GetUtcNow()));
}

public sealed record PingResponse(string Message, DateTimeOffset ServerTime);
