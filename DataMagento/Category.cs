namespace CSharpMagentoWooCommerceMigrator.DataMagento;

public class Category
{
    public int id {  get; set; }
    public int parent_id { get; set; }
    public string name { get; set; }
    public bool is_active {  get; set; }
    public int position {  get; set; }
    public int level {  get; set; }
    public string children {  get; set; }
    public DateTime created_at {  get; set; }
    public DateTime updated_at {  get; set; }
    public string path {  get; set; }
    public List<string> available_sort_by { get; set; }
    public List<CustomAttribute> custom_attributes { get; set; }
}

public class CustomAttribute
{
    public string attribute_code { get; set; }
    public string value { get; set; }
}
