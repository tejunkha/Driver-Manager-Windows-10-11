using System;
using System.Drawing;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public partial class DevicePrinterDetailsForm : Form
    {
        private readonly PrintServerPrinterModel _printer;

        public DevicePrinterDetailsForm(
            PrintServerPrinterModel printer)
        {
            _printer = printer
                ?? throw new ArgumentNullException(nameof(printer));

            InitializeComponent();
            LoadDetails();
        }

        private void LoadDetails()
        {
            txtPrinterName.Text = Safe(_printer.PrinterName);
            txtCategory.Text = Safe(_printer.Category);
            txtManufacturer.Text = Safe(_printer.Manufacturer);
            txtDriver.Text = Safe(_printer.DriverName);
            txtPort.Text = Safe(_printer.PortName);
            txtPortMonitor.Text = Safe(_printer.PortMonitor);
            txtStatus.Text = Safe(_printer.Status);
            txtState.Text = Safe(_printer.State);

            txtPresent.Text =
                _printer.Present ? "Yes" : "No";

            txtServer.Text = Safe(_printer.ServerName);
            txtShareName.Text = Safe(_printer.ShareName);
            txtDeviceId.Text = Safe(_printer.DeviceId);
            txtClassGuid.Text = Safe(_printer.ClassGuid);
            txtSource.Text = Safe(_printer.Source);
            txtJobs.Text = Safe(_printer.JobCount);

            txtWorkOffline.Text =
                _printer.WorkOffline ? "Yes" : "No";

            txtPublished.Text =
                _printer.Published ? "Yes" : "No";

            txtShared.Text =
                _printer.Shared ? "Yes" : "No";

            txtReason.Text = Safe(_printer.Reason);

            ApplyStatusColor();
        }

        private void ApplyStatusColor()
        {
            if (_printer.State.Equals(
                    "Not Present",
                    StringComparison.OrdinalIgnoreCase))
            {
                txtState.ForeColor =
                    Color.FromArgb(220, 38, 38);

                txtPresent.ForeColor =
                    Color.FromArgb(220, 38, 38);

                return;
            }

            if (_printer.State.Equals(
                    "Offline",
                    StringComparison.OrdinalIgnoreCase))
            {
                txtState.ForeColor =
                    Color.FromArgb(217, 119, 6);

                return;
            }

            if (_printer.Present)
            {
                txtState.ForeColor =
                    Color.FromArgb(22, 163, 74);

                txtPresent.ForeColor =
                    Color.FromArgb(22, 163, 74);
            }
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value.Trim();
        }

        private void btnClose_Click(
            object? sender,
            EventArgs e)
        {
            Close();
        }
    }
}
