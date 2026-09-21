using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    partial class PrinterPortDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;
        private Panel pnlContent;

        private Label lblPortNameTitle;
        private TextBox txtPortName;
        private Label lblDescriptionTitle;
        private TextBox txtDescription;
        private Label lblHostAddressTitle;
        private TextBox txtHostAddress;
        private Label lblPortNumberTitle;
        private TextBox txtPortNumber;
        private Label lblProtocolTitle;
        private TextBox txtProtocol;
        private Label lblSNMPTitle;
        private TextBox txtSNMP;
        private Label lblPortMonitorTitle;
        private TextBox txtPortMonitor;
        private Label lblComputerTitle;
        private TextBox txtComputer;

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

            lblPortNameTitle = new Label();
            txtPortName = new TextBox();
            lblDescriptionTitle = new Label();
            txtDescription = new TextBox();
            lblHostAddressTitle = new Label();
            txtHostAddress = new TextBox();
            lblPortNumberTitle = new Label();
            txtPortNumber = new TextBox();
            lblProtocolTitle = new Label();
            txtProtocol = new TextBox();
            lblSNMPTitle = new Label();
            txtSNMP = new TextBox();
            lblPortMonitorTitle = new Label();
            txtPortMonitor = new TextBox();
            lblComputerTitle = new Label();
            txtComputer = new TextBox();

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
            Name = "PrinterPortDetailsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Printer Port Details";

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
            lblTitle.Text = "Printer Port Details";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(26, 51);
            lblSubtitle.Text = "Installed printer port information";

            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(71, 85, 105);
            btnClose.Location = new Point(640, 24);
            btnClose.Size = new Size(56, 32);
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;

            // =========================================================
            // Content
            // =========================================================
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(24, 18, 24, 18);

            SetupLabel(lblPortNameTitle, "Port Name", 24, 18);
            SetupBox(txtPortName, 24, 38, 672, 27);

            SetupLabel(lblDescriptionTitle, "Description", 24, 76);
            SetupBox(txtDescription, 24, 96, 672, 27);

            SetupLabel(lblHostAddressTitle, "Host Address", 24, 134);
            SetupBox(txtHostAddress, 24, 154, 430, 27);

            SetupLabel(lblPortNumberTitle, "Port Number", 470, 134);
            SetupBox(txtPortNumber, 470, 154, 226, 27);

            SetupLabel(lblProtocolTitle, "Protocol", 24, 192);
            SetupBox(txtProtocol, 24, 212, 206, 27);

            SetupLabel(lblSNMPTitle, "SNMP", 246, 192);
            SetupBox(txtSNMP, 246, 212, 206, 27);

            SetupLabel(lblPortMonitorTitle, "Port Monitor", 468, 192);
            SetupBox(txtPortMonitor, 468, 212, 228, 27);

            SetupLabel(lblComputerTitle, "Computer", 24, 250);
            SetupBox(txtComputer, 24, 270, 672, 27);

            pnlContent.Controls.AddRange(new Control[]
            {
                lblPortNameTitle,
                txtPortName,
                lblDescriptionTitle,
                txtDescription,
                lblHostAddressTitle,
                txtHostAddress,
                lblPortNumberTitle,
                txtPortNumber,
                lblProtocolTitle,
                txtProtocol,
                lblSNMPTitle,
                txtSNMP,
                lblPortMonitorTitle,
                txtPortMonitor,
                lblComputerTitle,
                txtComputer
            });

            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ResumeLayout(false);
        }

        private void SetupLabel(Label label, string text, int x, int y)
        {
            label.AutoSize = true;
            label.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 9F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(51, 65, 85);
            label.Location = new Point(x, y);
            label.Text = text;
        }

        private void SetupBox(TextBox box, int x, int y, int width, int height)
        {
            box.BackColor = Color.White;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Font = new System.Drawing.Font("Segoe UI", 9F);
            box.ForeColor = Color.FromArgb(15, 23, 42);
            box.Location = new Point(x, y);
            box.ReadOnly = true;
            box.Size = new Size(width, height);
            box.TabStop = false;
        }
    }
}
