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


        int totalProducts = 0;

        foreach (AttributeSet attributeSet in attributeSets)
        {
            Console.Clear();
            Console.WriteLine($"Attribute Set: {attributeSet.attribute_set_name}");

            int totalCount = await magentoClient.GetTotalCountProductsInAttrSet(attributeSet, 1, 1);

            List<ProductMage> productsMageRoot = new List<ProductMage>();

            if (totalCount > 0)
            {
                int rounds = totalCount / 100;
                int rest = totalCount % 100;

                for (int i = 1; i <= rounds; i++)
                {
                    productsMageRoot.AddRange(await magentoClient.GetProductsByAttrSet(attributeSet, 100, i));
                }

                if(rest > 0) { 
                    productsMageRoot.AddRange(await magentoClient.GetProductsByAttrSet(attributeSet, rest, rounds));
                }

                foreach (ProductMage product in productsMageRoot)
                {
                    Console.WriteLine($"Product: {product.name} | SKU: {product.sku}");
                    totalProducts++;
                }

                await Task.Delay(7000);

            }

            

        }

        Console.WriteLine($"Number of Products To Migrate: ==== {totalProducts}");





    }
}