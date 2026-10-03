using System;
using System.Drawing;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public partial class InstalledPrinterDetailsForm : Form
    {
        private readonly InstalledPrinterModel _printer;

        public InstalledPrinterDetailsForm(InstalledPrinterModel printer)
        {
            _printer = printer ?? throw new ArgumentNullException(nameof(printer));

            InitializeComponent();
            LoadPrinterDetails();
        }

        private void LoadPrinterDetails()
        {
            txtPrinterName.Text = Safe(_printer.Name);
            txtType.Text = Safe(_printer.PrinterType);
            txtVendor.Text = Safe(_printer.Vendor);
            txtDriver.Text = Safe(_printer.DriverName);
            txtPort.Text = Safe(_printer.PortName);
            txtHostAddress.Text = Safe(_printer.HostAddress);
            txtStatus.Text = Safe(_printer.Status);
            txtDefault.Text = Safe(_printer.Default);
            txtNetwork.Text = Safe(_printer.Network);
            txtShared.Text = Safe(_printer.Shared);
            txtWorkOffline.Text = Safe(_printer.WorkOffline);
            txtServer.Text = Safe(_printer.ServerName);
            txtLocation.Text = Safe(_printer.Location);
            txtComment.Text = Safe(_printer.Comment);

            if (IsPositiveStatus(_printer.Status))
            {
                txtStatus.ForeColor = Color.FromArgb(22, 163, 74);
            }
            else if (!string.IsNullOrWhiteSpace(_printer.Status))
            {
                txtStatus.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        private static bool IsPositiveStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Equals("Idle", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("Ready", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("OK", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("Installed", StringComparison.OrdinalIgnoreCase);
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
