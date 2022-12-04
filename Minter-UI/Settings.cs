using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Minter_UI
{
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
        private static FileInfo SettingsFile = new FileInfo("settings.txt");
        private static ConcurrentDictionary<string, string> Properties = new ConcurrentDictionary<string, string>();
        private static object FileLock = new object();
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
        public static void Initialize()
        {

        }
        public static string GetProperty(string name)
        {
            if (!Properties.ContainsKey(name)) return null;
            return Properties[name];
        }
        
    }
}
