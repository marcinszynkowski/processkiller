using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace processkiller
{
    class Program
    {
        private static string GetProcessUser(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                WindowsIdentity wi = new WindowsIdentity(processHandle);
                string user = wi.Name;
                return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        static void Main(string[] args)
        {
            Process[] proces = Process.GetProcessesByName("notepad");

            foreach (Process process in proces)
            {
                Console.WriteLine(process.ProcessName + " " + Program.GetProcessUser(process));
            }

            while (true)
            {
                Process[] procesy = Process.GetProcessesByName("notepad");

                try
                {
                    if (procesy.Length > 1)
                    {
                        if (procesy[0].StartTime < procesy[1].StartTime)
                        {
                            procesy[1].Kill();
                            foreach (Process process in proces)
                            {
                                Console.WriteLine(process.ProcessName + " " + Program.GetProcessUser(process) + " " + process.Id);
                            }
                        }
                        else
                        {
                            procesy[0].Kill();
                            foreach (Process process in proces)
                            {
                                Console.WriteLine(process.ProcessName + " " + Program.GetProcessUser(process) + " " + process.Id);
                            }
                        }
                    }
                }
                catch
                {

                }

            }
        }
    }
}
