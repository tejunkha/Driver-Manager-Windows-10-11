namespace Driver_Manager_Windows_10_11.Models
{
    public class InstalledPrinterModel
    {
        public string Name { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string Vendor { get; set; } = "";
        public string PrinterType { get; set; } = "";
        public string PortName { get; set; } = "";
        public string HostAddress { get; set; } = "";
        public string Status { get; set; } = "";
        public string Default { get; set; } = "No";
        public string Network { get; set; } = "No";
        public string Shared { get; set; } = "No";
        public string WorkOffline { get; set; } = "No";
        public string ServerName { get; set; } = "";
        public string Location { get; set; } = "";
        public string Comment { get; set; } = "";
    }
}
