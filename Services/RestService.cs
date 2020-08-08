using BitCoin.API.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitCoin.API.Services
{
    public class RestService<T> : IRestService<T>, IDisposable
    {
        private readonly HttpClient _client;

        public RestService()
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
