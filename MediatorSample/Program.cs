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


app.MapGet("/mediator/{query}", async (IMediator mediator, int query, CancellationToken token) =>
{    
    var response = await mediator.Send(new FactorialQuery(query),token);
    return Results.Ok(response);
});

app.MapPost("/mediator", async (IMediator mediator, DemoCommand command, CancellationToken token) =>
{    
   await mediator.Send(command,token);   
});

app.Run();

