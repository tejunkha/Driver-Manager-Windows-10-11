using System;
using System.Drawing;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public partial class DriverStoreDetailsForm : Form
    {
        private readonly DriverStoreModel _driver;

        public DriverStoreDetailsForm(DriverStoreModel driver)
        {
            _driver = driver ??
                throw new ArgumentNullException(nameof(driver));

            InitializeComponent();
            LoadDetails();
        }

        private void LoadDetails()
        {
            txtPublished.Text = Safe(_driver.PublishedName);
            txtOriginal.Text = Safe(_driver.OriginalName);
            txtProvider.Text = Safe(_driver.ProviderName);
            txtClass.Text = Safe(_driver.ClassName);
            txtVersion.Text = Safe(_driver.Version);
            txtDate.Text = Safe(_driver.Date);
            txtSigner.Text = Safe(_driver.SignerName);
            txtClassGuid.Text = Safe(_driver.ClassGuid);
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
