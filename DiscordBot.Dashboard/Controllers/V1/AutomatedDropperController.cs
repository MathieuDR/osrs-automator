using System.Linq;
using Dashboard.Binders;
using Dashboard.Models.ApiRequests.DiscordEmbed;
using Dashboard.Transformers;
using DiscordBot.Common.Dtos.Runescape;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dashboard.Controllers.V1 {
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AutomatedDropperController : Controller {
        private readonly ILogger<AutomatedDropperController> _logger;
        private readonly IMapper<Embed, RunescapeDrop> _mapper;

        public AutomatedDropperController(ILogger<AutomatedDropperController> logger, IMapper<Embed, RunescapeDrop> mapper) {
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("dropper")]
        public IActionResult Get([FromBody]EmbedCollection bodyEmbeds, 
            [ModelBinder(BinderType = typeof(JsonModelBinder))]EmbedCollection formEmbed, 
            [FromForm] IFormFile file) {
            var embeds = bodyEmbeds ?? formEmbed;
            
            if (embeds != null) {
                _logger.LogInformation("Received embed {@embeds}", embeds);
                var drop = _mapper.Map(embeds.Embeds.FirstOrDefault()).Value;
                _logger.LogInformation("Embed {amount} of {item} for {player} makes {value}", drop.Amount, drop.Item.Name, drop.Recipient.Username,
                    drop.TotalValue);
            }

            if (file != null ) {
                _logger.LogInformation("Received file {name} : {size}b", file.FileName, file.Length);
            }

            return Ok();
        }
    }
}
