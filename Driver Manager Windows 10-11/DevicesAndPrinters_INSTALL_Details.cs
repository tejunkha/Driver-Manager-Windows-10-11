using System;
using System.Drawing;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public sealed class DevicePrinterDetailsForm : Form
    {
        public DevicePrinterDetailsForm(
            PrintServerPrinterModel item)
        {
            Text = "Device & Printer Details";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(720, 560);
            MinimumSize = new Size(620, 480);
            BackColor = Color.White;

            var title =
                new Label
                {
                    Dock = DockStyle.Top,
                    Height = 58,
                    Padding = new Padding(22, 16, 20, 0),
                    Font = new Font(
                        "Segoe UI Semibold",
                        13F,
                        FontStyle.Bold),
                    ForeColor =
                        Color.FromArgb(15, 23, 42),
                    Text =
                        string.IsNullOrWhiteSpace(item.PrinterName)
                            ? "Device Details"
                            : item.PrinterName
                };

            var grid =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(22, 8, 22, 22),
                    ColumnCount = 2,
                    RowCount = 1,
                    AutoScroll = true
                };

            grid.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    170F));

            grid.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            AddRow(grid, "Category", item.Category);
            AddRow(grid, "Manufacturer", item.Manufacturer);
            AddRow(grid, "Status", item.Status);
            AddRow(grid, "State", item.State);
            AddRow(grid, "Present", item.Present ? "Yes" : "No");
            AddRow(grid, "Driver", item.DriverName);
            AddRow(grid, "Port", item.PortName);
            AddRow(grid, "Port Monitor", item.PortMonitor);
            AddRow(grid, "Share Name", item.ShareName);
            AddRow(grid, "Device ID", item.DeviceId);
            AddRow(grid, "Class GUID", item.ClassGuid);
            AddRow(grid, "Source", item.Source);
            AddRow(grid, "Jobs", item.JobCount);
            AddRow(grid, "Reason", item.Reason);

            Controls.Add(grid);
            Controls.Add(title);
        }

        private static void AddRow(
            TableLayoutPanel grid,
            string label,
            string value)
        {
            int row =
                grid.RowCount++;

            grid.RowStyles.Add(
                new RowStyle(
                    SizeType.AutoSize));

            var key =
                new Label
                {
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0, 7, 10, 7),
                    Font = new Font(
                        "Segoe UI Semibold",
                        9.5F,
                        FontStyle.Bold),
                    ForeColor =
                        Color.FromArgb(71, 85, 105),
                    Text = label
                };

            var val =
                new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    MinimumSize = new Size(0, 31),
                    Text = value ?? "",
                    Font = new Font(
                        "Segoe UI",
                        9.5F),
                    ForeColor =
                        Color.FromArgb(15, 23, 42)
                };

            grid.Controls.Add(
                key,
                0,
                row);

            grid.Controls.Add(
                val,
                1,
                row);
        }
    }
}
