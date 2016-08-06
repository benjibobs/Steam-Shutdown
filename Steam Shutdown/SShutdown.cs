using System;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;

namespace Steam_Shutdown
{
    class SShutdown
    {

        public static string version = "1.1";

        static void Main(string[] args)
        {

            int interval = 300;
            string title = "Steam Auto Shutdown - version " + version;
            bool restart = false;

            Console.Title = title;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine();
            centerConsoleLine(title);
            Console.ForegroundColor = ConsoleColor.Cyan;
            centerConsoleLine("https://github.com/benjibobs/Steam-Shutdown");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;

            centerConsoleLine("This detects when Steam has finished downloading your stuff using the registry.");
            centerConsoleLine("It will shut down your computer when the download(s) are complete.");
            Console.WriteLine(); ;

            Console.ForegroundColor = ConsoleColor.White;

            interval = getInterval();

            RegistryKey steamBase = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"SOFTWARE\Valve\Steam\Apps\");

            isDownloading_First(steamBase); //check if any app is actually being updated

            while (updateCheck(steamBase))
            {
                Console.WriteLine("");
                centerConsoleLine("Steam is downloading something! Sleeping for " + interval + " seconds...");
                Thread.Sleep(interval * 1000);

            }

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            centerConsoleLine("Steam has finished downloading! Shutting down in 10 seconds...");

            Thread.Sleep(10000);

            ProcessStartInfo psi;

            if (restart)
            {
                psi = new ProcessStartInfo("shutdown", "/r /t 0");
            } else {
                psi = new ProcessStartInfo("shutdown", "/s /t 0");
            }

            
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);

        }

        

        static void centerConsoleLine(string text)
        {
            Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop);
            Console.WriteLine(text);
        }

        static void isDownloading_First(RegistryKey key)
        {
            if (!updateCheck(key))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                centerConsoleLine("No games or updates being downloaded.");
                centerConsoleLine("Start the download/update and press ENTER to try again");
                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                isDownloading_First(key);
                return;
            }
        }

        static int getInterval()
        {

            Console.Write("Interval in seconds between checks (recommended is 300): ");
            Console.ForegroundColor = ConsoleColor.Cyan;

            string[] input = Console.ReadLine().Split(':');

            int interval;
            bool success = Int32.TryParse(input[(input.Length - 1)].Trim(' '), out interval);

            if (!success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                centerConsoleLine("That is not a valid number!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                getInterval();
            }

            return interval;


        }

        static bool updateCheck(RegistryKey key)
        {

            /* Based off of http://stackoverflow.com/a/2915990/5893567 */
            foreach (string sub in key.GetSubKeyNames())
            {
                RegistryKey local = Registry.Users;
                local = key.OpenSubKey(sub, true);
               try
               {
                    object updating = local.GetValue("Updating");
                    if ((int)updating == 1)
                    {
                        return true;
                    }
              }
               catch (Exception)
              {
                    //not all apps having Updating key ... no point having anything here
               }
            }
            return false;
        }

    }
}
