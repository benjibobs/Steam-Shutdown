using System;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Steam_Shutdown
{
    class SShutdown
    {
        /// <summary>
        /// Checks if any subkey has the value Updating
        /// </summary>
        /// <param name="key">Steam registry key base</param>
        /// <returns>Returns true if something is updating</returns>
        private static bool IsAnythingUpdating(RegistryKey key)
        {
            /* Based off of http://stackoverflow.com/a/2915990/5893567 */
            foreach (var sub in key.GetSubKeyNames())
            {
                var subKey = key.OpenSubKey(sub, true);
                var value = subKey.GetValue("Updating");
                if (value != null && (int)value == 1)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Outputs text to the console centered
        /// </summary>
        /// <param name="text">String to output</param>
        private static void WriteCenterText(string text)
        {
            Console.Write(new string(' ', (Console.WindowWidth - text.Length) / 2));
            Console.WriteLine(text);
        }


        /// <summary>
        /// Checks if a string consists of only minutes
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>Returns true if only numbers</returns>
        private static bool IsNumber(string str)
        {
            var reg = new Regex("^[0-9]*$");
            return reg.IsMatch(str);
        }


        /// <summary>
        /// Parses user input as int
        /// </summary>
        /// <param name="str">String to parse</param>
        /// <returns>Returns -1 if failed</returns>
        private static int GetUserInputAsInt(string str)
        {
            int value = -1;
            if (IsNumber(str))
                int.TryParse(str, out value);

            return value;
        }


        /// <summary>
        /// Entry point
        /// Checks if any apps are being updated
        /// </summary>
        /// <param name="args">No args</param>
        static void Main(string[] args)
        {
            /*Top text in console*/
            string productVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            Console.Title = $"Steam Auto Shutdown - v{productVersion}";
            WriteCenterText("");
            WriteCenterText("https://github.com/benjibobs/Steam-Shutdown\n");
            WriteCenterText("Your computer will be shut down once Steam finishes downloading your games");
            WriteCenterText("--------------------------------------------------------------------------");
            WriteCenterText("");

            /*How many minutes we should sleep inbetween checks*/
            var intervalMinutes = -1;

            /*What we should do after steam finishes updating*/
            int mode = -1;

            /*Get user interval input*/
            WriteCenterText("Enter the amount of minutes we should wait inbetween checks:");
            while ((intervalMinutes = GetUserInputAsInt(Console.ReadLine())) == -1)
                WriteCenterText("Incorrect input. Try again. (Example input: 5)");

            /*Get user mode input*/
            WriteCenterText("Enter mode. Shutdown = 1 | Reboot = 2 | Sleep = 3");
            while ((mode = GetUserInputAsInt(Console.ReadLine())) < 1 || mode > 3)
                WriteCenterText("Incorrect input. Try again. (Example input: 1)");

            /*Steam apps registry key*/
            var steamRegBase = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"SOFTWARE\Valve\Steam\Apps\");

            /*Ensure any updates are happening at all*/
            /*We'll repeat this check while user presses Enter key*/
            var enterCode = ConsoleKey.Enter;
            while (enterCode == ConsoleKey.Enter)
            {
                if (!IsAnythingUpdating(steamRegBase))
                {
                    WriteCenterText("No games are being updated/downloaded. Press enter to try again.");
                    enterCode = Console.ReadKey().Key;
                }
                else break;

                /*Quit program if users presses anything but enter*/
                if (enterCode != ConsoleKey.Enter)
                    return;
            }

            /*Keep loop alive as long as something is updating/downloading*/
            while (IsAnythingUpdating(steamRegBase))
            {
                WriteCenterText($"Steam is updating something! Checking again in {intervalMinutes} minutes.");
                Thread.Sleep(TimeSpan.FromMinutes(intervalMinutes));
            }

            WriteCenterText("Steam has finished downloading! Shutting down in 10 seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(10));

            /*Set up process start info*/
            var procInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            /*Switch between modes*/
            switch (mode)
            {
                case 1:
                    /*Shutdown*/
                    procInfo.FileName = "shutdown";
                    procInfo.Arguments = "/s /t 0";
                    break;
                case 2:
                    /*Restart*/
                    procInfo.FileName = "shutdown";
                    procInfo.Arguments = "/r /t 0";
                    break;
                case 3:
                    /*Sleep*/
                    procInfo.FileName = "rundll32";
                    procInfo.Arguments = "powrprof.dll,SetSuspendState 0,1,0";
                    break;
            }

            Process.Start(procInfo);
        }
    }
}
