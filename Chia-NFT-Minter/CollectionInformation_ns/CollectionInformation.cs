using Chia_Metadata;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Chia_NFT_Minter.CollectionInformation_ns
{
    /// <summary>
    /// Collectioninformation scans through the file collection and provides information about metadata, missing files and ready to mint nfts
    /// </summary>
    public static partial class CollectionInformation
    {
        public static CollectionInformation_Object Information = new CollectionInformation_Object();
        public static void ReloadAll(bool caseSensitive)
        {
            lock (IsLoadingLock)
            {
                if (LoadTask == null || LoadTask.IsCompleted)
                {
                    LoadTask = Task.Run(() => ReloadAll_Async(caseSensitive));
                }
            }
            LoadTask.Wait();
            return;
        }
        public static async Task ReloadAll_Async(bool caseSensitive)
        {
            lock (IsLoadingLock)
            {
                if (IsLoading && LoadTask != null)
                {
                    LoadTask.Wait(); 
                    return;
                }
                IsLoading = true;
            }
            CollectionInformation_Object newInfo = new CollectionInformation_Object();
            ReLoadDirectories(caseSensitive, newInfo);
            GetAttributes(newInfo);
            _ = Task.Run(() => GeneratePreviews(caseSensitive, newInfo));
            Information = newInfo;
            IsLoading = false;
        }
        /// <summary>
        /// is used in an attempt to defy access violation exceptions in a multithreaded environment
        /// </summary>
        private static volatile bool IsLoading = false;
        private static object IsLoadingLock = new object();
        public static Task? LoadTask { get; set; }
        
    }
}
