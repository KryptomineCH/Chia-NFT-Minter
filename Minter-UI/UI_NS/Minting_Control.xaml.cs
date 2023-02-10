using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Minter_UI.Tasks_NS;
using System;
using Minter_UI.CollectionInformation_ns;

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
        }
        /// <summary>
        /// the token is passed to the minting and upoad task to stop them if requested by the user
        /// </summary>
        private CancellationTokenSource CancleProcessing = new CancellationTokenSource();
        /// <summary>
        /// the viewmodel is beeing hooked on by the nft preview wrappanel
        /// and automatically updates it with its observable collection
        /// </summary>
        internal MintingPreview_ViewModel _viewModel;
        /// <summary>
        /// this bool is beeing used to determine if the view has been refreshed on load yet or not.
        /// It is used to prevent a full load every time the tab is switched.
        /// </summary>
        bool Initialized = false;
        /// <summary>
        /// this function loads the nfts to display, but only on first visibility event 
        /// (eg the tab is selected in tab control)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UserControl userControl = sender as UserControl;
            if (userControl.IsVisible && !Initialized)
            {
                RefreshPreviews(false);
                Initialized = true;
            }
        }
        /// <summary>
        /// this button manually refreshes the directories and previews
        /// </summary>
        /// <param name="reloadDirs"></param>
        private async void RefreshPreviews(bool reloadDirs = true)
        {
            await MintNftFiles.CheckPendingTransactions(CancellationToken.None).ConfigureAwait(false);
            if (reloadDirs)
            {
                CollectionInformation.ReloadAll();
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                this._viewModel.Items.Clear();
            }));
            //return;
            List<FileInfo> additions = new List<FileInfo>();
            additions.AddRange(CollectionInformation.Information.ReadyToMint.Values);
            additions.AddRange(CollectionInformation.Information.MissingRPCs.Values);
            foreach (FileInfo nftFile in additions)
            {
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = CollectionInformation.GetKeyFromFile(nftFile);

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

                this.Dispatcher.Invoke(new Action(() =>
                {
                    this._viewModel.Items.Add(item);
                }));
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
                // cancle task on app close
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => {
                    CancleProcessing.Cancel();
                    CancleProcessing.Dispose();
                };
                // cancle task on app crash
                AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                    if (args.IsTerminating)
                    {
                        CancleProcessing.Cancel();
                        CancleProcessing.Dispose();
                    }
                };
                UploadNftFiles.UploadingInProgress = true;
                Tasks_NS.MintNftFiles.MintingInProgress = true;
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
        /// <summary>
        /// this function makes sure that the mind button transfers back to its starting state
        /// after all subprocesses have been ended/cancelled
        /// </summary>
        /// <returns></returns>
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
