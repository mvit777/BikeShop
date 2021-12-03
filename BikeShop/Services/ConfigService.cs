using BikeDistributor.Infrastructure.core;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BikeShop.Services
{
    public class ConfigService : IConfigService
    {
        private HttpClient _client;
        private string _jsonFilePath;
        private string _rawJson;
        public ConfigService(string jsonFilePath, HttpClient client)
        {
            _client = client;
            _jsonFilePath = jsonFilePath;
            
        }

        public async Task LoadAsync()
        {
            using var response = await _client.GetAsync(_jsonFilePath);
            _rawJson = await response.Content.ReadAsStringAsync();
        }

        public string GetSetting(string settingName)
        {
            JToken o = JObject.Parse(_rawJson);

            return o["Settings"][0][settingName].ToString();
        }

        public string GetUrl(string urlName)
        {
            string[] u = urlName.Split(new char[] { '.' });
            JToken o = JObject.Parse(_rawJson)["BikeShopWS"]["urls"];
            JToken url = o.SelectToken(u[0]).Value<string>(u[1]);
            //var url = o["BikeShopWS"]["urls"][0][u[0]][urlName].ToString();
            
            return url.ToString();
        }
    }
}
