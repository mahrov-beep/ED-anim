namespace Game.ServerRunner;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using Core;
using Shared;
using Shared.UserProfile;
using Shared.UserProfile.Data;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Multicast;
using Multicast.Collections;
using Multicast.DirtyDataEditor;
using Orleans.Serialization;
using Db;

internal class Program(string[] args) {
    private static readonly Option<string> OptEnvironment        = new("--environment");
    private static readonly Option<string> OptAssetsPath         = new("--assetsPath");
    private static readonly Option<string> OptHostUrls           = new("--urls");
    private static readonly Option<string> OptTempDirectory      = new("--tempDirectory");
    private static readonly Option<string> OptPostgresConnection = new("--postgresConnection");
    private static readonly Option<string> OptJwtIssuer          = new("--jwtIssuer");
    private static readonly Option<string> OptJwtAudience        = new("--jwtAudience");
    private static readonly Option<string> OptJwtSigningKey      = new("--jwtSigningKey");
    private static readonly Option<bool>   OptDisableLoadoutLost = new("--disableLoadoutLost", () => false);

    private static async Task Main(string[] args) {
        Console.Title = "Multicast.Game.ServerRunner";

        Console.WriteLine($"=== SERVER STARTING ===");
        var program = new Program(args);
        var command = new RootCommand();
        command.AddOption(OptEnvironment);
        command.AddOption(OptAssetsPath);
        command.AddOption(OptHostUrls);
        command.AddOption(OptTempDirectory);
        command.AddOption(OptPostgresConnection);
        command.AddOption(OptJwtIssuer);
        command.AddOption(OptJwtAudience);
        command.AddOption(OptJwtSigningKey);
        command.AddOption(OptDisableLoadoutLost);
        command.SetHandler(program.Run);
        await command.InvokeAsync(args);
        Console.WriteLine($"=== SERVER STOPPED ===");
    }

    private async Task Run(InvocationContext invocationContext) {
        var environment        = OptEnvironment.GetValueFrom(invocationContext);
        var assetsPath         = OptAssetsPath.GetValueFrom(invocationContext);
        var tempDirectory      = OptTempDirectory.GetValueFrom(invocationContext);
        var postgresConnection = OptPostgresConnection.GetValueFrom(invocationContext);
        var jwtIssuer          = OptJwtIssuer.GetValueFrom(invocationContext);
        var jwtAudience        = OptJwtAudience.GetValueFrom(invocationContext);
        var jwtSigningKey      = OptJwtSigningKey.GetValueFrom(invocationContext);
        var disableLoadoutLost = OptDisableLoadoutLost.GetValueFrom(invocationContext);

        ArgumentException.ThrowIfNullOrWhiteSpace(assetsPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(tempDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(postgresConnection);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtIssuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtAudience);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtSigningKey);

        if (!Directory.Exists(tempDirectory)) {
            Directory.CreateDirectory(tempDirectory);
        }

        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole());

        var logger = loggerFactory.CreateLogger<Program>();
            
        MulticastLog.DebugLogCallback = (tag, message) => logger.LogInformation("[{Tag}]: {Message}", tag, message);
        MulticastLog.ErrorLogCallback = (tag, message) => logger.LogError("[{Tag}]: {Message}", tag, message);

        Shared.ServerConfig.DisableLoadoutLost = disableLoadoutLost;
        logger.LogInformation("DisableLoadoutLost: {DisableLoadoutLost}", disableLoadoutLost);

        MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
            .WithSecurity(MessagePackSecurity.UntrustedData);

        var builder = WebApplication.CreateBuilder(args);

        var jwtService = new JwtService(jwtIssuer, jwtAudience, TimeSpan.FromHours(1), jwtSigningKey, environment);

        var textAssetCache = CreateTextAssetCache(assetsPath);
        var gameDef        = GameDef.FromCache(textAssetCache);

        logger.LogInformation("Found assets: {Assets}",
            textAssetCache.EnumeratePaths().Aggregate(new StringBuilder(), (sb, it) => sb.AppendLine(it)).ToString()
        );

        if (DirtyDataParser.Errors is { Count: > 0 } ddeErrors) {
            throw new Exception($"DDE configuration is invalid, errors: {ddeErrors.Aggregate(new StringBuilder(), (sb, it) => sb.AppendLine(it))}");
        }

        builder.Services.AddSingleton(_ => gameDef);
        builder.Services.AddSingleton(_ => jwtService);

        builder.Services.Scan(scan => scan.FromAssemblyOf<GameSharedAssembly>()
            .AddClasses(classes => classes.AssignableTo<IServerCommandHandlerBase>()).AsSelfWithInterfaces()
        );
        builder.Services.Scan(scan => scan.FromAssemblyOf<Program>()
            .AddClasses(classes => classes.AssignableTo<IServerCommandHandlerBase>()).AsSelfWithInterfaces()
        );

        builder.Services.AddSingleton<ServerCommandHandlerRegistry<UserProfileServerCommandContext, SdUserProfile>>();
        builder.Services.AddSingleton<ITimeService>(_ => new ServerTimeService());

        builder.Host.UseOrleans(siloBuilder => {
            siloBuilder.UseLocalhostClustering();
            siloBuilder.Services.AddSerializer(serializerBuilder => serializerBuilder.AddMessagePackSerializer());
            siloBuilder.AddMemoryStreams(OrleansConstants.Streams.SERVER_EVENTS)
                .AddMemoryGrainStorage("PubSubStore");
        });

        builder.Services.AddControllers(options => {
            options.InputFormatters.Add(new MessagePackInputFormatter());
            options.OutputFormatters.Add(new MessagePackOutputFormatter());
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.RequireHttpsMetadata      = false;
                options.TokenValidationParameters = jwtService.GetTokenValidationParameters();
            });

        builder.Services.AddDbContextFactory<GameDbContext>(options => options.UseNpgsql(postgresConnection));

        await using var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseWebSockets();

        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/error");
        }

        using (var dbContext = app.Services.GetRequiredService<IDbContextFactory<GameDbContext>>().CreateDbContext()) {
            await dbContext.Database.MigrateAsync();
        }

        app.MapGet("/", async (httpContext) => await httpContext.Response.WriteAsync("TDM"));
        app.MapGet("/version/", async (httpContext) => await httpContext.Response.WriteAsync("0.0.1"));
        app.MapGet("/ex/", async (httpContext) => throw new Exception("DEMO"));
        app.Map("/error", ap => ap.Run(async context => context.Response.StatusCode = StatusCodes.Status500InternalServerError));
        app.MapControllers();

        await app.RunAsync();
    }

    private static IEnumerableCache<TextAsset> CreateTextAssetCache(string assetsPath) {
        var validExtensions = new[] { ".yaml", ".json" };
        var keyToPathMap = Directory.EnumerateFiles(assetsPath, "*", SearchOption.AllDirectories)
            .Select(path => {
                var ext           = Path.GetExtension(path);
                var keyWithExt    = path.Substring(assetsPath.Length).Replace(Path.DirectorySeparatorChar, '/');
                var keyWithoutExt = keyWithExt.Remove(keyWithExt.Length - ext.Length);
                return new { ext, path, key = keyWithoutExt };
            })
            .Where(it => validExtensions.Contains(it.ext))
            .ToDictionary(it => it.key, it => it.path);

        return new FuncEnumerableCache<TextAsset>(
            getter: key => new TextAsset(File.ReadAllText(keyToPathMap[key])),
            pathsGetter: () => keyToPathMap.Keys
        );
    }
}