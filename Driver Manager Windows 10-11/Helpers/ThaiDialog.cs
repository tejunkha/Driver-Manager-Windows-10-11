using System;
using System.Drawing;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11.Helpers
{
    internal static class ThaiDialog
    {
        public static DialogResult Confirm(string title, string message)
        {
            using Form form = CreateBaseForm(title);
            TableLayoutPanel layout = CreateLayout();

            layout.Controls.Add(CreateMessageLabel(message), 0, 0);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };

            Button no = CreateButton("ไม่ใช่", 95);
            no.DialogResult = DialogResult.No;
            no.BackColor = Color.FromArgb(226, 232, 240);
            no.ForeColor = Color.FromArgb(15, 23, 42);

            Button yes = CreateButton("ใช่", 95);
            yes.DialogResult = DialogResult.Yes;
            yes.BackColor = Color.FromArgb(220, 38, 38);
            yes.ForeColor = Color.White;

            buttons.Controls.Add(no);
            buttons.Controls.Add(yes);
            layout.Controls.Add(buttons, 0, 1);

            form.Controls.Add(layout);
            form.AcceptButton = no;
            form.CancelButton = no;
            form.Shown += (_, _) => no.Focus();

            return form.ShowDialog();
        }

        public static void Info(string title, string message) => Show(title, message, Color.FromArgb(37, 99, 235));
        public static void Warning(string title, string message) => Show(title, message, Color.FromArgb(245, 158, 11));
        public static void Error(string title, string message) => Show(title, message, Color.FromArgb(220, 38, 38));

        private static void Show(string title, string message, Color accent)
        {
            using Form form = CreateBaseForm(title);
            TableLayoutPanel layout = CreateLayout();
            layout.Controls.Add(CreateMessageLabel(message), 0, 0);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };

            Button ok = CreateButton("ตกลง", 95);
            ok.BackColor = accent;
            ok.ForeColor = Color.White;
            ok.DialogResult = DialogResult.OK;

            buttons.Controls.Add(ok);
            layout.Controls.Add(buttons, 0, 1);
            form.Controls.Add(layout);
            form.AcceptButton = ok;
            form.CancelButton = ok;
            form.Shown += (_, _) => ok.Focus();
            form.ShowDialog();
        }

        private static Form CreateBaseForm(string title)
        {
            return new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                ClientSize = new Size(520, 230)
            };
        }

        private static TableLayoutPanel CreateLayout()
        {
            return new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(22),
                ColumnCount = 1,
                RowCount = 2,
                RowStyles =
                {
                    new RowStyle(SizeType.Percent, 100F),
                    new RowStyle(SizeType.Absolute, 48F)
                }
            };
        }

        private static Label CreateMessageLabel(string message)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Text = message,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 10F)
            };
        }

        private static Button CreateButton(string text, int width)
        {
            return new Button
            {
                Text = text,
                Width = width,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI Semibold", 9F),
                Cursor = Cursors.Hand,
                Margin = new Padding(6, 0, 0, 0)
            };
        }
    }
}
