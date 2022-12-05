﻿using Chia_NFT_Minter;
using CHIA_RPC;
using NFT.Storage.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        /// <summary>
        /// refreshes the directories and loads all nfts which are ready for minting into the minting preview 
        /// </summary>
        /// <param name="reloadDirs"></param>
        private void RefreshPreviews(bool reloadDirs = true)
        {
            if (reloadDirs)
            {
                CollectionInformation.ReLoadDirectories(GlobalVar.CaseSensitiveFilehandling);
            }
            this.Preview_WrapPanel.Children.Clear();
            foreach (FileInfo nftFile in CollectionInformation.NftFileInfos)
            {
                string nftName = System.IO.Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!GlobalVar.CaseSensitiveFilehandling)
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
        /// upload nft files to nft.storage <br/>
        /// create rpc <br/>
        /// mint (not yet implemented)
        /// create offer (not yet implemented)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mint_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.GetProperty("WalletID") == null)
            {
                MessageBox.Show("STOP: WalletID not specified! Please specify in settings");
                return;
            }
            if (Settings.GetProperty("LicenseLink") == null)
            {
                MessageBox.Show("STOP: NoLinkToLicenseSpeified! Please specify in settings");
                return;
            }
            int mintingFee = int.Parse(Settings.GetProperty("MintingFee"));
            foreach (FileInfo nftFile in CollectionInformation.NftFiles.Values)
            {
                string nftName = System.IO.Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!GlobalVar.CaseSensitiveFilehandling)
                {
                    key = key.ToLower();
                }
                if (CollectionInformation.MetadataFiles.ContainsKey(key) && !CollectionInformation.RpcFiles.ContainsKey(key))
                {
                    List<string> nftlinkList = new List<string>();
                    Task<NFT_File> nftUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(nftFile.FullName));
                    nftUploadTask.Wait();
                    nftlinkList.Add(nftUploadTask.Result.URL);
                    
                    List<string> metalinkList = new List<string>();
                    Task<NFT_File> metaUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(CollectionInformation.MetadataFiles[key]));
                    metaUploadTask.Wait();
                    metalinkList.Add(metaUploadTask.Result.URL);
                    if (Settings.GetProperty("Custom Link") != null)
                    {
                        string customLink = Settings.GetProperty("Custom Link");
                        if (!customLink.EndsWith("/")) customLink += "/";
                        Uri customLinkUri = new Uri(customLink+nftName);
                        nftlinkList.Add(customLink + Directories.Nfts.Name + "/" + nftFile.Name);
                        metalinkList.Add(customLink + Directories.Metadata.Name + "/" + CollectionInformation.MetadataFiles[key].Name);
                    }
                    List<string> licenseLinks = new List<string>();
                    licenseLinks.Add(Settings.GetProperty("LicenseLink"));
                    if (Settings.GetProperty("LicenseLink2") != null)
                    {
                        licenseLinks.Add(Settings.GetProperty("LicenseLink2"));
                    }
                    //file to be minted
                    NFT_Mint_RPC rpc = new NFT_Mint_RPC(
                        walletID: int.Parse(Settings.GetProperty("WalletID")),
                        nftLinks: nftlinkList.ToArray(),
                        metadataLinks: metalinkList.ToArray(),
                        licenseLinks: licenseLinks.ToArray(),
                        mintingFee_Mojos: mintingFee
                        );
                    rpc.Save(Path.Combine(Directories.Rpcs.FullName , (nftName+".rpc")));
                }
            }
            RefreshPreviews();
        }
    }
}
