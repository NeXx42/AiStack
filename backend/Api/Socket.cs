using Microsoft.AspNetCore.Mvc;

namespace Nexx.Models.Backend.Socket;

[ApiController]
[Route("api/[controller]")]
public class ModelSocket : ControllerBase
{
    [HttpGet("{modelId}/info")]
    public IActionResult GetModelInfo(Guid modelId)
    {
        ModelSession? session = ModelBackend.GetActiveSession(modelId);

        if (session == null)
            return NotFound();

        return Ok(new
        {
            url = $"ws://localhost:5000/{modelId}/ws"
        });
    }

    public static async Task ActiveSocket(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Invalid request type");

            return;
        }

        string modelId = context.Request.RouteValues["modelId"]?.ToString() ?? string.Empty;

        if (!Guid.TryParse(modelId, out Guid id) || !ModelBackend.GetActiveSession(id, out ModelSession session))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Model not found");

            return;
        }

        await session.AcceptSocket(context.WebSockets);
    }
}