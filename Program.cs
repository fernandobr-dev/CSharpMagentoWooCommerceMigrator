using System;
using System.Collections.Specialized;
using System.Configuration;
using CSharpMagentoWooCommerceMigrator.DataMagento;

namespace CSharpMagentoWooCommerceMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        MagentoClient magentoClient = new MagentoClient();

        bool isValidMageApiConn = await magentoClient.TestConnectionAsync();

        if(isValidMageApiConn)
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

        List<AttributeSet> attributeSets = await magentoClient.GetAttributeSetsAsync();

        foreach (AttributeSet attributeSet in attributeSets)
        {
            Console.Clear();
            Console.WriteLine($"Attribute Set: {attributeSet.attribute_set_name}");

            List<ProductMage> productsMage = await magentoClient.GetProductsByAttrSet(attributeSet);

            // check if the attribute_set has products.
            if (productsMage.Count() > 0) { 

                foreach (ProductMage productMage in productsMage)
                {
                    if (productMage.type_id == "simple") { //only export to Woocommerce simple products. 
                        string json = wooClient.PrepareWooProductJSONFromMagento(productMage);

                        // send product to Woo. 
                        bool productSentSuccessfully = await wooClient.SendProductToWooCommerce(json);

                        if (productSentSuccessfully)
                        {
                            Console.WriteLine($"Product: {productMage.name} sent successfully.");
                        }
                        else
                        {
                            Console.WriteLine(json);
                            throw new Exception($"Error sending product '{productMage.name}'. The operation will be terminated.");
                        }
                    }
                }

            }
            Task.Delay(1000);
        }










        }
}