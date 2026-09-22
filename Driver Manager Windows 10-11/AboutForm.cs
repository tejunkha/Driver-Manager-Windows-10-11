using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal sealed class AboutForm : Form
    {
        public AboutForm()
        {
            Text = $"About {AppInfo.Name}";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(500, 360);

            Panel accent = new Panel
            {
                Dock = DockStyle.Top,
                Height = 6,
                BackColor = Color.FromArgb(37, 99, 235)
            };

            Label title = new Label
            {
                AutoSize = true,
                Text = AppInfo.Name,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(28, 28)
            };

            Label version = new Label
            {
                AutoSize = true,
                Text = $"Version {AppInfo.Version}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(30, 66)
            };

            Panel line = new Panel
            {
                Location = new Point(30, 98),
                Size = new Size(440, 1),
                BackColor = Color.FromArgb(226, 232, 240)
            };

            Label details = new Label
            {
                AutoSize = false,
                Text =
                    $"Platform:           {AppInfo.Platform}\n" +
                    $"Framework:          {AppInfo.Framework}\n" +
                    $"Runtime:            {AppInfo.Runtime}\n" +
                    $"Deployment:         {AppInfo.BuildType}\n" +
                    $"Administrator:      {(AdminHelper.IsAdministrator() ? "Yes" : "No")}\n\n" +
                    $"Copyright:          {AppInfo.Copyright}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(30, 120),
                Size = new Size(440, 150)
            };

            Button close = new Button
            {
                Text = "Close",
                Width = 96,
                Height = 36,
                Location = new Point(374, 298),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            close.FlatAppearance.BorderSize = 0;
            close.Click += (_, _) => Close();

            Controls.Add(close);
            Controls.Add(details);
            Controls.Add(line);
            Controls.Add(version);
            Controls.Add(title);
            Controls.Add(accent);

            AcceptButton = close;
            CancelButton = close;
        }
    }
}
