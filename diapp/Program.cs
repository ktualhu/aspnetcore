using diapp;
using diapp.Extensions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(o =>
    {
        o.ValidateScopes = false;
        o.ValidateOnBuild = false;
    });
}

builder.Services.AddEmailSender();
builder.Services.AddSendMessage();

builder.Services.AddScoped<DataContext>();
builder.Services.AddSingleton<Repository>();

var app = builder.Build();

app.MapGet("/single/{username}", RegisterUser);
app.MapGet("/multi/{username}", SendMultiMessages);
app.MapGet("/transient", RowCounts);

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    Console.WriteLine($"Retrieved scope - {dbContext.RowCount}");
}

app.Run();

string RegisterUser(string username, IEmailSender emailSender)
{
    emailSender.SendEmail(username);
    return $"Email sent to {username}";
}

string SendMultiMessages(string username, IEnumerable<IMessageSender> senders)
{
    foreach (var sender in senders)
    {
        Console.WriteLine(sender.SendMessage(username));
    }

    return "Done!";
};

string RowCounts(DataContext dataContext, Repository repository)
{
    int dbCount = dataContext.RowCount;
    int repDbCount = repository.RowCount;
    int repCount = repository.RepRowCounts;
    return $"DataContext count: {dbCount}; Repository DataContext count: {repDbCount}; Repository: {repCount}";
}

interface IRepository<T>
{
    public void CreateRepository();
}

class DbRepository<T> : IRepository<T>
{
    public DbRepository() {}

    public void CreateRepository() => Console.WriteLine("DB repository created");
}

class TestRepository<T> : IRepository<T>
{
    public TestRepository() {}

    public void CreateRepository() => Console.WriteLine("Test repository created");
}

interface IMessageSender
{
    public string SendMessage(string message);
}

public class Repository
{
    private DataContext _dataContext;
    public int RepRowCounts { get; } = Random.Shared.Next(1, 1_000_000_000);

    public Repository(DataContext dataContext) => _dataContext = dataContext;

    public int RowCount => _dataContext.RowCount;
}

public class DataContext
{
    public int RowCount { get; } = Random.Shared.Next(1, 1_000_000_000);
}
