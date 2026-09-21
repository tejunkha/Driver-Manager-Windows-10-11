using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    partial class SystemDriverDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;

        private Panel pnlContent;

        private Label lblDeviceTitle;
        private TextBox txtDevice;

        private Label lblManufacturerTitle;
        private TextBox txtManufacturer;

        private Label lblVersionTitle;
        private TextBox txtVersion;

        private Label lblDateTitle;
        private TextBox txtDate;

        private Label lblInfTitle;
        private TextBox txtInf;

        private Label lblStatusTitle;
        private TextBox txtStatus;

        private Label lblSignedTitle;
        private TextBox txtSigned;

        private Label lblClassTitle;
        private TextBox txtClass;

        private Label lblDriverNameTitle;
        private TextBox txtDriverName;

        private Label lblDeviceIdTitle;
        private TextBox txtDeviceId;

        private Label lblLocationTitle;
        private TextBox txtLocation;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
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

            lblDeviceTitle = new Label();
            txtDevice = new TextBox();

            lblManufacturerTitle = new Label();
            txtManufacturer = new TextBox();

            lblVersionTitle = new Label();
            txtVersion = new TextBox();

            lblDateTitle = new Label();
            txtDate = new TextBox();

            lblInfTitle = new Label();
            txtInf = new TextBox();

            lblStatusTitle = new Label();
            txtStatus = new TextBox();

            lblSignedTitle = new Label();
            txtSigned = new TextBox();

            lblClassTitle = new Label();
            txtClass = new TextBox();

            lblDriverNameTitle = new Label();
            txtDriverName = new TextBox();

            lblDeviceIdTitle = new Label();
            txtDeviceId = new TextBox();

            lblLocationTitle = new Label();
            txtLocation = new TextBox();

            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(760, 535);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SystemDriverDetailsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "System Driver Details";

            // Header
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(760, 88);

            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 16);
            lblTitle.Text = "System Driver Details";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(26, 51);
            lblSubtitle.Text = "Installed system device driver information";

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
            btnClose.Size = new Size(56, 32);
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;

            // Content
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(24, 18, 24, 18);
            pnlContent.Size = new Size(760, 447);

            SetupLabel(lblDeviceTitle, "Device", 24, 18);
            SetupBox(txtDevice, 24, 38, 712, 27);

            SetupLabel(lblManufacturerTitle, "Manufacturer", 24, 74);
            SetupBox(txtManufacturer, 24, 94, 230, 27);

            SetupLabel(lblVersionTitle, "Driver Version", 270, 74);
            SetupBox(txtVersion, 270, 94, 210, 27);

            SetupLabel(lblDateTitle, "Driver Date", 496, 74);
            SetupBox(txtDate, 496, 94, 240, 27);

            SetupLabel(lblInfTitle, "INF Name", 24, 132);
            SetupBox(txtInf, 24, 152, 230, 27);

            SetupLabel(lblStatusTitle, "Status", 270, 132);
            SetupBox(txtStatus, 270, 152, 210, 27);

            SetupLabel(lblSignedTitle, "Signed", 496, 132);
            SetupBox(txtSigned, 496, 152, 240, 27);

            SetupLabel(lblClassTitle, "Device Class", 24, 190);
            SetupBox(txtClass, 24, 210, 230, 27);

            SetupLabel(lblDriverNameTitle, "Driver Name", 270, 190);
            SetupBox(txtDriverName, 270, 210, 466, 27);

            SetupLabel(lblDeviceIdTitle, "Device ID", 24, 248);
            SetupBox(txtDeviceId, 24, 268, 712, 52);

            txtDeviceId.Multiline = true;
            txtDeviceId.ScrollBars = ScrollBars.Vertical;

            SetupLabel(lblLocationTitle, "Location", 24, 334);
            SetupBox(txtLocation, 24, 354, 712, 52);

            txtLocation.Multiline = true;
            txtLocation.ScrollBars = ScrollBars.Vertical;

            pnlContent.Controls.AddRange(new Control[]
            {
                lblDeviceTitle, txtDevice,
                lblManufacturerTitle, txtManufacturer,
                lblVersionTitle, txtVersion,
                lblDateTitle, txtDate,
                lblInfTitle, txtInf,
                lblStatusTitle, txtStatus,
                lblSignedTitle, txtSigned,
                lblClassTitle, txtClass,
                lblDriverNameTitle, txtDriverName,
                lblDeviceIdTitle, txtDeviceId,
                lblLocationTitle, txtLocation
            });

            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ResumeLayout(false);
        }

        private void SetupLabel(
            Label label,
            string text,
            int x,
            int y)
        {
            label.AutoSize = true;
            label.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(51, 65, 85);
            label.Location = new Point(x, y);
            label.Text = text;
        }

        private void SetupBox(
            TextBox box,
            int x,
            int y,
            int width,
            int height)
        {
            box.BackColor = Color.White;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Location = new Point(x, y);
            box.ReadOnly = true;
            box.Size = new Size(width, height);
        }
    }
}
