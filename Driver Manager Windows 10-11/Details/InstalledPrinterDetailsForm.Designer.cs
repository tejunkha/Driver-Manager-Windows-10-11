using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    partial class InstalledPrinterDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;
        private Panel pnlContent;

        private Label lblPrinterNameTitle;
        private TextBox txtPrinterName;
        private Label lblTypeTitle;
        private TextBox txtType;
        private Label lblVendorTitle;
        private TextBox txtVendor;
        private Label lblDriverTitle;
        private TextBox txtDriver;
        private Label lblPortTitle;
        private TextBox txtPort;
        private Label lblHostAddressTitle;
        private TextBox txtHostAddress;
        private Label lblStatusTitle;
        private TextBox txtStatus;
        private Label lblDefaultTitle;
        private TextBox txtDefault;
        private Label lblNetworkTitle;
        private TextBox txtNetwork;
        private Label lblSharedTitle;
        private TextBox txtShared;
        private Label lblWorkOfflineTitle;
        private TextBox txtWorkOffline;
        private Label lblServerTitle;
        private TextBox txtServer;
        private Label lblLocationTitle;
        private TextBox txtLocation;
        private Label lblCommentTitle;
        private TextBox txtComment;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private TextBox CreateValueTextBox(string name, Point location, Size size)
        {
            TextBox textBox = new TextBox();
            textBox.BackColor = Color.White;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Location = location;
            textBox.Name = name;
            textBox.ReadOnly = true;
            textBox.Size = size;
            return textBox;
        }

        private Label CreateTitleLabel(string name, string text, Point location)
        {
            Label label = new Label();
            label.AutoSize = true;
            label.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(51, 65, 85);
            label.Location = location;
            label.Name = name;
            label.Text = text;
            return label;
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            pnlHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            btnClose = new Button();
            pnlContent = new Panel();

            lblPrinterNameTitle = CreateTitleLabel("lblPrinterNameTitle", "Printer Name", new Point(24, 20));
            txtPrinterName = CreateValueTextBox("txtPrinterName", new Point(24, 40), new Size(712, 27));

            lblTypeTitle = CreateTitleLabel("lblTypeTitle", "Type", new Point(24, 78));
            txtType = CreateValueTextBox("txtType", new Point(24, 98), new Size(220, 27));

            lblVendorTitle = CreateTitleLabel("lblVendorTitle", "Vendor", new Point(260, 78));
            txtVendor = CreateValueTextBox("txtVendor", new Point(260, 98), new Size(220, 27));

            lblDriverTitle = CreateTitleLabel("lblDriverTitle", "Driver", new Point(496, 78));
            txtDriver = CreateValueTextBox("txtDriver", new Point(496, 98), new Size(240, 27));

            lblPortTitle = CreateTitleLabel("lblPortTitle", "Port", new Point(24, 136));
            txtPort = CreateValueTextBox("txtPort", new Point(24, 156), new Size(220, 27));

            lblHostAddressTitle = CreateTitleLabel("lblHostAddressTitle", "Host Address", new Point(260, 136));
            txtHostAddress = CreateValueTextBox("txtHostAddress", new Point(260, 156), new Size(220, 27));

            lblStatusTitle = CreateTitleLabel("lblStatusTitle", "Status", new Point(496, 136));
            txtStatus = CreateValueTextBox("txtStatus", new Point(496, 156), new Size(240, 27));

            lblDefaultTitle = CreateTitleLabel("lblDefaultTitle", "Default Printer", new Point(24, 194));
            txtDefault = CreateValueTextBox("txtDefault", new Point(24, 214), new Size(220, 27));

            lblNetworkTitle = CreateTitleLabel("lblNetworkTitle", "Network", new Point(260, 194));
            txtNetwork = CreateValueTextBox("txtNetwork", new Point(260, 214), new Size(220, 27));

            lblSharedTitle = CreateTitleLabel("lblSharedTitle", "Shared", new Point(496, 194));
            txtShared = CreateValueTextBox("txtShared", new Point(496, 214), new Size(240, 27));

            lblWorkOfflineTitle = CreateTitleLabel("lblWorkOfflineTitle", "Work Offline", new Point(24, 252));
            txtWorkOffline = CreateValueTextBox("txtWorkOffline", new Point(24, 272), new Size(220, 27));

            lblServerTitle = CreateTitleLabel("lblServerTitle", "Server", new Point(260, 252));
            txtServer = CreateValueTextBox("txtServer", new Point(260, 272), new Size(220, 27));

            lblLocationTitle = CreateTitleLabel("lblLocationTitle", "Location", new Point(496, 252));
            txtLocation = CreateValueTextBox("txtLocation", new Point(496, 272), new Size(240, 27));

            lblCommentTitle = CreateTitleLabel("lblCommentTitle", "Comment", new Point(24, 310));
            txtComment = CreateValueTextBox("txtComment", new Point(24, 330), new Size(712, 27));

            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(760, 430);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InstalledPrinterDetailsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Printer Details";

            // Header
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(760, 88);

            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 16);
            lblTitle.Text = "Printer Details";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(26, 51);
            lblSubtitle.Text = "Installed printer information";

            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(71, 85, 105);
            btnClose.Location = new Point(680, 24);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(56, 32);
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;

            // Content
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 88);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(24, 20, 24, 20);

            pnlContent.Controls.AddRange(new Control[]
            {
                lblPrinterNameTitle, txtPrinterName,
                lblTypeTitle, txtType,
                lblVendorTitle, txtVendor,
                lblDriverTitle, txtDriver,
                lblPortTitle, txtPort,
                lblHostAddressTitle, txtHostAddress,
                lblStatusTitle, txtStatus,
                lblDefaultTitle, txtDefault,
                lblNetworkTitle, txtNetwork,
                lblSharedTitle, txtShared,
                lblWorkOfflineTitle, txtWorkOffline,
                lblServerTitle, txtServer,
                lblLocationTitle, txtLocation,
                lblCommentTitle, txtComment
            });

            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);

            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
        }
    }
}
