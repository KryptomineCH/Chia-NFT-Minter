using Chia_Metadata;
using System.Collections.Concurrent;

namespace Chia_NFT_Minter.CollectionInformation_ns
{
    /// <summary>
    /// Collectioninformation scans through the file collection and provides information about metadata, missing files and ready to mint nfts
    /// </summary>
    public static partial class CollectionInformation
    {
        public static CollectionInformation_Object Information { get; set; }
        public static void ReloadAll(bool caseSensitive)
        {
            lock (IsLoadingLock)
            {
                if (IsLoading) return;
                IsLoading = true;
                CollectionInformation_Object newInfo = new CollectionInformation_Object();
                ReLoadDirectories(caseSensitive, newInfo);
                GetAttributes(newInfo);
                GeneratePreviews(caseSensitive, newInfo);
                Information = newInfo;
            }
        }
        /// <summary>
        /// is used in an attempt to defy access violation exceptions in a multithreaded environment
        /// </summary>
        private static bool IsLoading = false;
        private static object IsLoadingLock = new object();
        
        
        
        
    }
}
