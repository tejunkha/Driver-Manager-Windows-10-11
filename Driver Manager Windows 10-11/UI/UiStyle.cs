using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal static class UiStyle
    {
        private const int ActionButtonHeight = 38;
        private const int ActionButtonWidth = 112;
        private const int ActionIconSize = 20;

        private static readonly Color SidebarBack = Color.FromArgb(30, 41, 59);
        private static readonly Color SidebarHover = Color.FromArgb(39, 52, 73);
        private static readonly Color SidebarActive = Color.FromArgb(37, 99, 235);
        private static readonly Color SidebarText = Color.FromArgb(226, 232, 240);

        private static readonly Color TextColor = Color.FromArgb(15, 23, 42);
        private static readonly Color BorderColor = Color.FromArgb(203, 213, 225);

        // Delete button colors
        private static readonly Color DeleteColor = Color.FromArgb(220, 38, 38);
        private static readonly Color DeleteHover = Color.FromArgb(185, 28, 28);
        private static readonly Color DeleteDisabledBack = Color.FromArgb(254, 226, 226);
        private static readonly Color DeleteDisabledText = Color.FromArgb(239, 68, 68);

        public static void ApplyActionBar(MainForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            StyleRefresh(form.GetRefreshButtonForStyle());
            StyleDelete(form.GetDeleteButtonForStyle());
        }

        public static void ApplySidebar(MainForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            StyleSidebarButton(form.GetSystemDriversButtonForStyle());
            StyleSidebarButton(form.GetDriverStoreButtonForStyle());
            StyleSidebarButton(form.GetPrinterDriversButtonForStyle());
            StyleSidebarButton(form.GetPrinterPortsButtonForStyle());
            StyleSidebarButton(form.GetPrintersButtonForStyle());
            StyleSidebarButton(form.GetGhostPrinterButtonForStyle());
        }

        private static void StyleSidebarButton(Button button)
        {
            if (button == null)
                return;

            // No icons on sidebar.
            button.Image = null;
            button.BackgroundImage = null;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.Overlay;

            button.Text = GetSidebarText(button.Name);
            button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(24, 0, 8, 0);

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.UseVisualStyleBackColor = false;
            button.BackColor = SidebarBack;
            button.ForeColor = SidebarText;
            button.Cursor = Cursors.Hand;

            button.MouseEnter -= Sidebar_MouseEnter;
            button.MouseLeave -= Sidebar_MouseLeave;
            button.MouseEnter += Sidebar_MouseEnter;
            button.MouseLeave += Sidebar_MouseLeave;
        }

        private static string GetSidebarText(string name)
        {
            return name switch
            {
                "btnSystemDrivers" => "System Drivers",
                "btnDriverStore" => "Driver Store",
                "btnPrinterDrivers" => "Printer Drivers",
                "btnPrinterPorts" => "Printer Ports",
                "btnPrinters" => "Printers (IPP / Vendor)",
                "btnGhostPrinter" => "Print Server",
                _ => ""
            };
        }

        private static void Sidebar_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is not Button button || !button.Enabled)
                return;

            if (IsActive(button))
                return;

            button.BackColor = SidebarHover;
        }

        private static void Sidebar_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            if (IsActive(button))
                return;

            button.BackColor = SidebarBack;
        }

        private static bool IsActive(Button button)
        {
            return button.BackColor.ToArgb() == SidebarActive.ToArgb();
        }

        private static void StyleRefresh(Button button)
        {
            if (button == null)
                return;

            button.Text = "Refresh";
            button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            button.Image = CreateGlyphBitmap("\uE72C", TextColor, ActionIconSize);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.Padding = new Padding(6, 0, 8, 0);
            button.Height = ActionButtonHeight;
            button.Width = ActionButtonWidth;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = BorderColor;
            button.BackColor = Color.White;
            button.ForeColor = TextColor;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            button.MouseEnter -= Action_MouseEnter;
            button.MouseLeave -= Action_MouseLeave;
            button.MouseEnter += Action_MouseEnter;
            button.MouseLeave += Action_MouseLeave;
            button.Tag = new ActionColors(Color.White, Color.FromArgb(241, 245, 249), TextColor);
        }

        private static void StyleDelete(Button button)
        {
            if (button == null)
                return;

            button.Text = "Delete";
            button.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.Padding = new Padding(6, 0, 8, 0);
            button.Height = ActionButtonHeight;
            button.Width = ActionButtonWidth;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            button.MouseEnter -= Action_MouseEnter;
            button.MouseLeave -= Action_MouseLeave;
            button.MouseEnter += Action_MouseEnter;
            button.MouseLeave += Action_MouseLeave;
            button.EnabledChanged -= Delete_EnabledChanged;
            button.EnabledChanged += Delete_EnabledChanged;

            button.Tag = new ActionColors(DeleteColor, DeleteHover, Color.White);
            UpdateDeleteState(button);
        }

        private static void Action_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is not Button button || !button.Enabled)
                return;

            if (button.Tag is not ActionColors colors)
                return;

            button.BackColor = colors.Hover;
            button.ForeColor = colors.Text;
        }

        private static void Action_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is not Button button || button.Tag is not ActionColors colors)
                return;

            if (button.Enabled)
            {
                button.BackColor = colors.Normal;
                button.ForeColor = colors.Text;
            }
        }

        private static void Delete_EnabledChanged(object? sender, EventArgs e)
        {
            if (sender is Button button)
                UpdateDeleteState(button);
        }

        private static void UpdateDeleteState(Button button)
        {
            if (button.Enabled)
            {
                // Strong red when available: clearly indicates a destructive action.
                button.BackColor = DeleteColor;
                button.ForeColor = Color.White;
                button.Image = CreateGlyphBitmap("\uE74D", Color.White, ActionIconSize);
            }
            else
            {
                // Keep a visible red tint even when disabled, so it is still
                // obvious which button is Delete before a row is selected.
                button.BackColor = DeleteDisabledBack;
                button.ForeColor = DeleteDisabledText;
                button.Image = CreateGlyphBitmap("\uE74D", DeleteDisabledText, ActionIconSize);
            }
        }

        private static Bitmap CreateGlyphBitmap(string glyph, Color color, int iconSize)
        {
            Bitmap bitmap = new Bitmap(iconSize, iconSize);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            using Font font = new Font(
                "Segoe MDL2 Assets",
                Math.Max(10F, iconSize * 0.72F),
                FontStyle.Regular,
                GraphicsUnit.Point);

            using Brush brush = new SolidBrush(color);

            SizeF size = graphics.MeasureString(glyph, font);
            float x = (iconSize - size.Width) / 2F - 1F;
            float y = (iconSize - size.Height) / 2F - 1F;

            graphics.DrawString(glyph, font, brush, x, y);
            return bitmap;
        }

        private sealed class ActionColors
        {
            public ActionColors(Color normal, Color hover, Color text)
            {
                Normal = normal;
                Hover = hover;
                Text = text;
            }

            public Color Normal { get; }
            public Color Hover { get; }
            public Color Text { get; }
        }
    }
}
