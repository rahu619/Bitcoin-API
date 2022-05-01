using BitCoin.API.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitCoin.API.Services
{
    /// <summary>
    /// Http client service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpClientService<T> : IHttpClientService<T>
    {
        private readonly HttpClient _client;

        public HttpClientService()
        {
            this._client = new HttpClient();
        }

        public async Task<T> GetContent(string url)
        {
            var result = await this._client.GetStreamAsync(url);
            return await JsonSerializer.DeserializeAsync<T>(result);
        }

        public void Dispose()
        {
            this._client.Dispose();
        }
    }
}
