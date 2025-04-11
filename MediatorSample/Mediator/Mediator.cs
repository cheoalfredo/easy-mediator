using System.Reflection;
using System.Xml.Schema;

namespace MediatorSample.Mediator;

public interface IMediator
{    
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken t);
}


public class Mediator(IServiceProvider Services) : IMediator
{   

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken token = default)
    {
        var requestType = request.GetType();
        var handlerInterfaceType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));
            
        
        dynamic handler = Services.GetRequiredService(handlerInterfaceType) 
            ?? throw new InvalidOperationException($"There's no handler registered to process {requestType}"); ;
        return await handler.HandleAsync((dynamic)request, token);

    }
}


public static class MediatorExtensions
{
    // Este método es el que se encarga de registrar via DI el Mediator y
    // los handlers que luego serán ubicados por el método IRequestHandler<,>.HandleAsync() 
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly? assembly = null)
    {
        if (assembly == null)
        {
            assembly = Assembly.GetExecutingAssembly();
        }

        var handlerInterfaceType = typeof(IRequestHandler<,>);

        var handlerTypes = assembly.GetTypes()
             .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(implementation => implementation.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType),
                        (implementation, iface) => new { Implementation = implementation, Interface = iface })
            .Distinct();

        // Register each handler as transient.
        foreach (var reg in handlerTypes)
        {            
            services.AddTransient(reg.Interface, reg.Implementation);
        }

        // Register the mediator so it can be resolved.
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }
}
