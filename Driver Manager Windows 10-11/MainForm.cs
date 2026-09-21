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
        // Ghost Printers
        // =========================================================
        private List<GhostPrinterModel> allGhostPrinters =
            new List<GhostPrinterModel>();

        // =========================================================
        // Printer Ports
        // =========================================================
        private List<PrinterPortModel> allPrinterPorts =
            new List<PrinterPortModel>();

        // =========================================================
        // Installed Printers (IPP / Vendor)
        // =========================================================
        private List<InstalledPrinterModel> allInstalledPrinters =
            new List<InstalledPrinterModel>();

        // =========================================================
        // Current Page
        // =========================================================
        private string currentPage = "";

        private bool isLoading = false;

        // =========================================================
        // Constructor
        // =========================================================
        public MainForm()
        {
            InitializeComponent();

            HideGhostPrinterStatus();

            // เปิดรายละเอียดเมื่อดับเบิลคลิกแถว
            dgvDrivers.CellDoubleClick += dgvDrivers_CellDoubleClick;

            // Printer Ports
            btnPrinterPorts.Click += btnPrinterPorts_Click;

            // Installed Printers (IPP / Vendor)
            btnPrinters.Click += btnPrinters_Click;

            // =====================================================
            // Startup: ยังไม่เลือกเมนูใด
            // =====================================================
            currentPage = "";

            lblPageTitle.Text = "Driver Manager";
            lblPageDescription.Text =
                "Select a driver category to begin";

            dgvDrivers.Rows.Clear();

            txtSearch.Clear();
            txtSearch.PlaceholderText =
                "Select a driver category...";

            txtSearch.Enabled = false;
            btnRefresh.Enabled = false;

            ClearActiveButtons();
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

            txtSearch.Enabled = true;
            btnRefresh.Enabled = true;

            HideGhostPrinterStatus();

            SetActiveButton(btnSystemDrivers);

            lblPageTitle.Text =
                "System Drivers";

            lblPageDescription.Text =
                "View installed system device drivers";

            txtSearch.Clear();
            txtSearch.PlaceholderText = "Search driver...";

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

            txtSearch.Enabled = true;
            btnRefresh.Enabled = true;

            HideGhostPrinterStatus();

            SetActiveButton(btnDriverStore);

            lblPageTitle.Text =
                "Driver Store";

            lblPageDescription.Text =
                "View driver packages stored in Windows";

            txtSearch.Clear();
            txtSearch.PlaceholderText = "Search driver package...";

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

            txtSearch.Enabled = true;
            btnRefresh.Enabled = true;

            HideGhostPrinterStatus();

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
        // PRINTER PORTS
        // =========================================================
        private async void btnPrinterPorts_Click(
            object sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            currentPage = "Printer Ports";

            txtSearch.Enabled = true;
            btnRefresh.Enabled = true;

            HideGhostPrinterStatus();

            SetActiveButton(btnPrinterPorts);

            lblPageTitle.Text =
                "Printer Ports";

            lblPageDescription.Text =
                "View installed printer ports";

            txtSearch.Clear();
            txtSearch.PlaceholderText =
                "Search printer port...";

            ConfigurePrinterPortGrid();

            await LoadPrinterPortsAsync();
        }

        // =========================================================
        // INSTALLED PRINTERS (IPP / VENDOR)
        // =========================================================
        private async void btnPrinters_Click(
            object sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            currentPage = "Printers";

            txtSearch.Enabled = true;
            btnRefresh.Enabled = true;

            HideGhostPrinterStatus();

            SetActiveButton(btnPrinters);

            lblPageTitle.Text =
                "Printers";

            lblPageDescription.Text =
                "View installed printers, IPP and vendor information";

            txtSearch.Clear();
            txtSearch.PlaceholderText =
                "Search printer...";

            ConfigureInstalledPrinterGrid();

            await LoadInstalledPrintersAsync();
        }

        // =========================================================
        // GHOST PRINTERS
        // =========================================================
        private async void btnGhostPrinter_Click(
            object sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            currentPage = "Ghost Printer";

            txtSearch.Enabled = true;
            btnRefresh.Enabled = true;

            HideGhostPrinterStatus();

            SetActiveButton(btnGhostPrinter);

            lblPageTitle.Text =
                "Ghost Printer";

            lblPageDescription.Text =
                "View non-present printer devices";

            txtSearch.Clear();
            txtSearch.PlaceholderText =
                "Search ghost printer...";

            ConfigureGhostPrinterGrid();

            await LoadGhostPrintersAsync();
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
        // PRINTER PORT GRID
        // =========================================================
        private void ConfigurePrinterPortGrid()
        {
            dgvDrivers.Columns.Clear();

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colName",
                    "Port Name",
                    150));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colDescription",
                    "Description",
                    150));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colHost",
                    "Host Address",
                    130));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colPortNumber",
                    "Port Number",
                    80));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colProtocol",
                    "Protocol",
                    80));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colSNMP",
                    "SNMP",
                    70));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colMonitor",
                    "Port Monitor",
                    120));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colComputer",
                    "Computer",
                    100));
        }

        // =========================================================
        // INSTALLED PRINTER GRID
        // =========================================================
        private void ConfigureInstalledPrinterGrid()
        {
            dgvDrivers.Columns.Clear();

            dgvDrivers.Columns.Add(CreateColumn("colName", "Printer Name", 170));
            dgvDrivers.Columns.Add(CreateColumn("colPrinterType", "Type", 95));
            dgvDrivers.Columns.Add(CreateColumn("colVendor", "Vendor", 110));
            dgvDrivers.Columns.Add(CreateColumn("colDriverName", "Driver", 170));
            dgvDrivers.Columns.Add(CreateColumn("colPortName", "Port", 150));
            dgvDrivers.Columns.Add(CreateColumn("colHostAddress", "Host Address", 120));
            dgvDrivers.Columns.Add(CreateColumn("colStatus", "Status", 90));

            dgvDrivers.Columns["colName"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            dgvDrivers.Columns["colDriverName"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            dgvDrivers.Columns["colPortName"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            dgvDrivers.Columns["colVendor"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            dgvDrivers.Columns["colPrinterType"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            dgvDrivers.Columns["colHostAddress"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            dgvDrivers.Columns["colStatus"]!.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
        }

        // =========================================================
        // GHOST PRINTER GRID
        // =========================================================
        private void ConfigureGhostPrinterGrid()
        {
            dgvDrivers.Columns.Clear();

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colName",
                    "Printer Name",
                    150));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colDeviceId",
                    "Device ID",
                    200));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colManufacturer",
                    "Manufacturer",
                    100));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colClassGuid",
                    "Class",
                    90));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colStatus",
                    "Status",
                    90));

            dgvDrivers.Columns.Add(
                CreateColumn(
                    "colProblem",
                    "Problem Code",
                    80));
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
        // LOAD PRINTER PORTS
        // =========================================================
        private async Task LoadPrinterPortsAsync()
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
                    "Loading printer ports...";

                allPrinterPorts =
                    await Task.Run(() =>
                        PrinterPortService.GetPorts());

                dgvDrivers.Rows.Clear();

                foreach (PrinterPortModel port
                    in allPrinterPorts)
                {
                    int row =
                        dgvDrivers.Rows.Add(
                            port.Name,
                            port.Description,
                            port.PrinterHostAddress,
                            port.PortNumber,
                            port.Protocol,
                            port.SNMPEnabled,
                            port.PortMonitor,
                            port.ComputerName);

                    dgvDrivers.Rows[row].Tag = port;
                }

                lblStatus.Text =
                    $"Printer Ports    {allPrinterPorts.Count} ports";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Driver Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Failed to load printer ports";
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
        // LOAD INSTALLED PRINTERS
        // =========================================================
        private async Task LoadInstalledPrintersAsync()
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
                    "Loading installed printers...";

                allInstalledPrinters =
                    await Task.Run(() =>
                        InstalledPrinterService.GetPrinters());

                dgvDrivers.Rows.Clear();

                foreach (InstalledPrinterModel printer
                    in allInstalledPrinters)
                {
                    int row =
                        dgvDrivers.Rows.Add(
                            printer.Name,
                            printer.PrinterType,
                            printer.Vendor,
                            printer.DriverName,
                            printer.PortName,
                            printer.HostAddress,
                            printer.Status);

                    dgvDrivers.Rows[row].Tag = printer;

                    string type = printer.PrinterType;
                    if (type.Equals("IPP", StringComparison.OrdinalIgnoreCase) ||
                        type.Equals("IPP (Microsoft)", StringComparison.OrdinalIgnoreCase))
                    {
                        dgvDrivers.Rows[row].Cells["colPrinterType"]
                            .Style.ForeColor = Color.FromArgb(37, 99, 235);
                    }
                    else if (type.Equals("Vendor", StringComparison.OrdinalIgnoreCase))
                    {
                        dgvDrivers.Rows[row].Cells["colPrinterType"]
                            .Style.ForeColor = Color.FromArgb(124, 58, 237);
                    }
                    else if (type.Equals("Offline", StringComparison.OrdinalIgnoreCase))
                    {
                        dgvDrivers.Rows[row].Cells["colStatus"]
                            .Style.ForeColor = Color.FromArgb(220, 38, 38);
                    }
                }

                lblStatus.Text =
                    $"Printers    {allInstalledPrinters.Count} printers";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Driver Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Failed to load installed printers";
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
        // LOAD GHOST PRINTERS
        // =========================================================
        private async Task LoadGhostPrintersAsync()
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
                    "Loading ghost printers...";

                allGhostPrinters =
                    await Task.Run(() =>
                        GhostPrinterService.GetGhostPrinters());

                dgvDrivers.Rows.Clear();

                foreach (GhostPrinterModel printer
                    in allGhostPrinters)
                {
                    int row =
                        dgvDrivers.Rows.Add(
                            printer.Name,
                            printer.DeviceId,
                            printer.Manufacturer,
                            printer.ClassGuid,
                            printer.Status,
                            printer.ProblemCode);

                    dgvDrivers.Rows[row].Tag = printer;

                    dgvDrivers.Rows[row]
                        .Cells["colStatus"]
                        .Style.ForeColor =
                        Color.FromArgb(220, 38, 38);
                }

                if (allGhostPrinters.Count == 0)
                {
                    ShowGhostPrinterStatus(
                        "No Ghost Printer Found",
                        "No disconnected printer devices detected.");
                }
                else
                {
                    HideGhostPrinterStatus();
                }

                lblStatus.Text =
                    allGhostPrinters.Count == 0
                        ? "Ghost Printer    No disconnected printers"
                        : $"Ghost Printer    {allGhostPrinters.Count} items";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Driver Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text =
                    "Failed to load ghost printers";
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
            else if (currentPage ==
                     "Printer Ports")
            {
                await LoadPrinterPortsAsync();
            }
            else if (currentPage ==
                     "Printers")
            {
                await LoadInstalledPrintersAsync();
            }
            else if (currentPage ==
                     "Ghost Printer")
            {
                await LoadGhostPrintersAsync();
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

                // =====================================================
                // PRINTER PORTS
                // =====================================================
                else if (currentPage ==
                         "Printer Ports")
                {
                    PrinterPortModel? port =
                        row.Tag as PrinterPortModel;

                    if (port != null)
                    {
                        string searchText =
                            $"{port.Name} " +
                            $"{port.Description} " +
                            $"{port.PrinterHostAddress} " +
                            $"{port.PortNumber} " +
                            $"{port.Protocol} " +
                            $"{port.SNMPEnabled} " +
                            $"{port.PortMonitor} " +
                            $"{port.ComputerName}";

                        found =
                            string.IsNullOrWhiteSpace(keyword) ||
                            searchText.Contains(
                                keyword,
                                StringComparison.OrdinalIgnoreCase);
                    }
                }

                // =====================================================
                // INSTALLED PRINTERS
                // =====================================================
                else if (currentPage ==
                         "Printers")
                {
                    InstalledPrinterModel? printer =
                        row.Tag as InstalledPrinterModel;

                    if (printer != null)
                    {
                        string searchText =
                            $"{printer.Name} " +
                            $"{printer.PrinterType} " +
                            $"{printer.Vendor} " +
                            $"{printer.DriverName} " +
                            $"{printer.PortName} " +
                            $"{printer.HostAddress} " +
                            $"{printer.Status} " +
                            $"{printer.Default} " +
                            $"{printer.Network} " +
                            $"{printer.Shared} " +
                            $"{printer.WorkOffline} " +
                            $"{printer.ServerName} " +
                            $"{printer.Location} " +
                            $"{printer.Comment}";

                        found =
                            string.IsNullOrWhiteSpace(keyword) ||
                            searchText.Contains(
                                keyword,
                                StringComparison.OrdinalIgnoreCase);
                    }
                }

                // =====================================================
                // GHOST PRINTER
                // =====================================================
                else if (currentPage ==
                         "Ghost Printer")
                {
                    GhostPrinterModel? printer =
                        row.Tag as GhostPrinterModel;

                    if (printer != null)
                    {
                        string searchText =
                            $"{printer.Name} " +
                            $"{printer.DeviceId} " +
                            $"{printer.Manufacturer} " +
                            $"{printer.ClassGuid} " +
                            $"{printer.Status} " +
                            $"{printer.ProblemCode} " +
                            $"{printer.Description}";

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
        // GHOST PRINTER STATUS
        // =========================================================
        private void ShowGhostPrinterStatus(
            string title,
            string description)
        {
            if (currentPage != "Ghost Printer")
                return;

            lblGhostStatusTitle.Text = title;
            lblGhostStatusDescription.Text = description;

            pnlGhostStatus.Visible = true;
            pnlGhostStatus.BringToFront();
        }

        private void HideGhostPrinterStatus()
        {
            if (pnlGhostStatus == null)
                return;

            pnlGhostStatus.Visible = false;
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

            if (e.RowIndex >= dgvDrivers.Rows.Count)
                return;

            object? tag = dgvDrivers.Rows[e.RowIndex].Tag;

            // =====================================================
            // SYSTEM DRIVERS
            // =====================================================
            if (currentPage == "System Drivers" &&
                tag is SystemDriverModel systemDriver)
            {
                using (var detailsForm =
                    new SystemDriverDetailsForm(systemDriver))
                {
                    detailsForm.ShowDialog(this);
                }

                return;
            }

            // =====================================================
            // DRIVER STORE
            // =====================================================
            if (currentPage == "Driver Store" &&
                tag is DriverStoreModel storeDriver)
            {
                using (var detailsForm =
                    new DriverStoreDetailsForm(storeDriver))
                {
                    detailsForm.ShowDialog(this);
                }

                return;
            }

            // =====================================================
            // PRINTER DRIVERS
            // =====================================================
            if (currentPage == "Printer Drivers" &&
                tag is PrinterDriverModel printerDriver)
            {
                using (var detailsForm =
                    new PrinterDriverDetailsForm(printerDriver))
                {
                    detailsForm.ShowDialog(this);
                }

                return;
            }

            // =====================================================
            // INSTALLED PRINTERS
            // =====================================================
            if (currentPage == "Printers" &&
                tag is InstalledPrinterModel installedPrinter)
            {
                using (var detailsForm =
                    new InstalledPrinterDetailsForm(installedPrinter))
                {
                    detailsForm.ShowDialog(this);
                }

                return;
            }

            // =====================================================
            // GHOST PRINTER
            // =====================================================
            if (currentPage == "Ghost Printer" &&
                tag is GhostPrinterModel ghostPrinter)
            {
                using (var detailsForm =
                    new GhostPrinterDetailsForm(ghostPrinter))
                {
                    detailsForm.ShowDialog(this);
                }
            }

            // =====================================================
            // PRINTER PORTS
            // =====================================================
            if (currentPage == "Printer Ports" &&
                tag is PrinterPortModel printerPort)
            {
                using (var detailsForm =
                    new PrinterPortDetailsForm(printerPort))
                {
                    detailsForm.ShowDialog(this);
                }

                return;
            }
        }

        // =========================================================
        // CLEAR ACTIVE BUTTON
        // =========================================================
        private void ClearActiveButtons()
        {
            Button[] buttons =
            {
                btnSystemDrivers,
                btnDriverStore,
                btnPrinterDrivers,
                btnPrinterPorts,
                btnPrinters,
                btnGhostPrinter
            };

            foreach (Button button in buttons)
            {
                button.BackColor =
                    Color.FromArgb(30, 41, 59);

                button.ForeColor =
                    Color.FromArgb(226, 232, 240);
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
                btnPrinterDrivers,
                btnPrinterPorts,
                btnPrinters,
                btnGhostPrinter
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

    }
}