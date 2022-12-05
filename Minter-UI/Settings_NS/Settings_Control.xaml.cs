using System;
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
    }
}
