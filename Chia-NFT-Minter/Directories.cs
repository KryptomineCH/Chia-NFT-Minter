namespace Chia_NFT_Minter
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
        public static DirectoryInfo Offers = new DirectoryInfo("offers");
        /// <summary>
        /// creates the specified dirs if they dont exist
        /// </summary>
        private static void InitializeDirectories()
        {
            if (!Nfts.Exists) Nfts.Create();
            if (!Preview.Exists) Preview.Create();
            if (!Metadata.Exists) Metadata.Create();
            if (!Rpcs.Exists) Rpcs.Create();
            if (!Offers.Exists) Offers.Create();
            if (!Minted.Exists) Minted.Create();
        }
    }
}
