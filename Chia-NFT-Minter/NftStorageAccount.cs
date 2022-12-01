using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chia_NFT_Minter
{
    internal static class NftStorageAccount
    {
        static NftStorageAccount()
        {
            LoadApiKey();
        }
        internal static string ApiKey {
            get
            {
                return Cipher.Decrypt(_ApiKeyEncrypted,Environment.UserName);
            }
            set
            {
                _ApiKeyEncrypted = Cipher.Encrypt(value,Environment.UserName);
            }
        }
        internal static string _ApiKeyEncrypted { get; set; }
        private static FileInfo _ApiKeyFile = new FileInfo("api.id");
        internal static void LoadApiKey()
        {
            if (_ApiKeyEncrypted == null || _ApiKeyEncrypted == "")
            {
                // try loading file
                if (_ApiKeyFile.Exists)
                {
                    _ApiKeyEncrypted = File.ReadAllText("api");
                }
                else
                {
                    Console.WriteLine("No api key defined!");
                    Console.WriteLine("please specify api key to nft.storage:");
                    ApiKey = Console.ReadLine();
                    File.WriteAllText("api", _ApiKeyFile.FullName);
                    File.SetAttributes(_ApiKeyFile.FullName, FileAttributes.Hidden);
                }
                // test api key
            }
        }
    }
}
