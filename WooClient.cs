using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using CSharpMagentoWooCommerceMigrator.DataMagento;
using CSharpMagentoWooCommerceMigrator.DataWoo;
using Newtonsoft.Json;
using System.Web;
using System;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;

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
                weight = magentoProduct.weight != null ? magentoProduct.weight.ToString() : "0",
                type = magentoProduct.type_id,
                regular_price = magentoProduct.price.ToString(),
                price = magentoProduct.price.ToString(),
                images = magentoProduct.media_gallery_entries.Select(entry => new Images { src = _mageBaseApiUrl + "/media/catalog/product" + entry.file }).ToList(),
                description = magentoProduct.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "description")?.value?.ToString(),
                short_description = magentoProduct.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "short_description")?.value?.ToString(),
                slug = magentoProduct.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "url_path")?.value?.ToString()
                
            };

            if (magentoProduct.extension_attributes.category_links is not null)
            {
                wooProduct.categories = magentoProduct.extension_attributes.category_links
                    .Select(categoryLink =>
                    {
                        int categoryId = Int32.Parse(categoryLink.category_id);
                        return new CatProduct { id = categoryId };
                    })
                    .ToList();
            }


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

        public string PrepareWooCategoryJSONFromMagento(DataMagento.Category magentoCategory)
        {
            DataWoo.Category wooCategory = new DataWoo.Category();

            wooCategory.name = magentoCategory.name;

            if (magentoCategory.custom_attributes != null) {

                var descriptionAttribute = magentoCategory.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "description");

                wooCategory.description = descriptionAttribute != null ? descriptionAttribute.value.ToString() : null;

                var slugAttribute = magentoCategory.custom_attributes.FirstOrDefault(attr => attr.attribute_code == "url_key");

                wooCategory.slug = slugAttribute != null ? slugAttribute.value.ToString() : null;

            }

            string json = JsonConvert.SerializeObject(wooCategory, Formatting.Indented);
            
            return json;
        }
    
        public async Task<int?> SendCategoryToWooCommerce(string categoryJson)
        {
            try
            {
                string sendCategoryEndpoint = $"{_baseApiUrl}/wp-json/wc/v3/products/categories";

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_consumerKey}:{_consumerSecret}"));

                var content = new StringContent(categoryJson, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                HttpResponseMessage response = await _httpClient.PostAsync(sendCategoryEndpoint, content);

                string responseBody = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(responseBody);
                    // the new ID of category in Woo
                    int id = (int)jsonObject["id"];

                    return id;

                }
                else
                {

                    Console.WriteLine($"Error: {responseBody}, JSON: {categoryJson}, ");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }

        public async Task<bool> UpdateParentCategoryWooCommerce(string parentCategoryJson, int categoryId)
        {
            try
            {
                string updateCategoryEndpoint = $"{_baseApiUrl}/wp-json/wc/v3/products/categories/{categoryId}";

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_consumerKey}:{_consumerSecret}"));

                var content = new StringContent(parentCategoryJson, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                HttpResponseMessage response = await _httpClient.PutAsync(updateCategoryEndpoint, content);

                string responseBody = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error: {responseBody}, JSON: {parentCategoryJson}");
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
