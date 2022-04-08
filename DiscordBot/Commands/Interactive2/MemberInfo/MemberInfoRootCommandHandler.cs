using System.Text;

namespace DiscordBot.Commands.Interactive2.MemberInfo;

public class MemberInfoRootCommandHandler : ApplicationCommandHandlerBase<MemberInfoRootCommandRequest> {
	public MemberInfoRootCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
	
	private const string CsvDelimiter = ",";
	
	protected override Task<Result> DoWork(CancellationToken cancellationToken) {
		var members = Context.Guild.Users.Where(x => !x.IsBot).ToList();
		var roles = Context.Guild.Roles.ToList();
		
		var csvBuilder = new StringBuilder();
		csvBuilder.Append("User ID");
		csvBuilder.Append(CsvDelimiter);
		csvBuilder.Append("User Name");
		csvBuilder.Append(CsvDelimiter);
		csvBuilder.Append("User Discriminator");
		csvBuilder.Append(CsvDelimiter);
		csvBuilder.Append("nickname");
		csvBuilder.Append(CsvDelimiter);
		csvBuilder.Append("Joined At");
		csvBuilder.Append(CsvDelimiter);
		csvBuilder.Append("osrs name");
		csvBuilder.Append(CsvDelimiter);
		
		foreach (var role in roles) {
			csvBuilder.Append(role.Name);
			csvBuilder.Append(CsvDelimiter);
		}
		
		csvBuilder.Append("\n");
		
		foreach (var member in members) {
			var osrs = member.Nickname?.Split("/").FirstOrDefault()?.Trim() ?? "";

			csvBuilder.Append(member.Id);
			csvBuilder.Append(CsvDelimiter);
			csvBuilder.Append(member.Username);
			csvBuilder.Append(CsvDelimiter);
			csvBuilder.Append(member.Discriminator);
			csvBuilder.Append(CsvDelimiter);
			csvBuilder.Append(member.Nickname);
			csvBuilder.Append(CsvDelimiter);
			csvBuilder.Append(member.JoinedAt);
			csvBuilder.Append(CsvDelimiter);
			csvBuilder.Append(osrs);
			csvBuilder.Append(CsvDelimiter);
			var memberRoles = member.Roles.ToList().Select(x=>x.Id).ToList();
			
			foreach (var role in roles) {
				csvBuilder.Append(memberRoles.Contains(role.Id) ? "true" : "false");
				csvBuilder.Append(CsvDelimiter);
			}
			
			csvBuilder.Append("\n");
		}
		
		var csv = csvBuilder.ToString();
		
		// string to stream
		using var memoryStream = new MemoryStream();
		using var streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(csv);
		streamWriter.Flush();
		memoryStream.Position = 0;

		Context.InnerContext.RespondWithFileAsync(memoryStream, "memberinfo.csv", "Members", null,  false, true, AllowedMentions.None, null, null, RequestOptions.Default);
		return Task.FromResult(Result.Ok());
	}
}