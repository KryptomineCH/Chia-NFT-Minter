namespace Minter_UI.Settings_NS
{
    internal class Settings_Object
    {
        public Settings_Object()
        {
            // set default settings
            CaseSensitiveFileHandling = false;
            MintingFee = 1000;
            LicenseURL = "https://bafkreicc3peq64kpclsssu344iroadtsvbloo7ofbkzdyyrqhybhvmblve.ipfs.nftstorage.link/";
            LicenseURL_Backup = "https://nft.kryptomine.ch/Kryptomine.ch_NFT_CreativeCommons_Attribution_License.pdf";
            PrimaryWallet = ulong.MaxValue;
        }
        // Metadata
        public string CustomServerURL { get; set; }
        public string LicenseURL { get; set; }
        public string LicenseURL_Backup { get; set; }
        public bool CaseSensitiveFileHandling { get; set; }
        // Minting
        public ulong PrimaryWallet { get; set; }
        public string DidWallet { get; set; }
        public ulong MintingFee { get; set; }
    }
}
