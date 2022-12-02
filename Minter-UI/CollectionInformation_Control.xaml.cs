using Chia_Metadata;
using Chia_Metadata_CHIP_0007_std;
using Chia_NFT_Minter;
using NFT.Storage.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for CollectionInformation_Control.xaml
    /// </summary>
    public partial class CollectionInformation_Control : UserControl
    {
        public CollectionInformation_Control()
        {
            InitializeComponent();
            InitializeCollection();
        }
        private FileInfo HeaderImageFile;
        private FileInfo LogoImageFile;
        private FileInfo CollectionInformationFile = new FileInfo(
            Path.Combine(Directories.Metadata.FullName, "CollectionInfo.json"));
        Metadata CollectionMetadata = new Metadata();

        private void CollectionAttributes_AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute());
        }
        private void InitializeCollection()
        {
            FileInfo[] files = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();
            // TRY LOAD images
            foreach (FileInfo file in files)
            {
                if (file.Name.ToLower().Contains("banner") &&
                    (file.Extension == ".png" || file.Extension == ".jpg" || file.Extension == ".webp"
                    ))
                {
                    this.headerImageDisplay.Source = new System.Uri(file.FullName);
                    HeaderImageFile = file;
                }
                else if
                    (file.Name.ToLower().Contains("logo") &&
                    (file.Extension == ".png" || file.Extension == ".jpg" || file.Extension == ".webp"
                    ))
                {
                    this.logoImageDisplay.Source = new System.Uri(file.FullName);
                    LogoImageFile = file;
                }
            }
            if (HeaderImageFile == null)
            {
                File.WriteAllBytes("banner.png", Properties.Resources.missing_collection_banner);
                HeaderImageFile = new FileInfo("banner.png");
                this.headerImageDisplay.Source = new System.Uri(HeaderImageFile.FullName);
            }
            if (LogoImageFile == null)
            {
                File.WriteAllBytes("logo.png", Properties.Resources.missing_collection_logo);
                LogoImageFile = new FileInfo("logo.png");
                this.logoImageDisplay.Source = new System.Uri(LogoImageFile.FullName);
            }
            // copy existing nft metadata if no collection information file exists bot anormal info exists
            if (!CollectionInformationFile.Exists)
            {
                if (CollectionInformation.LastKnownNftMetadata != null)
                {
                    CollectionInformation.LastKnownNftMetadata.Save(CollectionInformationFile.FullName);
                }
            }
            // try load metadata information
            if (CollectionInformationFile.Exists)
            {
                CollectionMetadata = IO.Load(CollectionInformationFile.FullName);
                this.CollectionName_TextBox.Text = CollectionMetadata.collection.name;
                this.Description_TextBox.Text = CollectionMetadata.collection.GetAttribute("description");
                this.SeriesTotal_TextBox.Text = CollectionMetadata.series_total.ToString();
                foreach (CollectionAttribute attr in CollectionMetadata.collection.attributes)
                {
                    if (attr.type != "description")
                    {
                        this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute(attr));
                    }
                }
                this.CollectionID_TextBox.Text = CollectionMetadata.collection.id;
            }
            else
            { // provide kryptomine default values
                this.Description_TextBox.Text = "this is a sample description";
                this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute(new CollectionAttribute("website", "https://kryptomine.ch/products/products.html")));
                this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute(new CollectionAttribute("twitter", "@KryptomineCH")));
                this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute(new CollectionAttribute("discord", "https://discord.gg/J7z3hVHT8a")));
                this.CollectionID_TextBox.Text = Guid.NewGuid().ToString();
                this.SeriesTotal_TextBox.Text = CollectionInformation.NftFiles.Count.ToString();
            }
        }

        private void SaveCollection_Button_Click(object sender, RoutedEventArgs e)
        {
            
            // try set collection total int field
            int collectionTotal;
            if (int.TryParse(this.SeriesTotal_TextBox.Text.Trim(), out collectionTotal))
            {
                CollectionMetadata.series_total = collectionTotal;
            }
            else
            {
                MessageBox.Show("STOP: Series Total could not be converted from text to number!");
                return;
            }
            CollectionMetadata = new Metadata();
            CollectionMetadata.collection.name = this.CollectionName_TextBox.Text;
            CollectionMetadata.collection.SetAttribute("description", this.Description_TextBox.Text);
            CollectionMetadata.collection.id = this.CollectionID_TextBox.Text;
            CollectionMetadata.series_total = collectionTotal;
            List<CollectionAttribute> attributes = new List<CollectionAttribute>();
            foreach (CollAttribute attr in this.CollectionAttributes_StackPanel.Children)
            {
                attributes.Add(attr.Value);
            }
            CollectionMetadata.collection.UpdateOrAddAttributes(attributes.ToArray());
            CollectionMetadata.Save(CollectionInformationFile.FullName);
            // upload image
            bool bannerNeedsToBeRefreshed = true;
            bool logoNeedsToBeRefreshed = true;
            /// check if benner needs to be uploaded
            string bannerLink = CollectionMetadata.collection.GetAttribute("banner");
            if (bannerLink != null && bannerLink != "")
            { // validate hashsums
                string localBannerSum = Sha256.GetSha256Sum(HeaderImageFile);
                try
                {
                    string remoteBannerSum = Sha256.GetSha256Sum(new Uri(bannerLink));
                    if (localBannerSum == remoteBannerSum)
                    {
                        bannerNeedsToBeRefreshed = false;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("STOP! banner link file could not be read! Is the link correct?");
                    return;
                }
            }
            /// check if collection logo needs to be uploaded
            string logoLink = CollectionMetadata.collection.GetAttribute("icon");
            if (logoLink != null && logoLink != "")
            { // validate hashsums
                string localLogoSum = Sha256.GetSha256Sum(LogoImageFile);
                try
                {
                    string remoteLogoSum = Sha256.GetSha256Sum(new Uri(logoLink));
                    if (localLogoSum == remoteLogoSum)
                    {
                        logoNeedsToBeRefreshed = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("STOP! icon link file could not be read! Is the link correct?");
                    return;
                }

            }
            if (!bannerNeedsToBeRefreshed && ! logoNeedsToBeRefreshed)
            {
                return;
            }
            if (NftStorageAccount.Client == null )
            {
                MessageBox.Show("STOP: please set the NFT.Storage api key in settings first!");
                return;
            }
            if (bannerNeedsToBeRefreshed)
            {
                Task<NFT_File> uploadFile = Task.Run(() => NftStorageAccount.Client.Upload(HeaderImageFile));
                uploadFile.Wait();
                NFT_File headerRemoteFile = uploadFile.Result;
                CollectionMetadata.collection.SetAttribute("banner",headerRemoteFile.URL);
                bool attributeExists = false;
                foreach(CollAttribute collAtt in this.CollectionAttributes_StackPanel.Children)
                {
                    if(collAtt.Value.type == "banner")
                    {
                        collAtt.Value.value = headerRemoteFile.URL;
                        attributeExists = true;
                    }
                }
                if (!attributeExists)
                {
                    this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute(new CollectionAttribute("banner", headerRemoteFile.URL.ToString())));
                }
                
            }
            if (logoNeedsToBeRefreshed)
            {
                Task<NFT_File> uploadFile = Task.Run(() => NftStorageAccount.Client.Upload(LogoImageFile));
                uploadFile.Wait();
                NFT_File logoRemoteFile = uploadFile.Result;
                CollectionMetadata.collection.SetAttribute("icon", logoRemoteFile.URL);
                bool attributeExists = false;
                foreach (CollAttribute collAtt in this.CollectionAttributes_StackPanel.Children)
                {
                    if (collAtt.Value.type == "icon")
                    {
                        collAtt.Value.value = logoRemoteFile.URL;
                        attributeExists = true;
                    }
                }
                if (!attributeExists)
                {
                    this.CollectionAttributes_StackPanel.Children.Add(new CollAttribute(new CollectionAttribute("icon", logoRemoteFile.URL.ToString())));
                }
            }
            CollectionMetadata.Save(CollectionInformationFile.FullName);
        }
        /// <summary>
        /// this function checks if the input for SeriesTotal_TextBox is valid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeriesTotal_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(SeriesTotal_TextBox.Text.Trim(), out _))
            {
                if (!LastIntParseTest)
                {
                    SeriesTotal_TextBox.Background = null;
                    LastIntParseTest = true;
                }
            }
            else
            {
                if (LastIntParseTest)
                {
                    SeriesTotal_TextBox.Background = Brushes.LightSalmon;
                    LastIntParseTest = false;
                }
            }
        }
        private bool LastIntParseTest = true;
    }
}
