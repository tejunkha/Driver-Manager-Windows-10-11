using System;
using System.Drawing;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public partial class PrinterDriverDetailsForm : Form
    {
        private readonly PrinterDriverModel _driver;

        public PrinterDriverDetailsForm(PrinterDriverModel driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));

            InitializeComponent();
            LoadDriverDetails();
        }

        private void LoadDriverDetails()
        {
            txtDriverName.Text = Safe(_driver.Name);
            txtProvider.Text = Safe(_driver.Provider);
            txtType.Text = Safe(_driver.Version);
            txtInf.Text = Safe(_driver.InfName);
            txtArchitecture.Text = Safe(_driver.SupportedPlatform);
            txtStatus.Text = string.IsNullOrWhiteSpace(_driver.Status)
                ? "Installed"
                : _driver.Status;

            txtDriverPath.Text = Safe(_driver.DriverPath);
            txtFilePath.Text = Safe(_driver.FilePath);
            txtDataFile.Text = Safe(_driver.DataFile);
            txtConfigFile.Text = Safe(_driver.ConfigFile);
            txtMonitor.Text = Safe(_driver.MonitorName);

            if (txtStatus.Text.Equals(
                    "Installed",
                    StringComparison.OrdinalIgnoreCase) ||
                txtStatus.Text.Equals(
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
