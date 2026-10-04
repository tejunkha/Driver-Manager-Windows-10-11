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

            Font = new Font("Segoe UI", 8.5F);
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

            const int radius = 10;

            Color background = isAdministrator
                ? Color.FromArgb(27, 92, 58)
                : Color.FromArgb(91, 72, 25);

            Color border = isAdministrator
                ? Color.FromArgb(66, 151, 101)
                : Color.FromArgb(166, 133, 53);

            Color subtitleColor = isAdministrator
                ? Color.FromArgb(198, 220, 205)
                : Color.FromArgb(232, 212, 164);

            using (GraphicsPath path = CreateRoundedRectangle(rect, radius))
            using (SolidBrush brush = new SolidBrush(background))
            using (Pen pen = new Pen(border, 1F))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            using (SolidBrush dot = new SolidBrush(
                isAdministrator
                    ? Color.FromArgb(82, 190, 123)
                    : Color.FromArgb(215, 169, 64)))
            {
                g.FillEllipse(dot, 13, 13, 9, 9);
            }

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
                ? "Administrator mode"
                : "Standard user mode";

            using Brush titleBrush = new SolidBrush(Color.White);
            using Brush subtitleBrush = new SolidBrush(subtitleColor);

            g.DrawString(
                title,
                titleFont,
                titleBrush,
                new PointF(31, 9));

            g.DrawString(
                subtitle,
                subtitleFont,
                subtitleBrush,
                new PointF(31, 30));
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
