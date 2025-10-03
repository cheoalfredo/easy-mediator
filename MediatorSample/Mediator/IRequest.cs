namespace MediatorSample.Mediator;

/// <summary>
/// Represents a unit type for requests that do not return a value.
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// Gets the singleton instance of the unit type.
    /// </summary>
    public static readonly Unit Value = new();
}

/// <summary>
/// Marker interface for requests that return a response of type TResponse.
/// </summary>
/// <typeparam name="TResponse">The type of response expected from the request.</typeparam>
public interface IRequest<TResponse> { }

/// <summary>
/// Defines a handler for a request of type TRequest that returns a response of type TResponse.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
/// <typeparam name="TResponse">The type of response to return.</typeparam>
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>The response from handling the request.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken token);
}
