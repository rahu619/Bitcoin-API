using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Serialization;
using BitCoin.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

// MVC controllers currently do not support Native AOT trimming. This is a known
// framework limitation. See: https://aka.ms/aspnet/trimming
#pragma warning disable IL2026
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, BitcoinApiJsonSerializerContext.Default));
#pragma warning restore IL2026
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder.Services
    .AddOptions<ExternalAPISettings>()
    .Bind(builder.Configuration.GetSection("ExternalAPISettings"))
    .Validate(
        settings => settings.Interval > 0,
        "The polling interval must be greater than zero.")
    .Validate(
        settings => settings.Count > 0,
        "The number of results to retrieve must be greater than zero.")
    .Validate(
        settings => settings.Url is not null,
        "The URL configuration must be provided.")
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
    .AddStandardResilienceHandler();
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
