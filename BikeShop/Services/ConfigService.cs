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
    }
}
