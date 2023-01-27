using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Minter_UI.Settings_NS;
using Chia_NFT_Minter.CollectionInformation_ns;
using System.Threading;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Minter_UI.Tasks_NS;

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
        internal MintingPreview_ViewModel _viewModel;

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
            this._viewModel.Items.Clear();
            //return;
            List<FileInfo> additions = new List<FileInfo>();
            additions.AddRange(CollectionInformation.Information.ReadyToMint.Values);
            additions.AddRange(CollectionInformation.Information.MissingRPCs.Values);
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
                if (CollectionInformation.Information.RpcFiles.ContainsKey(key)
                    && !CollectionInformation.Information.PendingTransactions.ContainsKey(key))
                {
                    item.IsUploaded = true;
                }
                else if (CollectionInformation.Information.PendingTransactions.ContainsKey(key))
                {
                    item.IsMinting = true;
                }

                this._viewModel.Items.Add(item);
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
        private async void Mint_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Tasks_NS.MintNftFiles.MintingInProgress || Tasks_NS.UploadNftFiles.UploadingInProgress)
            {
                Mint_Button.IsEnabled = false;
                Mint_Button.Content = "Stopping";
                CancleProcessing.Cancel();
                Mint_Button.Background = Brushes.DarkKhaki;
                return;
            }
            else
            {
                CancleProcessing = new CancellationTokenSource();
                _ = Tasks_NS.UploadNftFiles.UploadAndGenerateRpcs_Task(
                    CancleProcessing.Token, 
                    _viewModel,
                    this).ContinueWith(t => {
                        if (t.IsFaulted)
                        {
                            // Handle exception here
                            MessageBox.Show($"An exception occurred: {t.Exception}");
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted)
                            .ConfigureAwait(false);
                _ = Tasks_NS.MintNftFiles.MintNfts_Task(CancleProcessing.Token, _viewModel, this).ContinueWith(t => {
                    if (t.IsFaulted)
                    {
                        // Handle exception here
                        MessageBox.Show($"An exception occurred: {t.Exception}");
                    }
                }, TaskContinuationOptions.OnlyOnFaulted)
                            .ConfigureAwait(false);
                Mint_Button.Content = "Stop!";
                Mint_Button.Background = Brushes.Red;
                await UpdateButtonAfterStartingTasks();
            }
        }
        private async Task UpdateButtonAfterStartingTasks()
        {
            while (Tasks_NS.MintNftFiles.MintingInProgress || Tasks_NS.UploadNftFiles.UploadingInProgress)
            {
                await Task.Delay(1000);
            }

            Mint_Button.Content = "Mint!";
            Mint_Button.Background = new SolidColorBrush(ColorHelper.ColorConverter.FromHex("#697a1f"));
            Mint_Button.IsEnabled = true;
        }
    }
}
