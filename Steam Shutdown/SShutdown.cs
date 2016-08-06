using System;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace Steam_Shutdown
{
    class SShutdown
    {

        public static string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        static void Main(string[] args)
        {

            int interval = 300;
            string title = "Steam Auto Shutdown - version " + version;
            int mode = 0;

            Console.Title = title;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WindowWidth += 50;

            Console.WriteLine();
            centerConsoleLine(title);
            Console.ForegroundColor = ConsoleColor.Cyan;
            centerConsoleLine("https://github.com/benjibobs/Steam-Shutdown");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;

            centerConsoleLine("This detects when Steam has finished downloading your stuff using the registry.");
            centerConsoleLine("It will shut down your computer when the download(s) are complete.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;

            interval = getIntervalOrMode(false);

            if (interval < 0)
            {
                mode = interval;
                interval = getIntervalOrMode(true);
            }     

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

            switch (mode)
            {
                case -1:
                    psi = new ProcessStartInfo("shutdown", "/r /t 0");
                    break;
                case -2:
                    psi = new ProcessStartInfo("rundll32", "powrprof.dll,SetSuspendState 0,1,0");
                    break;
                default:
                    psi = new ProcessStartInfo("shutdown", "/s /t 0");
                    break;
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
                centerConsoleLine("No updates or downloads detected.");
                centerConsoleLine("Start the download/update and press ENTER to try again");
                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                isDownloading_First(key);
                return;
            }
        } 

        static int getIntervalOrMode(bool modeChosen)
        {

            Console.Write("> Interval in seconds between checks (type 'reboot' here to reboot, or 'sleep' to sleep): ");
            Console.ForegroundColor = ConsoleColor.Cyan;

            string[] inputArr = Console.ReadLine().Split(':');

            int interval;
            string input = inputArr[(inputArr.Length - 1)].Trim(' ');
            bool success = Int32.TryParse(inputArr[(inputArr.Length - 1)].Trim(' '), out interval);

            if (!success || interval < 1)
            {

                if (input.ToLower() == "reboot" && !modeChosen)
                {

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine();
                    centerConsoleLine("Reboot mode activated! You will now have to choose an actual interval.");
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.White;

                    return -1;

                } else if (input.ToLower() == "sleep" && !modeChosen) {

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine();
                    centerConsoleLine("Sleep mode activated! You will now have to choose an actual interval.");
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.White;

                    return -2;

                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                centerConsoleLine("That is not a valid interval!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                getIntervalOrMode(modeChosen);
            }

            return interval;


        }


        /* <summary>
           Checks if any subkey has the value Updating
           </summary>
           <param name="key">Steam registry key base</param>
           <returns>Returns true if something is updating</returns> */
        static bool updateCheck(RegistryKey key)
        {

            /* Based off of http://stackoverflow.com/a/2915990/5893567 */
            foreach (string sub in key.GetSubKeyNames())
            {
                RegistryKey local = Registry.Users;
                local = key.OpenSubKey(sub, true);

                object updating = local.GetValue("Updating");
                if (updating != null && (int)updating == 1)
                {
                    return true;
                }

            }
            return false;
        }

    }
}
