using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    public class ActivityLogForm : Form
    {
        private DataGridView dgvLog = null!;
        private Label lblCount = null!;
        private Button btnRefresh = null!;
        private Button btnClear = null!;
        private Button btnClose = null!;

        public ActivityLogForm()
        {
            InitializeUi();
            LoadLog();
        }

        private void InitializeUi()
        {
            Text = "Activity Log - Driver Manager";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1000, 560);
            Size = new Size(1150, 650);
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9F);

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = Color.White,
                Padding = new Padding(24, 14, 24, 10)
            };

            Label title = new Label
            {
                AutoSize = true,
                Text = "Activity Log",
                Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(24, 12)
            };

            Label description = new Label
            {
                AutoSize = true,
                Text = "View Driver Manager actions and results",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(26, 45)
            };

            header.Controls.Add(description);
            header.Controls.Add(title);

            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(24, 10, 24, 10)
            };

            btnRefresh = CreateButton("↻  Refresh", 100);
            btnRefresh.Location = new Point(24, 9);
            btnRefresh.Click += (_, _) => LoadLog();

            btnClear = CreateButton("Clear Log", 100);
            btnClear.Location = new Point(132, 9);
            btnClear.BackColor = Color.White;
            btnClear.Click += BtnClear_Click;

            lblCount = new Label
            {
                AutoSize = true,
                Text = "0 entries",
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(246, 18)
            };

            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(btnClear);
            toolbar.Controls.Add(lblCount);

            dgvLog = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 38,
                GridColor = Color.FromArgb(226, 232, 240),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 252)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            };

            dgvLog.Columns.Add(CreateColumn("Time", "Time", 125));
            dgvLog.Columns.Add(CreateColumn("Action", "Action", 90));
            dgvLog.Columns.Add(CreateColumn("Category", "Category", 120));
            dgvLog.Columns.Add(CreateColumn("Item", "Item", 190));
            dgvLog.Columns.Add(CreateColumn("Result", "Result", 85));
            dgvLog.Columns.Add(CreateColumn("Details", "Details", 230));
            dgvLog.Columns.Add(CreateColumn("User", "User", 100));
            dgvLog.Columns.Add(CreateColumn("Mode", "Mode", 100));

            Panel bottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.White,
                Padding = new Padding(24, 9, 24, 9)
            };

            btnClose = CreateButton("Close", 90);
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnClose.Location = new Point(1010, 9);
            btnClose.Click += (_, _) => Close();

            bottom.Controls.Add(btnClose);

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 0, 24, 12),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            content.Controls.Add(dgvLog);

            Controls.Add(content);
            Controls.Add(bottom);
            Controls.Add(toolbar);
            Controls.Add(header);
        }

        private static DataGridViewTextBoxColumn CreateColumn(
            string name,
            string header,
            float fillWeight)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = fillWeight,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private static Button CreateButton(string text, int width)
        {
            Button button = new Button
            {
                Text = text,
                Width = width,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private void LoadLog()
        {
            try
            {
                List<ActivityLogEntry> entries =
                    ActivityLogService.GetEntries(1000);

                dgvLog.Rows.Clear();

                foreach (ActivityLogEntry entry in entries)
                {
                    int index = dgvLog.Rows.Add(
                        entry.Time,
                        entry.Action,
                        entry.Category,
                        entry.Item,
                        entry.Result,
                        entry.Details,
                        entry.User,
                        entry.Mode);

                    if (entry.Result.Equals("Success", StringComparison.OrdinalIgnoreCase))
                    {
                        dgvLog.Rows[index].Cells["Result"].Style.ForeColor =
                            Color.FromArgb(22, 163, 74);
                    }
                    else if (entry.Result.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                    {
                        dgvLog.Rows[index].Cells["Result"].Style.ForeColor =
                            Color.FromArgb(220, 38, 38);
                    }
                    else if (entry.Result.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                    {
                        dgvLog.Rows[index].Cells["Result"].Style.ForeColor =
                            Color.FromArgb(180, 83, 9);
                    }
                }

                lblCount.Text = $"{entries.Count} entries";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "ไม่สามารถอ่าน Activity Log ได้\n\n" + ex.Message,
                    "Activity Log",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "ต้องการล้าง Activity Log ทั้งหมดหรือไม่?\n\n" +
                "ข้อมูลประวัติการทำงานที่ถูกลบจะไม่สามารถเรียกคืนได้",
                "ยืนยันการล้าง Log",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            ActivityLogService.Clear();
            ActivityLogService.Log(
                "CLEAR LOG",
                "Activity Log",
                "Activity Log",
                "Success",
                "Activity log was cleared");

            LoadLog();
        }
    }
}
