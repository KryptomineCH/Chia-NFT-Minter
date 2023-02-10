using System.IO;

namespace Minter_UI.CollectionInformation_ns
{
    /// <summary>
    /// this class specifies the default directories.<br/>
    /// upon call, it creates the Directories which do not exist
    /// </summary>
    public static class Directories
    {
        static Directories()
        {
            InitializeDirectories();
            HideFoldersAndFiles();
        }
        /// <summary>
        /// this function is required because a directory information is only loaded once upon creation.<br/>
        /// If the user adds more files to the Directory, the DirectoryInfo wont pick this information up without a refresh
        /// </summary>
        internal static void RefreshDirectoryInformations()
        {
            Nfts.Refresh();
            Rpcs.Refresh();
            Metadata.Refresh();
            Minted.Refresh();
            Offered.Refresh();
            PublishedOffers.Refresh();
        }
        /// <summary>
        /// this directory stores the NFT files. Eg, the artwork, image, video or document
        /// </summary>
        public static DirectoryInfo Nfts = new DirectoryInfo("final");
        /// <summary>
        /// this directory stores the preview images for the nfts.
        /// </summary>
        public static DirectoryInfo Preview = new DirectoryInfo("preview");
        /// <summary>
        /// this directory stores the Metadata json files for the NFTs
        /// </summary>
        public static DirectoryInfo Metadata = new DirectoryInfo("metadata");
        /// <summary>
        /// this directory stores the metadata json files which are used to describe the nft and its properties
        /// </summary>
        public static DirectoryInfo Rpcs = new DirectoryInfo("rpcs");
        /// <summary>
        /// this directiry contains the minted NFTs
        /// </summary>
        public static DirectoryInfo Minted = new DirectoryInfo("minted");
        /// <summary>
        /// the offers directory can be used to store offer files
        /// </summary>
        public static DirectoryInfo Offered = new DirectoryInfo("offers");
        /// <summary>
        /// The published_offers directory contains offer files which have been uploaded to a decentraliced exchange
        /// </summary>
        public static DirectoryInfo PublishedOffers = new DirectoryInfo("published_offers");
        /// <summary>
        /// This directory contains the nfts which are currently in the minting process (sent to the blockchain)<br/>
        /// The minting files are stored there to validate at a later point that the Minting has been completed sucessfully
        /// </summary>
        public static DirectoryInfo PendingTransactions = new DirectoryInfo("pending_transactions");
        /// <summary>
        /// creates the specified dirs if they dont exist
        /// </summary>
        private static void InitializeDirectories()
        {
            if (!Nfts.Exists) Nfts.Create();
            if (!Preview.Exists) Preview.Create();
            if (!Metadata.Exists) Metadata.Create();
            if (!Rpcs.Exists) Rpcs.Create();
            if (!Offered.Exists) Offered.Create();
            if (!PublishedOffers.Exists) PublishedOffers.Create();
            if (!Minted.Exists) Minted.Create();
            if (!PendingTransactions.Exists)
            {
                PendingTransactions.Create();
                PendingTransactions.Attributes = FileAttributes.Hidden;
            }
        }
        /// <summary>
        /// this function hides some files which are created by application libraries.
        /// mostly these are cache folders. no need for the user to see the cache.
        /// </summary>
        private static void HideFoldersAndFiles()
        {
            string[] hideFolderList = new[]
            {
                "DawnCache",
                "GPUCache"
            };
            string[] hideFileList = new[]
            {
                "debug.txt",
                "errors.txt"
            };
            foreach (string folder in hideFolderList)
            {
                DirectoryInfo hideThis = new DirectoryInfo(folder);
                if (hideThis.Exists)
                {
                    hideThis.Attributes = FileAttributes.Hidden;
                }
            }
            foreach (string file in hideFileList)
            {
                FileInfo hideThis = new FileInfo(file);
                if (hideThis.Exists)
                {
                    hideThis.Attributes = FileAttributes.Hidden;
                }
            }
        }
    }
}
