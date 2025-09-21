using System.Net;
using System.Net.Http;

using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder.Services
    .AddOptions<ExternalAPISettings>()
    .Bind(builder.Configuration.GetSection("ExternalAPISettings"))
    .ValidateDataAnnotations()
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Url?.Historical),
        "The external API historical URL must be configured.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Url?.Base),
        "The external API base URL must be configured.")
    .ValidateOnStart();

builder.Services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
builder.Services
    .AddHttpClient<IBitcoinPriceIndexClient, BitcoinPriceIndexClient>("BitcoinPriceIndex")
    .ConfigureHttpClient((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<ExternalAPISettings>>().Value;
        var baseAddress = options.Url?.Base;

        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            throw new InvalidOperationException("The external API base URL must be configured.");
        }

        client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
builder.Services.AddHostedService<BitCoinApiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
