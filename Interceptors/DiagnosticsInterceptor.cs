using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BitCoin.API.Interceptors
{
    /// <summary>
    /// Intercepts the API request. Nick Chapsas example
    /// </summary>
    /// <remarks>
    /// A middleware can be used instead of this approach. But just for the sake of trying it out 
    /// </remarks>
    public class DiagnosticsInterceptor : IInterceptor
    {
        private readonly ILogger _logger;
        public DiagnosticsInterceptor(ILogger<DiagnosticsInterceptor> logger)
        {
            _logger = logger;
        }
        public void Intercept(IInvocation invocation)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                invocation.Proceed();
            }
            finally
            {
                watch.Stop();
                _logger.LogInformation("{MethodName} took {Duration}ms", invocation.Method.Name, watch.ElapsedMilliseconds);
            }
        }
    }
}
