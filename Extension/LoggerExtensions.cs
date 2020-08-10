using Microsoft.Extensions.Logging;
using System;

namespace BitCoin.API.Extension
{
    public static class LoggerExtensions
    {
        public static void IncludeTimeStamp(this ILogger logger, string value)
        {
            logger.Log(LogLevel.Information, 1, value, null, (s, e) => DateTime.Now + " " + s.ToString());
        }
    }
}
