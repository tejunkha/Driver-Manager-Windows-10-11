using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using Driver_Manager_Windows_10_11.Models;
using Driver_Manager_Windows_10_11.Services;

namespace Driver_Manager_Windows_10_11
{
    public partial class MainForm : Form
    {
        // =========================================================
        // System Drivers
        // =========================================================
        private List<SystemDriverModel> allDrivers =
            new List<SystemDriverModel>();

        // =========================================================
        // Driver Store
        // =========================================================
        private List<DriverStoreModel> allDriverStore =
            new List<DriverStoreModel>();

        // =========================================================
        // Printer Drivers
        // =========================================================
        private List<PrinterDriverModel> allPrinterDrivers =
            new List<PrinterDriverModel>();

        // =========================================================
        // Current Page
        // =========================================================
        private string currentPage = "System Drivers";

        private bool isLoading = false;

        // =========================================================
        // Constructor
        // =========================================================
        public MainForm()
        {
            InitializeComponent();

            SetActiveButton(btnSystemDrivers);

            // เปิดรายละเอียด Printer Driver เมื่อดับเบิลคลิกแถว
            dgvDrivers.CellDoubleClick += dgvDrivers_CellDoubleClick;

            Shown += async (sender, e) =>
            {
                ConfigureSystemGrid();

                await LoadSystemDriversAsync();
            };
        }

        // =========================================================
        // SYSTEM DRIVERS
        // =========================================================
        private async void btnSystemDrivers_Click(
            object sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            currentPage = "System Drivers";

            SetActiveButton(btnSystemDrivers);

            lblPageTitle.Text =
                "System Drivers";

            lblPageDescription.Text =
                "View installed system device drivers";

            txtSearch.Clear();

            ConfigureSystemGrid();

            await LoadSystemDriversAsync();
        }

        // =========================================================
        // DRIVER STORE
        // =========================================================
        private async void btnDriverStore_Click(
            object sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            currentPage = "Driver Store";

            SetActiveButton(btnDriverStore);

            lblPageTitle.Text =
                "Driver Store";

            lblPageDescription.Text =
                "View driver packages stored in Windows";

            txtSearch.Clear();

            ConfigureDriverStoreGrid();

            await LoadDriverStoreAsync();
        }

        // =========================================================
        // PRINTER DRIVERS
        // =========================================================
        private async void btnPrinterDrivers_Click(
            object sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            currentPage = "Printer Drivers";

            SetActiveButton(btnPrinterDrivers);

            lblPageTitle.Text =
                "Printer Drivers";

            lblPageDescription.Text =
                "View installed printer drivers";

            txtSearch.Clear();

            ConfigurePrinterGrid();

            await LoadPrinterDriversAsync();
        }

        // =========================================================
        // SYSTEM GRID
        // =========================================================
        private void ConfigureSystemGrid()
        {
            dgvDrivers.Columns.Clear();

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colDevice",
                    "Device",
                    180));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colManufacturer",
                    "Manufacturer",
                    100));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colVersion",
                    "Driver Version",
                    90));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colDate",
                    "Driver Date",
                    80));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colInf",
                    "INF Name",
                    80));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colStatus",
                    "Status",
                    70));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colSigned",
                    "Signed",
                    60));
        }

        // =========================================================
        // DRIVER STORE GRID
        // =========================================================
        private void ConfigureDriverStoreGrid()
        {
            dgvDrivers.Columns.Clear();

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colPublished",
                    "Published Name",
                    90));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colOriginal",
                    "Original Name",
                    120));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colProvider",
                    "Provider",
                    100));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colClass",
                    "Class",
                    80));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colVersion",
                    "Driver Version",
                    100));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colDate",
                    "Date",
                    80));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colSigner",
                    "Signer",
                    150));
        }

        // =========================================================
        // PRINTER DRIVER GRID
        // =========================================================
        private void ConfigurePrinterGrid()
        {
            dgvDrivers.Columns.Clear();

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colName",
                    "Driver Name",
                    180));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colManufacturer",
                    "Provider",
                    90));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colVersion",
                    "Type",
                    65));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colInf",
                    "INF",
                    100));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colPath",
                    "Driver Path",
                    190));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colEnvironment",
                    "Architecture",
                    90));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colStatus",
                    "Status",
                    80));

            // =========================================================
            // Alignment
            // =========================================================
            dgvDrivers.Columns["colName"]!
                .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvDrivers.Columns["colManufacturer"]!
                .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvDrivers.Columns["colVersion"]!
                .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvDrivers.Columns["colInf"]!
                .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvDrivers.Columns["colPath"]!
                .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvDrivers.Columns["colEnvironment"]!
                .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvDrivers.Columns["colStatus"]!
                .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
        }

        // =========================================================
        // Create Column
        // =========================================================
        private DataGridViewTextBoxColumn CreateColumn(
            string name,
            string header,
            float fillWeight)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                ReadOnly = true,
                FillWeight = fillWeight,
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            };
        }

        // =========================================================
        // LOAD SYSTEM DRIVERS
        // =========================================================
        private async Task LoadSystemDriversAsync()
        {
            if (isLoading)
                return;

            isLoading = true;

            try
            {
                Cursor = Cursors.WaitCursor;

                btnRefresh.Enabled = false;
                txtSearch.Enabled = false;

                lblStatus.Text =
                    "Loading system drivers...";

                allDrivers =
                    await Task.Run(() =>
                        SystemDriverService.GetDrivers());

                dgvDrivers.Rows.Clear();

                foreach (SystemDriverModel driver
                    in allDrivers)
                {
                    int row =
                        dgvDrivers.Rows.Add(
                            driver.Device,
                            driver.Manufacturer,
                            driver.DriverVersion,
                            driver.DriverDate,
                            driver.InfName,
                            driver.Status,
                            driver.Signed);

                    dgvDrivers.Rows[row].Tag =
                        driver;
                }

                lblStatus.Text =
                    $"System Drivers    {allDrivers.Count} items";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Driver Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Failed to load system drivers";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
            }
        }

        // =========================================================
        // LOAD DRIVER STORE
        // =========================================================
        private async Task LoadDriverStoreAsync()
        {
            if (isLoading)
                return;

            isLoading = true;

            try
            {
                Cursor = Cursors.WaitCursor;

                btnRefresh.Enabled = false;
                txtSearch.Enabled = false;

                lblStatus.Text =
                    "Loading Driver Store...";

                allDriverStore =
                    await Task.Run(() =>
                        DriverStoreService.GetDrivers());

                dgvDrivers.Rows.Clear();

                foreach (DriverStoreModel driver
                    in allDriverStore)
                {
                    int row =
                        dgvDrivers.Rows.Add(
                            driver.PublishedName,
                            driver.OriginalName,
                            driver.ProviderName,
                            driver.ClassName,
                            driver.Version,
                            driver.Date,
                            driver.SignerName);

                    dgvDrivers.Rows[row].Tag =
                        driver;
                }

                lblStatus.Text =
                    $"Driver Store    {allDriverStore.Count} packages";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Driver Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Failed to load Driver Store";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
            }
        }

        // =========================================================
        // LOAD PRINTER DRIVERS
        // =========================================================
        private async Task LoadPrinterDriversAsync()
        {
            if (isLoading)
                return;

            isLoading = true;

            try
            {
                Cursor = Cursors.WaitCursor;

                btnRefresh.Enabled = false;
                txtSearch.Enabled = false;

                lblStatus.Text =
                    "Loading printer drivers...";

                allPrinterDrivers =
                    await Task.Run(() =>
                        PrinterDriverService.GetDrivers());

                dgvDrivers.Rows.Clear();

                foreach (PrinterDriverModel driver
                    in allPrinterDrivers)
                {
                    int row =
                        dgvDrivers.Rows.Add(
                            driver.Name,
                            driver.Provider,
                            driver.Version,
                            driver.InfName,
                            driver.DriverPath,
                            driver.SupportedPlatform,
                            driver.Status);

                    dgvDrivers.Rows[row].Tag =
                        driver;
                }

                // =====================================================
                // จัดสี Status
                // =====================================================
                foreach (DataGridViewRow row
                    in dgvDrivers.Rows)
                {
                    string status =
                        row.Cells["colStatus"]
                            .Value?.ToString() ?? "";

                    if (status.Equals(
                        "Installed",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["colStatus"]
                            .Style.ForeColor =
                            Color.FromArgb(22, 163, 74);
                    }
                    else
                    {
                        row.Cells["colStatus"]
                            .Style.ForeColor =
                            Color.FromArgb(220, 38, 38);
                    }
                }

                lblStatus.Text =
                    $"Printer Drivers    {allPrinterDrivers.Count} drivers";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Driver Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Failed to load printer drivers";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
            }
        }

        // =========================================================
        // REFRESH
        // =========================================================
        private async void btnRefresh_Click(
            object sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            if (currentPage ==
                "System Drivers")
            {
                await LoadSystemDriversAsync();
            }
            else if (currentPage ==
                     "Driver Store")
            {
                await LoadDriverStoreAsync();
            }
            else if (currentPage ==
                     "Printer Drivers")
            {
                await LoadPrinterDriversAsync();
            }
        }

        // =========================================================
        // SEARCH
        // =========================================================
        private void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            string keyword =
                txtSearch.Text.Trim();

            int visibleCount = 0;
            int totalCount = dgvDrivers.Rows.Count;

            foreach (DataGridViewRow row
                in dgvDrivers.Rows)
            {
                if (row.IsNewRow)
                    continue;

                bool found = false;

                // =====================================================
                // SYSTEM DRIVER
                // =====================================================
                if (currentPage ==
                    "System Drivers")
                {
                    SystemDriverModel? driver =
                        row.Tag as SystemDriverModel;

                    if (driver != null)
                    {
                        string searchText =
                            $"{driver.Device} " +
                            $"{driver.Manufacturer} " +
                            $"{driver.DriverVersion} " +
                            $"{driver.DriverDate} " +
                            $"{driver.InfName} " +
                            $"{driver.Status} " +
                            $"{driver.Signed} " +
                            $"{driver.DeviceId} " +
                            $"{driver.DriverName} " +
                            $"{driver.DeviceClass} " +
                            $"{driver.Location}";

                        found =
                            string.IsNullOrWhiteSpace(keyword) ||
                            searchText.Contains(
                                keyword,
                                StringComparison.OrdinalIgnoreCase);
                    }
                }

                // =====================================================
                // DRIVER STORE
                // =====================================================
                else if (currentPage ==
                         "Driver Store")
                {
                    DriverStoreModel? driver =
                        row.Tag as DriverStoreModel;

                    if (driver != null)
                    {
                        string searchText =
                            $"{driver.PublishedName} " +
                            $"{driver.OriginalName} " +
                            $"{driver.ProviderName} " +
                            $"{driver.ClassName} " +
                            $"{driver.Version} " +
                            $"{driver.Date} " +
                            $"{driver.SignerName} " +
                            $"{driver.ClassGuid}";

                        found =
                            string.IsNullOrWhiteSpace(keyword) ||
                            searchText.Contains(
                                keyword,
                                StringComparison.OrdinalIgnoreCase);
                    }
                }

                // =====================================================
                // PRINTER DRIVER
                // =====================================================
                else if (currentPage ==
                         "Printer Drivers")
                {
                    PrinterDriverModel? driver =
                        row.Tag as PrinterDriverModel;

                    if (driver != null)
                    {
                        string searchText =
                            $"{driver.Name} " +
                            $"{driver.Provider} " +
                            $"{driver.Version} " +
                            $"{driver.InfName} " +
                            $"{driver.DriverPath} " +
                            $"{driver.FilePath} " +
                            $"{driver.DataFile} " +
                            $"{driver.ConfigFile} " +
                            $"{driver.MonitorName} " +
                            $"{driver.SupportedPlatform} " +
                            $"{driver.Status}";

                        found =
                            string.IsNullOrWhiteSpace(keyword) ||
                            searchText.Contains(
                                keyword,
                                StringComparison.OrdinalIgnoreCase);
                    }
                }

                row.Visible = found;

                if (found)
                    visibleCount++;
            }

            lblStatus.Text =
                $"{currentPage}    {visibleCount} / {totalCount}";
        }

        // =========================================================
        // PRINTER DRIVER DETAILS
        // =========================================================
        private void dgvDrivers_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            // ไม่ทำงานเมื่อดับเบิลคลิก Header
            if (e.RowIndex < 0)
                return;

            // ทำงานเฉพาะหน้า Printer Drivers
            if (currentPage != "Printer Drivers")
                return;

            if (e.RowIndex >= dgvDrivers.Rows.Count)
                return;

            if (dgvDrivers.Rows[e.RowIndex].Tag
                is not PrinterDriverModel driver)
            {
                return;
            }

            using (var detailsForm =
                new PrinterDriverDetailsForm(driver))
            {
                detailsForm.ShowDialog(this);
            }
        }

        // =========================================================
        // ACTIVE BUTTON
        // =========================================================
        private void SetActiveButton(
            Button activeButton)
        {
            Button[] buttons =
            {
                btnSystemDrivers,
                btnDriverStore,
                btnPrinterDrivers
            };

            foreach (Button button in buttons)
            {
                button.BackColor =
                    Color.FromArgb(30, 41, 59);

                button.ForeColor =
                    Color.FromArgb(226, 232, 240);
            }

            activeButton.BackColor =
                Color.FromArgb(37, 99, 235);

            activeButton.ForeColor =
                Color.White;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}