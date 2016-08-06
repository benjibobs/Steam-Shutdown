using System;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Steam_Shutdown
{
    class Program
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
        /// Entry point
        /// Checks if any apps are being updated
        /// </summary>
        /// <param name="args">No args</param>
        static void Main(string[] args)
        {
            Console.Title = $"Steam Auto Shutdown - v{Application.ProductVersion}";
            WriteCenterText("");
            WriteCenterText("https://github.com/benjibobs/Steam-Shutdown\n");
            WriteCenterText("Your computer will be shut down once Steam finishes downloading your games");
            WriteCenterText("--------------------------------------------------------------------------");
            WriteCenterText("");

            /*User interval input*/
            WriteCenterText("Enter the amount of minutes we should wait inbetween checks:");

            var input = string.Empty;
            while (!IsNumber((input = Console.ReadLine())))
                WriteCenterText("Incorrect input. Try again. (Example input: 5)");

            /*Parse user input*/
            var intervalMinutes = 0;
            if (!int.TryParse(input, out intervalMinutes))
            {
                WriteCenterText("Could not parse interval input. Please type in only numbers. Exiting...");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                return;
            }

            /*Steam apps registry key*/
            var steamRegBase = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"SOFTWARE\Valve\Steam\Apps\");

            /*Ensure any updates are happening at all*/
            if (!IsAnythingUpdating(steamRegBase))
            {
                WriteCenterText("No games are being updated/downloaded. Exiting ...");
                Thread.Sleep(TimeSpan.FromSeconds(2));
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

            /*Initialize a shutdown*/
            Process.Start(new ProcessStartInfo()
            {
                FileName = "shutdown",
                Arguments = "/s /t 0",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
    }
}
