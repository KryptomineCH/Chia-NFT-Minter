﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Minter_UI.Settings_NS;
using Chia_NFT_Minter.CollectionInformation_ns;
using Microsoft.Web.WebView2.Wpf;
using System.Threading;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CHIA_RPC.FullNode_RPC_NS;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// Interaction logic for Minting_Control.xaml
    /// </summary>
    public partial class Minting_Control : UserControl
    {
        public Minting_Control()
        {
            _viewModel = new MintingPreview_ViewModel();
            _viewModel.Items = new ObservableCollection<MintingItem>();
            this.DataContext = _viewModel;
            InitializeComponent();
            RefreshPreviews(false);
        }
        private CancellationTokenSource CancleProcessing = new CancellationTokenSource();
        private MintingPreview_ViewModel _viewModel;

        private void RefreshPreviews(bool reloadDirs = true)
        {
            bool caseSensitive = true;
            if (Settings.All != null)
            {
                caseSensitive = Settings.All.CaseSensitiveFileHandling;
            }
            if (reloadDirs)
            {
                CollectionInformation.ReloadAll(caseSensitive);
            }
            //this.Preview_WrapPanel.Children.Clear();
            int index = 0;
            foreach (FileInfo nftFile in CollectionInformation.Information.NftFiles.Values)
            {
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!caseSensitive)
                {
                    key = key.ToLower();
                }
                if (CollectionInformation.Information.MetadataFiles.ContainsKey(key) 
                    && !CollectionInformation.Information.RpcFiles.ContainsKey(key))
                {
                    //file to be minted
                    string data;
                    if (CollectionInformation.Information.NftPreviewFiles.ContainsKey(key))
                    {
                        data = CollectionInformation.Information.NftPreviewFiles[key].FullName;
                    }
                    else
                    {
                        data = nftFile.FullName;
                    }
                    this._viewModel.Items.Add(new MintingItem(data));
                    index++;
                }
            }
            { }
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
            if (Tasks_NS.MintNftFiles.MintingInProgress || Tasks_NS.UploadNftFiles.UploadingInProgress)
            {
                Mint_Button.IsEnabled = false;
                Mint_Button.Content = "Stopping";
                CancleProcessing.Cancel();
                Mint_Button.Background = Brushes.DarkKhaki;
                while (Tasks_NS.MintNftFiles.MintingInProgress || Tasks_NS.UploadNftFiles.UploadingInProgress)
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
                Tasks_NS.MintNftFiles.MintingInProgress = true;
                Tasks_NS.UploadNftFiles.UploadingInProgress = true;
                CancleProcessing = new CancellationTokenSource();
                _ = Tasks_NS.UploadNftFiles.UploadAndGenerateRpcs_Task(CancleProcessing.Token);
                _ = Tasks_NS.MintNftFiles.MintNfts_Task(CancleProcessing.Token);
                Mint_Button.Content = "Stop!";
                Mint_Button.Background = Brushes.Red;
            }
        }
    }
}
