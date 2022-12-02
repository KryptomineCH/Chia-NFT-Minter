using Chia_Metadata;
using Chia_Metadata_CHIP_0007_std;
using Chia_NFT_Minter;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for Metadata_Control.xaml
    /// </summary>
    public partial class Metadata_Control : UserControl
    {
        public Metadata_Control()
        {
            InitializeComponent();
            LoadNextMissingMetadata();
        }
        private FileInfo CurrentMetadataPath;
        private void LoadInformation(FileInfo file)
        {
            imageDisplay.Source = new Uri(file.FullName);
            string nftName = Path.GetFileNameWithoutExtension(file.FullName);
            CurrentMetadataPath = new FileInfo(Path.Combine(file.DirectoryName,nftName+".json"));
            ClearAttributesPanel();
            if (CurrentMetadataPath.Exists)
            {
                Metadata metadata = IO.Load(CurrentMetadataPath.FullName);
                this.NftName_TextBox.Text = metadata.name;
                this.Description_TextBox.Text = metadata.description;
                foreach (MetadataAttribute attribute in metadata.attributes)
                {
                    this.Attributes_StackPanel.Children.Add(new Attribute(attribute));
                }
            }
            else
            {
                this.NftName_TextBox.Text = nftName.Replace("_", " ").Replace("-", " - ");
                foreach(MetadataAttribute attribute in CollectionInformation.LikelyAttributes)
                {
                    this.Attributes_StackPanel.Children.Add(new Attribute(attribute));
                }
            }
        }
        private void ClearAttributesPanel()
        {
            for (int i = 1; i < this.Attributes_StackPanel.Children.Count; i++)
            {
                this.Attributes_StackPanel.Children.RemoveAt(1);
            }
        }
        private void LoadNextMissingMetadata()
        {
            if (CollectionInformation.MissingMetadata.Count == 0)
            {
                MessageBox.Show("Info: no missing Metadata files.");
                return;
            }
            MissingNFTIndex++;
            if (MissingNFTIndex >= CollectionInformation.MissingMetadata.Count) MissingNFTIndex = 0;
            FileInfo missingMetadataFile = CollectionInformation.MissingMetadata.ElementAt(MissingNFTIndex).Value;
            LoadInformation(missingMetadataFile);
        }
        private void LoadPreviousMissingMetadata()
        {
            if(CollectionInformation.MissingMetadata.Count == 0)
            {
                MessageBox.Show("Info: no missing Metadata files.");
                return;
            }
            MissingNFTIndex--;
            if (MissingNFTIndex < 0) MissingNFTIndex = CollectionInformation.MissingMetadata.Count - 1;
            FileInfo missingMetadataFile = CollectionInformation.MissingMetadata.ElementAt(MissingNFTIndex).Value;
            LoadInformation(missingMetadataFile);
        }
        private int ExistingNFTIndex = -1;
        private int MissingNFTIndex = -1;
        private void PrimarySubject_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Attributes_StackPanel.Children.Add(new Attribute());
        }

        private void PreviousExisting_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreviousMissing_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadPreviousMissingMetadata();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveAndNext_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextExisting_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextMissing_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadNextMissingMetadata();
        }
    }
}
