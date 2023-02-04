using Chia_Metadata;
using Microsoft.Win32;
using Minter_UI.CollectionInformation_ns;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// Interaction logic for Metadata editing
    /// </summary>
    public partial class Metadata_Control : UserControl
    {
        public Metadata_Control()
        {
            _viewModel = new MintingPreview_ViewModel();
            _viewModel.Items = new ObservableCollection<MintingItem>();
            this.DataContext = _viewModel;
            InitializeComponent();
            this.Filters.FilteredNFTs = _viewModel;
            //if (CollectionInformation.Information.MissingMetadata.Count == 0 &&
            //    CollectionInformation.Information.MetadataFiles.Count > 0)
            //{
            //    LoadNextExistingMetadata();
            //}
            //else if (CollectionInformation.Information.MissingMetadata.Count > 0)
            //{
            //    LoadNextMissingMetadata();
            //}
            
            
        }
        internal MintingPreview_ViewModel _viewModel;
        private FileInfo? CurrentMetadataPath;
        Queue<Attribute> AttributeReuseElements = new Queue<Attribute>();
        /// <summary>
        ///  the key specifies the attribute name. int defines how often it is beeeing used
        /// </summary>
        SelectedAttributes UsedAttributes = new SelectedAttributes();
        /// <summary>
        /// Load metadata information into the ui for editing
        /// </summary>
        /// <param name="file"></param>
        /// <param name="reloadMetadata"></param>
        private void LoadInformation(MintingItem item)
        {
            if (item == null) return;
            string nftName = Path.GetFileNameWithoutExtension(item.Data);
            string key = CollectionInformation.GetKeyFromFile(item.Data);
            // load image
            if (CollectionInformation.Information.NftPreviewFiles.ContainsKey(key))
            {
                ImageWebView.Address = CollectionInformation.Information.NftPreviewFiles[key].FullName;
            }
            else
            {
                ImageWebView.Address = CollectionInformation.Information.NftFiles[key].FullName;
            }
            // load metadata
            if (CollectionInformation.Information.MetadataFiles.ContainsKey(key))
            {
                CurrentMetadataPath = CollectionInformation.Information.MetadataFiles[key];
            }
            else
            {
                this.NftName_TextBox.Text = nftName.Replace("_", " ").Replace("-", " - ");
                CurrentMetadataPath = new FileInfo(Path.Combine(Directories.Metadata.FullName, nftName + ".json"));
            }
            // load attributes
            ClearAttributesPanel();
            Metadata metadata;
            if (CollectionInformation.GetMetadataFromCache(key, out metadata))
            {
                this.NftName_TextBox.Text = metadata.name;
                this.Description_TextBox.Text = metadata.description;
                this.SensitiveContent_Checkbox.IsChecked = metadata.sensitive_content;
                foreach (MetadataAttribute attribute in metadata.attributes)
                {
                    if (AttributeReuseElements.Count>0)
                    {
                        Attribute attr = AttributeReuseElements.Dequeue();
                        attr.SetAttribute(attribute);
                        this.Attributes_StackPanel.Children.Add(attr);
                    }
                    else
                    {
                        this.Attributes_StackPanel.Children.Add(new Attribute(UsedAttributes, attribute));
                    }
                }
            }
            else
            {
                this.NftName_TextBox.Text = nftName.Replace("_", " ").Replace("-", " - ");
                foreach (MetadataAttribute attribute in CollectionInformation.Information.LikelyAttributes)
                {
                    if (AttributeReuseElements.Count > 0)
                    {
                        Attribute attr = AttributeReuseElements.Dequeue();
                        attr.SetAttribute(attribute);
                        this.Attributes_StackPanel.Children.Add(attr);
                    }
                    else
                    {
                        this.Attributes_StackPanel.Children.Add(new Attribute(UsedAttributes));
                    }
                }
            }
        }
        /// <summary>
        /// clears the attribute panel to prepare for new metadata to be loaded
        /// </summary>
        private void ClearAttributesPanel()
        {
            for(int i = this.Attributes_StackPanel.Children.Count-1; i > 0; i--)
            {
                AttributeReuseElements.Enqueue((Attribute)this.Attributes_StackPanel.Children[i]);
                this.Attributes_StackPanel.Children.RemoveAt(i);
            }
        }
        /// <summary>
        /// adds a new attribute to the attributes stack panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Attributes_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Attributes_StackPanel.Children.Add(new Attribute(UsedAttributes));
        }


        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadata();
        }

        private void SaveAndNext_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadata();
            NextNft();
        }
        private void NextNft()
        {
            int index = NFTselection_ListView.SelectedIndex;
            index++;
            if (index > NFTselection_ListView.Items.Count)
            {
                index = 0;
            }
            NFTselection_ListView.SelectedIndex = index;
        }
        private void SaveMetadata()
        {
            if (CurrentMetadataPath == null)
            {
                return;
            }
            // pre check
            string nftName = Path.GetFileNameWithoutExtension(CurrentMetadataPath.FullName);
            string key = CollectionInformation.GetKeyFromFile(CurrentMetadataPath);
            if (CollectionInformation.Information.RpcFiles.ContainsKey(key))
            {
                // nft is potentially minted!
                MessageBox.Show($"STOP: RPC for this nft was found! {Environment.NewLine} The nft is likely minted! {Environment.NewLine} If you wish to overwrite the information, please delete {nftName}.rpc and press refresh");
                return;
            }
            // load collection information
            Metadata metadata = IO.Load(Path.Combine(Directories.Metadata.FullName, "CollectionInfo.json"));
            // get / load series number
            if (CollectionInformation.Information.MetadataFiles.ContainsKey(key))
            {
                // load seriesnumber from existing metadata
                Metadata oldMetadata = IO.Load(CurrentMetadataPath.FullName);
                metadata.series_number = oldMetadata.series_number;
            }
            else
            {
                // reserve next free series number
                metadata.series_number = (ulong)CollectionInformation.Information.ReserveNextFreeCollectionNumber();
            }
            // fill meta information
            metadata.name = this.NftName_TextBox.Text;
            metadata.description = this.Description_TextBox.Text;
            metadata.minting_tool = "KryptoMine Chia-Nft-Minter";
            if (this.SensitiveContent_Checkbox.IsChecked == null)
                metadata.sensitive_content = false;
            else
                metadata.sensitive_content = (bool)this.SensitiveContent_Checkbox.IsChecked;
            metadata.attributes.Clear();
            // add attributes to metadata
            for (int i = 1; i < this.Attributes_StackPanel.Children.Count; i++)
            {
                MetadataAttribute attribute = ((Attribute)this.Attributes_StackPanel.Children[i]).GetAttribute();
                metadata.attributes.Add(attribute);
                // update metadata suggestions
                if ( !CollectionInformation.Information.AllMetadataAttributes.ContainsKey(attribute.trait_type))
                {
                    CollectionInformation.Information.AllMetadataAttributes[attribute.trait_type] = attribute;
                    CollectionInformation.Information.AllMetadataAttributeValues[attribute.trait_type] = new List<string>();
                }
                if (CollectionInformation.Information.AllMetadataAttributeValues[attribute.trait_type].Contains(attribute.value))
                {
                    CollectionInformation.Information.AllMetadataAttributeValues[attribute.trait_type].Add((string)attribute.value);
                }
            }
            metadata.Save(CurrentMetadataPath.FullName);
            // update collection information
            if (CollectionInformation.Information.MissingMetadata.ContainsKey(key))
            {
                CollectionInformation.Information.MissingMetadata.Remove(key, out _);
            }
            if (!CollectionInformation.Information.MetadataFiles.ContainsKey(key))
            {
                CollectionInformation.Information.MetadataFiles[key] = new FileInfo(CurrentMetadataPath.FullName);
            }
        }

        private void RefreshCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            CollectionInformation.ReloadAll(Settings_NS.Settings.All.CaseSensitiveFileHandling);
        }

        private void UpdateAllDescriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            // refresh all existing Metadata
            int updatedMetadata = 0;
            foreach (string unmintedNFT_Key in CollectionInformation.Information.MissingRPCs.Keys)
            {
                FileInfo? metadata_FileInfo;
                if (CollectionInformation.Information.MetadataFiles.TryGetValue(unmintedNFT_Key, out metadata_FileInfo))
                {
                    Metadata metaData = IO.Load(metadata_FileInfo.FullName);
                    metaData.description = this.Description_TextBox.Text;
                    metaData.Save(metadata_FileInfo.FullName);
                    updatedMetadata++;
                }
            }
            if (CollectionInformation.Information.RpcFiles.Count > 0)
            {
                MessageBox.Show($"Warning: {updatedMetadata} files have been updated. {Environment.NewLine}" +
                    $"{CollectionInformation.Information.RpcFiles.Count} files have not been updated because RPC files exist. {Environment.NewLine}" +
                    $"The software assumes they are already minted.");
            }
        }
        private void NFTselection_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MintingItem selectedItem = (MintingItem)(sender as ListView).SelectedItem;
            LoadInformation(selectedItem);
        }
        private void ImportMedia_Button_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Collection Files (img, vid, doc, meta, mint, rpc)|*";
            fileDialog.FilterIndex = 1;
            fileDialog.Multiselect = true;
            if (Settings_NS.Settings.All.LastImportPath != null && Directory.Exists(Settings_NS.Settings.All.LastImportPath))
            {
                fileDialog.InitialDirectory = Settings_NS.Settings.All.LastImportPath;
            }

            if (fileDialog.ShowDialog() == true)
            {
                string[] filePaths = fileDialog.FileNames;
                // Get the selected file's path
                List<FileInfo> filesToImport = new List<FileInfo>();
                // Merge File and directory infos
                foreach (string filePath in filePaths)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        FileInfo file = new FileInfo(filePath);
                        if (file.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            continue;
                        }
                        filesToImport.Add(file);
                    }
                    else if (System.IO.Directory.Exists(filePath))
                    {
                        DirectoryInfo di = new DirectoryInfo(filePath);
                        FileInfo[] files = di.GetFiles("*.*", SearchOption.AllDirectories)
                        .Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                        .ToArray();
                    }
                }
                // save directory path for next opening
                if (filesToImport.Count > 0)
                {
                    Settings_NS.Settings.All.LastImportPath = filesToImport[0].Directory.FullName;
                    Settings_NS.Settings.Save();
                }
                // import Files
                foreach (FileInfo file in filesToImport)
                {
                    string type = file.Extension;
                    if (file.Extension == null || file.Extension == "")
                    {
                        continue;
                    }
                    else if (file.Extension == ".nft")
                    {
                        file.CopyTo(Path.Combine(Directories.Minted.FullName, file.Name), overwrite: true);
                    }
                    else if (file.Extension == ".mint")
                    {
                        file.CopyTo(Path.Combine(Directories.PendingTransactions.FullName, file.Name), overwrite: true);
                    }
                    else if (file.Extension == ".offer")
                    {
                        file.CopyTo(Path.Combine(Directories.Offered.FullName, file.Name), overwrite: true);
                    }
                    else if (file.Extension == ".rpc")
                    {
                        file.CopyTo(Path.Combine(Directories.Rpcs.FullName, file.Name), overwrite: true);
                    }
                    else if (file.Extension == ".metadata" || file.Name == "CollectionInfo.json")
                    {
                        file.CopyTo(Path.Combine(Directories.Metadata.FullName, file.Name), overwrite: true);
                    }
                    else if (file.Extension == ".json")
                    {
                        try
                        {
                            Metadata test = Chia_Metadata.IO.Load(file.FullName);
                            file.CopyTo(Path.Combine(Directories.Metadata.FullName, file.Name), overwrite: true);
                        }
                        catch
                        {
                            MessageBox.Show($"the file {file.Name} could not be imported! It does not seem to be a valid metadata file!");
                            return;
                        }
                    }
                    else
                    {
                        file.CopyTo(Path.Combine(Directories.Nfts.FullName, file.Name), overwrite: true);
                    }
                }
            }
        }
    }
}
