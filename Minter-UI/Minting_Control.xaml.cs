using Chia_NFT_Minter;
using CHIA_RPC;
using NFT.Storage.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Minter_UI.Settings_NS;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for Minting_Control.xaml
    /// </summary>
    public partial class Minting_Control : UserControl
    {
        public Minting_Control()
        {
            InitializeComponent();
            RefreshPreviews(false);
        }
        private void RefreshPreviews(bool reloadDirs = true)
        {
            if (reloadDirs)
            {
                CollectionInformation.ReLoadDirectories(Settings.All.CaseSensitiveFileHandling);
            }
            this.Preview_WrapPanel.Children.Clear();
            foreach (FileInfo nftFile in CollectionInformation.NftFileInfos)
            {
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!Settings.All.CaseSensitiveFileHandling)
                {
                    key = key.ToLower();
                }
                if (CollectionInformation.MetadataFiles.ContainsKey(key) && !CollectionInformation.RpcFiles.ContainsKey(key))
                {
                    //file to be minted
                    MediaElement media = new MediaElement();
                    media.Source = new Uri(nftFile.FullName);
                    media.Width = 200;
                    media.Height = 200;
                    this.Preview_WrapPanel.Children.Add(media);
                }
            }
        }
        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreviews();
        }

        private void Mint_Button_Click(object sender, RoutedEventArgs e)
        {
            // pre-checks
            if (Settings.All.MintingWallet == null)
            {
                MessageBox.Show("STOP: WalletID not specified! Please specify in settings");
                return;
            }
            if (Settings.All.LicenseURL == null)
            {
                MessageBox.Show("STOP: NoLinkToLicenseSpeified! Please specify in settings");
                return;
            }
            // mint each nft
            foreach (FileInfo nftFile in CollectionInformation.NftFiles.Values)
            {
                // get nft name and identifier key
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!Settings.All.CaseSensitiveFileHandling)
                {
                    key = key.ToLower();
                }
                // check if this nft has metadata and is not yet minted
                if (CollectionInformation.MetadataFiles.ContainsKey(key) && !CollectionInformation.RpcFiles.ContainsKey(key))
                {
                    // upload files
                    /// upload nft file
                    List<string> nftlinkList = new List<string>();
                    Task<NFT_File> nftUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(nftFile.FullName));
                    nftUploadTask.Wait();
                    nftlinkList.Add(nftUploadTask.Result.URL);
                    /// upload metadata
                    List<string> metalinkList = new List<string>();
                    Task<NFT_File> metaUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(CollectionInformation.MetadataFiles[key]));
                    metaUploadTask.Wait();
                    metalinkList.Add(metaUploadTask.Result.URL);
                    // build link lists for rpc
                    if (Settings.All.CustomServerURL != null)
                    {
                        /// prepare custom url (~prefix)
                        string customLink = Settings.All.CustomServerURL;
                        if (!customLink.EndsWith("/")) customLink += "/";
                        /// finalize custom uri
                        nftlinkList.Add(customLink + Directories.Nfts.Name + "/" + nftFile.Name);
                        metalinkList.Add(customLink + Directories.Metadata.Name + "/" + CollectionInformation.MetadataFiles[key].Name);
                    }
                    /// add license url (is static)
                    List<string> licenseLinks = new List<string>();
                    licenseLinks.Add(Settings.All.LicenseURL);
                    if (Settings.All.LicenseURL_Backup != null)
                    {
                        licenseLinks.Add(Settings.All.LicenseURL_Backup);
                    }
                    // create rpc
                    NFT_Mint_RPC rpc = new NFT_Mint_RPC(
                        walletID: Settings.All.MintingWallet,
                        nftLinks: nftlinkList.ToArray(),
                        metadataLinks: metalinkList.ToArray(),
                        licenseLinks: licenseLinks.ToArray(),
                        mintingFee_Mojos: Settings.All.MintingFee
                        );
                    /// save rpc
                    rpc.Save(Path.Combine(Directories.Rpcs.FullName , (nftName+".rpc")));
                }
            }
            RefreshPreviews();
        }
    }
}
