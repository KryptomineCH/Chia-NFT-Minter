using System.Windows;
using Chia_NFT_Minter.CollectionInformation_ns;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // load collection information once
            CollectionInformation.ReloadAll(Settings_NS.Settings.All.CaseSensitiveFileHandling);
            InitializeComponent();
        }
    }
}
