using Chia_Metadata;
using System.Collections.Concurrent;

namespace Chia_NFT_Minter.CollectionInformation_ns
{
    public class CollectionInformation_Object
    {

        /// <summary>
        /// contains all nft files (eg, images, videos or documents)
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> NftFiles = new ConcurrentDictionary<string, FileInfo>();
        public ConcurrentDictionary<string, FileInfo> NftPreviewFiles = new ConcurrentDictionary<string, FileInfo>();

        public FileInfo[] NftFileInfos;
        /// <summary>
        /// contains all metadata files
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> MetadataFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all rpc files (~minted nft's)
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> RpcFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all nft's where metadata is missing
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> MissingMetadata = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all nfts which have not been minted
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> MissingRPCs = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// can be used to find out which nft belongs to which collectionnumber
        /// </summary>
        public ConcurrentDictionary<ulong, string> NFTIndexes = new ConcurrentDictionary<ulong, string>();
        /// <summary>
        /// is used to determine the nft numbers and to reserve the next free number in the collection
        /// </summary>
        public List<int> CollectionNumbers = new List<int>();
        /// <summary>
        /// this is the metadata of the highest NFT-Index. It should be one of the most recently minted ones and is the reference to build Metadata
        /// </summary>
        public Metadata LastKnownNftMetadata { get; set; }
        /// <summary>
        /// index to specify up to which index CollectionNumbers has been scanned for the next free index slot. makes finding gaps more performant
        /// </summary>
        public int CollectionNumbersIndex { get; set; }
        /// <summary>
        /// all attributes which are likely for a new nft. will be added upon first creation
        /// </summary>
        public MetadataAttribute[] LikelyAttributes { get; set; }
        /// <summary>
        /// a list of all attributes. Used to suggest attributes in the attribute selector dropdown
        /// </summary>
        public ConcurrentDictionary<string, MetadataAttribute> AllMetadataAttributes = new ConcurrentDictionary<string, MetadataAttribute>();
        public ConcurrentDictionary<string, List<string>> AllMetadataAttributeValues = new ConcurrentDictionary<string, List<string>>();
        /// <summary>
        /// finds the next free nft number for the collection. Reserves it and returns it. Should possibly only be used when minting
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int ReserveNextFreeCollectionNumber()
        {
            // TODO: Add Unit test
            if (CollectionNumbers.Count == 0)
            {
                CollectionNumbersIndex = CollectionNumbers.Count;
                CollectionNumbers.Add(CollectionNumbers.Count + 1);
                return CollectionNumbers.Count;
            }
            for (int i = CollectionNumbersIndex; i < CollectionNumbers.Count; i++)
            {
                if (i == CollectionNumbers.Count - 1)
                {
                    CollectionNumbers.Add(CollectionNumbers.Count + 1);
                    return CollectionNumbers.Count;
                }
                if (CollectionNumbers[i] < (CollectionNumbers[i + 1] - 1))
                {
                    CollectionNumbers.Add(i + 2);
                    CollectionNumbers = CollectionNumbers.OrderBy(x => x).ToList();
                    return i + 2;
                }
            }
            throw new Exception("Collection number could not be specified!");
        }
    }
}
