using System;
using System.Collections.Specialized;
using System.Configuration;

namespace CSharpMagentoWooCommerceMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        // Test if Connection with Magento is OK. 
        MagentoClient magentoClient = new MagentoClient();

        bool isValidMageApiConn = await magentoClient.TestConnectionAsync();

        if(isValidMageApiConn)
        {
            Console.WriteLine("Connection with Magento API is OK!");
        } 
        else
        {
            Console.WriteLine("Problem To Connect With Magento API, check your URL and Access Token.");
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
        }
    }
}