using Chia_Metadata;
using Minter_UI.CollectionInformation_ns;
using Minter_UI.Tasks_NS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        // Declare the delegate (if using non-generic delegate).
        public delegate void FilteringCompletedEventHandler(object sender, EventArgs e);
        // Declare the event using the delegate.
        public event FilteringCompletedEventHandler FilteringCompleted;
        // filter variables
        internal StatusFilter statusFilter = new StatusFilter();
        internal NameFilter nameFilter = new NameFilter();
        internal AttributeFilter attributeFilter = new AttributeFilter();


        //public event EventHandler AttributeChanged;

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            CollectionInformation.ReloadAll();
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
            }
            RefreshStatusFilters();
        }

        private void PendingMint_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
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
            }
            RefreshStatusFilters();
        }

        private void Offered_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshStatusFilters();
        }

        SelectedAttributes selectedIncludeAttributes = new SelectedAttributes();
        private void IncludedAttributes_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Attribute childControl = new Attribute(usedAttributes: null);
            childControl.AttributeChanged += ParentControl_AttributeChanged;
            IncludedAttributes_WrapPanel.Children.Add(childControl);

        }
        SelectedAttributes selectedExcludeAttributes = new SelectedAttributes();

        public event System.EventHandler AttributeChanged;

        private void ExcludedAttributes_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Attribute childControl = new Attribute(usedAttributes: null);
            childControl.AttributeChanged += ParentControl_AttributeChanged;
            ExcludedAttributes_WrapPanel.Children.Add(childControl);

        }

        private void ParentControl_AttributeChanged(object sender, EventArgs e)
        {
            RefreshAttributeFilters();
        }

        CancellationTokenSource cancelStatusFiltering = new CancellationTokenSource();
        bool IsStatusFiltering = false;
        SemaphoreSlim IsStatusFiltering_Lock = new SemaphoreSlim(1,1);
        
        internal async void RefreshStatusFilters()
        {
            if (!await IsStatusFiltering_Lock.WaitAsync(0).ConfigureAwait(false))
            {
                if (cancelStatusFiltering.IsCancellationRequested)
                {
                    return;
                }
                cancelStatusFiltering.Cancel();
                await IsStatusFiltering_Lock.WaitAsync().ConfigureAwait(false);
            }
            try {
                IsStatusFiltering = true;
                var progress = new Progress<float>(p => Filter_ProgressBar.Value = p * 100);
                await statusFilter.RefreshStatusFilter(
                    includeAllImages: (bool)All_CheckBox.IsChecked,
                    includeExistingMetadataImages: (bool)ExistingMetadata_CheckBox.IsChecked,
                    includeUploadedImages: (bool)Uploaded_CheckBox.IsChecked,
                    includePendingMints: (bool)PendingMint_CheckBox.IsChecked,
                    includeMintedImages: (bool)Minted_CheckBox.IsChecked,
                    includeOfferedImages: (bool)Offered_CheckBox.IsChecked,
                    progress,
                    cancellation: cancelStatusFiltering.Token).ConfigureAwait(false);
                if (!cancelStatusFiltering.IsCancellationRequested)
                {
                    IsStatusFiltering = false;
                    RefreshNameFilters(continuationTask: true);
                }
            }
            finally
            {
                cancelStatusFiltering = new CancellationTokenSource();
                IsStatusFiltering = false;
                IsStatusFiltering_Lock.Release();
            }
        }

        CancellationTokenSource cancelNameFiltering = new CancellationTokenSource();
        bool IsNameFiltering = false;
        SemaphoreSlim IsNameFiltering_Lock = new SemaphoreSlim(1,1);
        private async void RefreshNameFilters(bool continuationTask = false)
        {
            if (!await IsNameFiltering_Lock.WaitAsync(0).ConfigureAwait(false))
            {
                if (cancelNameFiltering.IsCancellationRequested || IsStatusFiltering)
                {
                    return;
                }
                cancelNameFiltering.Cancel();
                await IsNameFiltering_Lock.WaitAsync().ConfigureAwait(false);
            }
            try
            {
                var progress = new Progress<float>(p => Filter_ProgressBar.Value = p * 100);
                await nameFilter.RefreshNameFilter(
                    Namefilter_TextBox.Text,
                    statusFilter,
                    progress,
                    cancelNameFiltering.Token).ConfigureAwait(false);
                if (!cancelNameFiltering.IsCancellationRequested)
                {
                    IsNameFiltering = false;
                    RefreshAttributeFilters(continuationTask: true);
                }
            }
            finally
            {
                cancelNameFiltering = new CancellationTokenSource();
                IsNameFiltering = false;
                IsNameFiltering_Lock.Release();
            }
        }
        CancellationTokenSource cancelAttributeFiltering = new CancellationTokenSource();
        SemaphoreSlim IsAttributeFiltering_Lock = new SemaphoreSlim(1,1);
        private async void RefreshAttributeFilters(bool continuationTask = false)
        {
            if (!await IsAttributeFiltering_Lock.WaitAsync(0).ConfigureAwait(false))
            {
                if (cancelAttributeFiltering.IsCancellationRequested || IsStatusFiltering)
                {
                    return;
                }
                cancelAttributeFiltering.Cancel();
                await IsAttributeFiltering_Lock.WaitAsync().ConfigureAwait(false);
            }
            try
            {
                // load dictionaries
                List<MetadataAttribute> inclusions = new List<MetadataAttribute>();
                List<MetadataAttribute> exclusions = new List<MetadataAttribute>();
                for (int i = 1; i < this.ExcludedAttributes_WrapPanel.Children.Count; i++)
                {
                    MetadataAttribute attribute = ((Attribute)this.ExcludedAttributes_WrapPanel.Children[i]).GetAttribute();
                    exclusions.Add(attribute);
                }
                for (int i = 1; i < this.IncludedAttributes_WrapPanel.Children.Count; i++)
                {
                    MetadataAttribute attribute = ((Attribute)this.IncludedAttributes_WrapPanel.Children[i]).GetAttribute();
                    inclusions.Add(attribute);
                }
                var progress = new Progress<float>(p => Filter_ProgressBar.Value = p * 100);
                // apply Filter
                await attributeFilter.RefreshAttributeFilter(
                    inclusions,
                    exclusions,
                    nameFilter,
                    progress,
                    cancelAttributeFiltering.Token).ConfigureAwait(false);
                if (!cancelAttributeFiltering.IsCancellationRequested)
                {
                    FilteringCompleted(this, EventArgs.Empty);
                }
            }
            finally
            {
                cancelAttributeFiltering = new CancellationTokenSource();
                IsAttributeFiltering_Lock.Release();
            }
        }
    }
}
