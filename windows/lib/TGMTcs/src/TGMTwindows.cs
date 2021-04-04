using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TGMTcs
{
    class TGMTwindows
    {
        public static void SetStartupWithWindows(string programName, bool enable)
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (enable)
            {
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue(programName, Application.ExecutablePath);
            }
            else
            {
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue(programName, false);
            }

            rkApp.Close();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool IsStartupWithWindows(string programName)
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            bool result = rkApp.GetValue(programName) != null;
            rkApp.Close();
            return result;
        }
    }
}
