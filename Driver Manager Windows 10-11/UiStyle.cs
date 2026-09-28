using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal static class UiStyle
    {
        private const int SidebarIconSize = 24;
        private const int ActionIconSize = 20;

        private static readonly Color SidebarBack =
            Color.FromArgb(30, 41, 59);

        private static readonly Color SidebarHover =
            Color.FromArgb(39, 52, 73);

        private static readonly Color SidebarText =
            Color.FromArgb(226, 232, 240);

        private static readonly Color TextColor =
            Color.FromArgb(15, 23, 42);

        private static readonly Color BorderColor =
            Color.FromArgb(203, 213, 225);

        private static readonly Color DeleteColor =
            Color.FromArgb(220, 38, 38);

        private static readonly Color DeleteHover =
            Color.FromArgb(185, 28, 28);

        private static readonly Color DisabledBack =
            Color.FromArgb(226, 232, 240);

        private static readonly Color DisabledText =
            Color.FromArgb(148, 163, 184);

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

            StyleSidebar(
                form.GetSystemDriversButtonForStyle(),
                "\uE975"); // Devices2

            StyleSidebar(
                form.GetDriverStoreButtonForStyle(),
                "\uE8B7"); // Folder

            StyleSidebar(
                form.GetPrinterDriversButtonForStyle(),
                "\uE749"); // Print

            StyleSidebar(
                form.GetPrinterPortsButtonForStyle(),
                "\uE95F"); // Wire

            StyleSidebar(
                form.GetPrintersButtonForStyle(),
                "\uEDA5"); // NetworkPrinter

            StyleSidebar(
                form.GetGhostPrinterButtonForStyle(),
                "\uEA14"); // DisconnectDisplay
        }

        private static void StyleSidebar(
            Button button,
            string glyph)
        {
            if (button == null)
                return;

            button.Text = GetMenuText(button.Name);

            button.Font = new Font(
                "Segoe UI",
                10F,
                FontStyle.Regular);

            button.Image = CreateWindowsGlyph(
                glyph,
                SidebarText,
                SidebarIconSize);

            button.ImageAlign =
                ContentAlignment.MiddleLeft;

            button.TextAlign =
                ContentAlignment.MiddleLeft;

            button.TextImageRelation =
                TextImageRelation.ImageBeforeText;

            button.Padding =
                new Padding(14, 0, 8, 0);

            button.FlatStyle =
                FlatStyle.Flat;

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

        private static string GetMenuText(string name)
        {
            return name switch
            {
                "btnSystemDrivers" =>
                    "System Drivers",

                "btnDriverStore" =>
                    "Driver Store",

                "btnPrinterDrivers" =>
                    "Printer Drivers",

                "btnPrinterPorts" =>
                    "Printer Ports",

                "btnPrinters" =>
                    "Printers (IPP / Vendor)",

                "btnGhostPrinter" =>
                    "Ghost Printer",

                _ => ""
            };
        }

        private static void Sidebar_MouseEnter(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button button)
                return;

            if (!button.Enabled)
                return;

            if (IsActive(button))
                return;

            button.BackColor = SidebarHover;
        }

        private static void Sidebar_MouseLeave(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button button)
                return;

            if (IsActive(button))
                return;

            button.BackColor = SidebarBack;
        }

        private static bool IsActive(Button button)
        {
            return button.BackColor.ToArgb() ==
                   Color.FromArgb(
                       37,
                       99,
                       235).ToArgb();
        }

        private static void StyleRefresh(Button button)
        {
            button.Text = "Refresh";

            button.Font = new Font(
                "Segoe UI",
                9.5F,
                FontStyle.Regular);

            button.Image = CreateWindowsGlyph(
                "\uE72C", // Refresh
                TextColor,
                ActionIconSize);

            button.ImageAlign =
                ContentAlignment.MiddleLeft;

            button.TextAlign =
                ContentAlignment.MiddleCenter;

            button.TextImageRelation =
                TextImageRelation.ImageBeforeText;

            button.Padding =
                new Padding(7, 0, 8, 0);

            button.Width = 112;
            button.Height = 38;

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor =
                BorderColor;

            button.BackColor = Color.White;
            button.ForeColor = TextColor;

            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;

            button.MouseEnter -= Action_MouseEnter;
            button.MouseLeave -= Action_MouseLeave;

            button.MouseEnter += Action_MouseEnter;
            button.MouseLeave += Action_MouseLeave;
        }

        private static void StyleDelete(Button button)
        {
            button.Text = "Delete";

            button.Font = new Font(
                "Segoe UI",
                9.5F,
                FontStyle.Regular);

            button.Image = CreateWindowsGlyph(
                "\uE74D", // Delete
                Color.White,
                ActionIconSize);

            button.ImageAlign =
                ContentAlignment.MiddleLeft;

            button.TextAlign =
                ContentAlignment.MiddleCenter;

            button.TextImageRelation =
                TextImageRelation.ImageBeforeText;

            button.Padding =
                new Padding(7, 0, 8, 0);

            button.Width = 112;
            button.Height = 38;

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 0;

            button.BackColor = DeleteColor;
            button.ForeColor = Color.White;

            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;

            button.Tag = new ActionColors(
                DeleteColor,
                DeleteHover,
                Color.White);

            button.MouseEnter -= Action_MouseEnter;
            button.MouseLeave -= Action_MouseLeave;

            button.MouseEnter += Action_MouseEnter;
            button.MouseLeave += Action_MouseLeave;

            button.EnabledChanged -=
                Delete_EnabledChanged;

            button.EnabledChanged +=
                Delete_EnabledChanged;

            UpdateDeleteState(button);
        }

        private static void Action_MouseEnter(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button button)
                return;

            if (!button.Enabled)
                return;

            if (button.Tag is ActionColors colors)
            {
                button.BackColor =
                    colors.Hover;

                button.ForeColor =
                    colors.Text;
            }
            else if (ReferenceEquals(
                         button,
                         null) == false)
            {
                button.BackColor =
                    Color.FromArgb(
                        241,
                        245,
                        249);
            }
        }

        private static void Action_MouseLeave(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button button)
                return;

            if (!button.Enabled)
                return;

            if (button.Tag is ActionColors colors)
            {
                button.BackColor =
                    colors.Normal;

                button.ForeColor =
                    colors.Text;
            }
            else
            {
                button.BackColor =
                    Color.White;

                button.ForeColor =
                    TextColor;
            }
        }

        private static void Delete_EnabledChanged(
            object? sender,
            EventArgs e)
        {
            if (sender is Button button)
                UpdateDeleteState(button);
        }

        private static void UpdateDeleteState(
            Button button)
        {
            if (button.Enabled)
            {
                button.BackColor =
                    DeleteColor;

                button.ForeColor =
                    Color.White;

                button.Image =
                    CreateWindowsGlyph(
                        "\uE74D",
                        Color.White,
                        ActionIconSize);
            }
            else
            {
                button.BackColor =
                    DisabledBack;

                button.ForeColor =
                    DisabledText;

                button.Image =
                    CreateWindowsGlyph(
                        "\uE74D",
                        DisabledText,
                        ActionIconSize);
            }
        }

        private static Bitmap CreateWindowsGlyph(
            string glyph,
            Color color,
            int size)
        {
            string fontName =
                HasFont("Segoe Fluent Icons")
                    ? "Segoe Fluent Icons"
                    : "Segoe MDL2 Assets";

            Bitmap bitmap =
                new Bitmap(size, size);

            using Graphics graphics =
                Graphics.FromImage(bitmap);

            graphics.Clear(Color.Transparent);

            graphics.TextRenderingHint =
                TextRenderingHint.AntiAliasGridFit;

            using Font font =
                new Font(
                    fontName,
                    size,
                    FontStyle.Regular,
                    GraphicsUnit.Pixel);

            using Brush brush =
                new SolidBrush(color);

            SizeF measured =
                graphics.MeasureString(
                    glyph,
                    font);

            float x =
                (size - measured.Width) / 2F;

            float y =
                (size - measured.Height) / 2F - 1F;

            graphics.DrawString(
                glyph,
                font,
                brush,
                x,
                y);

            return bitmap;
        }

        private static bool HasFont(
            string fontName)
        {
            using InstalledFontCollection fonts =
                new InstalledFontCollection();

            foreach (FontFamily family in fonts.Families)
            {
                if (family.Name.Equals(
                    fontName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class ActionColors
        {
            public ActionColors(
                Color normal,
                Color hover,
                Color text)
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
