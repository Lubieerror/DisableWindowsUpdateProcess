using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Security.Principal;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.ServiceProcess;

namespace DisableWindowsUpdate {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            if (UACSecurity.IsAdministrator())
                Console.WriteLine("Odpalono z uprawnieniami administracyjnymi!");
            else {
                UACSecurity.RunProcess(System.AppDomain.CurrentDomain.BaseDirectory +"DisableWindowsUpdate.exe", null);
                //Console.WriteLine(System.AppDomain.CurrentDomain.BaseDirectory);
                Thread.Sleep(1000 * 10);
                return;
            }
            ServiceController sc = new ServiceController("wuauserv");
            while (true) {
                Console.WriteLine(sc.Status.ToString());
                if (sc.Status.ToString().Equals("Running")) {
                    sc.Stop();
                    sc.Refresh();
                    Thread.Sleep(1000 * 10);
                    Console.WriteLine(sc.Status.ToString());
                    Console.Beep();
                }
                Thread.Sleep(1000 * 60 * 15);
            }
        }

        public static class UACSecurity {
            public static bool IsAdministrator() {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                if (null != identity) {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                return false;
            }

            public static Process RunProcess(string name, string arguments) {
                string path = Path.GetDirectoryName(name);

                if (String.IsNullOrEmpty(path)) {
                    path = Environment.CurrentDirectory;
                }

                ProcessStartInfo info = new ProcessStartInfo {
                    UseShellExecute = true,
                    WorkingDirectory = path,
                    FileName = name,
                    Arguments = arguments
                };

                if (!IsAdministrator()) {
                    info.Verb = "runas";
                }

                try {
                    return Process.Start(info);
                }

                catch (Win32Exception ex) {
                    Trace.WriteLine(ex);
                }

                return null;
            }
        }
    }
}
