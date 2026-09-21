namespace Driver_Manager_Windows_10_11.Models
{
    public class GhostPrinterModel
    {
        public string Name { get; set; } = "";

        public string DeviceId { get; set; } = "";

        public string Manufacturer { get; set; } = "";

        // เก็บชื่อ Device Class เช่น Printer / PrintQueue
        // ชื่อ property เดิมคงไว้เพื่อไม่ต้องรื้อ MainForm
        public string ClassGuid { get; set; } = "";

        public string Status { get; set; } = "";

        public uint ProblemCode { get; set; }

        public string Description { get; set; } = "";
    }
}
