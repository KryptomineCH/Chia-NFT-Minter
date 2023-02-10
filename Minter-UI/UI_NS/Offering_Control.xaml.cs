using Minter_UI.CollectionInformation_ns;
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
            PublishToDexie_CheckBox.IsChecked = Settings_NS.Settings.All.PublishOffersTo_DexieSpace;
            PublishToSpaceScan_CheckBox.IsChecked = Settings_NS.Settings.All.PublishOffersTo_SpaceScan;
        }
        /// <summary>
        /// the viewmodel is beeiing hooked onto by the preview wrappanel and updated automatically with th observablecollection
        /// </summary>
        internal MintingPreview_ViewModel _viewModel;
        /// <summary>
        /// this token is passed to the subprocess to cancle offering when the user requests it
        /// </summary>
        private CancellationTokenSource CancleProcessing = new CancellationTokenSource();
        /// <summary>
        /// this bool is used to make sure the collection is only loaded once when this control comes into view and not every time
        /// </summary>
        bool Initialized = false;
        /// <summary>
        /// this function loads all nfts when the control is displayed. it uses Initialized to make sure this only happens once
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
        /// this function refreshes the collection directories and updated the displays accordingly
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
            additions.AddRange(CollectionInformation.Information.ReadyToOffer.Values);
            additions.AddRange(CollectionInformation.Information.OfferedFiles.Values);
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
            if (Tasks_NS.CreateNftOffers.OfferingInProgress || Tasks_NS.PublishOffers.OfferingInProgress)
            {
                Offer_Button.IsEnabled = false;
                Offer_Button.Content = "Stopping";
                CancleProcessing.Cancel();
                Offer_Button.Background = Brushes.DarkKhaki;
                return;
            }
            else
            {
                // Cancle task on app close
                CancleProcessing = new CancellationTokenSource();
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
                _ = CreateNftOffers.OfferNfts_Task(
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
                _ = PublishOffers.PublishOffers_Task(
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
        /// <summary>
        /// this task makes sure that the update button transferrs back to its initial state when the subprocess ends
        /// </summary>
        /// <returns></returns>
        private async Task UpdateButtonAfterStartingTasks()
        {
            while (Tasks_NS.MintNftFiles.MintingInProgress || Tasks_NS.UploadNftFiles.UploadingInProgress)
            {
                await Task.Delay(1000);
            }

            Offer_Button.Content = "Create Offers!";
            Offer_Button.Background = new SolidColorBrush(ColorHelper.ColorConverter.FromHex("#697a1f"));
            Offer_Button.IsEnabled = true;
        }
        /// <summary>
        /// saves the upload to dexie setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PublishToDexie_CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            bool uploadDexieOffers = (bool)PublishToDexie_CheckBox.IsChecked;
            if(Settings_NS.Settings.All.PublishOffersTo_DexieSpace != uploadDexieOffers)
            {
                Settings_NS.Settings.All.PublishOffersTo_DexieSpace = uploadDexieOffers;
                Settings_NS.Settings.Save();
            }
        }
        /// <summary>
        /// saves the upload to spacescan setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PublishToSpaceScan_CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            bool uploadSpacescanOffers = (bool)PublishToSpaceScan_CheckBox.IsChecked;
            if (Settings_NS.Settings.All.PublishOffersTo_SpaceScan != uploadSpacescanOffers)
            {
                Settings_NS.Settings.All.PublishOffersTo_SpaceScan = uploadSpacescanOffers;
                Settings_NS.Settings.Save();
            }
        }
    }
}
