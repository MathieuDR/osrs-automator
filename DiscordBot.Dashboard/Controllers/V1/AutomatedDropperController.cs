using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Common.Identities;
using DiscordBot.Dashboard.Binders;
using DiscordBot.Dashboard.Models.ApiRequests.DiscordEmbed;
using DiscordBot.Dashboard.Transformers;
using DiscordBot.Services.Interfaces;
using FluentResults;
using MathieuDR.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.Dashboard.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class AutomatedDropperController : Controller {
	private readonly IAutomatedDropperService _dropperService;
	private readonly ILogger<AutomatedDropperController> _logger;
	private readonly IMapper<Embed, RunescapeDrop> _mapper;

	public AutomatedDropperController(ILogger<AutomatedDropperController> logger, IMapper<Embed, RunescapeDrop> mapper,
		IAutomatedDropperService dropperService) {
		_logger = logger;
		_mapper = mapper;
		_dropperService = dropperService;
	}

	[HttpPost("drop/{id:required}")]
	public async Task<IActionResult> Get([FromBody] EmbedCollection bodyEmbeds,
		[ModelBinder(binderType: typeof(JsonModelBinder))]
		EmbedCollection? formEmbeds,
		[FromForm] IFormFile? file, [FromRoute] string id) {
		if (!Guid.TryParse(id, out var endpoint)) {
			BadRequest("Invalid endpoint");
		}

		_logger.LogInformation("Received drop");
		var dropResult = GetDrop(bodyEmbeds, formEmbeds);
		if (dropResult.IsFailed) {
			_logger.LogError("Error with receiving result: {0}", dropResult.CombineMessage());
			BadRequest(dropResult.Errors.FirstOrDefault());
		}

		string? image = null;
		if (file is not null) {
			image = await ToBase64String(file);
		}

		_ = _dropperService.HandleDropRequest(new EndpointId(endpoint), dropResult.Value, image);

		return Ok();
	}

	private Result<RunescapeDrop> GetDrop(EmbedCollection bodyEmbeds, EmbedCollection? formEmbeds) {
		var embeds = formEmbeds ?? bodyEmbeds;
		var embed = embeds?.Embeds.FirstOrDefault();

		if (embed is not null) {
			return _mapper.Map(embed);
		}

		return Result.Fail("could not find one");
	}
	
	private Result<RunescapeDrop> GetDrop(EmbedCollection bodyEmbeds) {
		var embeds = bodyEmbeds;
		var embed = embeds?.Embeds.FirstOrDefault();

		if (embed is not null) {
			return _mapper.Map(embed);
		}

		return Result.Fail("could not find one");
	}

	private static async Task<string> ToBase64String(IFormFile file) {
		await using var stream = file.OpenReadStream();
		await using var memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream);
		var bytes = memoryStream.ToArray();
		return Convert.ToBase64String(bytes);
	}
}
