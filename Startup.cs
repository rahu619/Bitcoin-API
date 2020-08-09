using BitCoin.API.Configuration;
using BitCoin.API.Interfaces;
using BitCoin.API.Services;
using DNTScheduler.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            services.AddSingleton<ICacheProvider, CacheProvider>();
            services.AddSingleton(typeof(IRestService<>), typeof(RestService<>));

            ConfigureAuthentication(services);
            ConfigureBackgroundServices(services);
            SetupConfigurations(services);
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

            services.AddDNTScheduler(options =>
            {
                // DNTScheduler needs a ping service to keep it alive. Set it to false if you don't need it. Its default value is true.
                options.AddPingTask = false;
                options.AddScheduledTask<BitCoinApiService>(
                    runAt: utcNow =>
                    {
                        return utcNow.Minute % 5 == 0;
                    });

            });
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
            app.UseDNTScheduler();
        }
    }
}
