using Chia_NFT_Minter;
using NFT.Storage.Net.API;
using System;
using System.IO;


namespace Minter_UI
{
    /// <summary>
    /// provides access to nft.storage api
    /// </summary>
    internal static class NftStorageAccount
    {
        static NftStorageAccount()
        {
            TryLoadApiKey();
        }
        /// <summary>
        /// the apii key, obtained from nft.storage
        /// </summary>
        internal static string ApiKey {
            get
            {
                return Cipher.Decrypt(_ApiKeyEncrypted,Environment.UserName);
            }
            set
            {
                _ApiKeyEncrypted = Cipher.Encrypt(value,Environment.UserName);
                Client = new NFT_Storage_API(ApiKey);
                File.WriteAllText(_ApiKeyFile.FullName, _ApiKeyEncrypted);
                File.SetAttributes(_ApiKeyFile.FullName, FileAttributes.Hidden);
            }
        }
        internal static string _ApiKeyEncrypted { get; set; }
        private static FileInfo _ApiKeyFile = new FileInfo("api.id");
        public static NFT_Storage_API Client;
        public static bool TryLoadApiKey()
        {
            if (_ApiKeyEncrypted == null || _ApiKeyEncrypted == "")
            {
                // try loading file
                if (_ApiKeyFile.Exists)
                {
                    _ApiKeyEncrypted = File.ReadAllText(_ApiKeyFile.FullName);
                    Client = new NFT_Storage_API(ApiKey);
                    return true;
                }
                else
                {
                    return false;
                }
                // test api key
            }
            return false;
        }

    }
}
