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

