namespace BitCoin.API.Configuration
{
    public class ExternalAPISettings
    {
        /// <summary>
        /// The interval for invoking the external api
        /// </summary>
        /// <remarks>
        ///  This value is in seconds at the moment
        /// </remarks>
        public int Interval { get; set; }

        /// <summary>
        /// The API Urls
        /// </summary>
        public Url Url { get; set; }
    }

    public class Url
    {
        /// <summary>
        /// Gets the historical values.
        /// </summary>
        public string Historical { get; set; }
    }
}
