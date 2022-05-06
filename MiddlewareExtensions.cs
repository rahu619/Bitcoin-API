using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BitCoin.API
{
    public static class MiddlewareExtensions
    {
        public static void AddInterceptedSingleton<TInterface, TImplementation, TInterceptor>(this IServiceCollection services)
                                                                                              where TInterface : class
                                                                                              where TImplementation : class, TInterface
                                                                                              where TInterceptor : class, IInterceptor
        {

            services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
            services.AddSingleton<TImplementation>();
            services.TryAddSingleton<TInterceptor>();
            services.AddSingleton(provider =>
            {
                var proxygenerator = provider.GetRequiredService<IProxyGenerator>();
                var implementation = provider.GetRequiredService<TImplementation>();
                var interceptor = provider.GetRequiredService<TInterceptor>();

                return proxygenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
            });

        }
    }
}
