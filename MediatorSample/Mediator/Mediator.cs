using System.Reflection;

namespace MediatorSample.Mediator;

/// <summary>
/// Defines the interface for the mediator pattern implementation.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to the appropriate handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>The response from the handler.</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken token = default);
}


/// <summary>
/// Implementation of the mediator pattern that resolves and invokes request handlers.
/// </summary>
public class Mediator(IServiceProvider Services) : IMediator
{
    /// <summary>
    /// Sends a request to the appropriate handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the request type.</exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (token.IsCancellationRequested)
        {
            throw new OperationCanceledException("Cannot perform this request, operation cancelled");
        }

        var requestType = request.GetType();
        var handlerInterfaceType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        dynamic handler = Services.GetRequiredService(handlerInterfaceType)
            ?? throw new InvalidOperationException($"No handler registered to process request of type '{requestType.Name}'");

        // Check if there are any pipeline behaviors for this request type
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = Services.GetServices(behaviorType)?.ToList();

        if (behaviors?.Count > 0)
        {
            // Build the pipeline with behaviors
            RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.HandleAsync((dynamic)request, token);

            foreach (dynamic behavior in ((IEnumerable<dynamic>)behaviors).Reverse())
            {
                var currentDelegate = handlerDelegate;
                handlerDelegate = () => behavior.HandleAsync((dynamic)request, currentDelegate, token);
            }

            return await handlerDelegate();
        }

        return await handler.HandleAsync((dynamic)request, token);
    }
}


/// <summary>
/// Extension methods for registering the mediator and its handlers.
/// </summary>
public static class MediatorExtensions
{
    /// <summary>
    /// Registers the mediator and automatically discovers and registers all request handlers in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="assembly">The assembly to scan for handlers. If null, the calling assembly is used.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly? assembly = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (assembly == null)
        {
            assembly = Assembly.GetExecutingAssembly();
        }

        var handlerInterfaceType = typeof(IRequestHandler<,>);

        // Discover all handler implementations in the assembly
        var handlerTypes = assembly.GetTypes()
             .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(implementation => implementation.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType),
                        (implementation, iface) => new { Implementation = implementation, Interface = iface })
            .Distinct();

        // Register each handler as transient
        foreach (var reg in handlerTypes)
        {
            services.AddTransient(reg.Interface, reg.Implementation);
        }

        // Register the mediator as a singleton
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }

    /// <summary>
    /// Adds a pipeline behavior that will be executed for all requests.
    /// Behaviors are executed in the order they are registered.
    /// </summary>
    /// <typeparam name="TBehavior">The type of behavior to register.</typeparam>
    /// <param name="services">The service collection to register the behavior with.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register the behavior for all IPipelineBehavior<,> interfaces it implements
        var behaviorType = typeof(TBehavior);
        var behaviorInterfaces = behaviorType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        foreach (var behaviorInterface in behaviorInterfaces)
        {
            services.AddTransient(behaviorInterface, behaviorType);
        }

        return services;
    }

    /// <summary>
    /// Adds a pipeline behavior that will be executed for all requests.
    /// Behaviors are executed in the order they are registered.
    /// </summary>
    /// <param name="services">The service collection to register the behavior with.</param>
    /// <param name="behaviorType">The type of behavior to register.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorBehavior(this IServiceCollection services, Type behaviorType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(behaviorType);

        // Register the behavior for all IPipelineBehavior<,> interfaces it implements
        var behaviorInterfaces = behaviorType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        foreach (var behaviorInterface in behaviorInterfaces)
        {
            services.AddTransient(behaviorInterface, behaviorType);
        }

        return services;
    }
}
