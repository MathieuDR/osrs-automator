using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.V2 {
    [ApiController]
    [ApiVersion( "2.0" )]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class AutomatedDropperController: Controller {
        [HttpGet("dropper")]
        
        public IActionResult Get() {
            return Ok(new {Name = "Lina"});
        }
    }
    
}
