var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddRazorPages();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHealthChecks("/healt");
app.MapRazorPages();

app.Run();

record class Person(Guid Id, string FirstName, string SecondName);
