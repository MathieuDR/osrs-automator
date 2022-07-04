using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.Dashboard.Controllers.V1; 

[ApiController]
[ApiVersion("1.0")]
public class HealthController : Controller {
    
    [AllowAnonymous]
    [Route("v{version:apiVersion}/[controller]")]
    public IActionResult Get() => Ok("Healthy");
}
