using BitCoin.API.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitCoin.API.Services
{
    /// <summary>
    /// A wrapper service for Http client
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpClientService<T> : IHttpClientService<T>
    {
        private readonly HttpClient _client;

        public HttpClientService()
        {
            this._client = new HttpClient();
        }

        /// <summary>
        /// Returns content from the url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<T> GetContent(string url)
        {
            var result = await this._client.GetStreamAsync(url);
            return await JsonSerializer.DeserializeAsync<T>(result);
        }

        /// <summary>
        /// Safely disposes the http client
        /// </summary>
        public void Dispose()
        {
            this._client.Dispose();
        }
    }
}
