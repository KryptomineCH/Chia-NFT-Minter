using Chia_Metadata;
using System.IO;
using System.Runtime.Caching;

namespace Minter_UI.CollectionInformation_ns
{
    /// <summary>
    /// Collectioninformation scans through the file collection and provides information about metadata, missing files and ready to mint nfts
    /// </summary>
    public static partial class CollectionInformation
    {
        static CollectionInformation()
        {
            MetadataCache = new MemoryCache("MyCache");
        }
        /// <summary>
        /// this cache is very important to not always load metadata from files again
        /// </summary>
        private static MemoryCache MetadataCache { get; set; }
        /// <summary>
        /// tries to load metadata from cache. If it is not in the cache, load from file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static bool GetMetadataFromCache(string key, out Metadata result)
        {
            result = new Metadata();
            if (MetadataCache.Contains(key))
            {
                result = MetadataCache.Get(key) as Metadata;
                return true;
            }
            FileInfo MetadataFile = null ;
            if (Information.MetadataFiles.TryGetValue(key,out MetadataFile))
            {
                result = IO.Load(MetadataFile.FullName);
                MetadataCache.Add(key,result,new CacheItemPolicy());
                return true;
            }
            return false;
        }
        /// <summary>
        /// tries to load metadata from cache. If it is not in the cache, load from file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static bool GetMetadataFromCache(FileInfo file, out Metadata result)
        {
            string key = GetKeyFromFile(file);
            result = new Metadata();
            if (MetadataCache.Contains(key))
            {
                result = MetadataCache.Get(key) as Metadata;
                return true;
            }
            if (file.Exists)
            {
                result = IO.Load(file.FullName);
                MetadataCache.Add(key, result, new CacheItemPolicy());
                return true;
            }
            return false;
        }
    }
}
