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
            LoadBasisInformation(true);
            LoadNextMissingMetadata();
        }
        private void LoadBasisInformation(bool firstLoad = false)
        {
            if (CollectionInformation.MetadataFiles.Count <= 0) return;
            FileInfo reference = CollectionInformation.MetadataFiles.First().Value;
            Metadata data = IO.Load(reference.FullName);
            if (firstLoad)
            {
                data = CollectionInformation.LastKnownNftMetadata;
                this.NftName_TextBox.Text = data.name;
                this.SensitiveContent_Checkbox.IsChecked = data.sensitive_content;
                this.SeriesTotal_TextBox.Text = data.series_total.ToString();
                this.Description_TextBox.Text = data.description;
                this.CollectionName_TextBox.Text = data.collection.name;
                foreach (CollectionAttribute colAttr in data.collection.attributes)
                {
                    this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute(colAttr));
                }
            }
            for (int i = 1; i < this.Attributes_StackPanel.Children.Count; i++)
            {
                this.Attributes_StackPanel.Children.RemoveAt(1);
            }
            foreach (MetadataAttribute attr in CollectionInformation.LikelyAttributes)
            {
                this.Attributes_StackPanel.Children.Add(new Attribute(attr));

            }
            { }
        }
        private void LoadNextMissingMetadata(bool reloaded = false)
        {

            FileInfo missingMetadataFile;
            if (CollectionInformation.MissingMetadata.TryDequeue(out missingMetadataFile))
            {
                LoadBasisInformation();
                //this.imageDisplay.Navigate(missingMetadataFile.FullName);
                this.imageDisplay.Source = new Uri(missingMetadataFile.FullName);
                //SeriesTotal_TextBox.Text = CollectionInformation.ReserveNextFreeCollectionNumber().ToString();
            }
            else if (reloaded == false)
            {
                CollectionInformation.ReLoadDirectories();
                LoadNextMissingMetadata(true);
            }
        }

        private void PrimarySubject_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Attributes_StackPanel.Children.Add(new Attribute());
        }

        private void CollectionAttributes_AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute());
        }
    }
}
