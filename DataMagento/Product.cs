namespace CSharpMagentoWooCommerceMigrator.DataMagento
{
    class ProductMage
    {
        public int id { get; set; }
        public string sku {  get; set; }
        public string type_id {  get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public bool status {  get; set; }
        public string weight {  get; set; }
        public ExtensionAttributes extension_attributes {  get; set; }
        public List<MediaGalleryEntries> media_gallery_entries {  get; set; }
        public List<CustomAttributes> custom_attributes { get; set; }

    }
}
