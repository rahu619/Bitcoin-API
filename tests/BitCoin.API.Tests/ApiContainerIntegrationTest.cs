using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitCoin.API.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ApiContainerIntegrationTest
{
    private const int ApiPort = 8080;
    private const string ImageName = "bitcoin-api-integration:local";
    private static readonly TimeSpan ContainerStartTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan HealthProbeTimeout = TimeSpan.FromSeconds(45);

    private static IFutureDockerImage? _image;
    private static IContainer? _container;
    private static HttpClient? _client;

    [ClassInitialize]
    public static async Task InitializeAsync(TestContext _)
    {
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", bool.TrueString);
        Environment.SetEnvironmentVariable("TESTCONTAINERS_CHECKS_DISABLE", bool.TrueString);

        var repositoryRoot = ResolveRepositoryRoot();
        await BuildApiImageAsync(repositoryRoot).ConfigureAwait(false);

        _container = new ContainerBuilder(ImageName)
            .WithPortBinding(ApiPort, true)
            .WithEnvironment("ASPNETCORE_URLS", "http://+:8080")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .Build();

        using var cts = new CancellationTokenSource(ContainerStartTimeout);
        await _container.StartAsync(cts.Token).ConfigureAwait(false);

        var host = _container.Hostname;
        if (string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), bool.TrueString, StringComparison.OrdinalIgnoreCase))
        {
            host = "host.docker.internal";
        }

        var baseAddress = new UriBuilder("http", host, _container.GetMappedPublicPort(ApiPort)).Uri;
        _client = new HttpClient
        {
            BaseAddress = baseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };

        await WaitUntilHealthyAsync(_client, HealthProbeTimeout, cts.Token).ConfigureAwait(false);
    }

    [ClassCleanup]
    public static async Task CleanupAsync()
    {
        _client?.Dispose();

        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }

        if (_image is not null)
        {
            await _image.DeleteAsync().ConfigureAwait(false);
        }
    }

    [TestMethod]
    public async Task HealthzShouldReturn200()
    {
        Assert.IsNotNull(_client, "Test HTTP client was not initialized.");

        using var response = await _client.GetAsync("/healthz").ConfigureAwait(false);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "BitCoin.API.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing BitCoin.API.slnx.");
    }

    private static async Task BuildApiImageAsync(string repositoryRoot)
    {
        _image = new ImageFromDockerfileBuilder()
            .WithName(ImageName)
            .WithDockerfileDirectory(repositoryRoot)
            .WithDockerfile(Path.Combine("src", "BitCoin.API", "Dockerfile"))
            .Build();

        await _image.CreateAsync().ConfigureAwait(false);
    }

    private static async Task WaitUntilHealthyAsync(HttpClient client, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var response = await client.GetAsync("/healthz", cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException("The API container did not become healthy in the expected time window.");
    }
}
