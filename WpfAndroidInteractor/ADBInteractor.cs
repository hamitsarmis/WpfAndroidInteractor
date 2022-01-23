using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAndroidInteractor
{
    internal static class ADBInteractor
    {

        private static string executeProcess(string arguments)
        {
            var process = new Process();
            process.StartInfo.FileName = "Resources/Adb/adb.exe";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            process.WaitForExit();
            process.Close();

            return output;
        }

        public static string StartADB()
        {
            string result = "success";
            executeProcess("kill-server");
            executeProcess("start-server");
            string attachedDevices = executeProcess("devices");
            bool unAuthorized = false;
            if ((!(unAuthorized = attachedDevices.Contains("unauthorized"))) && attachedDevices.Trim('\r', '\n') != "List of devices attached")
            {
                executeProcess("reverse tcp:3001 tcp:3001");
                executeProcess("reverse tcp:3002 tcp:3002");
            }
            else
                result = unAuthorized ? "no authorized connected devices" : "no connected devices";
            return result;
        }

        public static void StopADB()
        {
            executeProcess("kill-server");
        }

    }
}
