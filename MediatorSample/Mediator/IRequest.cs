namespace MediatorSample.Mediator
{

    public readonly struct Unit
    {
        public static readonly Unit Value = new();
    }
    public interface IRequest<TResponse> { }

    public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}
