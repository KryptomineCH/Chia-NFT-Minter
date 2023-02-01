﻿using Chia_NFT_Minter.CollectionInformation_ns;
using Minter_UI.Settings_NS;
using Minter_UI.Tasks_NS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// Interaction logic for Offering_Control.xaml
    /// </summary>
    public partial class Offering_Control : UserControl
    {
        public Offering_Control()
        {
            _viewModel = new MintingPreview_ViewModel();
            _viewModel.Items = new ObservableCollection<MintingItem>();
            this.DataContext = _viewModel;
            InitializeComponent();
            RefreshPreviews(false);
        }
        internal MintingPreview_ViewModel _viewModel;
        private CancellationTokenSource CancleProcessing = new CancellationTokenSource();
        private async void RefreshPreviews(bool reloadDirs = true)
        {
            bool caseSensitive = true;
            await MintNftFiles.CheckPendingTransactions().ConfigureAwait(false);
            if (Settings.All != null)
            {
                caseSensitive = Settings.All.CaseSensitiveFileHandling;
            }
            if (reloadDirs)
            {
                CollectionInformation.ReloadAll(caseSensitive);
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                this._viewModel.Items.Clear();
            }));
            
            //return;
            List<FileInfo> additions = new List<FileInfo>();
            additions.AddRange(CollectionInformation.Information.ReadyToOffer.Values);
            additions.AddRange(CollectionInformation.Information.OfferedFiles.Values);
            foreach (FileInfo nftFile in additions)
            {
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!caseSensitive)
                {
                    key = key.ToLower();
                }

                // add nft to ui
                string data;
                if (CollectionInformation.Information.NftPreviewFiles.ContainsKey(key))
                {
                    data = CollectionInformation.Information.NftPreviewFiles[key].FullName;
                }
                else
                {
                    data = CollectionInformation.Information.NftFiles[key].FullName;
                }
                MintingItem item = new MintingItem(data);
                if (CollectionInformation.Information.OfferedFiles.ContainsKey(key))
                {
                    item.IsUploaded = true;
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this._viewModel.Items.Add(item);
                }));
                
                { }
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
        private async void Offer_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Tasks_NS.MintNftFiles.MintingInProgress || Tasks_NS.UploadNftFiles.UploadingInProgress)
            {
                Offer_Button.IsEnabled = false;
                Offer_Button.Content = "Stopping";
                CancleProcessing.Cancel();
                Offer_Button.Background = Brushes.DarkKhaki;
                return;
            }
            else
            {
                CancleProcessing = new CancellationTokenSource();
                _ = Tasks_NS.CreateNftOffers.OfferNfts_Task(
                    CancleProcessing.Token,
                    _viewModel,
                    this).ContinueWith(t => {
                        if (t.IsFaulted)
                        {
                            // Handle exception here
                            _ = MessageBox.Show($"An exception occurred: {t.Exception}");
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted)
                            .ConfigureAwait(false);
                Offer_Button.Content = "Stop!";
                Offer_Button.Background = Brushes.Red;
                await UpdateButtonAfterStartingTasks();
            }
        }
        private async Task UpdateButtonAfterStartingTasks()
        {
            while (Tasks_NS.MintNftFiles.MintingInProgress || Tasks_NS.UploadNftFiles.UploadingInProgress)
            {
                await Task.Delay(1000);
            }

            Offer_Button.Content = "Mint!";
            Offer_Button.Background = new SolidColorBrush(ColorHelper.ColorConverter.FromHex("#697a1f"));
            Offer_Button.IsEnabled = true;
        }
    }
}