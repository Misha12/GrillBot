using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Duck;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Services.Duck
{
    public class DuckDataLoader : IDisposable
    {
        private HttpClient HttpClient { get; }
        private ILogger<DuckDataLoader> Logger { get; }
        private JsonSerializer JsonSerializer { get; }

        public DuckDataLoader(IHttpClientFactory httpClientFactory, ILogger<DuckDataLoader> logger)
        {
            HttpClient = httpClientFactory.CreateClient();
            Logger = logger;
            JsonSerializer = new JsonSerializer();
        }

        public async Task<CurrentState> GetDuckCurrentState(DuckConfig duckConfig)
        {
            try
            {
                HttpClient.BaseAddress = new Uri(duckConfig.IsKachnaOpenApiBase);

                var response = await HttpClient.GetAsync("api/duck/currentState");
                await ValidateResponseAsync(response, duckConfig.IsKachnaOpenApiBase);

                var jsonStream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(jsonStream);
                using var jsonReader = new JsonTextReader(streamReader);

                return JsonSerializer.Deserialize<CurrentState>(jsonReader);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Request na IsKachnaOpen skončil špatně (nepodařilo se navázat spojení nebo jiná výjimka.) ");
                throw new WebException("Nepodařilo se zjistit stav Kachny. Zkus " + duckConfig.IsKachnaOpenApiBase);
            }
        }

        private async Task ValidateResponseAsync(HttpResponseMessage response, string urlBase)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Logger.LogWarning($"Request na IsKachnaOpen skončil špatně (HTTP {(int)response.StatusCode}).\n{content}");
                throw new WebException("Nepodařilo se zjistit stav Kachny. Zkus " + urlBase);
            }
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
