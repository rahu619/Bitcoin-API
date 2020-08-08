using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BitCoin.API.Models
{
    public class BitCoinPriceIndexModel
    {
        /// <summary>
        /// Collection of historical data
        /// </summary>
        [JsonPropertyName("bpi")]
        public Dictionary<string, decimal> BitCoinPriceIndexHistory { get; set; }

        /// <summary>
        /// Disclaimer detail
        /// </summary>
        [JsonPropertyName("disclaimer")]
        public string Disclaimer { get; set; }

        /// <summary>
        /// Published time details
        /// </summary>
        [JsonPropertyName("time")]
        public BitCoinPriceIndexTimeModel BitCoinPriceIndexTime { get; set; }

    }

    public class BitCoinPriceIndexHistoryModel
    {
        /// <summary>
        /// The updated date
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// The US Dollar value 
        /// </summary>
        public decimal USD { get; set; }

    }

    public class BitCoinPriceIndexTimeModel
    {
        /// <summary>
        /// The published datetime
        /// </summary>
        [JsonPropertyName("updated")]
        public string Updated { get; set; }

        /// <summary>
        /// The publiched datetime in ISO string format.
        /// </summary>
        [JsonPropertyName("updatedISO")]
        public string UpdatedISO { get; set; }
    }
}
