namespace Driver_Manager_Windows_10_11.Models
{
    public class PrintServerPrinterModel
    {
        public string ServerName { get; set; } = "";
        public string PrinterName { get; set; } = "";
        public string Category { get; set; } = "";
        public string ShareName { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string PortName { get; set; } = "";
        public string PortMonitor { get; set; } = "";
        public string Status { get; set; } = "";
        public string PrinterState { get; set; } = "";
        public string State { get; set; } = "";
        public string Reason { get; set; } = "";
        public string JobCount { get; set; } = "0";
        public string Jobs { get; set; } = "";
        public string Source { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string ClassGuid { get; set; } = "";
        public bool Present { get; set; } = true;
        public bool Shared { get; set; }
        public bool Published { get; set; }
        public bool WorkOffline { get; set; }
    }
}
