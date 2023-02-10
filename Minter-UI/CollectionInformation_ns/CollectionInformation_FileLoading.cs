using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Minter_UI.CollectionInformation_ns
{
    /// <summary>
    /// Collectioninformation scans through the file collection and provides information about metadata, missing files and ready to mint nfts
    /// </summary>
    public static partial class CollectionInformation
    {
        /// <summary>
        /// This method is used to refresh and reload information about the different directories and files being used in the program.
        /// </summary>
        /// <param name="caseSensitive">A boolean value indicating whether or not the directory and file searching should be case sensitive.</param>
        /// <param name="newInfo">An object of type CollectionInformation_Object that will hold the information gathered from the directories.</param>
        private static void ReLoadDirectories(CollectionInformation_Object newInfo)
        {
            // refresh directory Informations to include newly added files
            Directories.RefreshDirectoryInformations();
            // load base directories
            /// nft base files (images, documents, ...)
            newInfo.NftFiles = LoadDirectory(dirInfo: Directories.Nfts);
            /// metadata files
            newInfo.MetadataFiles = LoadDirectory(
                dirInfo: Directories.Metadata, 
                fileTypes: new[] { ".json",".metadata"},fileNameFilter: new[] { "CollectionInfo.json" }, mustBeContainedWithin: newInfo.NftFiles
                );
            /// rpc files (uploaded to nft.storage)
            newInfo.RpcFiles = LoadDirectory(dirInfo: Directories.Rpcs, fileTypes: new[] { ".json", ".rpc" }, mustBeContainedWithin: newInfo.NftFiles);
            /// started mints which have not yet been validated for completeness
            newInfo.PendingTransactions = LoadDirectory(dirInfo: Directories.PendingTransactions, fileTypes: new[] { ".mint" }, mustBeContainedWithin: newInfo.NftFiles);
            /// finished nfts
            newInfo.MintedFiles = LoadDirectory(dirInfo: Directories.Minted, fileTypes: new[] { ".json", ".rpc",".nft" }, mustBeContainedWithin: newInfo.NftFiles);
            /// generated offer files
            newInfo.OfferedFiles = LoadDirectory(dirInfo: Directories.Offered, fileTypes: new[] { ".offer",".json", ".rpc", ".nft" }, mustBeContainedWithin: newInfo.NftFiles);
            /// uploaded offer Files
            newInfo.PublishedOffers = LoadDirectory(dirInfo: Directories.Offered, fileTypes: new[] { ".offer", ".dexieoffer" }, mustBeContainedWithin: newInfo.NftFiles);
            // generate arbitrary information which can be calculated using the base directories.
            // eg: nft has metadata information, but no rpc and mint has not been validated -> ready to mint
            foreach (string key in newInfo.NftFiles.Keys)
            {
                /// gather missing Metadata files
                if (!newInfo.MetadataFiles.ContainsKey(key))
                {
                    string caseSensitiveFileName = Path.GetFileNameWithoutExtension(newInfo.NftFiles[key].FullName) + ".json";
                    newInfo.MissingMetadata[key] = new FileInfo(Path.Combine(Directories.Metadata.FullName, caseSensitiveFileName));
                }
                /// gather missing rpc files (not uploaded to nft.storage)
                else if(!newInfo.RpcFiles.ContainsKey(key) && !newInfo.PendingTransactions.ContainsKey(key) && !newInfo.MintedFiles.ContainsKey(key))
                {
                    string caseSensitiveFileName = Path.GetFileNameWithoutExtension(newInfo.NftFiles[key].FullName) + ".rpc";
                    newInfo.MissingRPCs[key] = new FileInfo(Path.Combine(Directories.Metadata.FullName, caseSensitiveFileName));
                }
                /// gater nfts which can be minted
                else if (newInfo.RpcFiles.ContainsKey(key) && !newInfo.PendingTransactions.ContainsKey(key) && !newInfo.MintedFiles.ContainsKey(key))
                {
                    string caseSensitiveFileName = Path.GetFileNameWithoutExtension(newInfo.NftFiles[key].FullName) + ".rpc";
                    newInfo.ReadyToMint[key] = new FileInfo(Path.Combine(Directories.Metadata.FullName, caseSensitiveFileName));
                }
                /// gather nfts which are minted already but not offered
                else if (newInfo.MintedFiles.ContainsKey(key) && !newInfo.OfferedFiles.ContainsKey(key))
                {
                    newInfo.ReadyToOffer[key] = newInfo.MintedFiles[key];
                }
                /// gather nfts which can be offered
                else if (newInfo.OfferedFiles.ContainsKey(key))
                {
                    // nft has an offer file
                    // this is a specific case because multiple exchanges might be supported in the future
                    string[] exchangekeys = new[]
                    {
                        key+"_dexie"
                    };
                    foreach (string exchangekey in exchangekeys)
                    {
                        if (!newInfo.PublishedOffers.ContainsKey(exchangekey))
                        {
                            newInfo.ReadyToPublishOffer[key] = newInfo.OfferedFiles[key];
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Loads a file list from directory
        /// </summary>
        /// <param name="dirInfo">the directory of which files should be listed</param>
        /// <param name="caseSensitive">should the resulting file index keys be case sensitive?</param>
        /// <param name="fileTypes">file types (extensions) to include</param>
        /// <param name="fileNameFilter">filenames (example.png) to exclude</param>
        /// <param name="mustBeContainedWithin">another fileindex dictionary which is the true source. If the file doesnt exist there, exclude it</param>
        /// <returns></returns>
        private static ConcurrentDictionary<string,FileInfo> LoadDirectory(
            DirectoryInfo dirInfo,  
            string[]? fileTypes = null, 
            string[]? fileNameFilter = null, 
            ConcurrentDictionary<string, FileInfo>? mustBeContainedWithin = null)
        {
            // get files
            FileInfo[] filesList = dirInfo.GetFiles();
            filesList = filesList.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToArray();
            // filter files by type
            if (fileTypes != null)
            {
                List<FileInfo> fileInfos = new List<FileInfo>();
                foreach(string ftype in fileTypes)
                {
                    fileInfos.AddRange(filesList.Where(f => (f.Extension == ftype)));
                }
                filesList = fileInfos.ToArray();
            }
            // generate dictionary
            ConcurrentDictionary<string, FileInfo> files = new ConcurrentDictionary<string, FileInfo>();
            foreach (FileInfo file in filesList)
            {
                /// apply file name filter
                if (fileNameFilter != null && fileNameFilter.Contains(file.Name))
                {
                    continue;
                }
                /// get key
                string key = GetKeyFromFile(file);
                if (mustBeContainedWithin != null && !mustBeContainedWithin.ContainsKey(key)) continue;
                /// add files
                files[key] = file;
            }
            return files;
        }
    }
}

