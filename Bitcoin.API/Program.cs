using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Services;

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
    .ValidateOnStart();

builder.Services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
builder.Services.AddHttpClient(typeof(IHttpClientService<>), typeof(HttpClientService<>))
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
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
