using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minter_UI.Settings_NS
{
    /// <summary>
    /// Interaction logic for Settings_Control.xaml
    /// </summary>
    public partial class Settings_Control : UserControl
    {
        public Settings_Control()
        {
            InitializeComponent();
            if (NftStorageAccount.ApiKey != "")
            {
                NftStorageApiKeySet_Button.Background = Brushes.LightBlue;
            }
            this.NftCustomLink_TextBox.Text = Settings.All.CustomServerURL;
            this.WalletID_TextBox.Text = Settings.All.MintingWallet.ToString();
            this.LicenseLink_TextBox.Text = Settings.All.LicenseURL;
            this.LicenseLink2_TextBox.Text = Settings.All.LicenseURL_Backup;
            this.CaseSensitiveFilehandling_CheckBox.IsChecked = Settings.All.CaseSensitiveFileHandling;
            this.MintingFee_TextBox.Text = Settings.All.MintingFee.ToString();
            // check update
            UpdateCheck();
            { }
        }
        private string ReleasesURI = "https://github.com/KryptomineCH/Chia-NFT-Minter/releases/latest";
        private string DownloadURI = "https://github.com/KryptomineCH/Chia-NFT-Minter/releases/download/";
        private bool UpdateCheckSuccess = false;
        private bool NewerVersionAvailable = false;
        private async void UpdateCheck()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            HttpResponseMessage queryResult = await client.GetAsync(ReleasesURI);
            if (!queryResult.IsSuccessStatusCode)
            {
                this.Update_Button.Content = "Updatecheck failed, click here to check Manually...";
                UpdateCheckSuccess = false;
                this.Update_Button.IsEnabled = true;
                return;
            }
            ReleasesURI = queryResult.RequestMessage.RequestUri.ToString();
            // eg v1.2.342-alpha
            try
            {
                string onlineVersion_string = ReleasesURI.Split('/').Last();
                // extract version numbers such as 1.2.342
                string onlineVersion_extractedString = onlineVersion_string.Split('-')[0].Remove(0,1);
                Version onlineVersion = new Version(onlineVersion_extractedString);

                string localVersion_string = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Version localVersion = new Version(localVersion_string);
                if (onlineVersion.CompareTo(localVersion) > 0)
                {
                    NewerVersionAvailable = true;
                    this.Update_Button.Content = $"Update to {onlineVersion_string}";
                    this.Update_Button.IsEnabled = true;
                    DownloadURI += onlineVersion_string + "/Minter-UI.exe";
                }
                else
                {
                    this.Update_Button.Content = $"You have the newest version: {localVersion_string}";
                    this.Update_Button.IsEnabled = false;
                    this.Update_Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5d995d"));
                }
                UpdateCheckSuccess = true;
                FileInfo oldVersion = new FileInfo("Minter-UI-old.exe");
                if (oldVersion.Exists)
                {
                    oldVersion.Delete();
                    if (!NewerVersionAvailable)
                    {
                        MessageBox.Show($"Update Successful! {Environment.NewLine}New version: {localVersion_string}");
                    }
                }
            }
            catch (Exception ex)
            {
                this.Update_Button.Content = "Updatecheck failed, click here to check Manually...";
                UpdateCheckSuccess = false;
                this.Update_Button.IsEnabled = true;
                return;
            }
        }
        private void NftStorageApiKeySet_Button_Click(object sender, RoutedEventArgs e)
        {
            NftStorageAccount.ApiKey = NftStorageApiKey_TextBox.Text;
            NftStorageApiKeySet_Button.Background = Brushes.LightBlue;
            Settings.Save();
        }

        private void NftCustomLinkSet_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.All.CustomServerURL = NftCustomLink_TextBox.Text.Trim();
            Settings.Save();
        }

        private void WalletIDSet_Button_Click(object sender, RoutedEventArgs e)
        {
            int wallet;
            if(!int.TryParse(this.WalletID_TextBox.Text.Trim(),out wallet))
            {
                this.WalletID_TextBox.Background = Brushes.LightCoral;
                MessageBox.Show($"STOP: Wallet ID couldnt be parsed into a number! {Environment.NewLine}example: 3");
                return;
            }
            else
            {
                Settings.All.MintingWallet = wallet;
                this.WalletID_TextBox.Background = null;
                Settings.Save();
            }
        }

        private void LicenseLinkSet_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.All.LicenseURL = LicenseLink_TextBox.Text.Trim();
            Settings.All.LicenseURL_Backup = LicenseLink2_TextBox.Text.Trim();
            Settings.Save();
        }

        private void CaseSensitiveFilehandling_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.All.CaseSensitiveFileHandling = (bool)this.CaseSensitiveFilehandling_CheckBox.IsChecked;
            Settings.Save();
        }

        private void MintingFeeSet_Button_Click(object sender, RoutedEventArgs e)
        {
            int mintingFee;
            if (!int.TryParse(MintingFee_TextBox.Text.Trim(), out mintingFee))
            {
                MintingFee_TextBox.Background = Brushes.LightCoral;
                MessageBox.Show($"STOP: Minting fee couldnt be parsed into a number! {Environment.NewLine}example: 10000");
                return;
            }
            else
            {
                MintingFee_TextBox.Background = null;
                Settings.All.MintingFee = mintingFee;
                Settings.Save();
            }
        }

        private void Update_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!UpdateCheckSuccess)
            {
                Process.Start(new ProcessStartInfo(ReleasesURI) { UseShellExecute = true });
                return;
            }
            if (NewerVersionAvailable)
            {
                MessageBox.Show("the download might take a while depending on your connection speed!");
                // download file
                byte[] fileData = NFT.Storage.Net.API.DownloadClient.DownloadSync(DownloadURI);
                // rename old exe
                File.Move("Minter-UI.exe", "Minter-UI-old.exe");
                // write new exe
                File.WriteAllBytes("Minter-UI.exe", fileData);
                // start updated app
                Process updatedApp = new Process();
                updatedApp.StartInfo.FileName = "Minter-UI.exe";
                updatedApp.Start();
                // stop current process
                Environment.Exit(0);
            }
        }
    }
}
