using System;
using System.Threading;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal static class ErrorHandler
    {
        private static bool installed;

        public static void Install()
        {
            if (installed)
                return;

            installed = true;

            // WinForms UI thread exceptions
            Application.ThreadException += OnThreadException;

            // Exceptions from non-UI threads
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnThreadException(
            object sender,
            ThreadExceptionEventArgs e)
        {
            ShowError(e.Exception);
        }

        private static void OnUnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowError(ex);
            }
        }

        public static void ShowError(
            Exception ex,
            string operation = "")
        {
            if (ex == null)
                return;

            string title = "Driver Manager";

            string message =
                string.IsNullOrWhiteSpace(operation)
                    ? "เกิดข้อผิดพลาดระหว่างการทำงาน"
                    : $"เกิดข้อผิดพลาดระหว่าง{operation}";

            string detail = GetSafeExceptionMessage(ex);

            string fullMessage =
                message +
                Environment.NewLine +
                Environment.NewLine +
                detail;

            try
            {
                MessageBox.Show(
                    fullMessage,
                    title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // ป้องกัน ErrorHandler ทำให้โปรแกรม Error ซ้ำ
            }
        }

        private static string GetSafeExceptionMessage(
            Exception ex)
        {
            if (ex == null)
                return "ไม่ทราบรายละเอียดข้อผิดพลาด";

            if (!string.IsNullOrWhiteSpace(ex.Message))
                return ex.Message;

            if (ex.InnerException != null)
                return GetSafeExceptionMessage(
                    ex.InnerException);

            return "ไม่ทราบรายละเอียดข้อผิดพลาด";
        }
    }
}