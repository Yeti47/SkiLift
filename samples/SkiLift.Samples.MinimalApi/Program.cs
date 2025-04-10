using SkiLift.Configuration;
using SkiLift.Samples.MinimalApi.Behaviors;
using SkiLift.Samples.MinimalApi.Commands;
using SkiLift.Samples.MinimalApi.Queries;
using SkiLift.Samples.MinimalApi.Repositories;
using SkiLift;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Configure logging
builder.Services.AddLogging();

// Configure Open API
builder.Services.AddOpenApi();

// Configure SkiLift with handlers and behaviors
builder.Services.AddSkiLift(config =>
{
    // Register all handlers from this assembly
    config.AddHandlersFromAssemblyContaining<Program>();
    
    // Register pipeline behaviors
    config.AddPipelineBehavior(typeof(LoggingBehavior<,>));
    config.AddPipelineBehavior(typeof(ValidationBehavior<,>));
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}


// Define API endpoints
app.MapGet("/orders", async (IRequestDispatcher dispatcher) =>
{
    // Query to get all orders
    var orders = await dispatcher.Send(new GetAllOrdersQuery());

    return Results.Ok(orders);
})
.WithName("GetAllOrders")
.WithDescription("Gets all orders in the system");


app.MapGet("/orders/{id}", async (string id, IRequestDispatcher dispatcher) =>
{
    // Query to get a specific order by ID
    var order = await dispatcher.Send(new GetOrderByIdQuery { Id = id });
    
    if (order == null)
        return Results.NotFound();
        
    return Results.Ok(order);
})
.WithName("GetOrderById")
.WithDescription("Gets a specific order by its ID");


app.MapPost("/orders", async (CreateOrderCommand command, IRequestDispatcher dispatcher) =>
{
    try
    {
        // Command to create a new order
        var orderId = await dispatcher.Send(command);

        return Results.Created($"/orders/{orderId}", orderId);
    }
    catch (Exception ex) when (ex is System.ComponentModel.DataAnnotations.ValidationException)
    {
        return Results.BadRequest(ex.Message);
    }
})
.WithName("CreateOrder")
.WithDescription("Creates a new order");


app.MapDelete("/orders/{id}", async (string id, IRequestDispatcher dispatcher) =>
{
    // Command to delete an order
    await dispatcher.Send(new DeleteOrderCommand { Id = id });
    return Results.NoContent();
})
.WithName("DeleteOrder")
.WithDescription("Deletes an order by its ID");


app.Run();
