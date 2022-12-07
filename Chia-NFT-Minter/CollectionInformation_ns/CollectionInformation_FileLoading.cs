

using System.Collections.Concurrent;

namespace Chia_NFT_Minter.CollectionInformation_ns
{
    public static partial class CollectionInformation
    {
        /// <summary>
        /// indexes all collection files and generates missing files indexes
        /// </summary>
        /// <param name="caseSensitive"></param>
        private static void ReLoadDirectories(bool caseSensitive, CollectionInformation_Object newInfo)
        {
            Directories.Nfts.Refresh();
            Directories.Rpcs.Refresh();
            Directories.Metadata.Refresh();
            // load base directories
            newInfo.NftFiles = LoadDirectory(dirInfo: Directories.Nfts, caseSensitive: caseSensitive);
            newInfo.MetadataFiles = LoadDirectory(
                dirInfo: Directories.Metadata, caseSensitive: caseSensitive, 
                fileTypes: new[] { ".json"},fileNameFilter: new[] { "CollectionInfo.json" }, mustBeContainedWithin: newInfo.NftFiles
                );
            newInfo.RpcFiles = LoadDirectory(dirInfo: Directories.Rpcs, caseSensitive: caseSensitive, fileTypes: new[] { ".json", ".rpc" }, mustBeContainedWithin: newInfo.NftFiles);
            // load missing files
            foreach(string key in newInfo.NftFiles.Keys)
            {
                if (!newInfo.MetadataFiles.ContainsKey(key))
                {
                    string caseSensitiveFileName = Path.GetFileNameWithoutExtension(newInfo.NftFiles[key].FullName) + ".json";
                    newInfo.MissingMetadata[key] = new FileInfo(Path.Combine(Directories.Metadata.FullName, caseSensitiveFileName));
                }
                if(!newInfo.RpcFiles.ContainsKey(key))
                {
                    string caseSensitiveFileName = Path.GetFileNameWithoutExtension(newInfo.NftFiles[key].FullName) + ".rpc";
                    newInfo.MissingRPCs[key] = new FileInfo(Path.Combine(Directories.Metadata.FullName, caseSensitiveFileName));
                }
            }
            newInfo.NftFileInfos = newInfo.NftFiles.Values.ToArray();
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
            bool caseSensitive, 
            string[] fileTypes = null, 
            string[] fileNameFilter = null, 
            ConcurrentDictionary<string, FileInfo> mustBeContainedWithin = null)
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
            // fill dictionary
            foreach (FileInfo metadataFile in filesList)
            {
                // apply file name filter
                if (fileNameFilter != null && fileNameFilter.Contains(metadataFile.Name))
                {
                    continue;
                }
                // get key
                string key = Path.GetFileNameWithoutExtension(metadataFile.Name);
                if (!caseSensitive)
                {
                    key = key.ToLower();
                }
                if (mustBeContainedWithin != null && !mustBeContainedWithin.ContainsKey(key)) continue;
                // add files
                files[key] = metadataFile;
            }
            return files;
        }
    }
}

