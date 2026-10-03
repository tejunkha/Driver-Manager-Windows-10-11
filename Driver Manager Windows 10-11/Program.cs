using Driver_Manager_Windows_10_11.Helpers;
using System;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // ต้องตั้งค่าก่อนสร้าง Control ใด ๆ
            Application.SetUnhandledExceptionMode(
                UnhandledExceptionMode.CatchException);

            ErrorHandler.Install();

            ApplicationConfiguration.Initialize();

            Application.Run(new MainForm());
        }
    }
}