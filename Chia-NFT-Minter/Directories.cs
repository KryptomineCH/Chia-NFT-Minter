using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chia_NFT_Minter
{
    internal static class Directories
    {
        /// <summary>
        /// this directory stores the NFT files. Eg, the artwork, image, video or document
        /// </summary>
        internal static DirectoryInfo Nfts = new DirectoryInfo("final");
        /// <summary>
        /// this directory stores the preview images for the nfts.
        /// </summary>
        internal static DirectoryInfo Preview = new DirectoryInfo("preview");
        /// <summary>
        /// this directory stores the Metadata json files for the NFTs
        /// </summary>
        internal static DirectoryInfo Metadata = new DirectoryInfo("metadata");
        /// <summary>
        /// this directory stores the metadata json files which are used to describe the nft and its properties
        /// </summary>
        internal static DirectoryInfo Rpcs = new DirectoryInfo("rpcs");
        /// <summary>
        /// the offers directory can be used to store offer files
        /// </summary>
        internal static DirectoryInfo Offers = new DirectoryInfo("offers");
        internal static void InitializeDirectories()
        {
            if (!Nfts.Exists) Nfts.Create();
            if (!Preview.Exists) Preview.Create();
            if (!Metadata.Exists) Metadata.Create();
            if (!Rpcs.Exists) Rpcs.Create();
            if (!Offers.Exists) Offers.Create();
        }
    }
}
