using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    partial class PrinterDriverDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;

        private Panel pnlContent;

        private Label lblDriverNameTitle;
        private TextBox txtDriverName;

        private Label lblProviderTitle;
        private TextBox txtProvider;

        private Label lblTypeTitle;
        private TextBox txtType;

        private Label lblInfTitle;
        private TextBox txtInf;

        private Label lblArchitectureTitle;
        private TextBox txtArchitecture;

        private Label lblStatusTitle;
        private TextBox txtStatus;

        private Label lblDriverPathTitle;
        private TextBox txtDriverPath;

        private Label lblFilePathTitle;
        private TextBox txtFilePath;

        private Label lblDataFileTitle;
        private TextBox txtDataFile;

        private Label lblConfigFileTitle;
        private TextBox txtConfigFile;

        private Label lblMonitorTitle;
        private TextBox txtMonitor;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            pnlHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            btnClose = new Button();

            pnlContent = new Panel();

            lblDriverNameTitle = new Label();
            txtDriverName = new TextBox();

            lblProviderTitle = new Label();
            txtProvider = new TextBox();

            lblTypeTitle = new Label();
            txtType = new TextBox();

            lblInfTitle = new Label();
            txtInf = new TextBox();

            lblArchitectureTitle = new Label();
            txtArchitecture = new TextBox();

            lblStatusTitle = new Label();
            txtStatus = new TextBox();

            lblDriverPathTitle = new Label();
            txtDriverPath = new TextBox();

            lblFilePathTitle = new Label();
            txtFilePath = new TextBox();

            lblDataFileTitle = new Label();
            txtDataFile = new TextBox();

            lblConfigFileTitle = new Label();
            txtConfigFile = new TextBox();

            lblMonitorTitle = new Label();
            txtMonitor = new TextBox();

            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();

            // =========================================================
            // Form
            // =========================================================
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(760, 590);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PrinterDriverDetailsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Printer Driver Details";

            // =========================================================
            // pnlHeader
            // =========================================================
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(760, 88);
            pnlHeader.TabIndex = 0;

            // =========================================================
            // lblTitle
            // =========================================================
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(191, 30);
            lblTitle.Text = "Printer Driver Details";

            // =========================================================
            // lblSubtitle
            // =========================================================
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(26, 51);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(190, 15);
            lblSubtitle.Text = "Installed printer driver information";

            // =========================================================
            // btnClose
            // =========================================================
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor =
                Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font(
                "Segoe UI", 9F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(71, 85, 105);
            btnClose.Location = new Point(680, 24);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(56, 32);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;

            // =========================================================
            // pnlContent
            // =========================================================
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 88);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(24, 20, 24, 20);
            pnlContent.Size = new Size(760, 502);
            pnlContent.TabIndex = 1;

            // =========================================================
            // Driver Name
            // =========================================================
            lblDriverNameTitle.AutoSize = true;
            lblDriverNameTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblDriverNameTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblDriverNameTitle.Location = new Point(24, 20);
            lblDriverNameTitle.Name = "lblDriverNameTitle";
            lblDriverNameTitle.Text = "Driver Name";

            txtDriverName.BackColor = Color.White;
            txtDriverName.BorderStyle = BorderStyle.FixedSingle;
            txtDriverName.Location = new Point(24, 40);
            txtDriverName.Name = "txtDriverName";
            txtDriverName.ReadOnly = true;
            txtDriverName.Size = new Size(712, 27);
            txtDriverName.TabIndex = 0;

            // =========================================================
            // Provider
            // =========================================================
            lblProviderTitle.AutoSize = true;
            lblProviderTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblProviderTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblProviderTitle.Location = new Point(24, 78);
            lblProviderTitle.Name = "lblProviderTitle";
            lblProviderTitle.Text = "Provider";

            txtProvider.BackColor = Color.White;
            txtProvider.BorderStyle = BorderStyle.FixedSingle;
            txtProvider.Location = new Point(24, 98);
            txtProvider.Name = "txtProvider";
            txtProvider.ReadOnly = true;
            txtProvider.Size = new Size(220, 27);

            // =========================================================
            // Type
            // =========================================================
            lblTypeTitle.AutoSize = true;
            lblTypeTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblTypeTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblTypeTitle.Location = new Point(260, 78);
            lblTypeTitle.Name = "lblTypeTitle";
            lblTypeTitle.Text = "Type";

            txtType.BackColor = Color.White;
            txtType.BorderStyle = BorderStyle.FixedSingle;
            txtType.Location = new Point(260, 98);
            txtType.Name = "txtType";
            txtType.ReadOnly = true;
            txtType.Size = new Size(150, 27);

            // =========================================================
            // INF
            // =========================================================
            lblInfTitle.AutoSize = true;
            lblInfTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblInfTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblInfTitle.Location = new Point(426, 78);
            lblInfTitle.Name = "lblInfTitle";
            lblInfTitle.Text = "INF";

            txtInf.BackColor = Color.White;
            txtInf.BorderStyle = BorderStyle.FixedSingle;
            txtInf.Location = new Point(426, 98);
            txtInf.Name = "txtInf";
            txtInf.ReadOnly = true;
            txtInf.Size = new Size(310, 27);

            // =========================================================
            // Architecture
            // =========================================================
            lblArchitectureTitle.AutoSize = true;
            lblArchitectureTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblArchitectureTitle.ForeColor =
                Color.FromArgb(51, 65, 85);
            lblArchitectureTitle.Location = new Point(24, 136);
            lblArchitectureTitle.Name = "lblArchitectureTitle";
            lblArchitectureTitle.Text = "Architecture";

            txtArchitecture.BackColor = Color.White;
            txtArchitecture.BorderStyle = BorderStyle.FixedSingle;
            txtArchitecture.Location = new Point(24, 156);
            txtArchitecture.Name = "txtArchitecture";
            txtArchitecture.ReadOnly = true;
            txtArchitecture.Size = new Size(220, 27);

            // =========================================================
            // Status
            // =========================================================
            lblStatusTitle.AutoSize = true;
            lblStatusTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblStatusTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblStatusTitle.Location = new Point(260, 136);
            lblStatusTitle.Name = "lblStatusTitle";
            lblStatusTitle.Text = "Status";

            txtStatus.BackColor = Color.White;
            txtStatus.BorderStyle = BorderStyle.FixedSingle;
            txtStatus.Location = new Point(260, 156);
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.Size = new Size(150, 27);

            // =========================================================
            // Driver Path
            // =========================================================
            lblDriverPathTitle.AutoSize = true;
            lblDriverPathTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblDriverPathTitle.ForeColor =
                Color.FromArgb(51, 65, 85);
            lblDriverPathTitle.Location = new Point(24, 194);
            lblDriverPathTitle.Name = "lblDriverPathTitle";
            lblDriverPathTitle.Text = "Driver Path";

            txtDriverPath.BackColor = Color.White;
            txtDriverPath.BorderStyle = BorderStyle.FixedSingle;
            txtDriverPath.Location = new Point(24, 214);
            txtDriverPath.Name = "txtDriverPath";
            txtDriverPath.ReadOnly = true;
            txtDriverPath.Size = new Size(712, 27);

            // =========================================================
            // File Path
            // =========================================================
            lblFilePathTitle.AutoSize = true;
            lblFilePathTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblFilePathTitle.ForeColor =
                Color.FromArgb(51, 65, 85);
            lblFilePathTitle.Location = new Point(24, 252);
            lblFilePathTitle.Name = "lblFilePathTitle";
            lblFilePathTitle.Text = "File Path";

            txtFilePath.BackColor = Color.White;
            txtFilePath.BorderStyle = BorderStyle.FixedSingle;
            txtFilePath.Location = new Point(24, 272);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.ReadOnly = true;
            txtFilePath.Size = new Size(712, 27);

            // =========================================================
            // Data File
            // =========================================================
            lblDataFileTitle.AutoSize = true;
            lblDataFileTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblDataFileTitle.ForeColor =
                Color.FromArgb(51, 65, 85);
            lblDataFileTitle.Location = new Point(24, 310);
            lblDataFileTitle.Name = "lblDataFileTitle";
            lblDataFileTitle.Text = "Data File";

            txtDataFile.BackColor = Color.White;
            txtDataFile.BorderStyle = BorderStyle.FixedSingle;
            txtDataFile.Location = new Point(24, 330);
            txtDataFile.Name = "txtDataFile";
            txtDataFile.ReadOnly = true;
            txtDataFile.Size = new Size(712, 27);

            // =========================================================
            // Config File
            // =========================================================
            lblConfigFileTitle.AutoSize = true;
            lblConfigFileTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblConfigFileTitle.ForeColor =
                Color.FromArgb(51, 65, 85);
            lblConfigFileTitle.Location = new Point(24, 368);
            lblConfigFileTitle.Name = "lblConfigFileTitle";
            lblConfigFileTitle.Text = "Config File";

            txtConfigFile.BackColor = Color.White;
            txtConfigFile.BorderStyle = BorderStyle.FixedSingle;
            txtConfigFile.Location = new Point(24, 388);
            txtConfigFile.Name = "txtConfigFile";
            txtConfigFile.ReadOnly = true;
            txtConfigFile.Size = new Size(712, 27);

            // =========================================================
            // Monitor
            // =========================================================
            lblMonitorTitle.AutoSize = true;
            lblMonitorTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            lblMonitorTitle.ForeColor =
                Color.FromArgb(51, 65, 85);
            lblMonitorTitle.Location = new Point(24, 426);
            lblMonitorTitle.Name = "lblMonitorTitle";
            lblMonitorTitle.Text = "Monitor";

            txtMonitor.BackColor = Color.White;
            txtMonitor.BorderStyle = BorderStyle.FixedSingle;
            txtMonitor.Location = new Point(24, 446);
            txtMonitor.Name = "txtMonitor";
            txtMonitor.ReadOnly = true;
            txtMonitor.Size = new Size(712, 27);

            // =========================================================
            // Add Controls
            // =========================================================
            pnlContent.Controls.Add(lblDriverNameTitle);
            pnlContent.Controls.Add(txtDriverName);

            pnlContent.Controls.Add(lblProviderTitle);
            pnlContent.Controls.Add(txtProvider);

            pnlContent.Controls.Add(lblTypeTitle);
            pnlContent.Controls.Add(txtType);

            pnlContent.Controls.Add(lblInfTitle);
            pnlContent.Controls.Add(txtInf);

            pnlContent.Controls.Add(lblArchitectureTitle);
            pnlContent.Controls.Add(txtArchitecture);

            pnlContent.Controls.Add(lblStatusTitle);
            pnlContent.Controls.Add(txtStatus);

            pnlContent.Controls.Add(lblDriverPathTitle);
            pnlContent.Controls.Add(txtDriverPath);

            pnlContent.Controls.Add(lblFilePathTitle);
            pnlContent.Controls.Add(txtFilePath);

            pnlContent.Controls.Add(lblDataFileTitle);
            pnlContent.Controls.Add(txtDataFile);

            pnlContent.Controls.Add(lblConfigFileTitle);
            pnlContent.Controls.Add(txtConfigFile);

            pnlContent.Controls.Add(lblMonitorTitle);
            pnlContent.Controls.Add(txtMonitor);

            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();

            ResumeLayout(false);
        }
    }
}
