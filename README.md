# Easy Mediator

A lightweight implementation of the mediator pattern for .NET, providing a simple alternative to MediatR for CQRS applications.

## Overview

Easy Mediator is a minimal, easy-to-understand implementation of the mediator pattern that helps decouple request/response logic from your business logic. It's perfect for learning or for projects that need a straightforward mediator without additional complexity.

## Features

- **Simple API**: Minimal surface area with just `IMediator`, `IRequest<T>`, and `IRequestHandler<T, R>`
- **Automatic Handler Discovery**: Automatically registers all handlers in your assembly via dependency injection
- **Pipeline Behaviors**: Add cross-cutting concerns like logging, validation, and exception handling
- **Cancellation Support**: Built-in support for `CancellationToken` to cancel long-running operations
- **Type-Safe**: Strongly-typed request/response pattern
- **Well-Documented**: Comprehensive XML documentation for IntelliSense support
- **CQRS Ready**: Separate command and query patterns with `Unit` type for void operations

## Getting Started

### Installation

1. Add the `MediatorSample.Mediator` namespace to your project
2. Register the mediator in your service container

### Registration

```csharp
// Register mediator and auto-discover handlers in the current assembly
builder.Services.AddMediator();

// Or specify a different assembly
builder.Services.AddMediator(typeof(MyHandler).Assembly);
```

### Defining Requests and Handlers

**Query Example** (returns a value):
```csharp
// Define a query
public record GetUserQuery(int UserId) : IRequest<User>;

// Define the handler
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> HandleAsync(GetUserQuery request, CancellationToken token)
    {
        // Your logic here
        return await _repository.GetUserAsync(request.UserId, token);
    }
}
```

**Command Example** (performs an action without returning a value):
```csharp
// Define a command
public record CreateUserCommand(string Name, string Email) : IRequest<Unit>;

// Define the handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Unit>
{
    public async Task<Unit> HandleAsync(CreateUserCommand request, CancellationToken token)
    {
        // Your logic here
        await _repository.CreateUserAsync(request.Name, request.Email, token);
        return Unit.Value;
    }
}
```

### Sending Requests

```csharp
public class MyController
{
    private readonly IMediator _mediator;

    public MyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> GetUser(int id, CancellationToken token)
    {
        var user = await _mediator.Send(new GetUserQuery(id), token);
        return Ok(user);
    }

    public async Task<IActionResult> CreateUser(CreateUserCommand command, CancellationToken token)
    {
        await _mediator.Send(command, token);
        return Ok();
    }
}
```

## Pipeline Behaviors

Pipeline behaviors allow you to add cross-cutting concerns like logging, validation, caching, or exception handling that execute before and after request handlers.

### Creating a Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

### Registering Behaviors

```csharp
// Register a behavior using the extension method
builder.Services.AddMediatorBehavior<LoggingBehavior<TRequest, TResponse>>();

// Or register manually for all request types
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>), 
    typeof(LoggingBehavior<,>));
```

Behaviors execute in the order they are registered, wrapping around the handler like an onion:

```
Behavior 1 (before) → Behavior 2 (before) → Handler → Behavior 2 (after) → Behavior 1 (after)
```

## Sample Application

The repository includes a sample ASP.NET Core application demonstrating:
- **FactorialQuery**: A query that calculates factorial of a number
- **DemoCommand**: A command that logs data

Run the sample:
```bash
dotnet run --project MediatorSample
```

## Architecture

```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ Send(request)
       ▼
┌──────────────┐
│  IMediator   │
└──────┬───────┘
       │ Route
       ▼
┌──────────────┐
│ IRequestHandler │
└──────────────┘
```

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests.

## License

This project is provided as-is for educational and practical use.
