var builder = WebApplication.CreateBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Warning);
var app = builder.Build();

var version = Environment.GetEnvironmentVariable("GAME_VERSION") ?? "0";

app.MapGet("/", () => version);
app.MapGet("/version", () => version);

app.Run();
