using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal static class AdminModeStyle
    {
        private static readonly Color AdminBack = Color.FromArgb(22, 101, 52);
        private static readonly Color AdminBorder = Color.FromArgb(34, 197, 94);
        private static readonly Color AdminText = Color.White;

        private static readonly Color StandardBack = Color.FromArgb(245, 158, 11);
        private static readonly Color StandardBorder = Color.FromArgb(251, 191, 36);
        private static readonly Color StandardText = Color.FromArgb(31, 41, 55);

        public static void Apply(Label badge)
        {
            if (badge == null)
                return;

            bool isAdmin = AdminHelper.IsAdministrator();

            badge.AutoSize = false;
            badge.Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);
            badge.TextAlign = ContentAlignment.MiddleCenter;
            badge.BorderStyle = BorderStyle.FixedSingle;
            badge.Cursor = Cursors.Default;
            badge.Padding = new Padding(6, 0, 6, 0);

            if (isAdmin)
            {
                badge.Text = "●  ADMINISTRATOR";
                badge.BackColor = AdminBack;
                badge.ForeColor = AdminText;
                badge.FlatStyle = FlatStyle.Flat;
                badge.BorderStyle = BorderStyle.FixedSingle;
            }
            else
            {
                badge.Text = "●  STANDARD USER";
                badge.BackColor = StandardBack;
                badge.ForeColor = StandardText;
                badge.FlatStyle = FlatStyle.Flat;
                badge.BorderStyle = BorderStyle.FixedSingle;
            }

            ToolTip toolTip = new ToolTip();
            toolTip.InitialDelay = 300;
            toolTip.ReshowDelay = 100;
            toolTip.AutoPopDelay = 6000;

            toolTip.SetToolTip(
                badge,
                isAdmin
                    ? "Administrator mode\nAdministrative actions are available."
                    : "Standard user mode\nSome administrative actions require Run as administrator.");
        }
    }
}
