namespace MediatorSample.Mediator.CommandAndQuerySample;

/// <summary>
/// A sample query that calculates the factorial of a number.
/// </summary>
/// <param name="Number">The number to calculate the factorial for.</param>
public record FactorialQuery(int Number) : IRequest<int> { }

/// <summary>
/// Handles the FactorialQuery by computing the factorial of the given number.
/// </summary>
public class FactorialQueryHandler : IRequestHandler<FactorialQuery, int>
{
    public async Task<int> HandleAsync(FactorialQuery request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return await Task.FromResult(-1);
        }

        return await Task.FromResult(Factorial(request.Number));
    }

    private static int Factorial(int number)
    {
        if (number <= 1) return 1;
        return number * Factorial(number - 1);
    }
}
