using System.ComponentModel.DataAnnotations;

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
        [Range(1, int.MaxValue, ErrorMessage = "The polling interval must be greater than zero.")]
        public int Interval { get; set; }

        /// <summary>
        /// The number of results to take from the resultset
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "The number of results to retrieve must be greater than zero.")]
        public int Count { get; set; }

        /// <summary>
        /// The API Urls
        /// </summary>
        [Required]
        public Url? Url { get; set; }
    }

    public class Url
    {
        /// <summary>
        /// The base url
        /// </summary>
        [Required]
        [Url]
        public string? Base { get; set; }

        /// <summary>
        /// The url to get historical bitcoin values.
        /// </summary>
        [Required]
        public string? Historical
        {
            get => _historical is null ? null : string.Concat(Base, _historical);
            set => _historical = value;
        }

        private string? _historical;
    }
}
