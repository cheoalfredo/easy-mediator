namespace MediatorSample.Mediator;

/// <summary>
/// Defines a pipeline behavior that wraps around the request handler execution.
/// Behaviors are executed in the order they are registered.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Pipeline behavior handler. Perform any additional behavior and invoke the next delegate as necessary.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate will be the handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task returning the response.</returns>
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Represents an async continuation for the next task to execute in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <returns>Awaitable task returning the response.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
