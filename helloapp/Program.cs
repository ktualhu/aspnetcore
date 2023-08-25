using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();

var _fruit = new ConcurrentDictionary<Guid, Fruit>();
RouteGroupBuilder fruitApi = app.MapGroup("/fruit");
RouteGroupBuilder fruitApiValidation = fruitApi.MapGroup("/");

app.MapGet("/fruit", () => _fruit);

var getFruitById = (Guid id) =>
{
    return _fruit.TryGetValue(id, out var fruit)
        ? TypedResults.Ok(fruit)
        : Results.NotFound();
};

fruitApiValidation.MapGet("/{id:guid}", getFruitById);

var addFruit = (Fruit fruit) =>
{
    var guid = Guid.NewGuid();
    if (_fruit.Keys.Any(key => _fruit[key].Name == fruit.Name))
    {
        return Results.BadRequest(new { Name = $"Fruit with with name {fruit.Name} already exists" });
    }
    return _fruit.TryAdd<Guid, Fruit>(guid, fruit)
        ? TypedResults.Ok(new { id = guid })
        : Results.BadRequest();
};

app.MapPost("/fruit/new", addFruit)
    .AddEndpointFilter(ValidationHelper.ValidateFruit)
    .AddEndpointFilter(async (context, next) =>
    {
        app.Logger.LogInformation("Executing adding filter");
        object? result = await next(context);
        app.Logger.LogInformation($"Handler result: {result}");
        return result;
    });

var updateFruit = (Guid id, Fruit fruit) =>
{
    if (_fruit.TryUpdate(id, fruit, _fruit[id]))
    {
        return Results.Ok(new { Updated = "True" });
    }
    return Results.NotFound(new { Id = "Not found" });
};

fruitApiValidation.MapPut("/{id:guid}", updateFruit);

var removeFruit = (Guid id) =>
{
    return _fruit.TryRemove(id, out _)
        ? TypedResults.Ok(new { id = true })
        : Results.NotFound();
};

fruitApiValidation.MapDelete("/{id:guid}", removeFruit);

app.Run();

record Fruit(string? Name, int? Stock)
{
    public static readonly Dictionary<Guid, Fruit> All = new();
}

class Handlers
{
    public static void ReplaceFruit(Guid guid, Fruit fruit)
    {
        Fruit.All[guid] = fruit;
    }

    public static void AddFruit(Fruit fruit)
    {
        Fruit.All[Guid.NewGuid()] = fruit;
    }

    public static void DeleteFruit(Guid id)
    {
        Fruit.All.Remove(id);
    }
}

class ValidationHelper
{
    //internal static async ValueTask<object?> ValidateId(
    //    EndpointFilterInvocationContext context,
    //    EndpointFilterDelegate next)
    //{
    //    var id = context.GetArgument<Guid?>(0);
    //    if (string.IsNullOrEmpty(id))
    //    {
    //        return TypedResults.ValidationProblem(
    //            new Dictionary<string, string[]>
    //            {
    //                { "Id", new[] { "Cannot be a null" } }
    //            }
    //        );
    //    }
    //    return await next(context);
    //}

    internal static async ValueTask<object?> ValidateFruit(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var fruit = context.GetArgument<Fruit>(0);
        if (fruit is null)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    { "fruit", new[] {"Object cannot be an empty" } }
                }
            );
        }
        if (fruit.Name is null || fruit.Stock is null || fruit.Stock <= 0)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    { "Property", new[] { "Name cannot be null", "Stock cannot be null or less or equal then 0" } }
                });
        }
        return await next(context);
    }
}
