using Chia_Metadata;
using System.Collections.Concurrent;

namespace Chia_NFT_Minter
{
    /// <summary>
    /// Collectioninformation scans through the file collection and provides information about metadata, missing files and ready to mint nfts
    /// </summary>
    public static class CollectionInformation
    {
        static CollectionInformation()
        {
            //ReLoadDirectories(true);
        }
        /// <summary>
        /// contains all nft files (eg, images, videos or documents)
        /// </summary>
        public static ConcurrentDictionary<string, FileInfo> NftFiles = new ConcurrentDictionary<string, FileInfo>();

        public static FileInfo[] NftFileInfos;
        /// <summary>
        /// contains all metadata files
        /// </summary>
        public static ConcurrentDictionary<string, FileInfo> MetadataFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all rpc files (~minted nft's)
        /// </summary>
        public static ConcurrentDictionary<string, FileInfo> RpcFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all nft's where metadata is missing
        /// </summary>
        public static ConcurrentDictionary<string, FileInfo> MissingMetadata = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all nfts which have not been minted
        /// </summary>
        public static ConcurrentDictionary<string, FileInfo> MissingRPCs = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// can be used to find out which nft belongs to which collectionnumber
        /// </summary>
        public static ConcurrentDictionary<int, string> NFTIndexes = new ConcurrentDictionary<int, string>(); 
        /// <summary>
        /// is used to determine the nft numbers and to reserve the next free number in the collection
        /// </summary>
        private static List<int> CollectionNumbers = new List<int>();
        /// <summary>
        /// this is the metadata of the highest NFT-Index. It should be one of the most recently minted ones and is the reference to build Metadata
        /// </summary>
        public static Metadata LastKnownNftMetadata { get; private set; }
        /// <summary>
        /// index to specify up to which index CollectionNumbers has been scanned for the next free index slot. makes finding gaps more performant
        /// </summary>
        private static int CollectionNumbersIndex { get; set; }
        /// <summary>
        /// finds the next free nft number for the collection. Reserves it and returns it. Should possibly only be used when minting
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int ReserveNextFreeCollectionNumber()
        {
            // TODO: Add Unit test
            if (CollectionNumbers.Count == 0)
            {
                CollectionNumbersIndex = CollectionNumbers.Count;
                CollectionNumbers.Add(CollectionNumbers.Count+1);
                return CollectionNumbers.Count;
            }
            for(int i = CollectionNumbersIndex; i < CollectionNumbers.Count; i++)
            {
                if (i == CollectionNumbers.Count-1)
                {
                    CollectionNumbers.Add(CollectionNumbers.Count+1);
                    return CollectionNumbers.Count;
                }
                if (CollectionNumbers[i] < (CollectionNumbers[i+1]-1))
                {
                    CollectionNumbers.Add(i+2);
                    CollectionNumbers = CollectionNumbers.OrderBy(x => x).ToList();
                    return i+2;
                }
            }
            throw new Exception("Collection number could not be specified!");
        }
        /// <summary>
        /// is used in an attempt to defy access violation exceptions in a multithreaded environment
        /// </summary>
        private static bool IsLoading = false;
        private static object IsLoadingLock = new object();
        /// <summary>
        /// indexes all collection files and builds collection inwormation from it
        /// </summary>
        /// <param name="caseSensitive"></param>
        public static void ReLoadDirectories(bool caseSensitive)
        {
            lock (IsLoadingLock)
            {
                if (IsLoading) return;
                IsLoading = true;
                CollectionNumbersIndex = 0;
                CollectionNumbers.Clear();
                
                
                // nft files
                FileInfo[] nfts = Directories.Nfts.GetFiles();
                nfts = nfts.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToArray();
                ConcurrentDictionary<string, FileInfo> nftFiles = new ConcurrentDictionary<string, FileInfo>();
                foreach (FileInfo nftFile in nfts)
                {
                    string key = Path.GetFileNameWithoutExtension(nftFile.FullName);
                    if (!caseSensitive)
                    {
                        key = key.ToLower();
                    }
                    nftFiles[key] = nftFile;
                }
                NftFiles = nftFiles;
                NftFileInfos = nfts;

                // metadata files
            FileInfo[] metadataFilesList = Directories.Metadata.GetFiles();
                metadataFilesList = metadataFilesList
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .Where(f => (f.Extension == ".json")).ToArray();
                ConcurrentDictionary<string, FileInfo> metadataFiles = new ConcurrentDictionary<string, FileInfo>();
                    foreach (FileInfo metadataFile in metadataFilesList)
                    {
                        if (metadataFile.Name == "CollectionInfo.json")
                        {
                            // skip collection information
                            continue;
                        }
                        string key = Path.GetFileNameWithoutExtension(metadataFile.FullName);
                        if (!caseSensitive)
                        {
                            key = key.ToLower();
                        }
                        if (!nftFiles.ContainsKey(key)) continue;
                        metadataFiles[key] = metadataFile;
                        Metadata meta = IO.Load(metadataFile.FullName);
                        CollectionNumbers.Add(meta.series_number);
                        CollectionNumbers = CollectionNumbers.OrderBy(x => x).ToList();
                        if (LastKnownNftMetadata == null || meta.series_number > LastKnownNftMetadata.series_number)
                        {
                            LastKnownNftMetadata = meta;
                        }
                    }
                MetadataFiles = metadataFiles;
                ConcurrentDictionary<string, FileInfo> rpcFiles = new ConcurrentDictionary<string, FileInfo>();
                // rpcFiles
                FileInfo[] rpcs = Directories.Rpcs.GetFiles();
                rpcs = rpcs
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .Where(f => (f.Extension == ".json" || f.Extension == ".rpc")).ToArray();
                
                    foreach (FileInfo rpcFile in rpcs)
                    {
                        string key = Path.GetFileNameWithoutExtension(rpcFile.FullName);
                        if (!caseSensitive)
                        {
                            key = key.ToLower();
                        }
                        if (!rpcFiles.ContainsKey(key))
                        {
                            rpcFiles[key] = rpcFile;
                        }
                    }
                RpcFiles = rpcFiles;
                ConcurrentDictionary<string, FileInfo> missingMetadata = new ConcurrentDictionary<string, FileInfo>();
                ConcurrentDictionary<string, FileInfo> missingRPCs = new ConcurrentDictionary<string, FileInfo>();

                // nft files
                foreach (string key in nftFiles.Keys)
                        {
                            // add missing rpc and metadata files
                            if (!metadataFiles.ContainsKey(key))
                            {
                                missingMetadata[key] = nftFiles[key];
                            }
                            if (!rpcFiles.ContainsKey(key))
                            {
                                missingRPCs[key] = nftFiles[key];
                            }
                        }
                MissingMetadata = missingMetadata;
                MissingRPCs = missingRPCs;

                GetAttributes();
                IsLoading = false;
            }
        }
        /// <summary>
        /// all attributes which are likely for a new nft. will be added upon first creation
        /// </summary>
        public static MetadataAttribute[] LikelyAttributes { get; set; }
        /// <summary>
        /// a list of all attributes. Used to suggest attributes in the attribute selector dropdown
        /// </summary>
        public static ConcurrentDictionary<string, MetadataAttribute> AllMetadataAttributes = new ConcurrentDictionary<string, MetadataAttribute>();
        /// <summary>
        /// loads attributes and attribute stats into likelyAttributes and allmetadataattributes
        /// </summary>
        private static void GetAttributes()
        {
            ConcurrentDictionary<string, MetadataAttribute> allMetadataAttributes = new ConcurrentDictionary<string, MetadataAttribute>();
            ConcurrentDictionary<string,int> keyValuePairs = new ConcurrentDictionary<string,int>();
            foreach (FileInfo fi in MetadataFiles.Values)
            {
                if (fi.Name == "CollectionInfo.json")
                {
                    // skip collection information
                    continue;
                }
                Metadata data = IO.Load(fi.FullName);
                foreach (MetadataAttribute attr in data.attributes)
                {
                    if (!allMetadataAttributes.ContainsKey(attr.trait_type))
                    {
                        allMetadataAttributes[attr.trait_type] = attr;
                    }
                    if (!keyValuePairs.ContainsKey(attr.trait_type))
                    {
                        keyValuePairs[attr.trait_type] = 1;
                    }
                    else
                    {
                        keyValuePairs[attr.trait_type]++;
                    }
                }
                if (LastKnownNftMetadata == null || data.series_number > LastKnownNftMetadata.series_number)
                {
                    LastKnownNftMetadata = data;
                }
                AllMetadataAttributes = allMetadataAttributes;
            }
            List<MetadataAttribute> likelyAttributes = new List<MetadataAttribute>();
            foreach (string key in keyValuePairs.Keys)
            {
                if (keyValuePairs[key] > MetadataFiles.Count / 2)
                {
                    likelyAttributes.Add(AllMetadataAttributes[key]);
                }
            }
            LikelyAttributes = likelyAttributes.ToArray();
        }
    }
}
