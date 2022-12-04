using System.Windows;
using Chia_NFT_Minter;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CollectionInformation.ReLoadDirectories(GlobalVar.CaseSensitiveFilehandling);
        }
    }
}
