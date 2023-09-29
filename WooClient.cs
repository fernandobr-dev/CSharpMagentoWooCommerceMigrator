using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSharpMagentoWooCommerceMigrator
{
    class WooClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;


        public WooClient()
        {
            var wooConfig = ConfigurationManager.GetSection("WooConfig") as NameValueCollection;

            _baseApiUrl = wooConfig["WooCommerceBaseUrl"];
            _consumerKey = wooConfig["WooCommerceConsumerKey"];
            _consumerSecret = wooConfig["WooCommerceConsumerSecret"];

            _httpClient = new HttpClient();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                string testEndpoint = $"{_baseApiUrl}/wp-json/wc/v3/products";

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_consumerKey}:{_consumerSecret}"));

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);



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
