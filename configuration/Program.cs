using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"app.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

builder.Services.Configure<AppDisplaySettings>(
    builder.Configuration.GetSection(nameof(AppDisplaySettings)));
builder.Services.Configure<MapSettings>(
    builder.Configuration.GetSection(nameof(MapSettings)));

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

var app = builder.Build();
var zoomLevel = app.Configuration["MapSettings:DefaultZoomLevel"];
var lat = app.Configuration["MapSettings:DefaultLocation:latitude"];
var lon = app.Configuration["MapSettings:DefaultLocation:longitude"];

if (!builder.Environment.IsDevelopment())
    app.UseExceptionHandler();

app.Use(async (context, next) =>
{
    app.Logger.LogInformation($"Current env: {app.Environment.EnvironmentName} / {app.Environment.ApplicationName}");
    await next.Invoke(context);
});

app.MapGet("/", () => app.Configuration.AsEnumerable());
app.MapGet("/map", () => $"Zoom: {zoomLevel}\n Latitude: {lat}\n Longitude: {lon}");
app.MapGet("/display-settings", (IOptions<AppDisplaySettings> options) =>
{
    var settings = options.Value;
    var title = settings.Title;
    var showCopyright = settings.ShowCopyright;
    return new { title, showCopyright };
});

app.MapGet("/map-settings", (IOptions<MapSettings> options) =>
{
    var settings = options.Value;
    var defaultZoomLevel = settings.DefaultZoomLevel;
    var defaultLocation = settings.DefaultLocation;
    return new { defaultZoomLevel, latitude = defaultLocation.Latitude, longitude = defaultLocation.Longitude };
});

await using (var scope = app.Services.CreateAsyncScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var location = config.GetSection("MapSettings").GetValue<string>("DefaultZoomLevel");
    app.Logger.LogInformation($"ZoomLevel: {location}");
    if (app.Environment.IsDevelopment())
        app.Logger.LogWarning($"Password: {app.Configuration["Password"]}");
        
}

app.Run();

public class AppDisplaySettings
{
    public string Title { get; set; } = "";
    public bool ShowCopyright { get; set; }
}

public class MapSettings
{
    public int DefaultZoomLevel { get; set; }
    public Location DefaultLocation { get; set; } = new Location();
}

public class Location
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}