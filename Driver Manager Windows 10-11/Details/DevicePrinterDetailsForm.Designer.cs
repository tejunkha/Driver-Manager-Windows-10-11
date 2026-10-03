using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    partial class DevicePrinterDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;
        private Panel pnlContent;

        private Label lblPrinterNameTitle;
        private TextBox txtPrinterName;
        private Label lblCategoryTitle;
        private TextBox txtCategory;
        private Label lblManufacturerTitle;
        private TextBox txtManufacturer;
        private Label lblDriverTitle;
        private TextBox txtDriver;
        private Label lblPortTitle;
        private TextBox txtPort;
        private Label lblPortMonitorTitle;
        private TextBox txtPortMonitor;
        private Label lblStatusTitle;
        private TextBox txtStatus;
        private Label lblStateTitle;
        private TextBox txtState;
        private Label lblPresentTitle;
        private TextBox txtPresent;
        private Label lblServerTitle;
        private TextBox txtServer;
        private Label lblShareNameTitle;
        private TextBox txtShareName;
        private Label lblJobsTitle;
        private TextBox txtJobs;
        private Label lblSourceTitle;
        private TextBox txtSource;
        private Label lblWorkOfflineTitle;
        private TextBox txtWorkOffline;
        private Label lblPublishedTitle;
        private TextBox txtPublished;
        private Label lblSharedTitle;
        private TextBox txtShared;
        private Label lblDeviceIdTitle;
        private TextBox txtDeviceId;
        private Label lblClassGuidTitle;
        private TextBox txtClassGuid;
        private Label lblReasonTitle;
        private TextBox txtReason;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private TextBox CreateValueTextBox(
            string name,
            Point location,
            Size size)
        {
            return new TextBox
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = location,
                Name = name,
                ReadOnly = true,
                Size = size
            };
        }

        private Label CreateTitleLabel(
            string name,
            string text,
            Point location)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font(
                    "Segoe UI Semibold",
                    9F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = location,
                Name = name,
                Text = text
            };
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            pnlHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            btnClose = new Button();
            pnlContent = new Panel();

            lblPrinterNameTitle =
                CreateTitleLabel(
                    "lblPrinterNameTitle",
                    "Printer / Device Name",
                    new Point(24, 20));

            txtPrinterName =
                CreateValueTextBox(
                    "txtPrinterName",
                    new Point(24, 40),
                    new Size(712, 27));

            lblCategoryTitle =
                CreateTitleLabel(
                    "lblCategoryTitle",
                    "Category",
                    new Point(24, 78));

            txtCategory =
                CreateValueTextBox(
                    "txtCategory",
                    new Point(24, 98),
                    new Size(220, 27));

            lblManufacturerTitle =
                CreateTitleLabel(
                    "lblManufacturerTitle",
                    "Manufacturer",
                    new Point(260, 78));

            txtManufacturer =
                CreateValueTextBox(
                    "txtManufacturer",
                    new Point(260, 98),
                    new Size(220, 27));

            lblDriverTitle =
                CreateTitleLabel(
                    "lblDriverTitle",
                    "Driver",
                    new Point(496, 78));

            txtDriver =
                CreateValueTextBox(
                    "txtDriver",
                    new Point(496, 98),
                    new Size(240, 27));

            lblPortTitle =
                CreateTitleLabel(
                    "lblPortTitle",
                    "Port",
                    new Point(24, 136));

            txtPort =
                CreateValueTextBox(
                    "txtPort",
                    new Point(24, 156),
                    new Size(220, 27));

            lblPortMonitorTitle =
                CreateTitleLabel(
                    "lblPortMonitorTitle",
                    "Port Monitor",
                    new Point(260, 136));

            txtPortMonitor =
                CreateValueTextBox(
                    "txtPortMonitor",
                    new Point(260, 156),
                    new Size(220, 27));

            lblStatusTitle =
                CreateTitleLabel(
                    "lblStatusTitle",
                    "Status",
                    new Point(496, 136));

            txtStatus =
                CreateValueTextBox(
                    "txtStatus",
                    new Point(496, 156),
                    new Size(240, 27));

            lblStateTitle =
                CreateTitleLabel(
                    "lblStateTitle",
                    "State",
                    new Point(24, 194));

            txtState =
                CreateValueTextBox(
                    "txtState",
                    new Point(24, 214),
                    new Size(220, 27));

            lblPresentTitle =
                CreateTitleLabel(
                    "lblPresentTitle",
                    "Present",
                    new Point(260, 194));

            txtPresent =
                CreateValueTextBox(
                    "txtPresent",
                    new Point(260, 214),
                    new Size(220, 27));

            lblServerTitle =
                CreateTitleLabel(
                    "lblServerTitle",
                    "Server",
                    new Point(496, 194));

            txtServer =
                CreateValueTextBox(
                    "txtServer",
                    new Point(496, 214),
                    new Size(240, 27));

            lblShareNameTitle =
                CreateTitleLabel(
                    "lblShareNameTitle",
                    "Share Name",
                    new Point(24, 252));

            txtShareName =
                CreateValueTextBox(
                    "txtShareName",
                    new Point(24, 272),
                    new Size(220, 27));

            lblJobsTitle =
                CreateTitleLabel(
                    "lblJobsTitle",
                    "Jobs",
                    new Point(260, 252));

            txtJobs =
                CreateValueTextBox(
                    "txtJobs",
                    new Point(260, 272),
                    new Size(220, 27));

            lblSourceTitle =
                CreateTitleLabel(
                    "lblSourceTitle",
                    "Source",
                    new Point(496, 252));

            txtSource =
                CreateValueTextBox(
                    "txtSource",
                    new Point(496, 272),
                    new Size(240, 27));

            lblWorkOfflineTitle =
                CreateTitleLabel(
                    "lblWorkOfflineTitle",
                    "Work Offline",
                    new Point(24, 310));

            txtWorkOffline =
                CreateValueTextBox(
                    "txtWorkOffline",
                    new Point(24, 330),
                    new Size(220, 27));

            lblPublishedTitle =
                CreateTitleLabel(
                    "lblPublishedTitle",
                    "Published",
                    new Point(260, 310));

            txtPublished =
                CreateValueTextBox(
                    "txtPublished",
                    new Point(260, 330),
                    new Size(220, 27));

            lblSharedTitle =
                CreateTitleLabel(
                    "lblSharedTitle",
                    "Shared",
                    new Point(496, 310));

            txtShared =
                CreateValueTextBox(
                    "txtShared",
                    new Point(496, 330),
                    new Size(240, 27));

            lblDeviceIdTitle =
                CreateTitleLabel(
                    "lblDeviceIdTitle",
                    "Device ID",
                    new Point(24, 368));

            txtDeviceId =
                CreateValueTextBox(
                    "txtDeviceId",
                    new Point(24, 388),
                    new Size(712, 27));

            lblClassGuidTitle =
                CreateTitleLabel(
                    "lblClassGuidTitle",
                    "Class GUID",
                    new Point(24, 426));

            txtClassGuid =
                CreateValueTextBox(
                    "txtClassGuid",
                    new Point(24, 446),
                    new Size(712, 27));

            lblReasonTitle =
                CreateTitleLabel(
                    "lblReasonTitle",
                    "Reason",
                    new Point(24, 484));

            txtReason =
                CreateValueTextBox(
                    "txtReason",
                    new Point(24, 504),
                    new Size(712, 27));

            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(760, 620);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DevicePrinterDetailsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Device & Printer Details";

            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(760, 88);

            lblTitle.AutoSize = true;
            lblTitle.Font = new Font(
                "Segoe UI Semibold",
                16F,
                FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 16);
            lblTitle.Text = "Device & Printer Details";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(26, 51);
            lblSubtitle.Text = "Windows printer device information";

            btnClose.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            btnClose.BackColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor =
                Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font(
                "Segoe UI",
                9F,
                FontStyle.Bold);
            btnClose.ForeColor =
                Color.FromArgb(71, 85, 105);
            btnClose.Location = new Point(680, 24);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(56, 32);
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;

            pnlContent.BackColor =
                Color.FromArgb(248, 250, 252);

            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding =
                new Padding(24, 20, 24, 20);

            pnlContent.Controls.AddRange(
                new Control[]
                {
                    lblPrinterNameTitle,
                    txtPrinterName,
                    lblCategoryTitle,
                    txtCategory,
                    lblManufacturerTitle,
                    txtManufacturer,
                    lblDriverTitle,
                    txtDriver,
                    lblPortTitle,
                    txtPort,
                    lblPortMonitorTitle,
                    txtPortMonitor,
                    lblStatusTitle,
                    txtStatus,
                    lblStateTitle,
                    txtState,
                    lblPresentTitle,
                    txtPresent,
                    lblServerTitle,
                    txtServer,
                    lblShareNameTitle,
                    txtShareName,
                    lblJobsTitle,
                    txtJobs,
                    lblSourceTitle,
                    txtSource,
                    lblWorkOfflineTitle,
                    txtWorkOffline,
                    lblPublishedTitle,
                    txtPublished,
                    lblSharedTitle,
                    txtShared,
                    lblDeviceIdTitle,
                    txtDeviceId,
                    lblClassGuidTitle,
                    txtClassGuid,
                    lblReasonTitle,
                    txtReason
                });

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
