using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Minter_UI
{
    /// <summary>
    /// class to load and save settings. Note settings are collection-wide
    /// </summary>
    internal class Settings
    {
        static Settings()
        {
            if (SettingsFile.Exists)
            {
                string[] lines;
                lock (FileLock)
                {
                    lines = File.ReadAllLines(SettingsFile.FullName);
                    foreach (string line in lines)
                    {
                        string[] keyValue = line.Split('=');
                        Properties[keyValue[0]] = keyValue[1];

                    }
                    if (Properties.ContainsKey("CaseSensitiveFilehandling"))
                    {
                        string prop = Properties["CaseSensitiveFilehandling"];
                        GlobalVar.CaseSensitiveFilehandling = bool.Parse(prop);
                    }
                    else
                    {
                        Properties["CaseSensitiveFilehandling"] = false.ToString();
                        GlobalVar.CaseSensitiveFilehandling = false;
                    }
                }
            }
        }
        /// <summary>
        /// the location of the settings file
        /// </summary>
        private static FileInfo SettingsFile = new FileInfo("settings.txt");
        /// <summary>
        /// all settings/properties
        /// </summary>
        private static ConcurrentDictionary<string, string> Properties = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// in an attempt to circumvent accessviolationException
        /// </summary>
        private static object FileLock = new object();
        /// <summary>
        /// sets a property/setting
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetProperty(string name, string value)
        {
            lock (FileLock)
            {
                Properties[name] = value;
                List<string> lines = new List<string>();
                foreach (KeyValuePair<string, string> property in Properties)
                {
                    lines.Add($"{property.Key}={property.Value}");
                }
                File.WriteAllLines(SettingsFile.FullName, lines);
            }
        }
        /// <summary>
        /// gets a certain property/setting
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetProperty(string name)
        {
            if (!Properties.ContainsKey(name)) return null;
            return Properties[name];
        }
        /// <summary>
        /// this empty class is used to load the settingsfile. <br/>
        /// The issue arises from a static constructor which cannot be called manually.<br/>
        /// for example, if the program calls a var in globalvar, the settings might never have been initialized and are hence empty
        /// </summary>
        public static void Initialize()
        {

        }
    }
}
