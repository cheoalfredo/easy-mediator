namespace MediatorSample.Mediator.Behaviors;

/// <summary>
/// A sample pipeline behavior that logs request execution details.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling request: {RequestName}", requestName);

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await next();
            var elapsed = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Request {RequestName} completed in {ElapsedMs}ms",
                requestName,
                elapsed.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogError(
                ex,
                "Request {RequestName} failed after {ElapsedMs}ms: {ErrorMessage}",
                requestName,
                elapsed.TotalMilliseconds,
                ex.Message);
            throw;
        }
    }
}
