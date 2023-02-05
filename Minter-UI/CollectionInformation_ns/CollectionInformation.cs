using System.IO;
using System.Threading.Tasks;

namespace Minter_UI.CollectionInformation_ns
{
    /// <summary>
    /// Collectioninformation scans through the file collection and provides information about metadata, missing files and ready to mint nfts
    /// </summary>
    public static partial class CollectionInformation
    {
        public static CollectionInformation_Object Information = new CollectionInformation_Object();
        public static string GetKeyFromFile(FileInfo file)
        {
            return GetKeyFromFile(file.Name);
        }
        public static string GetKeyFromFile(string path)
        {
            string key = Path.GetFileNameWithoutExtension(path);
            if (!Settings_NS.Settings.All.CaseSensitiveFileHandling)
            {
                key = key.ToLower();
            }
            return key;
        }
        public static void ReloadAll()
        {
            lock (IsLoadingLock)
            {
                if (LoadTask == null || LoadTask.IsCompleted)
                {
                    LoadTask = Task.Run(() => ReloadAll_Async());
                }
            }
            LoadTask.Wait();
            return;
        }
        public static async Task ReloadAll_Async()
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
            ReLoadDirectories(newInfo);
            GetAttributes(newInfo);
            _ = Task.Run(() => GeneratePreviews(newInfo));
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
