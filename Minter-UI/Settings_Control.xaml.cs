using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Minter_UI
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
                NftStorsgeApiKeySet_Button.Background = Brushes.LightBlue;
            }
            this.NftCustomLink_TextBox.Text = Settings.GetProperty("Custom Link");
            this.WalletID_TextBox.Text = Settings.GetProperty("WalletID");
            this.LicenseLink_TextBox.Text = Settings.GetProperty("LicenseLink");
            this.LicenseLink2_TextBox.Text = Settings.GetProperty("LicenseLink2");
            string caseSensitive = Settings.GetProperty("CaseSensitiveFilehandling");
            if (caseSensitive == "True")
            {
                this.CaseSensitiveFilehandling_CheckBox.IsChecked = true;
                GlobalVar.CaseSensitiveFilehandling = true;
            }
            else
            {
                this.CaseSensitiveFilehandling_CheckBox.IsChecked = false;
                GlobalVar.CaseSensitiveFilehandling = false;
            }
            if (Settings.GetProperty("MintingFee") == null)
            {
                Settings.SetProperty("MintingFee", this.MintingFee_TextBox.Text.Trim());
            }
            else
            {
                this.MintingFee_TextBox.Text = Settings.GetProperty("MintingFee");
            }
        }

        private void NftStorsgeApiKeySet_Button_Click(object sender, RoutedEventArgs e)
        {
            NftStorageAccount.ApiKey = NftStorageApiKey_TextBox.Text;
            NftStorsgeApiKeySet_Button.Background = Brushes.LightBlue;
        }

        private void NftCustomLinkSet_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.SetProperty("Custom Link", this.NftCustomLink_TextBox.Text);
        }

        private void WalletIDSet_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.SetProperty("WalletID", this.WalletID_TextBox.Text);
        }

        private void LicenseLinkSet_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.SetProperty("LicenseLink", this.LicenseLink_TextBox.Text);
            Settings.SetProperty("LicenseLink2", this.LicenseLink_TextBox.Text);
        }

        private void CaseSensitiveFilehandling_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            GlobalVar.CaseSensitiveFilehandling = (bool)this.CaseSensitiveFilehandling_CheckBox.IsChecked;
            Settings.SetProperty("CaseSensitiveFilehandling", GlobalVar.CaseSensitiveFilehandling.ToString());
        }

        private void MintingFeeSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(MintingFee_TextBox.Text.Trim(), out _))
            {
                MintingFee_TextBox.Background = Brushes.LightCoral;
                MessageBox.Show($"STOP: Minting fee coulldnt be parsed into a number! {Environment.NewLine}example: 10000");
                return;
            }
            else
            {
                MintingFee_TextBox.Background = null;
                Settings.SetProperty("MintingFee", MintingFee_TextBox.Text.Trim());
            }
        }
    }
}
