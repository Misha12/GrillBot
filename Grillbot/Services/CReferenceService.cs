using Grillbot.Extensions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Grillbot.Exceptions;

namespace Grillbot.Services
{
    public class CReferenceService
    {
        private HttpClient Client { get; }

#pragma warning disable S1075 // URIs should not be hardcoded
        private const string BaseUrl = "https://en.cppreference.com/";
#pragma warning restore S1075 // URIs should not be hardcoded
        private const string SearchUrl = BaseUrl + "w/cpp/index.php?title=Special:Search&search=";

        public CReferenceService()
        {
            Client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
        }

        public async Task<string> GetReferenceUrlAsync(string search)
        {
            string getUrl = SearchUrl + search;

            using (var response = await Client.GetAsync(getUrl).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var errorMessage = $"StatusCode: {response.StatusCode}, {responseContent}";
                    throw new WebException($"Request on {getUrl} failed. {errorMessage}");
                }

                string data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                string topicUrl = data.GetMiddle("<a href=\"/w/c/", "</a>");

                if (topicUrl == string.Empty)
                    throw new NotFoundException("Topic not found");

                topicUrl = topicUrl.Substring(0, topicUrl.IndexOf('"'));
                return BaseUrl + "w/c/" + topicUrl;
            }
        }
    }
}

