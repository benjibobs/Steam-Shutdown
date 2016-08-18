using System;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace Steam_Shutdown
{
    class SShutdown
    {

        static string installLoc = "~/.steam/steam/SteamApps/common";

        static long fileSize = 1;

        static void Main(string[] args)
        {

            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            int interval = 0;
            string title = "Steam Auto Shutdown - version " + version;
            int mode = 0;
            string[] customCmd = {"", "" };

            Console.Title = title;
            Console.ForegroundColor = ConsoleColor.Green;

            if (!isMono())
            {

                Console.WindowWidth += 30;
            }

            Console.WriteLine();
            centerConsoleLine(title);
            Console.ForegroundColor = ConsoleColor.Cyan;
            centerConsoleLine("https://github.com/benjibobs/Steam-Shutdown\n");

            Console.ForegroundColor = ConsoleColor.White;

            centerConsoleLine("This detects when Steam has finished downloading your stuff using the registry.");
            centerConsoleLine("It will shut down your computer when the download(s) are complete.\n");
            centerConsoleLine("THIS PROGRAM REQUIRES ADMIN/SUDO ACCESS\n");

            if (isMono())
            {
                selectLibrary();
            }

            while (interval < 1) //mode has been chosen
            {
                interval = getIntervalOrMode();
                   
                if(interval < 1)
                {
                    mode = interval;
                }
            }

            if (mode == -4)
            {
                while (customCmd[0] == "")
                {
                    Console.Write("\n> Please enter your custom command (without arguments): ");
                    customCmd[0] = Regex.Split(Console.ReadLine(), "(without arguments):")[0];
                }

                Console.Write("\n> Please enter your command's arguments (can be empty): ");
                customCmd[1] = Regex.Split(Console.ReadLine(), "(can be empty):")[0];
                Console.WriteLine("\n> '" + customCmd[0] + " " + customCmd[1] + "' will be run");
                
            }

            RegistryKey steamBase = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"SOFTWARE\Valve\Steam\Apps\");

            if (!isMono())
            {

                isDownloading_First(steamBase); //check if any app is actually being updated

            }
            while (updateCheck(steamBase))
            {
                centerConsoleLine("\n> Steam is downloading something! Sleeping for " + interval + " seconds...");
                Thread.Sleep(interval * 1000);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            centerConsoleLine("Steam has finished downloading! Waiting 10 seconds...");

            Thread.Sleep(10000);

            ProcessStartInfo psi;

            switch (mode)
            {
                case -1: //reboot
                    string rebootCmd = isMono() ? "/sbin/sudo" : "shutdown";
                    string rebootArgs = isMono() ? "/sbin/reboot -f" : "/r /t 0";
                    psi = new ProcessStartInfo(rebootCmd, rebootArgs);
                    break;
                case -2: //sleep
                    string sleepCmd = isMono() ? "/sbin/sudo" : "rundll32";
                    string sleepArgs = isMono() ? "pm-suspend" : "powrprof.dll,SetSuspendState 0,1,0";
                    psi = new ProcessStartInfo(sleepCmd, sleepArgs); //may not work on some systems (sends into a kind of hibernation)
                    break;
                case -3: //hibernate
                    string hibCmd = isMono() ? "/sbin/sudo" : "rundll32";
                    string hibArgs = isMono() ? "pm-hibernate" : "powrprof.dll,SetSuspendState";
                    psi = new ProcessStartInfo(hibCmd, hibArgs);
                    break;
                case -4: //custom
                    psi = new ProcessStartInfo(customCmd[0], customCmd[1]);
                    break;
                default: //uh oh.
                    string shutdownCmd = isMono() ? "/sbin/sudo" : "shutdown";
                    string shutdownArgs = isMono() ? "/sbin/shutdown -h now" : "/s /t 0";
                    psi = new ProcessStartInfo(shutdownCmd, shutdownArgs);
                    break;
            }
            
            psi.CreateNoWindow = true; //prevent popup
            psi.UseShellExecute = false;
            Process.Start(psi);

        }

        static void selectLibrary()
        {

            Console.Write("> Steam library location (default is \"~/.steam/steam/SteamApps/common\", press enter for default): ");

            string library = Console.ReadLine().Replace("> Steam library location (default is \"~/.steam/steam/SteamApps/common\": ", ""); //TODO: Better implementation

            if (String.IsNullOrEmpty(library.Trim()))
            {
                library = "~/.steam/steam/SteamApps/common";
            }

            installLoc = library;

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

        static bool isMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        ///   Gets interval or mode from user input
        ///   </summary>
        ///   <param name="modeChosen">Has a mode been previously chosen</param>
        ///   <returns>Returns interval or corresponding mode number</returns>
        static int getIntervalOrMode()
        {

            Console.Write("> Interval in seconds between checks (or type 'reboot', 'sleep', 'hibernate', or 'custom'): ");

            string[] inputArr = Console.ReadLine().Split(':');

            int interval = 0;
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
                    case "custom":
                        centerConsoleLine("\n> Custom mode activated! You will now have to choose a command.\n");
                        return -4;
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
                return getIntervalOrMode();

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

            if (!isMono())
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
            else
            {

                DirectoryInfo DirInfo = new DirectoryInfo(installLoc);

                long newFS = 0;

                foreach (FileInfo fi in DirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                   newFS = newFS + fi.Length;
                }

                if (newFS == fileSize)
                {
                    return false;
                }

                fileSize = newFS;

                return true;
            }
        }

    }
}
