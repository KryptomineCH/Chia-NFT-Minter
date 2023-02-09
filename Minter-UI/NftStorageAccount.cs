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
        /// the api key, obtained from nft.storage
        /// </summary>
        internal static string? ApiKey {
            get
            {
                if (_ApiKeyEncrypted == null)
                {
                    return null;
                }
                return Cipher.Decrypt(_ApiKeyEncrypted,Environment.UserName);
            }
            set
            {
                if (value == null)
                {
                    _ApiKeyEncrypted = null;
                    File.Delete(_ApiKeyFile.FullName);
                    return;
                }
                _ApiKeyEncrypted = Cipher.Encrypt(value,Environment.UserName);
                Client = new NFT_Storage_API(value);
                File.WriteAllText(_ApiKeyFile.FullName, _ApiKeyEncrypted);
                File.SetAttributes(_ApiKeyFile.FullName, FileAttributes.Hidden);
            }
        }
        /// <summary>
        /// the api key, obtained from nft.storage in encrypted form. used to provide some memory readout protection. <br/>
        /// can be decrypted with the username
        /// </summary>
        internal static string? _ApiKeyEncrypted { get; set; }
        /// <summary>
        /// the file where the encrypted api key is stored
        /// </summary>
        private static FileInfo _ApiKeyFile = new FileInfo("api.id");
        /// <summary>
        /// the nft.storage client interface used to upload the media
        /// </summary>
        public static NFT_Storage_API? Client;
        /// <summary>
        /// tries to load the encrypted api key file from disk
        /// </summary>
        /// <returns>true if NFT_Storage_API was sucessfully initialized</returns>
        public static bool TryLoadApiKey()
        {
            if (_ApiKeyEncrypted == null || _ApiKeyEncrypted == "")
            {
                // try loading file
                if (_ApiKeyFile.Exists)
                {
                    _ApiKeyEncrypted = File.ReadAllText(_ApiKeyFile.FullName);
                    string? apiKey = ApiKey;
                    if (apiKey != null && apiKey != "") 
                    {
                        Client = new NFT_Storage_API(apiKey);
                        return true;
                    }
                    else
                    { return false; }
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
