using Chia_Metadata;
using Chia_Metadata_CHIP_0007_std;
using Chia_NFT_Minter;
using Chia_NFT_Minter.CollectionInformation_ns;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for Metadata editing
    /// </summary>
    public partial class Metadata_Control : UserControl
    {
        public Metadata_Control()
        {
            InitializeComponent();
            LoadNextMissingMetadata();
        }
        private FileInfo CurrentMetadataPath;
        /// <summary>
        /// Load metadata information into the ui for editing
        /// </summary>
        /// <param name="file"></param>
        /// <param name="reloadMetadata"></param>
        private void LoadInformation(FileInfo file, bool reloadMetadata = false)
        {
            string nftName = Path.GetFileNameWithoutExtension(file.FullName);
            string key = nftName;
            if (!Settings_NS.Settings.All.CaseSensitiveFileHandling)
            {
                key = key.ToLower();
            }
            imageDisplay.Source = new Uri(CollectionInformation.Information.NftFiles[key].FullName);
            if (CollectionInformation.Information.MetadataFiles.ContainsKey(key))
            {
                CurrentMetadataPath = CollectionInformation.Information.MetadataFiles[key];
            }
            else
            {
                CurrentMetadataPath = new FileInfo(Path.Combine(Directories.Metadata.FullName, nftName + ".json"));
            }
            if (!reloadMetadata)
            {
                this.NftName_TextBox.Text = nftName.Replace("_", " ").Replace("-", " - ");
                return;
            }
            ClearAttributesPanel();
            if (CurrentMetadataPath.Exists)
            {
                Metadata metadata = IO.Load(CurrentMetadataPath.FullName);
                this.NftName_TextBox.Text = metadata.name;
                this.Description_TextBox.Text = metadata.description;
                this.SensitiveContent_Checkbox.IsChecked = metadata.sensitive_content;
                foreach (MetadataAttribute attribute in metadata.attributes)
                {
                    this.Attributes_StackPanel.Children.Add(new Attribute(attribute));
                }
            }
            else
            {
                this.NftName_TextBox.Text = nftName.Replace("_", " ").Replace("-", " - ");
                foreach (MetadataAttribute attribute in CollectionInformation.Information.LikelyAttributes)
                {
                    this.Attributes_StackPanel.Children.Add(new Attribute(attribute));
                }
            }
        }
        /// <summary>
        /// clears the attribute panel to prepare for new metadata to be loaded
        /// </summary>
        private void ClearAttributesPanel()
        {
            while (this.Attributes_StackPanel.Children.Count > 1)
            {
                this.Attributes_StackPanel.Children.RemoveAt(1);
            }
        }
        /// <summary>
        /// looks for the next nft which does not have metadata and loads it
        /// </summary>
        /// <param name="loadMetadata"></param>
        private void LoadNextMissingMetadata(bool loadMetadata = true)
        {
            if (CollectionInformation.Information.MissingMetadata.Count == 0)
            {
                MessageBox.Show("Info: no missing Metadata files.");
                return;
            }
            MissingNFTIndex++;
            if (MissingNFTIndex >= CollectionInformation.Information.MissingMetadata.Count) MissingNFTIndex = 0;
            FileInfo missingMetadataFile = CollectionInformation.Information.MissingMetadata.ElementAt(MissingNFTIndex).Value;
            LoadInformation(missingMetadataFile,loadMetadata);
        }
        /// <summary>
        /// loads the next nft with existing metadata into the ui
        /// </summary>
        private void LoadNextExistingMetadata()
        {
            if (CollectionInformation.Information.MetadataFiles.Count == 0)
            {
                MessageBox.Show("Info: no Metadata files.");
                return;
            }
            ExistingNFTIndex++;
            if (ExistingNFTIndex >= CollectionInformation.Information.MetadataFiles.Count) ExistingNFTIndex = 0;
            FileInfo existingMetadataFile = CollectionInformation.Information.MetadataFiles.ElementAt(ExistingNFTIndex).Value;
            LoadInformation(existingMetadataFile);
        }
        /// <summary>
        /// loads the previus nft withiout metadata into the ui
        /// </summary>
        private void LoadPreviousMissingMetadata()
        {
            if (CollectionInformation.Information.MissingMetadata.Count == 0)
            {
                MessageBox.Show("Info: no missing Metadata files.");
                return;
            }
            MissingNFTIndex--;
            if (MissingNFTIndex < 0) MissingNFTIndex = CollectionInformation.Information.MissingMetadata.Count - 1;
            FileInfo missingMetadataFile = CollectionInformation.Information.MissingMetadata.ElementAt(MissingNFTIndex).Value;
            LoadInformation(missingMetadataFile);
        }
        /// <summary>
        /// loads the previous nft with existing metadata into the ui
        /// </summary>
        private void LoadPreviousExistingMetadata()
        {
            if (CollectionInformation.Information.MetadataFiles.Count == 0)
            {
                MessageBox.Show("Info: no Metadata files.");
                return;
            }
            ExistingNFTIndex--;
            if (ExistingNFTIndex < 0) ExistingNFTIndex = CollectionInformation.Information.MetadataFiles.Count - 1;
            FileInfo existingMetadataFile = CollectionInformation.Information.MetadataFiles.ElementAt(ExistingNFTIndex).Value;
            LoadInformation(existingMetadataFile);
        }
        private int ExistingNFTIndex = -1;
        private int MissingNFTIndex = -1;
        /// <summary>
        /// adds a ner attribute to the attributes stack panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrimarySubject_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Attributes_StackPanel.Children.Add(new Attribute());
        }

        private void PreviousExisting_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearAttributesPanel();
            LoadPreviousExistingMetadata();
        }

        private void PreviousMissing_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadPreviousMissingMetadata();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadata();
        }

        private void SaveAndNext_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadata();
            LoadNextExistingMetadata();
        }

        private void NextExisting_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearAttributesPanel();
            LoadNextExistingMetadata();
        }

        private void NextMissing_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadNextMissingMetadata();
        }

        private void SaveAndNextMissing_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadata();
            LoadNextMissingMetadata(false);
        }
        private void SaveMetadata()
        {
            // pre check
            string nftName = Path.GetFileNameWithoutExtension(CurrentMetadataPath.FullName);
            if (CollectionInformation.Information.RpcFiles.ContainsKey(nftName))
            {
                // nft is potentially minted!
                MessageBox.Show($"STOP: RPC for this nft was found! {Environment.NewLine} The nft is likely minted! {Environment.NewLine} If you wish to overwrite the information, please delete {nftName}.rpc and press refresh");
                return;
            }
            // load collection information
            Metadata metadata = IO.Load(Path.Combine(Directories.Metadata.FullName, "CollectionInfo.json"));
            // get / loand series number
            if (CollectionInformation.Information.MetadataFiles.ContainsKey(nftName))
            {
                // load seriesnumber from existing metadata
                Metadata oldMetadata = IO.Load(CurrentMetadataPath.FullName);
                metadata.series_number = oldMetadata.series_number;
            }
            else
            {
                // reserve next free series number
                metadata.series_number = CollectionInformation.Information.ReserveNextFreeCollectionNumber();
            }
            // fill meta information
            metadata.name = this.NftName_TextBox.Text;
            metadata.description = this.Description_TextBox.Text;
            metadata.minting_tool = "KryptoMine Chia-Nft-Minter";
            metadata.sensitive_content = (bool)this.SensitiveContent_Checkbox.IsChecked;
            metadata.attributes.Clear();
            for (int i = 1; i < this.Attributes_StackPanel.Children.Count; i++)
            {
                metadata.attributes.Add(((Attribute)this.Attributes_StackPanel.Children[i]).Value);
            }
            metadata.Save(CurrentMetadataPath.FullName);
            if (!CollectionInformation.Information.MetadataFiles.ContainsKey(nftName))
            {
                CollectionInformation.ReloadAll(Settings_NS.Settings.All.CaseSensitiveFileHandling);
            }
        }

        private void RefreshCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            CollectionInformation.ReloadAll(Settings_NS.Settings.All.CaseSensitiveFileHandling);
        }
    }
}
