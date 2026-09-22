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
        // Utility UI
        // =========================================================
        private Button? btnAbout;
        private Label? lblAdminMode;

        // =========================================================
        // Constructor
        // =========================================================
        public MainForm()
        {
            InitializeComponent();

            // Action bar styling: medium Windows 10/11 icons
            UiStyle.ApplyActionBar(this);

            // Sidebar styling: medium icons + consistent spacing
            UiStyle.ApplySidebar(this);

            ErrorHandler.Install();
            InitializeUtilityUi();

            HideGhostPrinterStatus();

            // เปิดรายละเอียดเมื่อดับเบิลคลิกแถว
            dgvDrivers.CellDoubleClick += dgvDrivers_CellDoubleClick;

            // Printer Ports
            btnPrinterPorts.Click += btnPrinterPorts_Click;

            // Installed Printers (IPP / Vendor)
            btnPrinters.Click += btnPrinters_Click;

            // ACTION / Delete
            btnDelete.Click += btnDelete_Click;
            dgvDrivers.SelectionChanged += dgvDrivers_SelectionChanged;

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
            btnDelete.Enabled = false;

            ClearActiveButtons();
            UpdateDeleteButtonState();
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

            UpdateDeleteButtonState();

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

            UpdateDeleteButtonState();

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

            UpdateDeleteButtonState();

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

            UpdateDeleteButtonState();

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

            UpdateDeleteButtonState();

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

            UpdateDeleteButtonState();

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
                ErrorHandler.ShowError(ex, "Load System Drivers");

                lblStatus.Text =
                    "Failed to load system drivers";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
                UpdateDeleteButtonState();
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
                ErrorHandler.ShowError(ex, "Load Driver Store");

                lblStatus.Text =
                    "Failed to load Driver Store";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
                UpdateDeleteButtonState();
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
                ErrorHandler.ShowError(ex, "Load Printer Drivers");

                lblStatus.Text =
                    "Failed to load printer drivers";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
                UpdateDeleteButtonState();
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
                ErrorHandler.ShowError(ex, "Load Printer Ports");

                lblStatus.Text =
                    "Failed to load printer ports";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
                UpdateDeleteButtonState();
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
                ErrorHandler.ShowError(ex, "Load Printers");

                lblStatus.Text =
                    "Failed to load installed printers";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
                UpdateDeleteButtonState();
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
                ErrorHandler.ShowError(ex, "Load Ghost Printer");

                lblStatus.Text =
                    "Failed to load ghost printers";
            }
            finally
            {
                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;

                Cursor = Cursors.Default;

                isLoading = false;
                UpdateDeleteButtonState();
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
        // Utility UI
        // =========================================================
        private void InitializeUtilityUi()
        {
            lblAdminMode = new Label
            {
                AutoSize = false,
                Text = AdminHelper.GetModeText(),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(22, 76),
                Size = new Size(200, 20)
            };

            pnlSidebar.Controls.Add(lblAdminMode);
            lblAdminMode.BringToFront();

            btnAbout = new Button
            {
                Text = "ⓘ   About",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Height = 42,
                Dock = DockStyle.Bottom,
                Cursor = Cursors.Hand
            };

            btnAbout.FlatAppearance.BorderSize = 0;
            btnAbout.Click += btnAbout_Click;

            pnlSidebar.Controls.Add(btnAbout);
            btnAbout.BringToFront();

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(
                btnAbout,
                $"{AppInfo.Name} v{AppInfo.Version}");
        }

        private void btnAbout_Click(
            object? sender,
            EventArgs e)
        {
            using AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }

        // =========================================================
        // UI STYLE ACCESSORS
        // =========================================================
        internal Button GetRefreshButtonForStyle() => btnRefresh;

        internal Button GetDeleteButtonForStyle() => btnDelete;

        internal Button GetSystemDriversButtonForStyle() => btnSystemDrivers;

        internal Button GetDriverStoreButtonForStyle() => btnDriverStore;

        internal Button GetPrinterDriversButtonForStyle() => btnPrinterDrivers;

        internal Button GetPrinterPortsButtonForStyle() => btnPrinterPorts;

        internal Button GetPrintersButtonForStyle() => btnPrinters;

        internal Button GetGhostPrinterButtonForStyle() => btnGhostPrinter;

        // =========================================================
        // ACTION / DELETE
        // =========================================================
        private void dgvDrivers_SelectionChanged(
            object? sender,
            EventArgs e)
        {
            UpdateDeleteButtonState();
        }

        private void UpdateDeleteButtonState()
        {
            if (btnDelete == null || dgvDrivers == null)
                return;

            bool supportedPage =
                currentPage == "Driver Store" ||
                currentPage == "Printer Drivers" ||
                currentPage == "Printer Ports" ||
                currentPage == "Printers" ||
                currentPage == "Ghost Printer";

            bool hasSelection =
                dgvDrivers.SelectedRows.Count > 0 &&
                dgvDrivers.SelectedRows[0].Tag != null;

            btnDelete.Enabled = supportedPage && hasSelection && !isLoading;
        }

        private async void btnDelete_Click(
            object? sender,
            EventArgs e)
        {
            if (isLoading)
                return;

            if (dgvDrivers.SelectedRows.Count == 0)
                return;

            DataGridViewRow row = dgvDrivers.SelectedRows[0];
            object? tag = row.Tag;

            if (tag == null)
                return;

            if (currentPage == "System Drivers")
            {
                MessageBox.Show(
                    "ไม่อนุญาตให้ลบ System Driver เพื่อความปลอดภัยของระบบ",
                    "ลบไดรเวอร์",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!AdminHelper.RequireAdministrator(this, "Delete"))
                return;

            string itemName = GetDeleteItemName(tag);
            string warning = GetDeleteWarning(tag);

            DialogResult confirm = MessageBox.Show(
                warning + Environment.NewLine + Environment.NewLine +
                $"รายการ: {itemName}" + Environment.NewLine + Environment.NewLine +
                "การดำเนินการนี้ไม่สามารถย้อนกลับจาก Driver Manager ได้\nต้องการดำเนินการต่อหรือไม่?",
                "ยืนยันการลบ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes)
                return;

            bool success = false;
            string message = "";

            try
            {
                Cursor = Cursors.WaitCursor;
                btnDelete.Enabled = false;
                btnRefresh.Enabled = false;
                txtSearch.Enabled = false;
                lblStatus.Text = "กำลังลบ...";

                if (tag is DriverStoreModel driverStore)
                {
                    success = await Task.Run(() =>
                        DriverActionService.DeleteDriverStore(
                            driverStore,
                            out message));
                }
                else if (tag is PrinterDriverModel printerDriver)
                {
                    bool inUse = DriverActionService.IsPrinterDriverInUse(
                        printerDriver.Name,
                        out string printersUsingDriver);

                    if (inUse)
                    {
                        DialogResult secondConfirm = MessageBox.Show(
                            "Printer Driver นี้กำลังถูกใช้งานโดย:" +
                            Environment.NewLine + Environment.NewLine +
                            printersUsingDriver +
                            Environment.NewLine + Environment.NewLine +
                            "การลบ Driver อาจส่งผลต่อ Printer เหล่านี้\nต้องการดำเนินการต่อหรือไม่?",
                            "Printer Driver กำลังถูกใช้งาน",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2);

                        if (secondConfirm != DialogResult.Yes)
                        {
                            lblStatus.Text = "ยกเลิกการลบ";
                            return;
                        }
                    }

                    success = await Task.Run(() =>
                        DriverActionService.DeletePrinterDriver(
                            printerDriver,
                            out message));
                }
                else if (tag is PrinterPortModel printerPort)
                {
                    success = await Task.Run(() =>
                        DriverActionService.DeletePrinterPort(
                            printerPort,
                            out message));
                }
                else if (tag is InstalledPrinterModel installedPrinter)
                {
                    success = await Task.Run(() =>
                        DriverActionService.DeletePrinter(
                            installedPrinter,
                            out message));
                }
                else if (tag is GhostPrinterModel ghostPrinter)
                {
                    success = await Task.Run(() =>
                        DriverActionService.DeleteGhostPrinter(
                            ghostPrinter,
                            out message));
                }
                else
                {
                    success = false;
                    message = "รายการนี้ไม่สามารถลบได้";
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = "เกิดข้อผิดพลาดระหว่างการลบรายการ\n\n" + ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (success)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(message)
                        ? "ลบรายการเรียบร้อยแล้ว"
                        : message,
                    "ลบเสร็จสมบูรณ์",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await ReloadCurrentPageAsync();
            }
            else
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(message)
                        ? "Windows ไม่สามารถดำเนินการลบรายการนี้ได้"
                        : message,
                    "ลบไม่สำเร็จ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                btnRefresh.Enabled = true;
                txtSearch.Enabled = true;
                UpdateDeleteButtonState();
            }
        }

        private string GetDeleteItemName(object tag)
        {
            if (tag is DriverStoreModel driverStore)
                return driverStore.PublishedName;

            if (tag is PrinterDriverModel printerDriver)
                return printerDriver.Name;

            if (tag is PrinterPortModel printerPort)
                return printerPort.Name;

            if (tag is InstalledPrinterModel installedPrinter)
                return installedPrinter.Name;

            if (tag is GhostPrinterModel ghostPrinter)
                return string.IsNullOrWhiteSpace(ghostPrinter.Name)
                    ? ghostPrinter.DeviceId
                    : ghostPrinter.Name;

            return "รายการที่เลือก";
        }

        private string GetDeleteWarning(object tag)
        {
            if (tag is DriverStoreModel)
            {
                return "การดำเนินการนี้จะลบ Driver Package ออกจาก Windows Driver Store\n" +
                       "อุปกรณ์ที่ใช้ Driver Package นี้อาจได้รับผลกระทบ";
            }

            if (tag is PrinterDriverModel)
            {
                return "การดำเนินการนี้จะลบ Printer Driver\n" +
                       "Printer เครื่องอื่นที่ใช้ Driver นี้อาจได้รับผลกระทบ";
            }

            if (tag is PrinterPortModel)
            {
                return "การดำเนินการนี้จะลบ Printer Port\n" +
                       "Printer ที่ใช้ Port นี้อาจไม่สามารถพิมพ์งานได้";
            }

            if (tag is InstalledPrinterModel)
            {
                return "การดำเนินการนี้จะลบ Printer Queue ที่ติดตั้งอยู่\n" +
                       "จะไม่ลบ Printer Driver";
            }

            if (tag is GhostPrinterModel)
            {
                return "การดำเนินการนี้จะลบ Ghost Printer Device หรือ Printer Queue ที่ค้างอยู่\n" +
                       "จะไม่ลบ Driver Package ออกจาก Driver Store";
            }

            return "รายการที่เลือกจะถูกลบออกจากระบบ";
        }

        private async Task ReloadCurrentPageAsync()
        {
            if (currentPage == "Driver Store")
                await LoadDriverStoreAsync();
            else if (currentPage == "Printer Drivers")
                await LoadPrinterDriversAsync();
            else if (currentPage == "Printer Ports")
                await LoadPrinterPortsAsync();
            else if (currentPage == "Printers")
                await LoadInstalledPrintersAsync();
            else if (currentPage == "Ghost Printer")
                await LoadGhostPrintersAsync();
            else
                UpdateDeleteButtonState();
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