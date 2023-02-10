using Chia_Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Minter_UI.CollectionInformation_ns
{
    public class CollectionInformation_Object
    {

        /// <summary>
        /// contains all nft files (eg, images, videos or documents) of the collection
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> NftFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all preview files (.png, .jpg) of the collection
        /// </summary>
        /// <remarks>
        /// the preview files are beeing used to display in the ui and reduce load
        /// </remarks>
        public ConcurrentDictionary<string, FileInfo> NftPreviewFiles = new ConcurrentDictionary<string, FileInfo>();

        //public FileInfo[] NftFileInfos;
        /// <summary>
        /// contains all ecisting metadata (.json) files of the nft collection
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> MetadataFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all rpc files
        /// </summary>
        /// <remarks>
        /// A .rpc file means that all nessesary information (images, metadata, license) has been uploaded and is ready for minting
        /// </remarks>
        public ConcurrentDictionary<string, FileInfo> RpcFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains al mintnft responses which have not been validated yet
        /// </summary>
        /// <remarks>
        /// this collection is temporary only. it serves the purpose to prevent duplicates.
        /// It is beeing used to protect against crashes so that nft mints can be verified at a later point.
        /// </remarks>
        public ConcurrentDictionary<string, FileInfo> PendingTransactions = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all minted .nft files. These nfts are complete and the mint has been verified
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> MintedFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// this nfts have offer files
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> OfferedFiles = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// This Dictionary contains all offer files which have been uploaded to a decentralized exchange
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> PublishedOffers = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all nft's where metadata is missing
        /// </summary>
        /// <remarks>this is an arbitrary collection which can be generated from NftFiles - MetadataFiles</remarks>
        public ConcurrentDictionary<string, FileInfo> MissingMetadata = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// contains all nfts which have not been minted
        /// </summary>
        /// <remarks>this is an arbitrary collection which can be generated from MetadataFiles - RpcFiles</remarks>
        public ConcurrentDictionary<string, FileInfo> MissingRPCs = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// these nfts are minted but have not generated offer files yet
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> ReadyToOffer = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// these NFTs are the offered nfts
        /// </summary>
        public ConcurrentDictionary<string, FileInfo> ReadyToPublishOffer = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// these are the nfts which have been uploaded to nftStorage and are ready for minting
        /// </summary>
        /// <remarks>this is an arbitrary collection which can be generated from RpcFiles - (MintedFiles + PendingTransactions)</remarks>
        public ConcurrentDictionary<string, FileInfo> ReadyToMint = new ConcurrentDictionary<string, FileInfo>();
        /// <summary>
        /// can be used to find out which nft belongs to which collection-number (aka image 3/1000 in the collection) 
        /// </summary>
        public ConcurrentDictionary<ulong, string> NFTIndexes = new ConcurrentDictionary<ulong, string>();
        /// <summary>
        /// is used to determine the nft numbers and to reserve the next free number in the collection
        /// </summary>
        public List<int> CollectionNumbers = new List<int>();
        /// <summary>
        /// this is the metadata of the highest NFT-Index. It should be one of the most recently minted ones and is the reference to build Metadata
        /// </summary>
        /// <remarks>it can be used to suggest certain metadata features in the new nft.
        /// For example, if the last nft was from a subcollection Fanta-Boys of Soda-Club-Boys, it is likely the next nft will be a fanta boy as well</remarks>
        public Metadata? LastKnownNftMetadata { get; set; }
        /// <summary>
        /// index to specify up to which index CollectionNumbers has been scanned for the next free index slot. makes finding gaps more performant
        /// </summary>
        public int CollectionNumbersIndex { get; set; }
        /// <summary>
        /// all metadata attributes which are likely for a new nft. will be added upon first creation
        /// </summary>
        /// <remarks>
        /// each attribute in here is applied to at least 60% of the NFT Collection
        /// </remarks>
        public MetadataAttribute[] LikelyAttributes = new MetadataAttribute[0];
        /// <summary>
        /// a list of all attributes. Used to suggest attributes in the attribute selector dropdown
        /// </summary>
        public ConcurrentDictionary<string, MetadataAttribute> AllMetadataAttributes = new ConcurrentDictionary<string, MetadataAttribute>();
        /// <summary>
        /// a list of all attribute values. Used to suggest attributes in the attribute selector dropdown
        /// </summary>
        public ConcurrentDictionary<string, List<string>> AllMetadataAttributeValues = new ConcurrentDictionary<string, List<string>>();
        /// <summary>
        /// finds the next free nft number for the collection. Reserves it and returns it. Should possibly only be used when minting
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int ReserveNextFreeCollectionNumber()
        {
            // TODO: Add Unit test
            // if the collection numbers list is empty, add a new collection number and return it
            if (CollectionNumbers.Count == 0)
            {
                CollectionNumbersIndex = CollectionNumbers.Count;
                CollectionNumbers.Add(CollectionNumbers.Count + 1);
                return CollectionNumbers.Count;
            }
            // iterates through the collection numbers list to find the next available collection number
            /// the last index without gap is beeing stored to prevent duplicate seeking
            for (int i = CollectionNumbersIndex; i < CollectionNumbers.Count; i++)
            {
                // if the current index is the last index in the list, add a new collection number and return it
                if (i == CollectionNumbers.Count - 1)
                {
                    CollectionNumbers.Add(CollectionNumbers.Count + 1);
                    return CollectionNumbers.Count;
                }
                // if a gap is found in the list, add a new collection number and return it
                if (CollectionNumbers[i] < (CollectionNumbers[i + 1] - 1))
                {
                    CollectionNumbers.Add(i + 2);
                    CollectionNumbers = CollectionNumbers.OrderBy(x => x).ToList();
                    return i + 2;
                }
            }
            // if no free collection number can be found, throw an exception
            throw new Exception("no free collection number could be found!");
        }
    }
}
