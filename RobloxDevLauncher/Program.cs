using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Web;

namespace RobloxDevLauncher
{
    /// <summary>
    /// Interaction logic for RobloxDevLauncher
    /// </summary>
    class Program
    {
        [DllImport("user32.dll")]
        public static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType);
        private static readonly uint MB_OK = (uint)0x00000000L;

        private static readonly byte[] MagicHeader = new byte[] { 0x72, 0x6f, 0x62, 0x6c, 0x6f, 0x78, 0x2d, 0x70, 0x6c, 0x61, 0x79, 0x65, 0x72, 0x3a, 0x31, 0x2b, 0x6c, 0x61, 0x75, 0x6e, 0x63, 0x68, 0x6d, 0x6f, 0x64, 0x65, 0x3a, 0x70, 0x6c, 0x61, 0x79 }; // `roblox-player:1+launchmode:play`
        private static readonly string ApplicationName = "RobloxPlayerLauncher"; // Must be in sync with project output assembly filename but without the file type
        private static string[] ApplicationArguments;
        
        /// <summary>
        /// Defines the Application entrypoint
        /// </summary>
        /// <param name="args">Program arguments</param>
        static void Main(string[] args)
        {
            ApplicationArguments = args;
            StartRoblox();
        }

        /// <summary>
        /// Fails the launching process by displaying a message box with the specified message, and closes the application.
        /// </summary>
        /// <param name="message">Error message</param>
        private static void FailAndClose(string message)
        {
            if (message == String.Empty || message == null)
            {
                throw new ArgumentException("Parameter cannot be empty", "message");
            }
            message = String.Format("Failed to start Roblox: {0}", message);

            MessageBoxA((IntPtr)null, message, ApplicationName, MB_OK);
            Environment.Exit(0);
        }

        /// <summary>
        /// Launcher entrypoint
        /// </summary>
        private static void StartRoblox()
        {
            // Convert the magic header to a string
            // Our "magic header" is a rudimentary way of pre-determining if our arguments are invalid / corrupt
            string signature;
            try
            {
                signature = Encoding.ASCII.GetString(MagicHeader);
            }
            catch (Exception e)
            {
                throw e;
            }

            // Verify our arguments
            if (ApplicationArguments.Length != 1) { FailAndClose("No arguments or too many arguments specified"); }
            if (ApplicationArguments[0].Substring(0, signature.Length) != signature) { FailAndClose("Invalid arguments specified"); }

            // Parse the arguments
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            try
            {
                var items = ApplicationArguments[0].Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(arg => arg.Split(new[] { ':' }));

                foreach (var item in items)
                {
                    arguments.Add(item[0], item[1]);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            // Create our arguments
            string finalArguments;
            finalArguments = String.Format("--play -a https://wwww.roblox.com/Login/Negotiate.ashx -t {0} -j {1} -b {2} --launchtime={3} --rloc {4} --gloc {5}", arguments["gameinfo"], HttpUtility.UrlDecode(arguments["placelauncherurl"]), arguments["browsertrackerid"], arguments["launchtime"], arguments["robloxLocale"], arguments["gameLocale"]);

            // Do we have a RobloxPlayerBeta in our local folder? We need that in order to launch.
            string player = String.Format("{0}{1}RobloxPlayerBeta.exe", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Path.DirectorySeparatorChar);
            if (!File.Exists(player))
            {
                FailAndClose("No RobloxPlayerBeta in current path" + player);
            }

            // Launch it
            Process client = new Process();
            client.StartInfo.FileName = player;
            client.StartInfo.Arguments = finalArguments;
            client.StartInfo.UseShellExecute = true;
            client.Start();

            // Bye
            Environment.Exit(0);
        }
    }
}
