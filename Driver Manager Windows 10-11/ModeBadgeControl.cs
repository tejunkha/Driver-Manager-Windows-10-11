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
            BackColor = Color.FromArgb(30, 41, 59);

            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 200,
                ReshowDelay = 100
            };

            UpdateTooltip();
        }

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

            Color background = isAdministrator
                ? Color.FromArgb(20, 83, 45)
                : Color.FromArgb(120, 83, 10);

            Color border = isAdministrator
                ? Color.FromArgb(34, 197, 94)
                : Color.FromArgb(245, 158, 11);

            using (GraphicsPath path = CreateRoundedRectangle(rect, 10))
            using (SolidBrush brush = new SolidBrush(background))
            using (Pen pen = new Pen(border, 1F))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            DrawCompactIcon(g);

            using Font titleFont = new Font(
                "Segoe UI Semibold",
                9.5F,
                FontStyle.Bold);

            string title = isAdministrator
                ? "ADMINISTRATOR"
                : "STANDARD USER";

            using Brush titleBrush = new SolidBrush(Color.White);

            g.DrawString(
                title,
                titleFont,
                titleBrush,
                new PointF(47, 11));
        }

        private void DrawCompactIcon(Graphics g)
        {
            if (isAdministrator)
            {
                DrawShield(
                    g,
                    10,
                    7,
                    27,
                    Color.FromArgb(34, 197, 94));

                using Pen checkPen = new Pen(Color.White, 2.2F);
                checkPen.StartCap = LineCap.Round;
                checkPen.EndCap = LineCap.Round;

                g.DrawLines(
                    checkPen,
                    new[]
                    {
                        new PointF(17, 19),
                        new PointF(22, 24),
                        new PointF(31, 14)
                    });
            }
            else
            {
                PointF[] triangle =
                {
                    new PointF(24, 7),
                    new PointF(37, 30),
                    new PointF(11, 30)
                };

                using Brush brush = new SolidBrush(
                    Color.FromArgb(245, 158, 11));

                using Pen pen = new Pen(
                    Color.FromArgb(255, 211, 80),
                    1F);

                g.FillPolygon(brush, triangle);
                g.DrawPolygon(pen, triangle);

                using Font exFont = new Font(
                    "Segoe UI Semibold",
                    12F,
                    FontStyle.Bold);

                using Brush exBrush = new SolidBrush(
                    Color.FromArgb(92, 64, 6));

                using StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(
                    "!",
                    exFont,
                    exBrush,
                    new RectangleF(17, 8, 14, 20),
                    sf);
            }
        }

        private static void DrawShield(
            Graphics g,
            float x,
            float y,
            float size,
            Color color)
        {
            PointF[] shield =
            {
                new PointF(x + size * 0.50F, y),
                new PointF(x + size * 0.87F, y + size * 0.18F),
                new PointF(x + size * 0.80F, y + size * 0.65F),
                new PointF(x + size * 0.50F, y + size),
                new PointF(x + size * 0.20F, y + size * 0.65F),
                new PointF(x + size * 0.13F, y + size * 0.18F)
            };

            using Brush brush = new SolidBrush(color);
            using Pen pen = new Pen(Color.White, 0.9F);

            g.FillPolygon(brush, shield);
            g.DrawPolygon(pen, shield);
        }

        private static GraphicsPath CreateRoundedRectangle(
            Rectangle rect,
            int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
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
