using System.Security.Principal;
using System.Windows.Forms;

namespace Driver_Manager_Windows_10_11
{
    internal static class AdminHelper
    {
        public static bool IsAdministrator()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static string GetModeText()
        {
            return IsAdministrator()
                ? "Mode: Administrator"
                : "Mode: Standard User";
        }

        public static bool RequireAdministrator(
            IWin32Window owner,
            string actionText)
        {
            if (IsAdministrator())
                return true;

            ThaiDialog.Warning(
                "ต้องใช้สิทธิ์ Administrator",
                "การดำเนินการนี้ต้องใช้สิทธิ์ผู้ดูแลระบบ\n\n" +
                $"การดำเนินการ: {actionText}\n\n" +
                "กรุณาปิดโปรแกรม แล้วเปิด Driver Manager ด้วย Run as administrator จากนั้นลองใหม่อีกครั้ง");

            return false;
        }
    }
}
