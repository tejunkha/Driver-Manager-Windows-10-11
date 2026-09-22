using System.Drawing;
using System.Windows.Forms;
using DrawingFont = System.Drawing.Font;
namespace Driver_Manager_Windows_10_11
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // =========================================================
        // Sidebar
        // =========================================================
        private Panel pnlSidebar;
        private Label lblAppTitle;
        private Label lblAppSubtitle;
        private Label lblDriversMenu;

        private Button btnSystemDrivers;
        private Button btnDriverStore;
        private Button btnPrinterDrivers;
        private Button btnPrinterPorts;
        private Button btnPrinters;
        private Button btnGhostPrinter;

        private Panel pnlGhostStatus;
        private Label lblGhostStatusIcon;
        private Label lblGhostStatusTitle;
        private Label lblGhostStatusDescription;

        // =========================================================
        // Main
        // =========================================================
        private Panel pnlMain;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Panel pnlToolbar;

        private Label lblPageTitle;
        private Label lblPageDescription;

        private TextBox txtSearch;
        private Button btnRefresh;
        private Button btnDelete;

        // =========================================================
        // DataGridView
        // =========================================================
        private DataGridView dgvDrivers;

        private DataGridViewTextBoxColumn colDevice;
        private DataGridViewTextBoxColumn colManufacturer;
        private DataGridViewTextBoxColumn colVersion;
        private DataGridViewTextBoxColumn colDate;
        private DataGridViewTextBoxColumn colInf;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colSigned;

        // =========================================================
        // Status
        // =========================================================
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            pnlSidebar = new Panel();
            lblAppTitle = new Label();
            lblAppSubtitle = new Label();
            lblDriversMenu = new Label();
            btnSystemDrivers = new Button();
            btnDriverStore = new Button();
            btnPrinterDrivers = new Button();
            btnPrinterPorts = new Button();
            btnPrinters = new Button();
            btnGhostPrinter = new Button();

            pnlGhostStatus = new Panel();
            lblGhostStatusIcon = new Label();
            lblGhostStatusTitle = new Label();
            lblGhostStatusDescription = new Label();

            pnlMain = new Panel();
            pnlContent = new Panel();
            dgvDrivers = new DataGridView();
            colDevice = new DataGridViewTextBoxColumn();
            colManufacturer = new DataGridViewTextBoxColumn();
            colVersion = new DataGridViewTextBoxColumn();
            colDate = new DataGridViewTextBoxColumn();
            colInf = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colSigned = new DataGridViewTextBoxColumn();
            pnlToolbar = new Panel();
            txtSearch = new TextBox();
            btnRefresh = new Button();
            btnDelete = new Button();
            pnlHeader = new Panel();
            lblPageDescription = new Label();
            lblPageTitle = new Label();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            pnlSidebar.SuspendLayout();
            pnlMain.SuspendLayout();
            pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDrivers).BeginInit();
            pnlToolbar.SuspendLayout();
            pnlHeader.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(30, 41, 59);
            pnlSidebar.Controls.Add(lblAppTitle);
            pnlSidebar.Controls.Add(lblAppSubtitle);
            pnlSidebar.Controls.Add(lblDriversMenu);
            pnlSidebar.Controls.Add(btnSystemDrivers);
            pnlSidebar.Controls.Add(btnDriverStore);
            pnlSidebar.Controls.Add(btnPrinterDrivers);
            pnlSidebar.Controls.Add(btnPrinterPorts);
            pnlSidebar.Controls.Add(btnPrinters);
            pnlSidebar.Controls.Add(btnGhostPrinter);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(245, 739);
            pnlSidebar.TabIndex = 0;
            // 
            // lblAppTitle
            // 
            lblAppTitle.AutoSize = true;
            lblAppTitle.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.White;
            lblAppTitle.Location = new Point(20, 22);
            lblAppTitle.Name = "lblAppTitle";
            lblAppTitle.Size = new Size(152, 28);
            lblAppTitle.TabIndex = 0;
            lblAppTitle.Text = "Driver Manager";
            // 
            // lblAppSubtitle
            // 
            lblAppSubtitle.AutoSize = true;
            lblAppSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblAppSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
            lblAppSubtitle.Location = new Point(22, 53);
            lblAppSubtitle.Name = "lblAppSubtitle";
            lblAppSubtitle.Size = new Size(94, 15);
            lblAppSubtitle.TabIndex = 1;
            lblAppSubtitle.Text = "Windows 10 / 11";
            // 
            // lblDriversMenu
            // 
            lblDriversMenu.AutoSize = true;
            lblDriversMenu.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
            lblDriversMenu.ForeColor = Color.FromArgb(148, 163, 184);
            lblDriversMenu.Location = new Point(20, 105);
            lblDriversMenu.Name = "lblDriversMenu";
            lblDriversMenu.Size = new Size(51, 13);
            lblDriversMenu.TabIndex = 2;
            lblDriversMenu.Text = "DRIVERS";
            // 
            // btnSystemDrivers
            // 
            btnSystemDrivers.BackColor = Color.FromArgb(30, 41, 59);
            btnSystemDrivers.Cursor = Cursors.Hand;
            btnSystemDrivers.FlatAppearance.BorderSize = 0;
            btnSystemDrivers.FlatStyle = FlatStyle.Flat;
            btnSystemDrivers.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnSystemDrivers.ForeColor = Color.White;
            btnSystemDrivers.Location = new Point(12, 135);
            btnSystemDrivers.Name = "btnSystemDrivers";
            btnSystemDrivers.Padding = new Padding(12, 0, 0, 0);
            btnSystemDrivers.Size = new Size(221, 44);
            btnSystemDrivers.TabIndex = 3;
            btnSystemDrivers.Text = "▣   System Drivers";
            btnSystemDrivers.TextAlign = ContentAlignment.MiddleLeft;
            btnSystemDrivers.UseVisualStyleBackColor = false;
            btnSystemDrivers.Click += btnSystemDrivers_Click;
            // 
            // btnDriverStore
            // 
            btnDriverStore.BackColor = Color.FromArgb(30, 41, 59);
            btnDriverStore.Cursor = Cursors.Hand;
            btnDriverStore.FlatAppearance.BorderSize = 0;
            btnDriverStore.FlatStyle = FlatStyle.Flat;
            btnDriverStore.Font = new Font("Segoe UI", 10F);
            btnDriverStore.ForeColor = Color.FromArgb(226, 232, 240);
            btnDriverStore.Location = new Point(12, 185);
            btnDriverStore.Name = "btnDriverStore";
            btnDriverStore.Padding = new Padding(12, 0, 0, 0);
            btnDriverStore.Size = new Size(221, 44);
            btnDriverStore.TabIndex = 4;
            btnDriverStore.Text = "▤   Driver Store";
            btnDriverStore.TextAlign = ContentAlignment.MiddleLeft;
            btnDriverStore.UseVisualStyleBackColor = false;
            btnDriverStore.Click += btnDriverStore_Click;
            // 
            // btnPrinterDrivers
            // 
            btnPrinterDrivers.BackColor = Color.FromArgb(30, 41, 59);
            btnPrinterDrivers.Cursor = Cursors.Hand;
            btnPrinterDrivers.FlatAppearance.BorderSize = 0;
            btnPrinterDrivers.FlatStyle = FlatStyle.Flat;
            btnPrinterDrivers.Font = new Font("Segoe UI", 10F);
            btnPrinterDrivers.ForeColor = Color.FromArgb(226, 232, 240);
            btnPrinterDrivers.Location = new Point(12, 235);
            btnPrinterDrivers.Name = "btnPrinterDrivers";
            btnPrinterDrivers.Padding = new Padding(12, 0, 0, 0);
            btnPrinterDrivers.Size = new Size(221, 44);
            btnPrinterDrivers.TabIndex = 5;
            btnPrinterDrivers.Text = "▦   Printer Drivers";
            btnPrinterDrivers.TextAlign = ContentAlignment.MiddleLeft;
            btnPrinterDrivers.UseVisualStyleBackColor = false;
            btnPrinterDrivers.Click += btnPrinterDrivers_Click;
            // 
            // btnPrinterPorts
            // 
            btnPrinterPorts.BackColor = Color.FromArgb(30, 41, 59);
            btnPrinterPorts.Cursor = Cursors.Hand;
            btnPrinterPorts.FlatAppearance.BorderSize = 0;
            btnPrinterPorts.FlatStyle = FlatStyle.Flat;
            btnPrinterPorts.Font = new Font("Segoe UI", 10F);
            btnPrinterPorts.ForeColor = Color.FromArgb(226, 232, 240);
            btnPrinterPorts.Location = new Point(12, 285);
            btnPrinterPorts.Name = "btnPrinterPorts";
            btnPrinterPorts.Padding = new Padding(12, 0, 0, 0);
            btnPrinterPorts.Size = new Size(221, 44);
            btnPrinterPorts.TabIndex = 6;
            btnPrinterPorts.Text = "▤   Printer Ports";
            btnPrinterPorts.TextAlign = ContentAlignment.MiddleLeft;
            btnPrinterPorts.UseVisualStyleBackColor = false;

            // 
            // btnPrinters
            // 
            btnPrinters.BackColor = Color.FromArgb(30, 41, 59);
            btnPrinters.Cursor = Cursors.Hand;
            btnPrinters.FlatAppearance.BorderSize = 0;
            btnPrinters.FlatStyle = FlatStyle.Flat;
            btnPrinters.Font = new Font("Segoe UI", 10F);
            btnPrinters.ForeColor = Color.FromArgb(226, 232, 240);
            btnPrinters.Location = new Point(12, 335);
            btnPrinters.Name = "btnPrinters";
            btnPrinters.Padding = new Padding(12, 0, 0, 0);
            btnPrinters.Size = new Size(221, 44);
            btnPrinters.TabIndex = 7;
            btnPrinters.Text = "▣   Printers (IPP / Vendor)";
            btnPrinters.TextAlign = ContentAlignment.MiddleLeft;
            btnPrinters.UseVisualStyleBackColor = false;

            // 
            // btnGhostPrinter
            // 
            btnGhostPrinter.BackColor = Color.FromArgb(30, 41, 59);
            btnGhostPrinter.Cursor = Cursors.Hand;
            btnGhostPrinter.FlatAppearance.BorderSize = 0;
            btnGhostPrinter.FlatStyle = FlatStyle.Flat;
            btnGhostPrinter.Font = new Font("Segoe UI", 10F);
            btnGhostPrinter.ForeColor = Color.FromArgb(226, 232, 240);
            btnGhostPrinter.Location = new Point(12, 385);
            btnGhostPrinter.Name = "btnGhostPrinter";
            btnGhostPrinter.Padding = new Padding(12, 0, 0, 0);
            btnGhostPrinter.Size = new Size(221, 44);
            btnGhostPrinter.TabIndex = 8;
            btnGhostPrinter.Text = "▧   Ghost Printer";
            btnGhostPrinter.TextAlign = ContentAlignment.MiddleLeft;
            btnGhostPrinter.UseVisualStyleBackColor = false;
            btnGhostPrinter.Click += btnGhostPrinter_Click;
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(248, 250, 252);
            pnlMain.Controls.Add(pnlContent);
            pnlMain.Controls.Add(pnlHeader);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(245, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1139, 739);
            pnlMain.TabIndex = 1;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Controls.Add(dgvDrivers);
            pnlContent.Controls.Add(pnlGhostStatus);
            pnlContent.Controls.Add(pnlToolbar);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 82);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(25, 18, 25, 20);
            pnlContent.Size = new Size(1139, 657);
            pnlContent.TabIndex = 1;
            // 
            // dgvDrivers
            // 
            dgvDrivers.AllowUserToAddRows = false;
            dgvDrivers.AllowUserToDeleteRows = false;
            dgvDrivers.AllowUserToResizeRows = false;
            dgvDrivers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDrivers.BackgroundColor = Color.White;
            dgvDrivers.BorderStyle = BorderStyle.None;
            dgvDrivers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvDrivers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvDrivers.ColumnHeadersHeight = 42;
            dgvDrivers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDrivers.Columns.AddRange(new DataGridViewColumn[] { colDevice, colManufacturer, colVersion, colDate, colInf, colStatus, colSigned });
            dgvDrivers.Dock = DockStyle.Fill;
            dgvDrivers.EnableHeadersVisualStyles = false;
            dgvDrivers.GridColor = Color.FromArgb(226, 232, 240);
            dgvDrivers.Location = new Point(25, 68);
            dgvDrivers.MultiSelect = false;
            dgvDrivers.Name = "dgvDrivers";
            dgvDrivers.ReadOnly = true;
            dgvDrivers.RowHeadersVisible = false;
            dgvDrivers.RowTemplate.Height = 38;
            dgvDrivers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDrivers.Size = new Size(1089, 569);
            dgvDrivers.TabIndex = 1;
            // 
            // colDevice
            // 
            colDevice.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDevice.FillWeight = 180F;
            colDevice.HeaderText = "Device";
            colDevice.Name = "colDevice";
            colDevice.ReadOnly = true;
            // 
            // colManufacturer
            // 
            colManufacturer.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colManufacturer.HeaderText = "Manufacturer";
            colManufacturer.Name = "colManufacturer";
            colManufacturer.ReadOnly = true;
            // 
            // colVersion
            // 
            colVersion.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colVersion.FillWeight = 90F;
            colVersion.HeaderText = "Driver Version";
            colVersion.Name = "colVersion";
            colVersion.ReadOnly = true;
            // 
            // colDate
            // 
            colDate.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDate.FillWeight = 80F;
            colDate.HeaderText = "Driver Date";
            colDate.Name = "colDate";
            colDate.ReadOnly = true;
            // 
            // colInf
            // 
            colInf.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colInf.FillWeight = 80F;
            colInf.HeaderText = "INF Name";
            colInf.Name = "colInf";
            colInf.ReadOnly = true;
            // 
            // colStatus
            // 
            colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colStatus.FillWeight = 70F;
            colStatus.HeaderText = "Status";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            // 
            // colSigned
            // 
            colSigned.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSigned.FillWeight = 60F;
            colSigned.HeaderText = "Signed";
            colSigned.Name = "colSigned";
            colSigned.ReadOnly = true;
            // 
            // pnlGhostStatus
            // 
            pnlGhostStatus.BackColor = Color.White;
            pnlGhostStatus.Controls.Add(lblGhostStatusDescription);
            pnlGhostStatus.Controls.Add(lblGhostStatusTitle);
            pnlGhostStatus.Controls.Add(lblGhostStatusIcon);
            pnlGhostStatus.Location = new Point(25, 68);
            pnlGhostStatus.Name = "pnlGhostStatus";
            pnlGhostStatus.Size = new Size(1089, 569);
            pnlGhostStatus.TabIndex = 2;
            pnlGhostStatus.Visible = false;

            // 
            // lblGhostStatusIcon
            // 
            lblGhostStatusIcon.Anchor =
                AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblGhostStatusIcon.Font =
                new Font("Segoe UI Symbol", 30F);
            lblGhostStatusIcon.ForeColor =
                Color.FromArgb(148, 163, 184);
            lblGhostStatusIcon.Location =
                new Point(20, 145);
            lblGhostStatusIcon.Name =
                "lblGhostStatusIcon";
            lblGhostStatusIcon.Size =
                new Size(1049, 55);
            lblGhostStatusIcon.TabIndex = 0;
            lblGhostStatusIcon.Text = "▣";
            lblGhostStatusIcon.TextAlign =
                ContentAlignment.MiddleCenter;

            // 
            // lblGhostStatusTitle
            // 
            lblGhostStatusTitle.Anchor =
                AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblGhostStatusTitle.Font =
                new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
            lblGhostStatusTitle.ForeColor =
                Color.FromArgb(51, 65, 85);
            lblGhostStatusTitle.Location =
                new Point(20, 214);
            lblGhostStatusTitle.Name =
                "lblGhostStatusTitle";
            lblGhostStatusTitle.Size =
                new Size(1049, 30);
            lblGhostStatusTitle.TabIndex = 1;
            lblGhostStatusTitle.Text =
                "No Ghost Printer Found";
            lblGhostStatusTitle.TextAlign =
                ContentAlignment.MiddleCenter;

            // 
            // lblGhostStatusDescription
            // 
            lblGhostStatusDescription.Anchor =
                AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblGhostStatusDescription.Font =
                new Font("Segoe UI", 9.5F);
            lblGhostStatusDescription.ForeColor =
                Color.FromArgb(100, 116, 139);
            lblGhostStatusDescription.Location =
                new Point(20, 251);
            lblGhostStatusDescription.Name =
                "lblGhostStatusDescription";
            lblGhostStatusDescription.Size =
                new Size(1049, 24);
            lblGhostStatusDescription.TabIndex = 2;
            lblGhostStatusDescription.Text =
                "No disconnected printer devices detected.";
            lblGhostStatusDescription.TextAlign =
                ContentAlignment.MiddleCenter;

            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.FromArgb(248, 250, 252);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(25, 18);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1089, 50);
            pnlToolbar.TabIndex = 0;
            // 
            // txtSearch
            // 
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 10F);
            txtSearch.Location = new Point(0, 5);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search driver...";
            txtSearch.Size = new Size(390, 25);
            txtSearch.TabIndex = 0;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(405, 4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(105, 32);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "⟳  Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.FromArgb(220, 38, 38);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(520, 4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(105, 32);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "✕  Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Enabled = false;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblPageDescription);
            pnlHeader.Controls.Add(lblPageTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1139, 82);
            pnlHeader.TabIndex = 0;
            // 
            // lblPageDescription
            // 
            lblPageDescription.AutoSize = true;
            lblPageDescription.Font = new Font("Segoe UI", 9F);
            lblPageDescription.ForeColor = Color.FromArgb(100, 116, 139);
            lblPageDescription.Location = new Point(27, 47);
            lblPageDescription.Name = "lblPageDescription";
            lblPageDescription.Size = new Size(194, 15);
            lblPageDescription.TabIndex = 1;
            lblPageDescription.Text = "View installed system device drivers";
            // 
            // lblPageTitle
            // 
            lblPageTitle.AutoSize = true;
            lblPageTitle.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            lblPageTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblPageTitle.Location = new Point(25, 12);
            lblPageTitle.Name = "lblPageTitle";
            lblPageTitle.Size = new Size(169, 31);
            lblPageTitle.TabIndex = 0;
            lblPageTitle.Text = "System Drivers";
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.White;
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 739);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1384, 22);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 17);
            lblStatus.Text = "Ready";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1384, 761);
            Controls.Add(pnlMain);
            Controls.Add(pnlSidebar);
            Controls.Add(statusStrip);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1100, 650);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Driver Manager - Windows 10 / 11";
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            pnlMain.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDrivers).EndInit();
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}