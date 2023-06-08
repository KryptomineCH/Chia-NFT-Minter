using Chia_Metadata;
using Microsoft.Win32;
using Minter_UI.CollectionInformation_ns;
using Minter_UI.Tasks_NS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Metadata = Chia_Metadata.Metadata;

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
            this.Filters.attributeFilter.AttributeFilteredNFTs = _viewModel;
            this.Filters.FilteringCompleted += OnFilteringCompleted;
        }
        /// <summary>
        /// the viewmodel is beeing used for automatic fill and updating of the virtualized wrappanel which displays the nfts
        /// </summary>
        internal MintingPreview_ViewModel _viewModel;
        /// <summary>
        /// this variable contains the path of the Metadata which is currently beeing edited.
        /// </summary>
        private FileInfo? CurrentMetadataPath;
        /// <summary>
        /// this queue is used because of a memory leak when destroying a control. Instead the element is refilled and reused
        /// </summary>
        Queue<Attribute> AttributeReuseElements = new Queue<Attribute>();
        
        /// <summary>
        ///  the key specifies the attribute name. int defines how often it is beeeing used
        /// </summary>
        SelectedAttributes UsedAttributes = new SelectedAttributes();
        bool Initialized = false;
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UserControl userControl = sender as UserControl;
            if (userControl.IsVisible && ! Initialized)
            {
                Filters.RefreshStatusFilters();
                Initialized = true;
            }
            if (OpenAiAccount.ApiKey != null)
            {
                GenerateDescription_Button.Visibility= Visibility.Visible;
            }
        }
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

        /// <summary>
        /// saves the current metadata to disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadata();
        }
        /// <summary>
        /// saves the current metadata to disk and loads the next nft in the Collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAndNext_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveMetadata();
            NextNft();
        }
        /// <summary>
        /// displays the first nft in the filter after filtering has been completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFilteringCompleted(object sender, EventArgs e)
        {
            NFTselection_ListView.SelectedIndex = 0;
        }
        /// <summary>
        /// displays the next nft in the collection
        /// </summary>
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
        /// <summary>
        /// saves the current metadata to disk
        /// </summary>
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
            metadata.attributes = new MetadataAttribute[] { };
            // add attributes to metadata
            for (int i = 1; i < this.Attributes_StackPanel.Children.Count; i++)
            {
                MetadataAttribute attribute = ((Attribute)this.Attributes_StackPanel.Children[i]).GetAttribute();
                metadata.AddAttribute(attribute);
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
        /// <summary>
        /// this function is beeing called when the user selects a new nft in the list. the nft will be loaded into preview to
        /// edit its metadata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NFTselection_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MintingItem selectedItem = (MintingItem)(sender as ListView).SelectedItem;
            LoadInformation(selectedItem);
        }
        /// <summary>
        /// this button opens a file dialog and imports the according files to their corresponding folders<br/>
        /// it also updated the collection information and refreshes the filters/view in the end to display the imported files/nfts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportMedia_Button_Click(object sender, RoutedEventArgs e)
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
                // execute the import task without blocking the ui
                var progressBar = (ProgressBar)ImportMedia_Button.Template.FindName("ImportProgress_ProgressBar", ImportMedia_Button);
                progressBar.Visibility = Visibility.Visible;
                Progress<float> progress = new Progress<float>(p => progressBar.Value = p);
                Task.Run(() => ImportFiles.Import(filePaths, progress))
                    .ContinueWith((t) => 
                    {
                        Dispatcher.Invoke(() => {
                            progressBar.Visibility = Visibility.Collapsed;
                            Filters.RefreshStatusFilters();
                        });
                    });
            }
        }
        private FileInfo CollectionInformationFile = new FileInfo(
            Path.Combine(Directories.Metadata.FullName, "CollectionInfo.json"));
        private async void GenerateDescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            Brush background = null;
            await Dispatcher.InvokeAsync(() =>
            {
                GenerateDescription_Button.IsEnabled = false;
                background = GenerateDescription_Button.Background;
                GenerateDescription_Button.Background = Brushes.Turquoise;
            });
            StringBuilder prompt = new StringBuilder();
            // name and collection
            prompt.Append("please generate a short story for an nft with the name: ");
            prompt.Append(NftName_TextBox.Text);
            // collection name
            CollectionInformationFile.Refresh();
            if (CollectionInformationFile.Exists)
            {
                prompt.Append(Environment.NewLine);
                Metadata CollectionMetadata = IO.Load(CollectionInformationFile.FullName);
                prompt.Append("The NFT is in the collection: ");
                prompt.Append(CollectionMetadata.name);
            }
            // sensitive information
            if ((bool)SensitiveContent_Checkbox.IsChecked)
            {
                prompt.Append(Environment.NewLine);
                prompt.Append("The NFT contains sensitive information");
            }
            // attributes
            
            List<string> attributes = new List<string>();
            for (int i = 1; i < this.Attributes_StackPanel.Children.Count; i++)
            {
                MetadataAttribute attribute = ((Attribute)this.Attributes_StackPanel.Children[i]).GetAttribute();
                if (attribute.trait_type != "TraitType")
                {
                    string trait = $"{attribute.trait_type}:{attribute.value.ToString()}";
                    attributes.Add(trait);
                }
            }
            if (attributes.Count > 0)
            {
                prompt.Append(Environment.NewLine);
                prompt.Append("The Nft contains the following Attributes: \"");
                prompt.Append(string.Join(',', attributes));
            }
            // user description
            if (Description_TextBox.Text != "")
            {
                prompt.Append(Environment.NewLine);
                prompt.Append("The user specified the following prompt: ");
                prompt.Append(Description_TextBox.Text);
            }
            string result = await OpenAiAccount.CompleteTextAsync(prompt.ToString().Trim()).ConfigureAwait(false);
            await Dispatcher.InvokeAsync(() =>
            {
                Description_TextBox.Text = result;
                GenerateDescription_Button.IsEnabled = true;
                GenerateDescription_Button.Background = background;
            });
        }

        private void GenerateAllDescriptionsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
