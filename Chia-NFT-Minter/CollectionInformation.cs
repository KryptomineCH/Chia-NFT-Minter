using Chia_Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chia_NFT_Minter
{
    public static class CollectionInformation
    {
        static CollectionInformation()
        {
            ReLoadDirectories(true);
        }
        public static Dictionary<string, FileInfo> NftFiles = new Dictionary<string, FileInfo>();
        public static Dictionary<string, FileInfo> MetadataFiles = new Dictionary<string, FileInfo>();
        public static Dictionary<string, FileInfo> RpcFiles = new Dictionary<string, FileInfo>();
        public static Dictionary<string, FileInfo> MissingMetadata = new Dictionary<string, FileInfo>();
        public static Dictionary<string, FileInfo> MissingRPCs = new Dictionary<string, FileInfo>();
        public static Dictionary<int, string> NFTIndexes = new Dictionary<int, string>(); 
        //public static Queue<FileInfo> MissingMetadata = new Queue<FileInfo>();
        //public static Queue<FileInfo> MissingRpcs = new Queue<FileInfo>();
        private static List<int> CollectionNumbers = new List<int>();
        public static Metadata LastKnownNftMetadata { get; private set; }
        private static int CollectionNumbersIndex { get; set; }
        public static int ReserveNextFreeCollectionNumber()
        {
            // TODO: Add Unit test
            if (CollectionNumbers.Count == 0 || CollectionNumbers.Last() == CollectionNumbers.Count)
            {
                CollectionNumbersIndex = CollectionNumbers.Count;
                CollectionNumbers.Add(CollectionNumbers.Count+1);
                return CollectionNumbers.Count;
            }
            throw new NotImplementedException("gaps are not yet properly!");
            for(int i = CollectionNumbersIndex; i < CollectionNumbers.Count; i++)
            {
                if (CollectionNumbersIndex == CollectionNumbers.Count)
                {
                    CollectionNumbers.Add(CollectionNumbers.Count);
                    CollectionNumbersIndex++;
                    return CollectionNumbers.Count;
                }
                if (CollectionNumbers[i] < (CollectionNumbers[i+1]-1))
                {
                    CollectionNumbers.Add(i);
                    CollectionNumbersIndex++;
                    return i;
                }
            }
            throw new Exception("Collection number could not be specified!");
        }
        public static void ReLoadDirectories(bool caseSensitive)
        {
            CollectionNumbersIndex = 0;
            CollectionNumbers.Clear();
            // metadata files
            FileInfo[] metadataFiles = Directories.Metadata.GetFiles();
            metadataFiles = metadataFiles
                .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                .Where(f => (f.Extension == ".json")).ToArray();
            MetadataFiles.Clear();
            foreach (FileInfo metadataFile in metadataFiles)
            {
                if (metadataFile.Name == "CollectionInfo.json")
                {
                    // skip collection information
                    continue;
                }
                string key = Path.GetFileNameWithoutExtension(metadataFile.FullName);
                if(!caseSensitive)
                {
                    key = key.ToLower();
                }
                MetadataFiles.Add(key, metadataFile);
                Metadata meta = IO.Load(metadataFile.FullName);
                CollectionNumbers.Add(meta.series_number);
                CollectionNumbers = CollectionNumbers.OrderBy(x => x).ToList();
            }
            // rpcFiles
            FileInfo[] rpcs = Directories.Rpcs.GetFiles();
            rpcs = rpcs
                .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                .Where(f => (f.Extension == ".json" || f.Extension == ".rpc")).ToArray();
            RpcFiles.Clear();
            foreach (FileInfo rpcFile in rpcs)
            {
                string key = Path.GetFileNameWithoutExtension(rpcFile.FullName);
                if (!caseSensitive)
                {
                    key = key.ToLower();
                }
                if (!RpcFiles.ContainsKey(key))
                {
                    RpcFiles.Add(key, rpcFile);
                }
            }
            // nft files
            /// note: first load the other folders, so that missing metadata and rpc can be build
            FileInfo[] nfts = Directories.Nfts.GetFiles();
            nfts = nfts.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToArray();
            NftFiles.Clear();
            MissingMetadata.Clear();
            MissingRPCs.Clear();
            foreach (FileInfo nftFile in nfts)
            {
                string key = Path.GetFileNameWithoutExtension(nftFile.FullName);
                if (!caseSensitive)
                {
                    key = key.ToLower();
                }
                NftFiles.Add(key, nftFile);
                // add missing rpc and metadata files
                if (!MetadataFiles.ContainsKey(key))
                {
                    MissingMetadata.Add(key,nftFile);
                }
                if (!RpcFiles.ContainsKey(key))
                {
                    MissingRPCs.Add(key,nftFile);
                }
            }
            GetAttributes();
        }
        public static MetadataAttribute[] LikelyAttributes { get; set; }
        public static Dictionary<string, MetadataAttribute> AllMetadataAttributes = new Dictionary<string, MetadataAttribute>();
        private static void GetAttributes()
        {
            AllMetadataAttributes.Clear();
            Dictionary<string,int> keyValuePairs = new Dictionary<string,int>();
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
                    if (!AllMetadataAttributes.ContainsKey(attr.trait_type))
                    {
                        AllMetadataAttributes.Add(attr.trait_type, attr);
                    }
                    if (!keyValuePairs.ContainsKey(attr.trait_type))
                    {
                        keyValuePairs.Add(attr.trait_type, 1);
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
