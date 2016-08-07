﻿using System;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace Steam_Shutdown
{
    class SShutdown
    {

        static void Main(string[] args)
        {
            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            int interval = -1;
            string title = "Steam Auto Shutdown - version " + version;
            int mode = 0;

            Console.Title = title;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WindowWidth += 30;

            Console.WriteLine();
            centerConsoleLine(title);
            Console.ForegroundColor = ConsoleColor.Cyan;
            centerConsoleLine("https://github.com/benjibobs/Steam-Shutdown\n");

            Console.ForegroundColor = ConsoleColor.White;

            centerConsoleLine("This detects when Steam has finished downloading your stuff using the registry.");
            centerConsoleLine("It will shut down your computer when the download(s) are complete.\n");

            while (interval < 1) //mode has been chosen
            {
                mode = interval;
                interval = getIntervalOrMode();
            }

            RegistryKey steamBase = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"SOFTWARE\Valve\Steam\Apps\");

            isDownloading_First(steamBase); //check if any app is actually being updated

            while (updateCheck(steamBase))
            {

                centerConsoleLine("\n> Steam is downloading something! Sleeping for " + interval + " seconds...");
                Thread.Sleep(interval * 1000);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            centerConsoleLine("Steam has finished downloading! Shutting down in 10 seconds...");

            Thread.Sleep(10000);

            ProcessStartInfo psi;

            switch (mode)
            {
                case -1:
                    psi = new ProcessStartInfo("shutdown", "/r /t 0");
                    break;
                case -2:
                    psi = new ProcessStartInfo("rundll32", "powrprof.dll,SetSuspendState 0,1,0"); //may not work on some systems (sends into a kind of hibernation)
                    break;
                case -3:
                    psi = new ProcessStartInfo("rundll32", "powrprof.dll,SetSuspendState");
                    break;
                default:
                    psi = new ProcessStartInfo("shutdown", "/s /t 0");
                    break;
            }
            
            psi.CreateNoWindow = true; //prevent popup
            psi.UseShellExecute = false;
            Process.Start(psi);

        }

        /// <summary>
        ///   Prints a centered message to Console
        ///   </summary>
        ///   <param name="text">Text to print</param>
        static void centerConsoleLine(string text)
        {
            Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop);
            Console.WriteLine(text);
        }

        /// <summary>
        ///   RecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursionRecursion
        ///   </summary>
        ///   <param name="key">Steam registry base key</param>
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

                isDownloading_First(key); //NOTE: Temporary workaround http://i.imgur.com/3eDjSGm.png

                return;
            }
        }

        /// <summary>
        ///   Gets interval or mode from user input
        ///   </summary>
        ///   <param name="modeChosen">Has a mode been previously chosen</param>
        ///   <returns>Returns interval or corresponding mode number</returns>
        static int getIntervalOrMode()
        {

            Console.Write("> Interval in seconds between checks (or type 'reboot', 'sleep', or 'hibernate' to change modes): ");

            string[] inputArr = Console.ReadLine().Split(':');

            int interval = 300;
            string input = inputArr[(inputArr.Length - 1)].Trim(' ');
            bool success = Int32.TryParse(inputArr[(inputArr.Length - 1)].Trim(' '), out interval);

            if (!success || interval < 1)
            {

                switch (input.ToLower())
                {
                    case "reboot":
                        centerConsoleLine("\n> Reboot mode activated! You will now have to choose an actual interval.\n");
                        return -1;
                    case "sleep":
                        centerConsoleLine("\n> Sleep mode activated! You will now have to choose an actual interval.\n");
                        return -2;
                    case "hibernate":
                        centerConsoleLine("\n> Hibernate mode activated! You will now have to choose an actual interval.\n");
                        return -3;
                    case "shutdown":
                        centerConsoleLine("\n> Shutdown mode activated! You will now have to choose an actual interval.\n");
                        return 0;
                    default:
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                centerConsoleLine("That is not a valid interval!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                getIntervalOrMode();

            }

            return interval;


        }

        /// <summary>
        ///   Checks if any subkey has the value Updating
        ///   </summary>
        ///   <param name="key">Steam registry key base</param>
        ///   <returns>Returns true if something is updating</returns>
        static bool updateCheck(RegistryKey key)
        {

            /* http://stackoverflow.com/a/2915990/5893567 */
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
