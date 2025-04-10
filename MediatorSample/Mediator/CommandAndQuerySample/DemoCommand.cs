namespace MediatorSample.Mediator.CommandAndQuerySample;

public record DemoCommand(string Data) : IRequest<Unit> { }
    

public class DemoCommandHandler : IRequestHandler<DemoCommand, Unit>
{
    public async Task<Unit> HandleAsync(DemoCommand request)
    {        
        await Task.Run(() => Console.WriteLine($"Doing something with {request.Data}"));
        // como no retornamos na... retorne el unit     
        return Unit.Value;
    }
}