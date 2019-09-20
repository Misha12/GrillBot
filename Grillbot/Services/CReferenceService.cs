using System;
using Grillbot.Extensions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Grillbot.Exceptions;

namespace Grillbot.Services
{
    public class CReferenceService
    {
        private static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler(){AutomaticDecompression = DecompressionMethods.GZip});
        private const string BaseUrl = "https://en.cppreference.com/";
        private const string SearchUrl = BaseUrl + "w/cpp/index.php?title=Special:Search&search=";

        public CReferenceService()
        {
            HttpClient.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            HttpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip");
            HttpClient.DefaultRequestHeaders.Add("accept-language", "cs-CZ,cs;q=0.9,en;q=0.8");
            HttpClient.DefaultRequestHeaders.Add("cache-control", "max-age=0");
            HttpClient.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
            HttpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/75.0.3770.90 Chrome/75.0.3770.90 Safari/537.36");
        }
        
        public static async Task<string> GetReferenceUrl(string search)
        {
            string getRequest = SearchUrl + search;

            try
            {
                using (var response = await HttpClient.GetAsync(getRequest).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Request failed");
                    }

                    string data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    //....
                    string topicUrl = data.GetMiddle("<a href=\"/w/c/", "</a>");
                    
                    if (topicUrl == string.Empty)
                        throw new NotFoundException("Topic not found");
                    
                    topicUrl = topicUrl.Substring(0, topicUrl.IndexOf('"'));

                    return BaseUrl + "w/c/" + topicUrl;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}

