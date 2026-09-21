namespace Driver_Manager_Windows_10_11.Models
{
    public class DriverStoreModel
    {
        public string PublishedName { get; set; } = "";
        public string OriginalName { get; set; } = "";
        public string ProviderName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string Version { get; set; } = "";
        public string Date { get; set; } = "";
        public string SignerName { get; set; } = "";

        // ข้อมูลเพิ่มเติม
        public string ClassGuid { get; set; } = "";
        public string Attributes { get; set; } = "";
    }
}