namespace Tests.E2E;

using System.Net;
using System.Net.Http;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;
public sealed class E2EHost {
    private const    string              JWT_SIGNING_KEY            = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private readonly INetwork            network;
    private readonly PostgreSqlContainer postgres;
    private readonly IContainer          server;
    private readonly string              dockerContextDir;

    public Uri    ServerBaseAddress              { get; }
    public string PublicPostgresConnectionString { get; }

    public E2EHost() {
        var repoRoot = FindRepositoryRoot();

        var assetsConfigsHostPath = Path.Combine(repoRoot, "src", "Escape.Unity", "Assets", "Content.Addressables", "Configs");

        if (!Directory.Exists(assetsConfigsHostPath)) {
            throw new DirectoryNotFoundException($"Assets configs path not found: {assetsConfigsHostPath}");
        }

        var networkName = $"escape-e2e-net-{Guid.NewGuid():N}";
        network = new NetworkBuilder().WithName(networkName).Build();
        network.CreateAsync().GetAwaiter().GetResult();

        var pgAlias = "escape-e2e-pg";

        postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16.9")
            .WithDatabase("escape")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithName($"escape-e2e-postgres-{Guid.NewGuid():N}")
            .WithNetwork(network)
            .WithNetworkAliases(pgAlias)
            .Build();

        postgres.StartAsync().GetAwaiter().GetResult();

        dockerContextDir = Path.Combine(Path.GetTempPath(), $"escape-e2e-docker-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dockerContextDir);
        var ctxSrcRoot    = Path.Combine(dockerContextDir, "src");
        var ctxServerRoot = Path.Combine(ctxSrcRoot, "Server");
        var ctxUnityRoot  = Path.Combine(ctxSrcRoot, "Escape.Unity");
        Directory.CreateDirectory(ctxServerRoot);
        Directory.CreateDirectory(ctxUnityRoot);

        CopyDirectory(Path.Combine(repoRoot, "src", "Server", "Game.ServerRunner"), Path.Combine(ctxServerRoot, "Game.ServerRunner"));
        CopyDirectory(Path.Combine(repoRoot, "src", "Server", "Game.Shared"), Path.Combine(ctxServerRoot, "Game.Shared"));
        CopyDirectory(Path.Combine(repoRoot, "src", "Server", "Quantum.Shared"), Path.Combine(ctxServerRoot, "Quantum.Shared"));
        CopyDirectory(Path.Combine(repoRoot, "src", "Server", "assemblies"), Path.Combine(ctxServerRoot, "assemblies"));
        CopyDirectory(Path.Combine(repoRoot, "src", "Escape.Unity", "Assets", "Content.Addressables", "Configs"), Path.Combine(ctxUnityRoot, "Assets", "Content.Addressables", "Configs"));
        File.Copy(Path.Combine(repoRoot, "Dockerfile"), Path.Combine(dockerContextDir, "Dockerfile"), true);

        var image = new ImageFromDockerfileBuilder()
            .WithName($"escape-e2e-server-image-{Guid.NewGuid():N}")
            .WithDockerfileDirectory(dockerContextDir)
            .WithDockerfile("Dockerfile")
            .Build();

        image.CreateAsync().GetAwaiter().GetResult();

        server = new ContainerBuilder()
            .WithImage(image)
            .WithName($"escape-e2e-server-{Guid.NewGuid():N}")
            .WithNetwork(network)
            .WithEnvironment("ENV", "dev")
            .WithEnvironment("PG_IP", pgAlias)
            .WithEnvironment("PG_USER", "postgres")
            .WithEnvironment("PG_PASSWORD", "postgres")
            .WithEnvironment("JWTSIGNINGKEY", JWT_SIGNING_KEY)
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(8080))
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
            .Build();

        server.StartAsync().GetAwaiter().GetResult();

        var httpPort = server.GetMappedPublicPort(8080);
        ServerBaseAddress = new Uri($"http://localhost:{httpPort}");

        var pgPort = postgres.GetMappedPublicPort(5432);
        PublicPostgresConnectionString = $"Host=localhost;Port={pgPort};Database=escape;Username=postgres;Password=postgres";

        WaitForServerReady(ServerBaseAddress, TimeSpan.FromMinutes(2), "TDM").GetAwaiter().GetResult();
    }

    public HttpClient CreateHttpClient() {
        var client = new HttpClient();
        client.BaseAddress = ServerBaseAddress;
        return client;
    }

    private static string FindRepositoryRoot() {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null) {
            var srcDir = Path.Combine(dir.FullName, "src");
            if (Directory.Exists(Path.Combine(srcDir, "Server")) && Directory.Exists(Path.Combine(srcDir, "Escape.Unity"))) {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Repository root not found (expected 'src/Server' and 'src/Escape.Unity').");
    }

    private static async Task WaitForServerReady(Uri baseAddress, TimeSpan timeout, string expectedBody) {
        using var client   = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(5) };
        var       deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline) {
            try {
                using var resp = await client.GetAsync("/");
                if (resp.StatusCode == HttpStatusCode.OK) {
                    var body = (await resp.Content.ReadAsStringAsync()).Trim();
                    if (string.Equals(body, expectedBody, StringComparison.Ordinal)) {
                        return;
                    }
                }
            }
            catch (Exception e) { }
            await Task.Delay(1000);
        }
        throw new TimeoutException($"Server did not become ready at {baseAddress} within {timeout}.");
    }

    private static void CopyDirectory(string sourceDir, string destinationDir) {
        Directory.CreateDirectory(destinationDir);
        foreach (var filePath in Directory.EnumerateFiles(sourceDir, "*", SearchOption.TopDirectoryOnly)) {
            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(destinationDir, fileName);
            File.Copy(filePath, destPath, true);
        }
        foreach (var subDir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly)) {
            var dirName = Path.GetFileName(subDir);
            CopyDirectory(subDir, Path.Combine(destinationDir, dirName));
        }
    }
}