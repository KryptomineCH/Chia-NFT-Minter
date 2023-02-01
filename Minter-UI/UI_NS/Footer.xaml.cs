using Chia_Client_API.Wallet_NS.WalletAPI_NS;
using CHIA_RPC.General;
using CHIA_RPC.Objects_NS;
using CHIA_RPC.Wallet_RPC_NS.DID_NS;
using CHIA_RPC.Wallet_RPC_NS.KeyManagement;
using CHIA_RPC.Wallet_RPC_NS.NFT;
using CHIA_RPC.Wallet_RPC_NS.Wallet_NS;
using CHIA_RPC.Wallet_RPC_NS.WalletManagement_NS;
using Minter_UI.Settings_NS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// Interaction logic for Footer.xaml
    /// </summary>
    public partial class Footer : UserControl
    {
        public Footer()
        {
            InitializeComponent();
            _ = RefreshWallets();
            _ = GetNftWallets();
            _ = ObtainStatusLoop(TimeSpan.FromSeconds(3));
        }
        List<ulong> WalletFingerprints = new List<ulong>();
        List<string> NftWallets = new List<string>();
        private async Task RefreshWallets()
        {

            // fill wallets combobox
            WalletFingerprints.Clear();
            GetPublicKeys_Response wallets = await WalletApi.GetPublicKeys_Async();
            WalletFingerprints = wallets.public_key_fingerprints.ToList();
            this.WalletSelector_ComboBox.ItemsSource = WalletFingerprints;
            // get currently logged in wallet
            LogIn_Response loggedInWallet = await WalletApi.GetLoggedInFingerprint_Async();
            // select currently logged in wallet
            if (Settings.All == null || Settings.All.PrimaryWallet == null || Settings.All.PrimaryWallet == ulong.MaxValue)
            {
                int index = WalletFingerprints.IndexOf(loggedInWallet.fingerprint);
                this.WalletSelector_ComboBox.SelectedIndex = index;
                if (Settings.All != null)
                {
                    Settings.All.PrimaryWallet = loggedInWallet.fingerprint;
                    Settings.Save();
                }
            }
            else if (Settings.All.PrimaryWallet != null)
            {
                int index = WalletFingerprints.IndexOf((ulong)Settings.All.PrimaryWallet);
                this.WalletSelector_ComboBox.SelectedIndex = index;
            }
        }

        private void WalletSelector_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ = Login();
        }
        private async Task Login()
        {
            ulong selectedWallet = (ulong)WalletSelector_ComboBox.SelectedItem;
            GlobalVar.CurrentlyLoggedInWallet.Value = await WalletApi.GetLoggedInFingerprint_Async();
            if (GlobalVar.CurrentlyLoggedInWallet.Value.fingerprint != selectedWallet && this.WalletSelector_ComboBox.IsEnabled)
            {
                // reset first sync indicator
                GlobalVar.FullSync = false;
                // log in
                this.WalletSelector_ComboBox.IsEnabled = false;
                StatusLabel.Content = "Logging in";
                Settings.All.PrimaryWallet = selectedWallet;
                Settings.Save();
                GlobalVar.CurrentlyLoggedInWallet.Value = await WalletApi.LogIn_Async(new FingerPrint_RPC { fingerprint = selectedWallet });
                GlobalVar.FullSync = false;
                this.WalletSelector_ComboBox.IsEnabled = true;
                GetNextAddress_RPC rpc = new GetNextAddress_RPC()
                {
                    new_address = false,
                    wallet_id = 1
                };
                GetNextAddress_Response adress = await WalletApi.GetNextAddress_Async(rpc);
                GlobalVar.PrimaryWalletAdress = adress.address;
                await GetNftWallets();
            }
        }
        /// <summary>
        /// this function gets the wallets associated with the currently logged in public key.</br>
        /// it then propagates all nft wallets with a did 
        /// </summary>
        /// <returns></returns>
        private async Task GetNftWallets(bool fullSync = false)
        {
            // get all associated Wallets
            GetWallets_Response subWallets = await WalletApi.GetWallets_Async();
            // refresh cache
            NftWallets.Clear();
            Wallets_info? selectedWallet = null;
            foreach (Wallets_info info in subWallets.wallets)
            {
                if (info.type == WalletType.did_wallet)
                {
                    NftWallets.Add(info.name);
                    if (Settings.All.DidWallet == info.name)
                    {
                        selectedWallet = info;
                    }
                }
            }
            if (NftWallets.Count <= 0)
            {
                MessageBox.Show($"Warning, you do not have a digital identity yet! {Environment.NewLine}" +
                    $"Please create a profile in the settings of the chia client. This will automatically create a digital identity for you.");
            }
            // update selector combobox
            NftWalletSelector_ComboBox.ItemsSource = NftWallets;
            // if there are selectable wallets, choost the first available nft wallet
            if (selectedWallet != null)
            {
                NftWalletSelector_ComboBox.SelectedIndex = NftWallets.IndexOf(selectedWallet.name);
            }
            else if (NftWallets.Count > 0)
            {
                NftWalletSelector_ComboBox.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// This function goes in a loop and checks the wallet sync status. </br>
        /// It updates the status label.
        /// </summary>
        /// <remarks>
        /// Does not refresh while WalletSelector_ComboBox is disabled because this is the case while an account switch is ocurring.
        /// </remarks>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        private async Task ObtainStatusLoop(TimeSpan timeSpan)
        {
            var periodicTimer = new PeriodicTimer(timeSpan);
            while (await periodicTimer.WaitForNextTickAsync())
            {
                // do not update while login/account switch is ongoing
                if (!WalletSelector_ComboBox.IsEnabled) continue;
                try
                {
                    // check which if the logged in wallet changed
                    GlobalVar.CurrentlyLoggedInWallet.Value = await WalletApi.GetLoggedInFingerprint_Async();
                    if (WalletSelector_ComboBox.SelectedItem == null ||
                        GlobalVar.CurrentlyLoggedInWallet.Value.fingerprint != (ulong)WalletSelector_ComboBox.SelectedItem)
                    {
                        GlobalVar.FullSync = false;
                        GetNftWallets();
                        
                    }
                    // obtain sync status and write it to globalvar
                    GlobalVar.SyncStatus.Value = await WalletApi.GetSyncStatus_Async();
                    // update SyncStatus Label in UI
                    if (GlobalVar.SyncStatus.Value.success)
                    {
                        GetNextAddress_RPC rpc = new GetNextAddress_RPC()
                        {
                            new_address = false,
                            wallet_id = 1
                        };
                        GetNextAddress_Response adress = await WalletApi.GetNextAddress_Async(rpc);
                        GlobalVar.PrimaryWalletAdress = adress.address;
                        if (GlobalVar.SyncStatus.Value.synced)
                        {
                            this.StatusLabel.Content = "Synced";
                            this.StatusLabel.Foreground = Brushes.Cyan;
                            if (!GlobalVar.FullSync)
                            {
                                // first full sync of the wallet! You may want to check subwallets
                                GlobalVar.FullSync = true;
                                GetNftWallets(fullSync: true);
                            }
                            LicenseCheck();
                        }
                        else if (GlobalVar.SyncStatus.Value.syncing)
                        {
                            this.StatusLabel.Content = "Syncing";
                            this.StatusLabel.Foreground = Brushes.Yellow;
                        }
                        else
                        {
                            this.StatusLabel.Content = "not Syncing";
                            this.StatusLabel.Foreground = Brushes.Red;
                        }
                    }
                }
                catch
                {
                    this.StatusLabel.Content = "not Syncing";
                    this.StatusLabel.Foreground = Brushes.Red;
                }
            }
        }
        private async Task LicenseCheck()
        {
            if (GlobalVar.Licensed) return;
            // get all associated Wallets
            GetWallets_Response subWallets = await WalletApi.GetWallets_Async();
            // refresh cache
            NftWallets.Clear();
            List<Task<NftGetNfts_Response>> allNftTasks = new List<Task<NftGetNfts_Response>>();
            foreach (Wallets_info info in subWallets.wallets)
            {
                if (info.type == WalletType.nft_wallet)
                {
                    WalletID_RPC id = new WalletID_RPC { wallet_id = info.id };
                    allNftTasks.Add(WalletApi.NftGetNfts_Async(id));
                }
            }
            await Task.WhenAll(allNftTasks.ToArray());
            string[] officialDids =
            {
                "0xc88c6f5e5f751143fd169dd34b78b58d79af0563990ab1ca609cb8b6562f9d84"
            };
            foreach (Task<NftGetNfts_Response> task in allNftTasks)
            {
                foreach (Nft nft in task.Result.nft_list)
                {
                    if (officialDids.Contains(nft.minter_did))
                    {
                        GlobalVar.Licensed = true;
                        License_Label.Foreground = Brushes.Blue;
                        License_Label.Content = "Prime";
                        ObtainLicense_Button.Visibility = System.Windows.Visibility.Collapsed;
                        return;
                    }
                }
            }
            License_Label.Foreground = Brushes.Orange;
            License_Label.Content = "Free";
        }

        private void ObtainLicense_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string url = "https://dexie.space/offers/col1cpt7ey5229nmn5r7gq5z670hl8dhpue5xxg4qm3z8aj0j33d9hwqm2nltw/XCH";
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void NftWalletSelector_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.All.DidWallet = (string)NftWalletSelector_ComboBox.SelectedItem;
            Settings.Save();
            // get wallet id
            GetWallets_Response subWallets = WalletApi.GetWallets_Sync();
            foreach (Wallets_info info in subWallets.wallets)
            {
                if (info.type == WalletType.did_wallet)
                {
                    NftWallets.Add(info.name);
                    if (Settings.All.DidWallet == info.name)
                    {
                        DidGetDid_Response didWallet = WalletApi.DidGetDid_Sync(new WalletID_RPC { wallet_id = info.id });
                        WalletID_Response id = WalletApi.NftGetByDID_Sync(new DidID_RPC() { did_id = didWallet.my_did} );
                        GlobalVar.NftWallet_ID = id.wallet_id;
                    }
                }
            }
            
        }
    }
}
