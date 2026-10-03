using System;
using System.Drawing;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public partial class SystemDriverDetailsForm : Form
    {
        private readonly SystemDriverModel _driver;

        public SystemDriverDetailsForm(SystemDriverModel driver)
        {
            _driver = driver ??
                throw new ArgumentNullException(nameof(driver));

            InitializeComponent();
            LoadDetails();
        }

        private void LoadDetails()
        {
            txtDevice.Text = Safe(_driver.Device);
            txtManufacturer.Text = Safe(_driver.Manufacturer);
            txtVersion.Text = Safe(_driver.DriverVersion);
            txtDate.Text = Safe(_driver.DriverDate);
            txtInf.Text = Safe(_driver.InfName);
            txtStatus.Text = Safe(_driver.Status);
            txtSigned.Text = Safe(_driver.Signed);
            txtClass.Text = Safe(_driver.DeviceClass);
            txtDriverName.Text = Safe(_driver.DriverName);
            txtDeviceId.Text = Safe(_driver.DeviceId);
            txtLocation.Text = Safe(_driver.Location);

            if (_driver.Status.Equals(
                    "OK",
                    StringComparison.OrdinalIgnoreCase))
            {
                txtStatus.ForeColor =
                    Color.FromArgb(22, 163, 74);
            }
            else
            {
                txtStatus.ForeColor =
                    Color.FromArgb(220, 38, 38);
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
