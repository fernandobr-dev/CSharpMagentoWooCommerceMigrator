namespace CSharpMagentoWooCommerceMigrator.DataWoo;

public class ProductWoo
{
    public string name {  get; set; }
    public string sku { get; set; }
    public string weight {  get; set; }
    public string type {  get; set; }
    public string price {  get; set; }
    public string regular_price {  get; set; }
    public string description {  get; set; }
    public string short_description {  get; set; }
    public List<Images> images { get; set; }
    public string slug {  get; set; }
    public List<CatProduct> categories {  get; set; }
}

public class CatProduct
{
    public int id { get; set; }
}



