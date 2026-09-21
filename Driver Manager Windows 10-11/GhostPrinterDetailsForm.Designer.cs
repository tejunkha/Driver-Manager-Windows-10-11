using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    partial class GhostPrinterDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;

        private Panel pnlContent;

        private Label lblPrinterNameTitle;
        private TextBox txtPrinterName;

        private Label lblDeviceIdTitle;
        private TextBox txtDeviceId;

        private Label lblManufacturerTitle;
        private TextBox txtManufacturer;

        private Label lblClassTitle;
        private TextBox txtClass;

        private Label lblStatusTitle;
        private TextBox txtStatus;

        private Label lblProblemCodeTitle;
        private TextBox txtProblemCode;

        private Label lblDescriptionTitle;
        private TextBox txtDescription;

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

            lblPrinterNameTitle = new Label();
            txtPrinterName = new TextBox();

            lblDeviceIdTitle = new Label();
            txtDeviceId = new TextBox();

            lblManufacturerTitle = new Label();
            txtManufacturer = new TextBox();

            lblClassTitle = new Label();
            txtClass = new TextBox();

            lblStatusTitle = new Label();
            txtStatus = new TextBox();

            lblProblemCodeTitle = new Label();
            txtProblemCode = new TextBox();

            lblDescriptionTitle = new Label();
            txtDescription = new TextBox();

            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();

            // =========================================================
            // Form
            // =========================================================
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(720, 410);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GhostPrinterDetailsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ghost Printer Details";

            // =========================================================
            // Header
            // =========================================================
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Size = new Size(720, 88);

            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 16);
            lblTitle.Text = "Ghost Printer Details";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(26, 51);
            lblSubtitle.Text = "Disconnected printer device information";

            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor =
                Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font(
                "Segoe UI", 9F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(71, 85, 105);
            btnClose.Location = new Point(640, 24);
            btnClose.Size = new Size(56, 32);
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;

            // =========================================================
            // Content
            // =========================================================
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(24, 18, 24, 18);

            SetupLabel(
                lblPrinterNameTitle,
                "Printer Name",
                24, 18);

            SetupBox(
                txtPrinterName,
                24, 38,
                672, 27);

            SetupLabel(
                lblDeviceIdTitle,
                "Device ID",
                24, 76);

            SetupBox(
                txtDeviceId,
                24, 96,
                672, 50);

            txtDeviceId.Multiline = true;
            txtDeviceId.ScrollBars = ScrollBars.Vertical;

            SetupLabel(
                lblManufacturerTitle,
                "Manufacturer",
                24, 162);

            SetupBox(
                txtManufacturer,
                24, 182,
                260, 27);

            SetupLabel(
                lblClassTitle,
                "Class",
                304, 162);

            SetupBox(
                txtClass,
                304, 182,
                392, 27);

            SetupLabel(
                lblStatusTitle,
                "Status",
                24, 220);

            SetupBox(
                txtStatus,
                24, 240,
                260, 27);

            SetupLabel(
                lblProblemCodeTitle,
                "Problem Code",
                304, 220);

            SetupBox(
                txtProblemCode,
                304, 240,
                392, 27);

            SetupLabel(
                lblDescriptionTitle,
                "Description / Service",
                24, 278);

            SetupBox(
                txtDescription,
                24, 298,
                672, 50);

            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;

            pnlContent.Controls.AddRange(new Control[]
            {
                lblPrinterNameTitle,
                txtPrinterName,
                lblDeviceIdTitle,
                txtDeviceId,
                lblManufacturerTitle,
                txtManufacturer,
                lblClassTitle,
                txtClass,
                lblStatusTitle,
                txtStatus,
                lblProblemCodeTitle,
                txtProblemCode,
                lblDescriptionTitle,
                txtDescription
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
