using MediatorSample.Mediator;
using MediatorSample.Mediator.CommandAndQuerySample;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMediator();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/mediator", async (IMediator mediator) =>
{
    var request = new FactorialQuery(5);
    var response = await mediator.Send(request);
    return Results.Ok(response);
});

app.MapPost("/mediator", async (IMediator mediator, DemoCommand command) =>
{    
   await mediator.Send(command);   
});

app.Run();

