using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BitCoin.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
                    {
                        options.AddDefaultPolicy(
                            builder =>
                            {
                                builder.WithOrigins("http://localhost:3000");
                            });
                    });

            services.AddControllers();
            services.AddMemoryCache();

            services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
            services.AddSingleton(typeof(IHttpClientService<>), typeof(HttpClientService<>));

            //ConfigureAuthentication(services);
            ConfigureBackgroundServices(services);
            SetupConfigurations(services);

            services.AddLogging(opt =>
            {
                opt.AddConsole(c =>
                {
                    c.TimestampFormat = "[HH:mm:ss] ";
                });
            });
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = Configuration["Jwt:Issuer"],
                            ValidAudience = Configuration["Jwt:Issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                        };
                    });
        }

        private void ConfigureBackgroundServices(IServiceCollection services)
        {
            services.AddHostedService<BitCoinApiService>();
        }

        private void SetupConfigurations(IServiceCollection services)
        {
            services.Configure<ExternalAPISettings>(options => Configuration.GetSection("ExternalAPISettings").Bind(options));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Allowing request from bitcoin.ui

            app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseAuthentication();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //for fetching the data in background
            //app.UseDNTScheduler();
        }
    }
}
