using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal class ModeBadgeControl : Control
    {
        private bool isAdministrator;
        private readonly ToolTip toolTip;

        public ModeBadgeControl()
        {
            DoubleBuffered = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            Font = new Font("Segoe UI", 9F);

            // Control does not support Color.Transparent here.
            // Use the sidebar color because the badge paints its full area anyway.
            BackColor = Color.FromArgb(30, 41, 59);

            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 200,
                ReshowDelay = 100
            };

            UpdateTooltip();
        }

        // Runtime-only method. No public serializable property is exposed.
        public void SetAdministratorMode(bool value)
        {
            if (isAdministrator == value)
                return;

            isAdministrator = value;
            UpdateTooltip();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(
                1,
                1,
                Math.Max(1, Width - 2),
                Math.Max(1, Height - 2));

            const int radius = 12;

            Color background;
            Color border;
            Color subtitleColor;

            if (isAdministrator)
            {
                background = Color.FromArgb(18, 95, 52);
                border = Color.FromArgb(45, 220, 120);
                subtitleColor = Color.FromArgb(205, 235, 218);
            }
            else
            {
                background = Color.FromArgb(105, 72, 8);
                border = Color.FromArgb(245, 180, 35);
                subtitleColor = Color.FromArgb(255, 230, 165);
            }

            using (GraphicsPath path = CreateRoundedRectangle(rect, radius))
            using (SolidBrush brush = new SolidBrush(background))
            using (Pen pen = new Pen(border, 1.5F))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            DrawModeIcon(g);

            using Font titleFont = new Font(
                "Segoe UI Semibold",
                10.5F,
                FontStyle.Bold);

            using Font subtitleFont = new Font(
                "Segoe UI",
                7.8F,
                FontStyle.Regular);

            string title = isAdministrator
                ? "ADMINISTRATOR"
                : "STANDARD USER";

            string subtitle = isAdministrator
                ? "Full privileges available"
                : "Some actions require administrator privileges";

            using Brush titleBrush =
                new SolidBrush(Color.White);

            using Brush subtitleBrush =
                new SolidBrush(subtitleColor);

            g.DrawString(
                title,
                titleFont,
                titleBrush,
                new PointF(62, 16));

            g.DrawString(
                subtitle,
                subtitleFont,
                subtitleBrush,
                new PointF(62, 40));
        }

        private void DrawModeIcon(Graphics g)
        {
            if (isAdministrator)
            {
                DrawShield(
                    g,
                    14,
                    14,
                    38,
                    Color.FromArgb(40, 210, 110));

                using Pen checkPen =
                    new Pen(Color.White, 3F);

                checkPen.StartCap = LineCap.Round;
                checkPen.EndCap = LineCap.Round;

                PointF p1 = new PointF(24, 33);
                PointF p2 = new PointF(31, 40);
                PointF p3 = new PointF(43, 25);

                g.DrawLines(
                    checkPen,
                    new[] { p1, p2, p3 });
            }
            else
            {
                PointF[] triangle =
                {
                    new PointF(33, 14),
                    new PointF(51, 48),
                    new PointF(15, 48)
                };

                using Brush warningBrush =
                    new SolidBrush(Color.FromArgb(255, 193, 7));

                using Pen warningPen =
                    new Pen(
                        Color.FromArgb(255, 220, 90),
                        1.5F);

                g.FillPolygon(warningBrush, triangle);
                g.DrawPolygon(warningPen, triangle);

                using Font warningFont =
                    new Font(
                        "Segoe UI Semibold",
                        18F,
                        FontStyle.Bold);

                using Brush exclamationBrush =
                    new SolidBrush(
                        Color.FromArgb(80, 55, 0));

                using StringFormat format =
                    new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                g.DrawString(
                    "!",
                    warningFont,
                    exclamationBrush,
                    new RectangleF(24, 20, 18, 25),
                    format);
            }
        }

        private static void DrawShield(
            Graphics g,
            float x,
            float y,
            float size,
            Color color)
        {
            float w = size;
            float h = size;

            PointF[] shield =
            {
                new PointF(
                    x + w * 0.50F,
                    y),

                new PointF(
                    x + w * 0.88F,
                    y + h * 0.18F),

                new PointF(
                    x + w * 0.82F,
                    y + h * 0.65F),

                new PointF(
                    x + w * 0.50F,
                    y + h),

                new PointF(
                    x + w * 0.18F,
                    y + h * 0.65F),

                new PointF(
                    x + w * 0.12F,
                    y + h * 0.18F)
            };

            using Brush brush = new SolidBrush(color);
            using Pen pen = new Pen(Color.White, 1.2F);

            g.FillPolygon(brush, shield);
            g.DrawPolygon(pen, shield);
        }

        private static GraphicsPath CreateRoundedRectangle(
            Rectangle rect,
            int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int diameter = radius * 2;

            path.AddArc(
                rect.X,
                rect.Y,
                diameter,
                diameter,
                180,
                90);

            path.AddArc(
                rect.Right - diameter,
                rect.Y,
                diameter,
                diameter,
                270,
                90);

            path.AddArc(
                rect.Right - diameter,
                rect.Bottom - diameter,
                diameter,
                diameter,
                0,
                90);

            path.AddArc(
                rect.X,
                rect.Bottom - diameter,
                diameter,
                diameter,
                90,
                90);

            path.CloseFigure();

            return path;
        }

        private void UpdateTooltip()
        {
            toolTip.SetToolTip(
                this,
                isAdministrator
                    ? "Administrator mode - administrative actions are available."
                    : "Standard User mode - some administrative actions require elevation.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                toolTip.Dispose();

            base.Dispose(disposing);
        }
    }
}
