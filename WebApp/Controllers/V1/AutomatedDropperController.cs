using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.V1 {
    [ApiController]
    [ApiVersion( "1.0", Deprecated = true)]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class AutomatedDropperController: Controller {
        [HttpGet("dropper")]
        
        public IActionResult Get() {
            return Ok(new {Name = "matthew"});
        }
    }
}
