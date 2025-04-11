namespace MediatorSample.Mediator.CommandAndQuerySample;

public record FactorialQuery(int Number) : IRequest<int> { }

public class FactorialQueryHandler : IRequestHandler<FactorialQuery, int>
{
    public async Task<int> HandleAsync(FactorialQuery request)
    {
        return await Task.FromResult(Factorial(request.Number));
    }

    private int Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}
