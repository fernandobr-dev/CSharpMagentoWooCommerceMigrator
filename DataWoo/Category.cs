using Newtonsoft.Json;

namespace CSharpMagentoWooCommerceMigrator.DataWoo;

public class Category
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? old_id_mage {  get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? id { get; set; }

    public string name { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? slug { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? parent {  get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? description {  get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? display {  get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<Image>? image { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? menu_order {  get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? count { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Links? _links {  get; set; }
}

public class Image
{
    public int id { get; set; }
    public DateTime date_created {  get; set; }
    public DateTime date_created_gmt {  get; set; }
    public DateTime date_modified { get; set; }
    public DateTime date_modified_gmt { get; set; }
    public string src {  get; set; }
    public string name { get; set; }
    public string alt {  get; set; }
}

public class Links
{
    public List<LinkItem> self { get; set; }
    public List<LinkItem> collection { get; set; }
}

public class LinkItem
{
    public string href { get; set; }
}


