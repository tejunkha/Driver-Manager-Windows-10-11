namespace Driver_Manager_Windows_10_11.Models
{
    /// <summary>
    /// Represents a printer queue/device that needs attention.
    /// This is intentionally broader than a disconnected PnP device:
    /// stale queues and duplicate/copy queues are also supported.
    /// </summary>
    public class GhostPrinterModel
    {
        public string Name { get; set; } = "";

        public string DriverName { get; set; } = "";

        public string PortName { get; set; } = "";

        public string PrinterType { get; set; } = "";

        public string DeviceName { get; set; } = "";

        public string DeviceId { get; set; } = "";

        public string Manufacturer { get; set; } = "";

        public string State { get; set; } = "";

        public string Reason { get; set; } = "";

        /// <summary>
        /// PNP = remove PnP device with pnputil.
        /// Queue = remove installed printer queue with Remove-Printer.
        /// </summary>
        public string ActionType { get; set; } = "Queue";

        // Backward-compatible fields used by the existing details/search UI.
        public string ClassGuid { get; set; } = "";

        public string Status { get; set; } = "";

        public uint ProblemCode { get; set; }

        public string Description { get; set; } = "";

        public bool IsDefault { get; set; }

        public bool IsNetwork { get; set; }

        public bool WorkOffline { get; set; }
    }
}
