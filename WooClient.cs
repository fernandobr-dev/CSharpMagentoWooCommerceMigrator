using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using CSharpMagentoWooCommerceMigrator.DataMagento;
using CSharpMagentoWooCommerceMigrator.DataWoo;
using Newtonsoft.Json;

namespace CSharpMagentoWooCommerceMigrator
{
    class WooClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;

        private readonly string _mageBaseApiUrl;

        public WooClient()
        {
            var wooConfig = ConfigurationManager.GetSection("WooConfig") as NameValueCollection;

            _baseApiUrl = wooConfig["WooCommerceBaseUrl"];
            _consumerKey = wooConfig["WooCommerceConsumerKey"];
            _consumerSecret = wooConfig["WooCommerceConsumerSecret"];

            var mageConfig = ConfigurationManager.GetSection("MageConfig") as NameValueCollection;

            _mageBaseApiUrl = mageConfig["MagentoBaseUrl"];

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
        
        public string PrepareWooProductJSONFromMagento(ProductMage magentoProduct)
        {
            ProductWoo wooProduct = new ProductWoo()
            {
                name = magentoProduct.name,
                sku = magentoProduct.sku,
                weight = magentoProduct.weight.ToString() != null ? magentoProduct.weight.ToString() : "0",
                type = magentoProduct.type_id,
                regular_price = magentoProduct.price.ToString(),
                price = magentoProduct.price.ToString(),
                images = magentoProduct.media_gallery_entries.Select(entry => new Images { src = _mageBaseApiUrl + "/media/catalog/product" + entry.file }).ToList(),
                description = magentoProduct.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "description")?.value?.ToString(),
                short_description = magentoProduct.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "short_description")?.value?.ToString(),
                slug = magentoProduct.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "url_path")?.value?.ToString()
        };

            string json = JsonConvert.SerializeObject(wooProduct);

            return json;
        }

        public async Task<bool> SendProductToWooCommerce(string productJson)
        {
            try
            {
                string sendProductEndpoint = $"{_baseApiUrl}/wp-json/wc/v3/products";

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_consumerKey}:{_consumerSecret}"));

                var content = new StringContent(productJson, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                HttpResponseMessage response = await _httpClient.PostAsync(sendProductEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }

    }
}
