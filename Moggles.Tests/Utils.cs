using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Moggles.Tests
{
    public static class Utils
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string url, T data)
        {
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject((object)data));
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpClient.PostAsync(url, (HttpContent)stringContent);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            string str = await content.ReadAsStringAsync();
            string dataAsString = str;
            str = (string)null;
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }

        public static void ClearStorage()
        {
            var path = @".\nodb_storage";
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
