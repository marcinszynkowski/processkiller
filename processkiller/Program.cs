using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace processkiller
{
    class Program
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        static void Main(string[] args)
        {
            CheckProcess();
            ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            startWatch.Start();
            ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
            stopWatch.Start();
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
            startWatch.Stop();
            stopWatch.Stop();
        }

        static void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Process stopped: {0}", e.NewEvent.Properties["ProcessName"].Value);

            //CheckProcess();
        }

        static void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Process started: {0}", e.NewEvent.Properties["ProcessName"].Value);

            CheckProcess();
        }

        private static void CheckProcess()
        {
            Dictionary<string, string> UserProcess = new Dictionary<string, string>();
            Process[] pname = Process.GetProcessesByName("notepad");
            pname.OrderBy(pnames => pnames.StartTime);
            foreach (Process process in pname)
            {
                try
                {
                    if (UserProcess.ContainsKey(GetProcessUser(process))) process.Kill();
                    else
                        UserProcess.Add(GetProcessUser(process), process.ProcessName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }



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


    }
}
