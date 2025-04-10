using Microsoft.AspNetCore.Server.HttpSys;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MediatorSample.Mediator
{

    public interface IMediator
    {
        public void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler) where TRequest : IRequest<TResponse>;
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
    }


    public class Mediator : IMediator
    {

        private readonly Dictionary<Type, Func<object, Task<object>>> Handlers;

        public Mediator()
        {
            Handlers = [];
        }



        public void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler) where TRequest : IRequest<TResponse>
        {

            if (Handlers is not null)
            {
                Handlers[typeof(TRequest)] = async (request) =>
                        await handler.HandleAsync((TRequest)request)
                        ?? throw new InvalidOperationException($"Unable to register handler for {request.GetType().Name}");
            }
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            var requestType = request.GetType();
            if (Handlers.TryGetValue(requestType, out var handler))
            {
                return (TResponse)await handler(request);
            }
            throw new InvalidOperationException($"No handler registered for {requestType.Name}");
        }
    }


    public static class MediatorExtensions
    {
        // Este método es el que se encarga de registrar via DI el Mediator y
        // los handlers en en singleton del mismo
        public static IServiceCollection AddMediator(this IServiceCollection services, Assembly? assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            // Listamos todos aquellos objetos que implementan IRequestHandler
            var handlers = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToList();

            var mediator = new Mediator();

            // Iteramos todos los handlers encontrados.
            foreach (var entry in handlers)
            {
                var instance = Activator.CreateInstance(entry);
                // Sacamos TRequest & TResponse
                var handlerInterface = entry.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
                var requestType = handlerInterface.GetGenericArguments()[0];
                var responseType = handlerInterface.GetGenericArguments()[1];

                // Reflexion para invocar el método RegisterHandler (esto porque lo hacemos en ejecución y no en compilación)
                var registerMethod = typeof(Mediator).GetMethod(nameof(Mediator.RegisterHandler))!
                    .MakeGenericMethod(requestType, responseType);

                registerMethod.Invoke(mediator, [instance]);
            }

            services.AddSingleton<IMediator>(svc =>
            {

                return mediator;
            });

            return services;
        }
    }
}
