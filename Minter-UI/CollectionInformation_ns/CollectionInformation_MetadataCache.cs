using Chia_Metadata;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Minter_UI.CollectionInformation_ns
{
    public static partial class CollectionInformation
    {
        static CollectionInformation()
        {
            MetadataCache = new MemoryCache("MyCache");
        }
        private static MemoryCache MetadataCache { get; set; }
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
