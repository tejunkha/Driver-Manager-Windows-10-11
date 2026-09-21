using System;
using System.Windows.Forms;
using Driver_Manager_Windows_10_11.Models;

namespace Driver_Manager_Windows_10_11
{
    public partial class PrinterPortDetailsForm : Form
    {
        private readonly PrinterPortModel _port;

        public PrinterPortDetailsForm(PrinterPortModel port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));

            InitializeComponent();
            LoadDetails();
        }

        private void LoadDetails()
        {
            txtPortName.Text = Safe(_port.Name);
            txtDescription.Text = Safe(_port.Description);
            txtHostAddress.Text = Safe(_port.PrinterHostAddress);
            txtPortNumber.Text = Safe(_port.PortNumber);
            txtProtocol.Text = Safe(_port.Protocol);
            txtSNMP.Text = Safe(_port.SNMPEnabled);
            txtPortMonitor.Text = Safe(_port.PortMonitor);
            txtComputer.Text = Safe(_port.ComputerName);
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
