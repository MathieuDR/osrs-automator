using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dashboard.Binders;
using Dashboard.Models.ApiRequests.DiscordEmbed;
using Dashboard.Transformers;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Services.Interfaces;
using FluentResults;
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
        private readonly IAutomatedDropperService _dropperService;

        public AutomatedDropperController(ILogger<AutomatedDropperController> logger, IMapper<Embed, RunescapeDrop> mapper, IAutomatedDropperService dropperService) {
            _logger = logger;
            _mapper = mapper;
            _dropperService = dropperService;
        }

        [HttpPost("dropper/{endpoint:guid}")]
        public async Task<IActionResult> Get([FromBody]EmbedCollection bodyEmbeds, 
            [ModelBinder(BinderType = typeof(JsonModelBinder))]EmbedCollection formEmbeds, 
            [FromForm] IFormFile file, [FromRoute] Guid endpoint) {
            _logger.LogInformation("Received drop");
            var dropResult = GetDrop(bodyEmbeds, formEmbeds);
            if (dropResult.IsFailed) {
                BadRequest(dropResult.Errors.FirstOrDefault());
            }
            
            var image = await ToBase64String(file);
            _ = _dropperService.HandleDropRequest(endpoint, dropResult.Value, image);
            
            return Ok();
        }

        private Result<RunescapeDrop> GetDrop(EmbedCollection bodyEmbeds, EmbedCollection formEmbeds) {
            var embeds = bodyEmbeds ?? formEmbeds;
            var embed = embeds?.Embeds.FirstOrDefault();

            if (embed is not null) {
                return _mapper.Map(embed);
            }

            return Result.Ok();
        }

        private static async Task<string> ToBase64String(IFormFile file) {
            string result = null;
            if (file is not null) {
                await using var stream = file.OpenReadStream();
                await using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();
                result = Convert.ToBase64String(bytes);
            }

            return result;
        }
    }
}
