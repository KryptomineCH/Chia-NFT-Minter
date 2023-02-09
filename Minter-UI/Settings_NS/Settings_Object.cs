using Microsoft.Windows.Themes;

namespace Minter_UI.Settings_NS
{
    /// <summary>
    /// the settings object contains all application settings. it is used to serialize/deserialize the jsons
    /// </summary>
    internal class Settings_Object
    {
        // Metadata
        /// <summary>
        /// you can use your own webserver by providing the files in the same folder structure than from the tool.
        /// in fact, the tool is designed to be kept in the folder on the webserver. Then the paths will be automatically correct.
        /// Provide the URL to the base path.
        /// </summary>
        public string? CustomServerURL { get; set; }
        /// <summary>
        /// the url to the License which should be minted into the nfts
        /// </summary>
        public string LicenseURL { get; set; } = "https://bafkreicc3peq64kpclsssu344iroadtsvbloo7ofbkzdyyrqhybhvmblve.ipfs.nftstorage.link/";
        /// <summary>
        /// the backup license file link which should be minted into the nft. File must be equal to LisendURL file
        /// </summary>
        public string LicenseURL_Backup { get; set; } = "https://nft.kryptomine.ch/Kryptomine.ch_NFT_CreativeCommons_Attribution_License.pdf";
        /// <summary>
        /// specifies if there is a difference to DifFereNcE
        /// </summary>
        public bool CaseSensitiveFileHandling { get; set; } = false;
        // Minting
        /// <summary>
        /// the primary wallet which should be used for minting
        /// </summary>
        public ulong? PrimaryWallet { get; set; } = ulong.MaxValue;
        /// <summary>
        /// the did wallet which should be used for minting
        /// </summary>
        public string? DidWallet { get; set; }
        /// <summary>
        /// the minting fee in mojos which should be used to mint one nft
        /// </summary>
        public ulong MintingFee { get; set; } = 1000;
        // Offering
        /// <summary>
        /// the price in xch for which the nfts should be offered
        /// </summary>
        public decimal OfferingPrice { get; set; } = (decimal)0.25;
        /// <summary>
        /// this is used to determine whch folder the importer should open. is updatd automatically
        /// </summary>
        public string LastImportPath { get; set; }
    }
}
