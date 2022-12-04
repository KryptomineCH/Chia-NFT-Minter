using Chia_Metadata;
using System.Collections.Concurrent;

namespace Chia_NFT_Minter
{
    public static class CollectionInformation
    {
        static CollectionInformation()
        {
            //ReLoadDirectories(true);
        }
        public static ConcurrentDictionary<string, FileInfo> NftFiles = new ConcurrentDictionary<string, FileInfo>();
        public static FileInfo[] NftFileInfos;
        public static ConcurrentDictionary<string, FileInfo> MetadataFiles = new ConcurrentDictionary<string, FileInfo>();
        public static ConcurrentDictionary<string, FileInfo> RpcFiles = new ConcurrentDictionary<string, FileInfo>();
        public static ConcurrentDictionary<string, FileInfo> MissingMetadata = new ConcurrentDictionary<string, FileInfo>();
        public static ConcurrentDictionary<string, FileInfo> MissingRPCs = new ConcurrentDictionary<string, FileInfo>();
        public static ConcurrentDictionary<int, string> NFTIndexes = new ConcurrentDictionary<int, string>(); 
        //public static Queue<FileInfo> MissingMetadata = new Queue<FileInfo>();
        //public static Queue<FileInfo> MissingRpcs = new Queue<FileInfo>();
        private static List<int> CollectionNumbers = new List<int>();
        public static Metadata LastKnownNftMetadata { get; private set; }
        private static int CollectionNumbersIndex { get; set; }
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
        private static bool IsLoading = false;
        private static object IsLoadingLock = new object();
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
        public static MetadataAttribute[] LikelyAttributes { get; set; }
        public static ConcurrentDictionary<string, MetadataAttribute> AllMetadataAttributes = new ConcurrentDictionary<string, MetadataAttribute>();
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
