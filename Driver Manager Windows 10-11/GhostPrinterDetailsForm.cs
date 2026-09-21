using System;
using System.Drawing;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public partial class GhostPrinterDetailsForm : Form
    {
        private readonly GhostPrinterModel _printer;

        public GhostPrinterDetailsForm(GhostPrinterModel printer)
        {
            _printer = printer ??
                throw new ArgumentNullException(nameof(printer));

            InitializeComponent();
            LoadDetails();
        }

        private void LoadDetails()
        {
            txtPrinterName.Text = Safe(_printer.Name);
            txtDeviceId.Text = Safe(_printer.DeviceId);
            txtManufacturer.Text = Safe(_printer.Manufacturer);
            txtClass.Text = Safe(_printer.ClassGuid);
            txtStatus.Text = Safe(_printer.Status);
            txtProblemCode.Text = _printer.ProblemCode.ToString();
            txtDescription.Text = Safe(_printer.Description);

            if (_printer.Status.Equals(
                    "Disconnected",
                    StringComparison.OrdinalIgnoreCase) ||
                _printer.Status.Equals(
                    "Non-present",
                    StringComparison.OrdinalIgnoreCase))
            {
                txtStatus.ForeColor =
                    Color.FromArgb(220, 38, 38);
            }
            else
            {
                txtStatus.ForeColor =
                    Color.FromArgb(100, 116, 139);
            }
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value;
        }

        private void btnClose_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }
    }
}
