using Minter_UI.Tasks_NS;
using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minter_UI.Settings_NS
{
    /// <summary>
    /// the settings control is the user control to interact with the settings. 
    /// Some properties in the settings file are not included here as they are managed automatically (persistent vars)
    /// or because the setting is in the apropriate minter option in form of a checkbox, input field or dropdown
    /// </summary>
    public partial class Settings_Control : UserControl
    {

        public Settings_Control()
        {
            InitializeComponent();
            if (NftStorageAccount.ApiKey != null && NftStorageAccount.ApiKey != "")
            {
                NftStorageApiKeySet_Button.Background = Brushes.LightBlue;
            }
            // load settings int ui
            if (Settings.All != null)
            {
                this.NftStorageApiKey_TextBox.Text = NftStorageAccount.ApiKey;
                this.OpenAIApiKey_TextBox.Text = OpenAiAccount.ApiKey;
                if (!string.IsNullOrEmpty( Settings.All.CustomServerURL))
                    this.NftCustomLink_TextBox.Text = Settings.All.CustomServerURL;
                this.LicenseLink_TextBox.Text = Settings.All.LicenseURL;
                this.LicenseLink2_TextBox.Text = Settings.All.LicenseURL_Backup;
                this.CaseSensitiveFilehandling_CheckBox.IsChecked = Settings.All.CaseSensitiveFileHandling;
                this.MintingFee_TextBox.Text = Settings.All.MintingFee.ToString();
                this.AutoUploadErrors_Checkbox.IsChecked = Settings.All.AutoUploadAnonymousErrorReport;
            }
            // check update
            UpdateCheck();
            { }
        }
        /// <summary>
        /// this uri is beeing used for the update check. It refers to the latest release page
        /// </summary>
        private string ReleasesURI = "https://github.com/KryptomineCH/Chia-NFT-Minter/releases/latest";
        /// <summary>
        /// this uri is th base to build the final download link from the file name
        /// </summary>
        private string DownloadURI = "https://github.com/KryptomineCH/Chia-NFT-Minter/releases/download/";
        /// <summary>
        /// bool that specifies if the update check was successful. 
        /// This is beeing used to update the ui accordingly or open the webpage for manual update
        /// </summary>
        private bool UpdateCheckSuccess = false;
        /// <summary>
        /// specifies to the ui if the update check button should be enabled and the text changed
        /// </summary>
        private bool NewerVersionAvailable = false;
        /// <summary>
        /// performs the check if a newer version is available
        /// </summary>
        /// <remarks>
        /// the function is in the settings ui because it specifies how the update button should look like and behave
        /// </remarks>
        private async void UpdateCheck()
        {
            HttpClient client = new HttpClient();
            // disable caching, we dont want to load an old page!
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            // fetch latest release page
            HttpResponseMessage queryResult = await client.GetAsync(ReleasesURI);
            if (!queryResult.IsSuccessStatusCode ||
                queryResult.RequestMessage == null ||
                queryResult.RequestMessage.RequestUri == null
                )
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
                Version? localVersion = Assembly.GetExecutingAssembly().GetName().Version;
                // check wether newer version exists
                if (localVersion == null)
                {
                    this.Update_Button.Content = "Updatecheck failed, click here to check Manually...";
                    UpdateCheckSuccess = false;
                    this.Update_Button.IsEnabled = true;
                    return;
                }
                if (onlineVersion.CompareTo(localVersion) > 0)
                {
                    NewerVersionAvailable = true;
                    this.Update_Button.Content = $"Update to {onlineVersion_string}";
                    this.Update_Button.IsEnabled = true;
                    DownloadURI += onlineVersion_string + "/Minter-UI.exe";
                }
                else
                {
                    this.Update_Button.Content = $"You have the newest version: {localVersion.ToString()}";
                    this.Update_Button.IsEnabled = false;
                    this.Update_Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5d995d"));
                }
                // wrap up update check
                UpdateCheckSuccess = true;
                FileInfo oldVersion = new FileInfo("Minter-UI-old.exe");
                if (oldVersion.Exists)
                {
                    oldVersion.Delete();
                    if (!NewerVersionAvailable)
                    {
                        MessageBox.Show($"Update Successful! {Environment.NewLine}New version: {localVersion.ToString()}");
                    }
                }
            }
            catch
            {
                this.Update_Button.Content = "Updatecheck failed, click here to check Manually...";
                UpdateCheckSuccess = false;
                this.Update_Button.IsEnabled = true;
                return;
            }
        }
        /// <summary>
        /// save the nft.storage api key to the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NftStorageApiKeySet_Button_Click(object sender, RoutedEventArgs e)
        {
            NftStorageAccount.ApiKey = NftStorageApiKey_TextBox.Text;
            NftStorageApiKeySet_Button.Background = Brushes.LightBlue;
            Settings.Save();
        }
        private void OpenAIApiKeySet_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenAiAccount.ApiKey = OpenAIApiKey_TextBox.Text;
            OpenAIApiKeySet_Button.Background = Brushes.LightBlue;
            Settings.Save();
        }
        /// <summary>
        /// set the custom url to the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NftCustomLinkSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.All == null)
            {
                NftCustomLink_TextBox.Text = "error";
                MessageBox.Show("setting settings failed, Settings.All == null");
                return;
            }
            Settings.All.CustomServerURL = NftCustomLink_TextBox.Text.Trim();
            Settings.Save();
        }
        /// <summary>
        /// save the link to your nft usage license
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LicenseLinkSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.All == null)
            {
                LicenseLink_TextBox.Text = "error";
                MessageBox.Show("setting settings failed, Settings.All == null");
                return;
            }
                Settings.All.LicenseURL = LicenseLink_TextBox.Text.Trim();
            Settings.All.LicenseURL_Backup = LicenseLink2_TextBox.Text.Trim();
            Settings.Save();
        }
        /// <summary>
        /// saves wether the application should treat files casesensitive or not.
        /// default is set to false because users might import manually edited files which might have typos (such as mine)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CaseSensitiveFilehandling_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.CaseSensitiveFilehandling_CheckBox.IsChecked == null)
            {
                Settings.All.CaseSensitiveFileHandling = false;
            }
            else
            {
                Settings.All.CaseSensitiveFileHandling = (bool)this.CaseSensitiveFilehandling_CheckBox.IsChecked;
            }
            Settings.Save();
        }
        // saves the minting fee
        private void MintingFeeSet_Button_Click(object sender, RoutedEventArgs e)
        {
            ulong mintingFee;
            if (!ulong.TryParse(MintingFee_TextBox.Text.Trim(), out mintingFee))
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
        /// <summary>
        /// executes the update intallation task async
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!UpdateCheckSuccess)
            {
                Process.Start(new ProcessStartInfo(ReleasesURI) { UseShellExecute = true });
                return;
            }
            if (NewerVersionAvailable)
            {
                // download file
                BackgroundWorker worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                worker.DoWork += Update.DownloadUpdate;
                worker.ProgressChanged += UpdateProgress;
                worker.RunWorkerCompleted += Update.UpdateCompleted;
                worker.RunWorkerAsync(DownloadURI);
            }
        }
        /// <summary>
        /// refreshes the progressbar which shows the update progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            Update_Progressbar.Value = e.ProgressPercentage;
        }

        private void AutoUploadErrors_Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.All.AutoUploadAnonymousErrorReport = this.AutoUploadErrors_Checkbox.IsChecked;
            Settings.Save();
        }
    }
}
