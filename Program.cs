using System;
using System.Collections.Specialized;
using System.Configuration;
using CSharpMagentoWooCommerceMigrator.DataMagento;
using Newtonsoft.Json;

namespace CSharpMagentoWooCommerceMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        MagentoClient magentoClient = new MagentoClient();

        bool isValidMageApiConn = await magentoClient.TestConnectionAsync();

        if (isValidMageApiConn)
        {
            Console.WriteLine("Connection with Magento API is OK!");
        }
        else
        {
            Console.WriteLine("Problem To Connect With Magento API, check your URL and Access Token.");
            throw new Exception("Problem To Connect With Magento API, check your URL and Access Token.");
        }

        WooClient wooClient = new WooClient();

        bool isValidWooApiConn = await wooClient.TestConnectionAsync();

        if (isValidWooApiConn)
        {
            Console.WriteLine("Connection with WooCommerce API is OK!");
        }
        else
        {
            Console.WriteLine("Problem To Connect With WooCommerce API, check your URL, Consumer Key and Consumer Secret.");
            throw new Exception("Problem To Connect With WooCommerce API, check your URL, Consumer Key and Consumer Secret.");
        }

        List<DataMagento.Category> categoriesList = await magentoClient.GetCategories();


        Dictionary<int, int> categoriesIdsMageToWoo = new Dictionary<int, int>();

        // creating categories.
        foreach (DataMagento.Category category in categoriesList)
        {
            string json = wooClient.PrepareWooCategoryJSONFromMagento(category);


            Console.WriteLine($"Sending Category: {category.name}");
            int? newCategoryId = await wooClient.SendCategoryToWooCommerce(json);

            if (newCategoryId is not null)
            {
                Console.WriteLine(newCategoryId);
                categoriesIdsMageToWoo[category.id] = (int)newCategoryId;
                Console.WriteLine("Success");
            }

        }

        foreach (DataMagento.Category category in categoriesList)
        {
            int parentId = category.parent_id;

            if (parentId != 0) { 

                parentId = categoriesIdsMageToWoo[category.parent_id];

                int categoryId = categoriesIdsMageToWoo[category.id];

                var jsonObject = new
                {
                    parent = parentId
                };

                string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

                bool resultUpdateParent = await wooClient.UpdateParentCategoryWooCommerce(jsonString, categoryId);

                if (resultUpdateParent)
                {
                    Console.WriteLine($"{category.name} added to Parent Category ID: {parentId}");
                }
                else
                {
                    Console.WriteLine($"ERROR: {category.name} not added to Parent Category ID: {parentId}");
                }

            }
        }

        List<AttributeSet> attributeSets = await magentoClient.GetAttributeSetsAsync();

        foreach (AttributeSet attributeSet in attributeSets)
        {
            Console.Clear();
            Console.WriteLine($"Attribute Set: {attributeSet.attribute_set_name}");

            List<ProductMage> productsMage = await magentoClient.GetProductsByAttrSet(attributeSet);

            // check if the attribute_set has products.
            if (productsMage.Count() > 0)
            {

                foreach (ProductMage productMage in productsMage)
                {
                    foreach (var category in productMage.extension_attributes.category_links)
                    {
                        int catId = Int32.Parse(category.category_id);

                        category.category_id = categoriesIdsMageToWoo[catId].ToString();

                    }

                    if (productMage.type_id == "simple")
                    { //only export to Woocommerce simple products. 
                        string json = wooClient.PrepareWooProductJSONFromMagento(productMage);

                        // send product to Woo. 
                        bool productSentSuccessfully = await wooClient.SendProductToWooCommerce(json);

                        if (productSentSuccessfully)
                        {
                            Console.WriteLine($"Product: {productMage.name} sent successfully.");
                            Task.Delay(1000);
                        }
                        else
                        {
                            Console.WriteLine($"Error sending product '{productMage.name}'. The operation will be terminated.");
                        }
                    }
                }

            }
            
        }






    }
}