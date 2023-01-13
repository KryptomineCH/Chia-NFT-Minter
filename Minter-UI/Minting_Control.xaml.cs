using Chia_NFT_Minter;
using NFT.Storage.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Minter_UI.Settings_NS;
using Chia_NFT_Minter.CollectionInformation_ns;
using Microsoft.Web.WebView2.Wpf;
using CHIA_RPC.Wallet_RPC_NS.NFT;
using CHIA_RPC.Wallet_RPC_NS.WalletManagement_NS;
using Chia_Client_API.Wallet_NS.WalletAPI_NS;
using CHIA_RPC.Wallet_RPC_NS.KeyManagement;
using System.Threading;
using System.Drawing;
using System.Windows.Media;

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
        private bool MintingInProgress = false;
        private bool UploadingInProgress = false;
        private CancellationTokenSource CancleProcessing = new CancellationTokenSource();
        /// <summary>
        /// refreshes the directories and loads all nfts which are ready for minting into the minting preview 
        /// </summary>
        /// <param name="reloadDirs"></param>
        private void RefreshPreviews(bool reloadDirs = true)
        {
            if (reloadDirs)
            {
                CollectionInformation.ReloadAll(Settings.All.CaseSensitiveFileHandling);
            }
            this.Preview_WrapPanel.Children.Clear();
            int previewcount = 20;
            int index = 0;
            foreach (FileInfo nftFile in CollectionInformation.Information.NftFileInfos)
            {
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!Settings.All.CaseSensitiveFileHandling)
                {
                    key = key.ToLower();
                }
                if (CollectionInformation.Information.MetadataFiles.ContainsKey(key) 
                    && !CollectionInformation.Information.RpcFiles.ContainsKey(key))
                {
                    //file to be minted
                    WebView2 media = new WebView2();
                    if (CollectionInformation.Information.NftPreviewFiles.ContainsKey(key))
                    {
                        media.Source = new Uri(CollectionInformation.Information.NftPreviewFiles[key].FullName);
                    }
                    else
                    {
                        media.Source = new Uri(nftFile.FullName);
                    }
                    media.Width = 200;
                    media.Height = 200;
                    this.Preview_WrapPanel.Children.Add(media);
                    index++;
                    if (index >= previewcount) return;
                }
            }
        }
        /// <summary>
        /// initiates a refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreviews();
        }
        /// <summary>
        /// this function will upload the files and generate the rpc for minting in advance.
        /// </summary>
        private async Task UploadAndGenerateRPCs()
        {
            if (UploadInProgress) return;
        }
        /// <summary>
        /// this function will process the mintable files
        /// </summary>
        /// <returns></returns>
        private async Task Mint()
        { 
            if (MintingInProgress) return;
            // pre-checks
            if (Settings.All.PrimaryWallet == null)
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
            foreach (FileInfo nftFile in CollectionInformation.Information.NftFiles.Values)
            {
                // get nft name and identifier key
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!Settings.All.CaseSensitiveFileHandling)
                {
                    key = key.ToLower();
                }
                // check if this nft has metadata and is not yet minted
                if (CollectionInformation.Information.MetadataFiles.ContainsKey(key)
                    && !CollectionInformation.Information.RpcFiles.ContainsKey(key))
                {
                    // upload files
                    /// upload nft file
                    List<string> nftlinkList = new List<string>();
                    Task<NFT_File> nftUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(nftFile.FullName));
                    nftUploadTask.Wait();
                    nftlinkList.Add(nftUploadTask.Result.URL);
                    /// upload metadata
                    List<string> metalinkList = new List<string>();
                    Task<NFT_File> metaUploadTask =
                        Task.Run(() => NftStorageAccount.Client.Upload(CollectionInformation.Information.MetadataFiles[key]));
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
                        metalinkList.Add(
                            customLink + Directories.Metadata.Name + "/" + CollectionInformation.Information.MetadataFiles[key].Name);
                    }
                    /// add license url (is static)
                    List<string> licenseLinks = new List<string>();
                    licenseLinks.Add(Settings.All.LicenseURL);
                    if (Settings.All.LicenseURL_Backup != null)
                    {
                        licenseLinks.Add(Settings.All.LicenseURL_Backup);
                    }
                    // create rpc
                    NftMintNFT_RPC rpc = new NftMintNFT_RPC(
                        walletID: (ulong)Settings.All.PrimaryWallet,
                        nftLinks: nftlinkList.ToArray(),
                        metadataLinks: metalinkList.ToArray(),
                        licenseLinks: licenseLinks.ToArray(),
                        mintingFee_Mojos: (ulong)Settings.All.MintingFee//,
                                                                        //royaltyAddress: WalletApi.
                        );
                    /// save rpc
                    rpc.Save(Path.Combine(Directories.Rpcs.FullName, (nftName + ".rpc")));
                }
            }
            RefreshPreviews();
        }
        /// <summary>
        /// upload nft files to nft.storage <br/>
        /// create rpc <br/>
        /// mint (not yet implemented)
        /// create offer (not yet implemented)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mint_Button_Click(object sender, RoutedEventArgs e)
        {
            if (MintingInProgress || UploadingInProgress)
            {
                Mint_Button.IsEnabled = false;
                Mint_Button.Content = "Stopping";
                CancleProcessing.Cancel();
                Mint_Button.Background = System.Windows.Media.Brushes.DarkKhaki;
                while (MintingInProgress || UploadingInProgress)
                {
                    Task.Delay(1000).Wait();
                }
                Mint_Button.Content = "Mint!";
                Mint_Button.Background = new SolidColorBrush(ColorHelper.ColorConverter.FromHex("#697a1f"));
                Mint_Button.IsEnabled = true;
                return;
            }
            else
            {
                MintingInProgress = true;
                UploadingInProgress = true;
                CancleProcessing = new CancellationTokenSource();
                Mint();
                UploadAndGenerateRPCs();
                Mint_Button.Content = "Stop!";
                Mint_Button.Background = System.Windows.Media.Brushes.Red;
            }
        }
    }
}
