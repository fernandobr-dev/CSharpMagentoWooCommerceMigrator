using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using CSharpMagentoWooCommerceMigrator.DataMagento;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public async Task<List<AttributeSet>> GetAttributeSetsAsync()
        {
            try
            {
                string attributeSetsEndpoint = $"{_baseApiUrl}/rest/V1/products/attribute-sets/sets/list?searchCriteria=0";

                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                HttpResponseMessage response = await _httpClient.GetAsync(attributeSetsEndpoint);

                if (response.IsSuccessStatusCode) 
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    JObject jsonObject = JObject.Parse(responseBody);
                    JArray items = (JArray)jsonObject["items"];

                    List<AttributeSet> attributeSets = items.ToObject<List<AttributeSet>>();

                    return attributeSets;
                }
                else
                {
                    Console.WriteLine("No attribute-set found");

                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }

        public async Task<List<ProductMage>> GetProductsByAttrSet(AttributeSet attributeSet)
        {
            int maxRetries = 3;  
            int currentRetry = 0;

            List<ProductMage> productsData = null;

            while (currentRetry < maxRetries)
            {
                try
                {
                    string productsByAttributeSetEndpoint = $"{_baseApiUrl}/rest/V1/products?" +
                        $"searchCriteria[filterGroups][0][filters][0][field]=attribute_set_id&" +
                        $"searchCriteria[filterGroups][0][filters][0][value]={attributeSet.attribute_set_id}&" +
                        $"searchCriteria[filterGroups][0][filters][0][conditionType]=eq";

                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                    HttpResponseMessage response = await _httpClient.GetAsync(productsByAttributeSetEndpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JObject jsonObject = JObject.Parse(responseBody);
                        JArray items = (JArray)jsonObject["items"];
                        productsData = items.ToObject<List<ProductMage>>();
                        break;  
                    }

                    Console.WriteLine($"No Produts Found in Attribute Set: {attributeSet.attribute_set_name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {currentRetry + 1} failed: {ex.Message}");
                }

                currentRetry++;
                await Task.Delay(1000);  
            }

            if (productsData == null)
            {
                Console.WriteLine("Failed to retrieve products after multiple attempts.");
            }

            // Continue com os dados obtidos (ou nulos, se todas as tentativas falharem)
            return productsData;
        }

        public async Task<List<Category>> GetCategories()
        {
            try
            {
                string categoriesEndpoint = $"{_baseApiUrl}/rest/V1/categories/list?searchCriteria=0";

                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                HttpResponseMessage response = await _httpClient.GetAsync(categoriesEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    JObject jsonObject = JObject.Parse(responseBody);
                    JArray items = (JArray)jsonObject["items"];

                    List<Category> categoriesList = items.ToObject<List<Category>>();


                    return categoriesList;
                }

                Console.WriteLine("No Categories Found");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }
        }
    }
}
