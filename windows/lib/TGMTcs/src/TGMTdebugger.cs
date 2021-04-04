using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TGMTcs
{
    public class TGMTdebugger
    {
        delegate int RecoveryDelegate(IntPtr parameter);

        [DllImport("kernel32.dll")]
        private static extern int RegisterApplicationRecoveryCallback(
                RecoveryDelegate recoveryCallback,
                IntPtr parameter,
                uint pingInterval,
                uint flags);

        [DllImport("kernel32.dll")]
        private static extern void ApplicationRecoveryFinished(bool success);

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void RegisterForRecovery()
        {
            var callback = new RecoveryDelegate(p =>
            {
                Recover();
                ApplicationRecoveryFinished(true);
                return 0;
            });

            var interval = 100U;
            var flags = 0U;

            RegisterApplicationRecoveryCallback(callback, IntPtr.Zero, interval, flags);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void Recover()
        {
            //do the recovery and cleanup
            Process.Start(Assembly.GetEntryAssembly().Location);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void RegisterHandleCrash()
        {
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler((s, e) =>
                {
                    Recover();
                    Environment.Exit(1);
                });

            RegisterForRecovery();
        }
        
    }
}
