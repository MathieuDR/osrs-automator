using MathieuDR.Common.Extensions;

namespace DiscordBot.Commands.Interactive2.Base.Handlers;

public abstract class
	CommandHandlerBase<TRequest, TContext> : ICommandHandler<TRequest, TContext>
	where TRequest : ICommandRequest<TContext> where TContext : BaseInteractiveContext {
	protected IServiceProvider ServiceProvider { get; }
	protected TContext Context { get; set; }
	protected TRequest Request { get; set; }
	protected ICommandDefinition CommandDefinition { get; set; }

	public CommandHandlerBase(IServiceProvider serviceProvider) {
		ServiceProvider = serviceProvider;
	}

	public Task<Result> Handle(TRequest request, CancellationToken cancellationToken = default) {
		// Set context and request
		Context = request.Context;
		Request = request;
		CommandDefinition = GetCommandDefinition();

		// Do work
		return DoWork(cancellationToken);
	}

	private ICommandDefinition GetCommandDefinition() {
		// Get first generic parameter
		var genericType = typeof(TRequest).GetInterfaces()
			.Select(x => x.GetGenericArguments().FirstOrDefault(genericType => typeof(ICommandDefinition).IsAssignableFrom(genericType)))
			.FirstOrDefault();

		// Check if generic parameter is ICommandDefinition
		if (genericType is not null) {
			// Instantiate a object of the generic type
			// Use activator instead of a compiled lambda.
			// We can improve this by creating a singleton service that holds all the activators.
			return Activator.CreateInstance(genericType, ServiceProvider).As<ICommandDefinition>();
		}

		throw new Exception("Cannot find generic parameter");
	}


	protected abstract Task<Result> DoWork(CancellationToken cancellationToken);
}