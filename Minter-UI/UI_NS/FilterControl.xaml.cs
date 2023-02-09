using Chia_Metadata;
using Minter_UI.CollectionInformation_ns;
using Minter_UI.Tasks_NS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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
        /// <summary>
        /// Declares the delegate for the FilteringCompleted event.
        /// </summary>
        public delegate void FilteringCompletedEventHandler(object sender, EventArgs e);
        /// <summary>
        /// Declares the FilteringCompleted event.using the delegate.
        /// </summary>
        public event FilteringCompletedEventHandler FilteringCompleted;
        /// <summary>
        /// class which handles the status filtering and contains the nfts filtered by status *new, with metadata, uploaded,...( 
        /// </summary>
        internal StatusFilter statusFilter = new StatusFilter();
        /// <summary>
        /// class which handles the name filtering and contains the nfts filtered by the nft name
        /// </summary>
        internal NameFilter nameFilter = new NameFilter();
        /// <summary>
        /// class which handles the attribute filtering and contains the nfts filtered by the attributes
        /// </summary>
        /// <remarks>
        /// nfts can only be filtered by attribute if they contain metadata.
        /// This is the final step before displaying the filtered nfts in the ui
        /// </remarks>
        internal AttributeFilter attributeFilter = new AttributeFilter();

        /// <summary>
        /// Handles the click event of the Refresh button.<br/>
        /// this will refresh the collection information and reapply all filters
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            CollectionInformation.ReloadAll();
            RefreshStatusFilters();
        }
        /// <summary>
        /// Handles the TextChanged event of the Namefilter TextBox.<br/>
        /// this starts the name filter and will refresh all subsequent filter steps
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void Namefilter_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshNameFilters();
        }
        /// <summary>
        /// Handles the Checked event of the All CheckBox.<br/>
        /// The all checkbox can check/uncheck all subsequent status selection checkboxes<br/>
        /// also re-executes all filters
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Checked event of the ExistingMetadata CheckBox.<br/>
        /// This checkbox toggles wether nfts with metadata should be included or excludedby the filter<br/>
        /// also refreshes all filters
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Checked event of the Uploaded_CheckBox CheckBox.<br/>
        /// This checkbox toggles wether nfts which are uploaded to NFT.Storage should be included or excludedby the filter<br/>
        /// also refreshes all filters
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Checked event of the PendingMint_CheckBox CheckBox.<br/>
        /// This checkbox toggles wether nfts which are currently minting should be included or excludedby the filter<br/>
        /// also refreshes all filters
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PendingMint_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshStatusFilters();
        }
        /// <summary>
        /// Handles the Checked event of the Minted_CheckBox CheckBox.<br/>
        /// This checkbox toggles wether nfts which are minted should be included or excludedby the filter<br/>
        /// also refreshes all filters
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Checked event of the Offered_CheckBox CheckBox.<br/>
        /// This checkbox toggles wether nfts which are offered should be included or excludedby the filter<br/>
        /// also refreshes all filters
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Offered_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshStatusFilters();
        }
        /// <summary>
        /// This function gets called when the addbutton of the include attribbute filter gets pressed<br/>
        /// It then adds a Metadata attribute to the include filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IncludedAttributes_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Attribute childControl = new Attribute(usedAttributes: null);
            childControl.AttributeChanged += ParentControl_AttributeChanged;
            IncludedAttributes_WrapPanel.Children.Add(childControl);

        }
        /// <summary>
        /// this event is beeing raised from the child elements in include/exclude filter. it triggers a refresh of the filters.
        /// </summary>
        public event EventHandler AttributeChanged;
        /// <summary>
        /// This function gets called when the addbutton of the exclude attribbute filter gets pressed<br/>
        /// It then adds a Metadata attribute to the exclude filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExcludedAttributes_TagTanel_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Attribute childControl = new Attribute(usedAttributes: null);
            childControl.AttributeChanged += ParentControl_AttributeChanged;
            ExcludedAttributes_WrapPanel.Children.Add(childControl);

        }
        /// <summary>
        /// this function is called from the child elements from AttributeChanged and triggers the filter refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentControl_AttributeChanged(object sender, EventArgs e)
        {
            RefreshAttributeFilters();
        }
        /// <summary>
        /// this cancel token is used when the filter changes but filtering is still on progress.<br/>
        /// it stops the current filtering process so that the new Filters can be applied
        /// </summary>
        CancellationTokenSource cancelStatusFiltering = new CancellationTokenSource();
        /// <summary>
        /// this variable is used to indicate that a filter process is currently running
        /// </summary>
        bool IsStatusFiltering = false;
        /// <summary>
        /// this semaphore is beeing used to make sure only one filtering process is in the queue at any give time.
        /// If a filter is processing and a new request is in the queue, waiting for the processing to stop, all further
        /// requests will be blocked off. This is not an issue, as the filtering process pulls the newest filters on start.
        /// It just needs to be started
        /// </summary>
        SemaphoreSlim IsStatusFiltering_Lock = new SemaphoreSlim(1,1);
        /// <summary>
        /// this function starts the filtering process and oulls the most current filters upon start.
        /// when the filters change during filtering process, the current filtering process is beeing canceled in order to pull the new
        /// Filters.
        /// </summary>
        internal async void RefreshStatusFilters()
        {
            // make sure only one filter process in processing and max 1 request to refresh the filters is in the queue
            if (!await IsStatusFiltering_Lock.WaitAsync(0).ConfigureAwait(false))
            {
                if (cancelStatusFiltering.IsCancellationRequested)
                {
                    // there is already a task in the queue requesting filter refreshes. Block off further requests
                    return;
                }
                // cancle the current filter process and wait until the process is cancelled so that
                // the filter can be reexecuted with new filter values
                cancelStatusFiltering.Cancel();
                await IsStatusFiltering_Lock.WaitAsync().ConfigureAwait(false);
            }
            // pull filter values and start filtering
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
            // reset variables and wrap up this stage of the filter
            finally
            {
                cancelStatusFiltering = new CancellationTokenSource();
                IsStatusFiltering = false;
                IsStatusFiltering_Lock.Release();
            }
        }
        /// <summary>
        /// this cancel token is used when the filter changes but filtering is still on progress.<br/>
        /// it stops the current filtering process so that the new Filters can be applied
        /// </summary>
        CancellationTokenSource cancelNameFiltering = new CancellationTokenSource();
        /// <summary>
        /// this variable is used to indicate that a filter process is currently running
        /// </summary>
        bool IsNameFiltering = false;
        /// <summary>
        /// this semaphore is beeing used to make sure only one filtering process is in the queue at any give time.
        /// If a filter is processing and a new request is in the queue, waiting for the processing to stop, all further
        /// requests will be blocked off. This is not an issue, as the filtering process pulls the newest filters on start.
        /// It just needs to be started
        /// </summary>
        SemaphoreSlim IsNameFiltering_Lock = new SemaphoreSlim(1,1);
        /// <summary>
        /// this function starts the filtering process and oulls the most current filters upon start.
        /// when the filters change during filtering process, the current filtering process is beeing canceled in order to pull the new
        /// Filters.
        /// </summary>
        private async void RefreshNameFilters(bool continuationTask = false)
        {
            // make sure only one filter process in processing and max 1 request to refresh the filters is in the queue
            if (!await IsNameFiltering_Lock.WaitAsync(0).ConfigureAwait(false))
            {
                if (cancelNameFiltering.IsCancellationRequested || IsStatusFiltering)
                {
                    // there is already a task in the queue requesting filter refreshes. Block off further requests
                    return;
                }
                // cancle the current filter process and wait until the process is cancelled so that
                // the filter can be reexecuted with new filter values
                cancelNameFiltering.Cancel();
                await IsNameFiltering_Lock.WaitAsync().ConfigureAwait(false);
            }
            // pull filter values and start filtering
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
            // reset variables and wrap up this stage of the filter
            finally
            {
                cancelNameFiltering = new CancellationTokenSource();
                IsNameFiltering = false;
                IsNameFiltering_Lock.Release();
            }
        }
        /// <summary>
        /// this cancel token is used when the filter changes but filtering is still on progress.<br/>
        /// it stops the current filtering process so that the new Filters can be applied
        /// </summary>
        CancellationTokenSource cancelAttributeFiltering = new CancellationTokenSource();
        /// <summary>
        /// this semaphore is beeing used to make sure only one filtering process is in the queue at any give time.
        /// If a filter is processing and a new request is in the queue, waiting for the processing to stop, all further
        /// requests will be blocked off. This is not an issue, as the filtering process pulls the newest filters on start.
        /// It just needs to be started
        /// </summary>
        SemaphoreSlim IsAttributeFiltering_Lock = new SemaphoreSlim(1,1);
        /// <summary>
        /// this function starts the filtering process and oulls the most current filters upon start.
        /// when the filters change during filtering process, the current filtering process is beeing canceled in order to pull the new
        /// Filters.
        /// </summary>
        private async void RefreshAttributeFilters(bool continuationTask = false)
        {
            // make sure only one filter process in processing and max 1 request to refresh the filters is in the queue
            if (!await IsAttributeFiltering_Lock.WaitAsync(0).ConfigureAwait(false))
            {
                if (cancelAttributeFiltering.IsCancellationRequested || IsStatusFiltering)
                {
                    // there is already a task in the queue requesting filter refreshes. Block off further requests
                    return;
                }
                // cancle the current filter process and wait until the process is cancelled so that
                // the filter can be reexecuted with new filter values
                cancelAttributeFiltering.Cancel();
                await IsAttributeFiltering_Lock.WaitAsync().ConfigureAwait(false);
            }
            // pull filter values and start filtering
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
            // reset variables and wrap up this stage of the filter
            finally
            {
                cancelAttributeFiltering = new CancellationTokenSource();
                IsAttributeFiltering_Lock.Release();
            }
        }
    }
}
