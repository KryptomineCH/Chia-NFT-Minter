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
                string[] lines = File.ReadAllLines(SettingsFile.FullName);
                foreach (string line in lines)
                {
                    string[] keyValue = line.Split('=');
                    Properties.Add(keyValue[0], keyValue[1]);
                }
             }
        }
        private static FileInfo SettingsFile = new FileInfo("settings.txt");
        private static Dictionary<string, string> Properties = new Dictionary<string, string>();
        public static void SetProperty(string name, string value)
        {
            Properties[name] = value;
            List<string> lines = new List<string>();
            foreach (KeyValuePair<string,string> property in Properties)
            {
                lines.Add($"{property.Key}={property.Value}");
            }
            File.WriteAllLines(SettingsFile.FullName, lines);
        }
        public static string GetProperty(string name)
        {
            if (!Properties.ContainsKey(name)) return null;
            return Properties[name];
        }
        
    }
}
