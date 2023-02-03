using Chia_Metadata;
using Chia_NFT_Minter.CollectionInformation_ns;
using Minter_UI.Tasks_NS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using static Minter_UI.UI_NS.Attribute;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// Interaction logic for FilterControl.xaml
    /// </summary>
    public partial class FilterControl : UserControl, IParentControl
    {
        public FilterControl()
        {
            InitializeComponent();
        }
        public Dictionary<string, FileInfo> FilteredNFTs = new Dictionary<string, FileInfo>();
        StatusFilter StatusFilteredNFTs = new StatusFilter();
        Dictionary<string, FileInfo> NameFilteredNFTs = new Dictionary<string, FileInfo>();

        //public event EventHandler AttributeChanged;

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatusFilters();
        }

        private void Namefilter_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshNameFilters();
        }

        private void All_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)All_CheckBox.IsChecked)
            {
                ExistingMetadata_CheckBox.IsChecked = true;
                Uploaded_CheckBox.IsChecked = true;
                PendingMint_CheckBox.IsChecked = true;
                Minted_CheckBox.IsChecked = true;
                Offered_CheckBox.IsChecked = true;
            }
            else
            {
                ExistingMetadata_CheckBox.IsChecked = false;
                Uploaded_CheckBox.IsChecked = false;
                PendingMint_CheckBox.IsChecked = false;
                Minted_CheckBox.IsChecked = false;
                Offered_CheckBox.IsChecked = false;
            }
            RefreshStatusFilters();
        }

        private void ExistingMetadata_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)ExistingMetadata_CheckBox.IsChecked)
            {
                Uploaded_CheckBox.IsChecked = true;
                PendingMint_CheckBox.IsChecked = true;
                Minted_CheckBox.IsChecked = true;
                Offered_CheckBox.IsChecked = true;
            }
            else
            {
                Uploaded_CheckBox.IsChecked = false;
                PendingMint_CheckBox.IsChecked = false;
                Minted_CheckBox.IsChecked = false;
                Offered_CheckBox.IsChecked = false;
                All_CheckBox.IsChecked = false;
            }
            RefreshStatusFilters();
        }

        private void Uploaded_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Uploaded_CheckBox.IsChecked)
            {
                PendingMint_CheckBox.IsChecked = true;
                Minted_CheckBox.IsChecked = true;
                Offered_CheckBox.IsChecked = true;
            }
            else
            {
                PendingMint_CheckBox.IsChecked = false;
                Minted_CheckBox.IsChecked = false;
                Offered_CheckBox.IsChecked = false;
                All_CheckBox.IsChecked = false;
            }
            RefreshStatusFilters();
        }

        private void PendingMint_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!(bool)PendingMint_CheckBox.IsChecked)
            { 
                All_CheckBox.IsChecked = false;
            }
            RefreshStatusFilters();
        }

        private void Minted_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Minted_CheckBox.IsChecked)
            {
                Offered_CheckBox.IsChecked = true;
            }
            else
            {
                Offered_CheckBox.IsChecked = false;
                All_CheckBox.IsChecked = false;
            }
            RefreshStatusFilters();
        }

        private void Offered_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!(bool)Offered_CheckBox.IsChecked)
            { 
                All_CheckBox.IsChecked = false;
            }
            RefreshStatusFilters();
        }

        SelectedAttributes selectedIncludeAttributes = new SelectedAttributes();
        private void IncludedAttributes_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Attribute childControl = new Attribute(selectedExcludeAttributes);
            childControl.AttributeChanged += ParentControl_AttributeChanged;
            IncludedAttributes_WrapPanel.Children.Add(childControl);

        }
        SelectedAttributes selectedExcludeAttributes = new SelectedAttributes();

        public event System.EventHandler AttributeChanged;

        private void ExcludedAttributes_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Attribute childControl = new Attribute(selectedIncludeAttributes);
            childControl.AttributeChanged += ParentControl_AttributeChanged;
            ExcludedAttributes_WrapPanel.Children.Add(childControl);

        }

        private void ParentControl_AttributeChanged(object sender, EventArgs e)
        {
            RefreshAttributeFilters();
        }

        private void RefreshStatusFilters()
        {
            StatusFilteredNFTs.RefreshStatusFilter(
                includeAllImages: (bool) All_CheckBox.IsChecked,
                includeExistingMetadataImages: (bool)ExistingMetadata_CheckBox.IsChecked,
                includeUploadedImages: (bool)Uploaded_CheckBox.IsChecked,
                includePendingMints: (bool)PendingMint_CheckBox.IsChecked,
                includeMintedImages: (bool)Minted_CheckBox.IsChecked,
                includeOfferedImages: (bool)Offered_CheckBox.IsChecked,
                progress: new Progress<int>(),
                cancellation: CancellationToken.None).ConfigureAwait(false);
            RefreshNameFilters();
        }
        private void RefreshNameFilters()
        {
            NameFilteredNFTs.Clear();
            if (Namefilter_TextBox.Text.Trim() == "")
            {
                NameFilteredNFTs = StatusFilteredNFTs.StatusFilteredNFTs;
            }
            else
            {
                string name = Namefilter_TextBox.Text;
                name = name.Replace("*", ".*");
                name = name.Replace("..*", ".*");
                if (Settings_NS.Settings.All.CaseSensitiveFileHandling!)
                {
                    name = name.ToLower();
                }
                foreach (KeyValuePair<string, FileInfo> nft in StatusFilteredNFTs.StatusFilteredNFTs)
                {
                    if (Regex.IsMatch(nft.Key, name))
                    {
                        NameFilteredNFTs[nft.Key] = nft.Value;
                    }
                }
            }
            RefreshAttributeFilters();
        }
        private void RefreshAttributeFilters()
        {
            FilteredNFTs.Clear();
            if (this.ExcludedAttributes_WrapPanel.Children.Count <= 1 &&
                this.IncludedAttributes_WrapPanel.Children.Count <= 1)
            {
                FilteredNFTs = NameFilteredNFTs;
                return;
            }
            // load dictionaries
            Dictionary<string,MetadataAttribute> inclusions = new Dictionary<string,MetadataAttribute>();
            Dictionary<string,MetadataAttribute> exclusions = new Dictionary<string,MetadataAttribute>();
            for (int i = 1; i < this.ExcludedAttributes_WrapPanel.Children.Count; i++)
            {
                MetadataAttribute attribute = ((Attribute)this.ExcludedAttributes_WrapPanel.Children[i]).GetAttribute();
                inclusions.Add(attribute.trait_type, attribute);
            }
            for (int i = 1; i < this.IncludedAttributes_WrapPanel.Children.Count; i++)
            {
                MetadataAttribute attribute = ((Attribute)this.IncludedAttributes_WrapPanel.Children[i]).GetAttribute();
                exclusions.Add(attribute.trait_type, attribute);
            }
            // apply Filter
            MetadataAttribute excludeFilterAttribute_temp;
            MetadataAttribute includeFilterAttribute_temp;
            bool include;
            foreach (KeyValuePair<string, FileInfo> nft in NameFilteredNFTs)
            {
                FileInfo metadataFile;                
                if (CollectionInformation.Information.MetadataFiles.TryGetValue(nft.Key, out metadataFile))
                {
                    Metadata metadata = Chia_Metadata.IO.Load(metadataFile.FullName);
                    include = false;
                    foreach (MetadataAttribute attribute in metadata.attributes)
                    {
                        if (exclusions.TryGetValue(attribute.trait_type, out excludeFilterAttribute_temp))
                        {
                            // might be to exclude
                            if (((string)excludeFilterAttribute_temp.value == null || (string)excludeFilterAttribute_temp.value == "" || Regex.IsMatch((string)attribute.value, (string)excludeFilterAttribute_temp.value))
                                && (excludeFilterAttribute_temp.min_value == null || excludeFilterAttribute_temp.min_value <= (int)attribute.value)
                                && (excludeFilterAttribute_temp.max_value == null || excludeFilterAttribute_temp.max_value >= (int)attribute.value))
                            {
                                // its in exclusions, dont check any further!
                                include = false;
                                // exclusions have priority, so here is stop!
                                break;
                            }
                        }
                        if (inclusions.TryGetValue(attribute.trait_type, out includeFilterAttribute_temp))
                        {
                            // might be to include
                            if (((string)includeFilterAttribute_temp.value == null || (string)includeFilterAttribute_temp.value == "" || Regex.IsMatch((string)attribute.value, (string)includeFilterAttribute_temp.value))
                                && (includeFilterAttribute_temp.min_value == null || includeFilterAttribute_temp.min_value <= (int)attribute.value)
                                && (includeFilterAttribute_temp.max_value == null || includeFilterAttribute_temp.max_value >= (int)attribute.value))
                            {
                                // its in exclusions, dont check any further!
                                include = true;
                                // do not break here, there might be an exclusion which is taking priority.
                            }
                        }
                    }
                    if (include)
                    {
                        FilteredNFTs[nft.Key] = nft.Value;
                    }
                }
            }
        }
    }
}
