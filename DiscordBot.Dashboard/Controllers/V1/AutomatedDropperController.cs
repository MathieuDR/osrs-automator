using DiscordBot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.Dashboard.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AutomatedDropperController : Controller {
    private readonly IAutomatedDropperService _dropperService;
    private readonly ILogger<AutomatedDropperController> _logger;

    public AutomatedDropperController(ILogger<AutomatedDropperController> logger, IAutomatedDropperService dropperService) {
        _logger = logger;
        _dropperService = dropperService;
    }

    // create an endpoint where we have an ID as a guid from route
    // an file form a form
    // and a json payload also from the form under the value "payload_json"

    [HttpPost("dropper/{id:required}")]
    public async Task<IActionResult> Get([FromForm] IFormFile? image, [FromRoute] string id, [FromForm(Name = "payload_json")] string? payload) {
        if (!Guid.TryParse(id, out var endpoint)) {
            return BadRequest("Invalid endpoint");
        }

        if (payload == null) {
            return BadRequest("No payload provided");
        }

        if (image != null && image.Length > 0) {
            var base64Image = await ConvertFileToBase64String(image);
        }


        return Ok();
    }


    private async Task<string> ConvertFileToBase64String(IFormFile file) {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var bytes = stream.ToArray();
        return Convert.ToBase64String(bytes);
    }
}
