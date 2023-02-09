using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Minter_UI.Settings_NS
{
    /// <summary>
    /// the settings class contains user configuration and some other settigns
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// initialize settings (load if existing, otherwise create settings file with defaults)
        /// </summary>
        static Settings()
        {
            if (SettingsFile.Exists)
            {
                // settings file does exist (load)
                lock (FileLock)
                {
                    string text = File.ReadAllText(SettingsFile.FullName);
                    try
                    {
                        Settings_Object? json = JsonSerializer.Deserialize<Settings_Object>(text);
                        if (json != null)
                        {
                            All = json;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"STOP: Settings couldnt be loaded! did you change settingt manually?{Environment.NewLine}" +
                            $"Exception message: {ex.Message}");
                    }
                }
            }
            else
            {
                // settings file does not exist, create one with defaults
                All = new Settings_Object();
                Save();
            }
        }
        /// <summary>
        /// path to the settigns file
        /// </summary>
        private static FileInfo SettingsFile = new FileInfo("settings.json");
        /// <summary>
        /// locks the settings file for a thread safe manner
        /// </summary>
        /// <remarks>
        /// only locks inside the application. the settings can still be edited with an editor of choice which might caus eissues
        /// </remarks>
        private static object FileLock = new object();
        /// <summary>
        /// contains all settigns
        /// </summary>
        internal static Settings_Object All = new Settings_Object();
        /// <summary>
        /// saves the settings object (All) to disk
        /// </summary>
        internal static void Save()
        {
            lock (FileLock)
            {
                if (SettingsFile.Exists)
                {
                    SettingsFile.Delete();
                }
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                string testText = JsonSerializer.Serialize(All, options: options);
                Encoding utf8WithoutBom = new UTF8Encoding(false); // IMPORTANT: no bom
                File.WriteAllText(SettingsFile.FullName, testText, utf8WithoutBom);
            }
        }
    }
}
