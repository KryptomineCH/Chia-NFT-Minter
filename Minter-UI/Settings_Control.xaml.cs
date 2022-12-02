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
        }

        private void NftStorsgeApiKeySet_Button_Click(object sender, RoutedEventArgs e)
        {
            NftStorageAccount.ApiKey = NftStorageApiKey_TextBox.Text;
            NftStorsgeApiKeySet_Button.Background = Brushes.LightBlue;
        }
    }
}
