namespace MediatorSample.Mediator.CommandAndQuerySample;

/// <summary>
/// A sample command that demonstrates command handling with logging.
/// </summary>
/// <param name="Data">The data to process.</param>
public record DemoCommand(string Data) : IRequest<Unit> { }

/// <summary>
/// Handles the DemoCommand by logging the data.
/// </summary>
public class DemoCommandHandler : IRequestHandler<DemoCommand, Unit>
{
    readonly ILogger<DemoCommandHandler> _logger;

    public DemoCommandHandler(ILogger<DemoCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Unit> HandleAsync(DemoCommand request, CancellationToken token)
    {
        if (!token.IsCancellationRequested)
        {
            await Task.Run(() => _logger.LogInformation($"Processing command with data: {request.Data}"), token);
        }
        // Since we don't return anything, return Unit.Value
        return Unit.Value;
    }
}