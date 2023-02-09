using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Minter_UI.Tasks_NS
{
    internal static class CheckVCRedis
    {
        /// <summary>
        /// This code checks if the Visual C++ runtime is installed, 
        /// and if it is not, it downloads and runs the VC_redist.x64.exe installer.
        /// </summary>
        internal static void CheckInstall()
        {
            if (File.Exists("VC_redist_install.x64.exe"))
            {
                File.Delete("VC_redist_install.x64.exe");
            }
            bool vcRuntimeInstalled = false;
            //The registry key for the Visual C++ runtime
            string vcRuntimeKey = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(vcRuntimeKey))
            {
                //Checks if the key exists in the registry
                if (key != null)
                {
                    //Gets the version property of the runtime
                    var versionProperty = key.GetValue("version");
                    if (versionProperty != null)
                    {
                        var versionRegex = new Regex(@"(\d+(\.\d+)*)");
                        var match = versionRegex.Match(versionProperty.ToString());
                        if (match.Success)
                        {
                            Version version = new Version(match.Value);
                            Version minVersion = new Version("14.34.27820");
                            //checks if the version of the runtime is lower than the required version
                            if (version < minVersion)
                            {
                                Install();
                            }
                        }
                    }
                }
                else
                {
                    // not installed at all
                    Install();
                }
            }
        }
        /// <summary>
        ///  initiates installation of vc redistributable
        /// </summary>
        private static void Install()
        {
            // download and install start
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile("https://aka.ms/vs/17/release/vc_redist.x64.exe", "VC_redist_install.x64.exe");
            }
            Process.Start("VC_redist_install.x64.exe");
        }
    }
}
