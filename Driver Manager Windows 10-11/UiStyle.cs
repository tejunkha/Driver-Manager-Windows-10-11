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
        private const int SidebarIconSize = 22;

        private static readonly Color SidebarTextColor = Color.FromArgb(226, 232, 240);
        private static readonly Color TextColor = Color.FromArgb(15, 23, 42);
        private static readonly Color BorderColor = Color.FromArgb(203, 213, 225);
        private static readonly Color DeleteColor = Color.FromArgb(220, 38, 38);
        private static readonly Color DeleteHoverColor = Color.FromArgb(185, 28, 28);
        private static readonly Color DisabledBackColor = Color.FromArgb(226, 232, 240);
        private static readonly Color DisabledTextColor = Color.FromArgb(148, 163, 184);

        public static void ApplyActionBar(MainForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            Button refresh = form.GetRefreshButtonForStyle();
            Button delete = form.GetDeleteButtonForStyle();

            StyleRefresh(refresh);
            StyleDelete(delete);
        }

        public static void ApplySidebar(MainForm form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            StyleSidebarButton(
                form.GetSystemDriversButtonForStyle(),
                SidebarIcon.System);

            StyleSidebarButton(
                form.GetDriverStoreButtonForStyle(),
                SidebarIcon.Store);

            StyleSidebarButton(
                form.GetPrinterDriversButtonForStyle(),
                SidebarIcon.Printer);

            StyleSidebarButton(
                form.GetPrinterPortsButtonForStyle(),
                SidebarIcon.Port);

            StyleSidebarButton(
                form.GetPrintersButtonForStyle(),
                SidebarIcon.PrinterNetwork);

            StyleSidebarButton(
                form.GetGhostPrinterButtonForStyle(),
                SidebarIcon.Ghost);
        }

        private static void StyleSidebarButton(Button button, SidebarIcon icon)
        {
            button.Text = GetSidebarText(button.Name);
            button.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
            button.Image = null;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.Overlay;
            button.Padding = new Padding(18, 0, 6, 0);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
            button.BackColor = Color.FromArgb(30, 41, 59);
            button.ForeColor = SidebarTextColor;
        }

        private static string GetSidebarText(string buttonName)
        {
            return buttonName switch
            {
                "btnSystemDrivers" => "System Drivers",
                "btnDriverStore" => "Driver Store",
                "btnPrinterDrivers" => "Printer Drivers",
                "btnPrinterPorts" => "Printer Ports",
                "btnPrinters" => "Printers (IPP / Vendor)",
                "btnGhostPrinter" => "Devices & Printers",
                _ => ""
            };
        }

        private static void StyleRefresh(Button button)
        {
            button.Text = "Refresh";
            button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            button.Image = null;
            button.ImageAlign = ContentAlignment.MiddleCenter;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.TextImageRelation = TextImageRelation.Overlay;
            button.Padding = new Padding(8, 0, 8, 0);

            button.Height = ActionButtonHeight;
            button.Width = ActionButtonWidth;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = BorderColor;
            button.BackColor = Color.White;
            button.ForeColor = TextColor;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            HookHover(button, Color.White, Color.FromArgb(241, 245, 249), TextColor, TextColor);
        }

        private static void StyleDelete(Button button)
        {
            button.Text = "Delete";
            button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            button.Image = null;
            button.ImageAlign = ContentAlignment.MiddleCenter;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.TextImageRelation = TextImageRelation.Overlay;
            button.Padding = new Padding(8, 0, 8, 0);

            button.Height = ActionButtonHeight;
            button.Width = ActionButtonWidth;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = DeleteColor;
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            HookHover(button, DeleteColor, DeleteHoverColor, Color.White, Color.White);
            button.EnabledChanged += Delete_EnabledChanged;
            ApplyDeleteEnabledState(button);
        }

        private static void HookHover(
            Button button,
            Color normalBack,
            Color hoverBack,
            Color normalText,
            Color hoverText)
        {
            button.MouseEnter -= ActionButton_MouseEnter;
            button.MouseLeave -= ActionButton_MouseLeave;
            button.MouseEnter += ActionButton_MouseEnter;
            button.MouseLeave += ActionButton_MouseLeave;

            button.Tag = new HoverColors(normalBack, hoverBack, normalText, hoverText);
        }

        private static void ActionButton_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is not Button button || !button.Enabled)
                return;

            if (button.Tag is HoverColors colors)
            {
                button.BackColor = colors.HoverBack;
                button.ForeColor = colors.HoverText;
            }
        }

        private static void ActionButton_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Tag is HoverColors colors && button.Enabled)
            {
                button.BackColor = colors.NormalBack;
                button.ForeColor = colors.NormalText;
            }
        }

        private static void Delete_EnabledChanged(object? sender, EventArgs e)
        {
            if (sender is Button button)
                ApplyDeleteEnabledState(button);
        }

        private static void ApplyDeleteEnabledState(Button button)
        {
            if (button.Enabled)
            {
                button.BackColor = DeleteColor;
                button.ForeColor = Color.White;
                button.Image = null;
            }
            else
            {
                button.BackColor = DisabledBackColor;
                button.ForeColor = DisabledTextColor;
                button.Image = null;
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
                Math.Max(10f, iconSize * 0.72f),
                FontStyle.Regular,
                GraphicsUnit.Point);

            using Brush brush = new SolidBrush(color);

            SizeF size = graphics.MeasureString(glyph, font);
            float x = (iconSize - size.Width) / 2f - 1f;
            float y = (iconSize - size.Height) / 2f - 1f;

            graphics.DrawString(glyph, font, brush, x, y);
            return bitmap;
        }

        private static Bitmap CreateSidebarIcon(SidebarIcon icon, Color color)
        {
            Bitmap bitmap = new Bitmap(SidebarIconSize, SidebarIconSize);

            using Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            using Pen pen = new Pen(color, 1.8f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            using Brush brush = new SolidBrush(color);

            switch (icon)
            {
                case SidebarIcon.System:
                    DrawChip(g, pen, brush);
                    break;
                case SidebarIcon.Store:
                    DrawFolder(g, pen, brush);
                    break;
                case SidebarIcon.Printer:
                    DrawPrinter(g, pen, brush, false);
                    break;
                case SidebarIcon.Port:
                    DrawPlug(g, pen, brush);
                    break;
                case SidebarIcon.PrinterNetwork:
                    DrawPrinter(g, pen, brush, true);
                    break;
                case SidebarIcon.Ghost:
                    DrawGhost(g, pen, brush);
                    break;
            }

            return bitmap;
        }


        private static void DrawRoundedRectangle(
            Graphics g,
            Pen pen,
            float x,
            float y,
            float width,
            float height,
            float radius)
        {
            using GraphicsPath path = new GraphicsPath();

            float d = radius * 2f;

            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + width - d, y, d, d, 270, 90);
            path.AddArc(x + width - d, y + height - d, d, d, 0, 90);
            path.AddArc(x, y + height - d, d, d, 90, 90);
            path.CloseFigure();

            g.DrawPath(pen, path);
        }

        private static void DrawChip(Graphics g, Pen pen, Brush brush)
        {
            DrawRoundedRectangle(g, pen, 5, 5, 12, 12, 2);
            g.FillRectangle(brush, 8, 8, 6, 6);

            for (int i = 0; i < 3; i++)
            {
                float p = 7 + i * 4;
                g.DrawLine(pen, p, 2, p, 5);
                g.DrawLine(pen, p, 17, p, 20);
                g.DrawLine(pen, 2, p, 5, p);
                g.DrawLine(pen, 17, p, 20, p);
            }
        }

        private static void DrawFolder(Graphics g, Pen pen, Brush brush)
        {
            PointF[] folder =
            {
                new PointF(3, 6), new PointF(8, 6), new PointF(10, 8),
                new PointF(19, 8), new PointF(17, 17), new PointF(4, 17)
            };
            g.DrawLines(pen, folder);
            g.DrawLine(pen, 3, 6, 3, 16);
            g.DrawLine(pen, 4, 17, 17, 17);
            g.DrawLine(pen, 5, 10, 17, 10);
        }

        private static void DrawPrinter(Graphics g, Pen pen, Brush brush, bool network)
        {
            g.DrawRectangle(pen, 5, 3, 12, 6);
            g.DrawRectangle(pen, 3, 8, 16, 7);
            g.DrawRectangle(pen, 6, 13, 12, 6);
            g.FillRectangle(brush, 14, 10, 2, 2);

            if (network)
            {
                g.DrawLine(pen, 3, 19, 9, 19);
                g.DrawLine(pen, 9, 19, 9, 16);
                g.DrawLine(pen, 9, 16, 12, 16);
            }
        }

        private static void DrawPlug(Graphics g, Pen pen, Brush brush)
        {
            DrawRoundedRectangle(g, pen, 7, 6, 8, 9, 2);
            g.DrawLine(pen, 9, 3, 9, 7);
            g.DrawLine(pen, 13, 3, 13, 7);
            g.DrawLine(pen, 15, 10, 19, 10);
            g.DrawLine(pen, 3, 15, 7, 15);
            g.DrawArc(pen, 7, 11, 7, 7, 180, 180);
        }

        private static void DrawGhost(Graphics g, Pen pen, Brush brush)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(5, 3, 12, 12, 180, 180);
            path.AddLine(5, 9, 5, 18);
            path.AddLine(8, 16, 10, 18);
            path.AddLine(12, 16, 14, 18);
            path.AddLine(17, 16, 17, 9);
            path.AddLine(17, 9, 17, 7);
            path.AddLine(5, 7, 5, 9);
            path.CloseFigure();
            g.DrawPath(pen, path);
            g.FillEllipse(brush, 8, 8, 2, 2);
            g.FillEllipse(brush, 13, 8, 2, 2);
        }

        private enum SidebarIcon
        {
            System,
            Store,
            Printer,
            Port,
            PrinterNetwork,
            Ghost
        }

        private sealed class HoverColors
        {
            public HoverColors(Color normalBack, Color hoverBack, Color normalText, Color hoverText)
            {
                NormalBack = normalBack;
                HoverBack = hoverBack;
                NormalText = normalText;
                HoverText = hoverText;
            }

            public Color NormalBack { get; }
            public Color HoverBack { get; }
            public Color NormalText { get; }
            public Color HoverText { get; }
        }
    }
}
