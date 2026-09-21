using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    partial class DriverStoreDetailsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnClose;

        private Panel pnlContent;

        private Label lblPublishedTitle;
        private TextBox txtPublished;

        private Label lblOriginalTitle;
        private TextBox txtOriginal;

        private Label lblProviderTitle;
        private TextBox txtProvider;

        private Label lblClassTitle;
        private TextBox txtClass;

        private Label lblVersionTitle;
        private TextBox txtVersion;

        private Label lblDateTitle;
        private TextBox txtDate;

        private Label lblSignerTitle;
        private TextBox txtSigner;

        private Label lblClassGuidTitle;
        private TextBox txtClassGuid;

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

            lblPublishedTitle = new Label();
            txtPublished = new TextBox();

            lblOriginalTitle = new Label();
            txtOriginal = new TextBox();

            lblProviderTitle = new Label();
            txtProvider = new TextBox();

            lblClassTitle = new Label();
            txtClass = new TextBox();

            lblVersionTitle = new Label();
            txtVersion = new TextBox();

            lblDateTitle = new Label();
            txtDate = new TextBox();

            lblSignerTitle = new Label();
            txtSigner = new TextBox();

            lblClassGuidTitle = new Label();
            txtClassGuid = new TextBox();

            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(720, 390);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DriverStoreDetailsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Driver Store Details";

            // Header
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Size = new Size(720, 88);

            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font(
                "Segoe UI Semibold", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 16);
            lblTitle.Text = "Driver Store Details";

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(26, 51);
            lblSubtitle.Text = "Driver package information";

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

            // Content
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(24, 18, 24, 18);

            SetupLabel(lblPublishedTitle, "Published Name", 24, 18);
            SetupBox(txtPublished, 24, 38, 220, 27);

            SetupLabel(lblOriginalTitle, "Original Name", 260, 18);
            SetupBox(txtOriginal, 260, 38, 436, 27);

            SetupLabel(lblProviderTitle, "Provider", 24, 76);
            SetupBox(txtProvider, 24, 96, 220, 27);

            SetupLabel(lblClassTitle, "Class", 260, 76);
            SetupBox(txtClass, 260, 96, 200, 27);

            SetupLabel(lblVersionTitle, "Driver Version", 476, 76);
            SetupBox(txtVersion, 476, 96, 220, 27);

            SetupLabel(lblDateTitle, "Date", 24, 134);
            SetupBox(txtDate, 24, 154, 220, 27);

            SetupLabel(lblSignerTitle, "Signer", 260, 134);
            SetupBox(txtSigner, 260, 154, 436, 27);

            SetupLabel(lblClassGuidTitle, "Class GUID", 24, 192);
            SetupBox(txtClassGuid, 24, 212, 672, 52);

            txtClassGuid.Multiline = true;
            txtClassGuid.ScrollBars = ScrollBars.Vertical;

            pnlContent.Controls.AddRange(new Control[]
            {
                lblPublishedTitle, txtPublished,
                lblOriginalTitle, txtOriginal,
                lblProviderTitle, txtProvider,
                lblClassTitle, txtClass,
                lblVersionTitle, txtVersion,
                lblDateTitle, txtDate,
                lblSignerTitle, txtSigner,
                lblClassGuidTitle, txtClassGuid
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
