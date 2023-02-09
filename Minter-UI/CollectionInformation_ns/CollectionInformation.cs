using System.IO;
using System.Threading.Tasks;

namespace Minter_UI.CollectionInformation_ns
{
    /// <summary>
    /// Collectioninformation scans through the file collection and provides information about metadata, missing files and ready to mint nfts
    /// </summary>
    public static partial class CollectionInformation
    {
        /// <summary>
        /// Object that contains information about the collection
        /// </summary>
        public static CollectionInformation_Object Information = new CollectionInformation_Object();
        /// <summary>
        /// Gets the key of a file from its `FileInfo` object
        /// </summary>
        /// <remarks>
        /// key is the uneque identifier for dictionaries and hashsets. It is the filename without extension.
        /// </remarks>
        /// <param name="file">The `FileInfo` object of the file</param>
        /// <returns>The key of the file</returns>
        public static string GetKeyFromFile(FileInfo file)
        {
            return GetKeyFromFile(file.Name);
        }
        /// <summary>
        /// Gets the key of a file from its file path
        /// </summary>
        /// <remarks>
        /// key is the uneque identifier for dictionaries and hashsets. It is the filename without extension.
        /// </remarks>
        /// <param name="path">The file path of the file</param>
        /// <returns>The key of the file</returns>
        public static string GetKeyFromFile(string path)
        {
            string key = Path.GetFileNameWithoutExtension(path);
            if (!Settings_NS.Settings.All.CaseSensitiveFileHandling)
            {
                key = key.ToLower();
            }
            return key;
        }
        /// <summary>
        /// Reloads all the information about the collection
        /// </summary>
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
        /// <summary>
        /// Reloads all the information about the collection asynchronously
        /// </summary>
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
        /// Indicates if the collection information is being loaded
        /// </summary>
        private static volatile bool IsLoading = false;

        /// <summary>
        /// Object used to lock the `IsLoading` property in a multithreaded environment
        /// </summary>
        private static object IsLoadingLock = new object();

        /// <summary>
        /// Task for loading the collection information
        /// </summary>
        public static Task? LoadTask { get; set; }

    }
}
