namespace BitCoin.API.Configuration
{
    /// <summary>
    /// The external API configuration entity.
    /// </summary>
    public class ExternalAPISettings
    {
        /// <summary>
        /// The interval for invoking the BitCoin API.
        /// </summary>
        /// <remarks>
        ///  This value is configured in seconds.
        /// </remarks>
        public int Interval { get; set; }

        /// <summary>
        /// The number of results to take from the resultset
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The API Urls
        /// </summary>
        public Url Url { get; set; }
    }

    public class Url
    {
        /// <summary>
        /// The base url
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// The url to get historical bitcoin values.
        /// </summary>
        public string Historical
        {
            get => string.Concat(Base, _historical);
            set => _historical = value;
        }

        private string _historical;
    }
}
