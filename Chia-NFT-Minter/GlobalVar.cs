using System.Collections.Generic;

namespace Chia_NFT_Minter
{
    internal static class GlobalVar
    {
        static GlobalVar()
        {
            Load();
        }
        public static string CollectionUrl
        {
            get
            {
                return Settings["CollectionLink"];
            }
            set
            {
                Settings["CollectionLink"] = value;
                Save();
            }
        }
        private static FileInfo ConfigFile = new FileInfo("collectionConfiguration.txt");
        private static Dictionary<string, string> Settings = new Dictionary<string, string>();
        private static void InitConfig()
        {
            if (!ConfigFile.Exists)
            {
                Console.WriteLine("do you want to use a custom webserver as backup? (y/n)");
                if (Console.ReadLine().Trim().StartsWith("y"))
                {
                    Console.WriteLine("please provide collection base link");
                    Console.WriteLine("eg: https://nft.kryptomine.ch/crypto-crests/");
                    string collectionUri = Console.ReadLine().Trim();
                    if (!collectionUri.EndsWith("/")) collectionUri += '/';
                    Settings.Add("CollectionLink", collectionUri);
                }
                Save();
            }
        }
        private static void Save()
        {
            if (ConfigFile.Exists) ConfigFile.Delete();
            List<string> content = new List<string>();
            foreach (KeyValuePair<string,string> setting in Settings)
            {
                content.Add($"{setting.Key}={setting.Value}");
            }
            File.WriteAllLines(ConfigFile.FullName, content);
        }
        private static void Load()
        {
            InitConfig();
            string[] lines = File.ReadAllLines(ConfigFile.FullName);
            foreach (string line in lines)
            {
                string[] content = line.Split('=');
                Settings.Add(content[0].Trim(),content[1].Trim());
            }
        }
    }
}
