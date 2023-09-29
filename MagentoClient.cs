using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace CSharpMagentoWooCommerceMigrator
{
    class MagentoClient 
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;
        private readonly string _accessToken;

        public MagentoClient()
        {
            var mageConfig = ConfigurationManager.GetSection("MageConfig") as NameValueCollection;

            _baseApiUrl = mageConfig["MagentoBaseUrl"];
            _accessToken = mageConfig["MagentoAccessToken"];

            _httpClient = new HttpClient();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                string testEndpoint = $"{_baseApiUrl}/rest/V1/store/storeConfigs";

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                HttpResponseMessage response = await _httpClient.GetAsync(testEndpoint);

                return response.IsSuccessStatusCode;
            } 
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }




    }
}
