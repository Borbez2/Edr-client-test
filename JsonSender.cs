using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Edr_client_test
{
    /// <summary>
    /// Sends JSON-formatted logs to a configured HTTP endpoint.
    /// </summary>
    public class JsonSender
    {
        private readonly string _url;

        public JsonSender(string url)
        {
            _url = url;
        }

        public async Task SendAsync(string json)
        {
            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync(_url, content);
        }
    }
}
