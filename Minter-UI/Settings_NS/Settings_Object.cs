namespace Minter_UI.Settings_NS
{
    internal class Settings_Object
    {
        public Settings_Object()
        {
            // set default settings
            CaseSensitiveFileHandling = false;
            MintingFee = 10000;
        }
        // Metadata
        public string CustomServerURL { get; set; }
        public string LicenseURL { get; set; }
        public string LicenseURL_Backup { get; set; }
        public bool CaseSensitiveFileHandling { get; set; }
        // Minting
        public int MintingWallet { get; set; }
        public int MintingFee { get; set; }
    }
}
