using Chia_Metadata;
using Minter_UI.CollectionInformation_ns;
using Minter_UI.UI_NS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Minter_UI.Tasks_NS
{
    /// <summary>
    /// this class filters the nfts by attribute and contains the filtered nft collection.
    /// automatically updates the ui with the viewmodel.
    /// this is a complicated filter and might use resources and time. can be cancelled and reports progress
    /// hence it should be executed as the last filter
    /// </summary>
    internal class AttributeFilter
    {
        public MintingPreview_ViewModel AttributeFilteredNFTs = new MintingPreview_ViewModel();
        internal async Task RefreshAttributeFilter(
            List<MetadataAttribute> inclusions,
            List<MetadataAttribute> exclusions,
            NameFilter NameFilter,
            IProgress<float> progress,
            CancellationToken cancellation)
        {
            // clear old information
            AttributeFilteredNFTs.Items.Clear();
            // no filter, just copy values
            if (inclusions.Count <= 0 && exclusions.Count <= 0)
            {
                foreach (KeyValuePair<string, FileInfo> nft in NameFilter.NameFilteredNFTs)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    MintingItem mintingItem = new MintingItem(nft.Value.FullName);
                    if (CollectionInformation.Information.MetadataFiles.ContainsKey(nft.Key))
                    {
                        mintingItem.IsUploaded = true;
                    }
                    AttributeFilteredNFTs.Items.Add(mintingItem);
                }
                // report progress full. This is quick so no other progress is required
                progress.Report(1);
                return;
            }
            // load exclude and include filters
            MetadataAttribute excludeFilterAttribute_temp;
            MetadataAttribute includeFilterAttribute_temp;
            // update pre progress. two filters have been running before so it starts at 2/3rd
            float prePprogress = 2f / 3f;
            // prepare the preselection from dictionary to array so that a for loop can be used and the progress calculated easily
            KeyValuePair<string, FileInfo>[] NameFilteredNFTs = NameFilter.NameFilteredNFTs.ToArray();
            for (int i = 0; i < NameFilteredNFTs.Length; i++)
            {
                // loop through each preselected nft to apply filter
                if (cancellation.IsCancellationRequested)
                {
                    progress.Report(0);
                    return;
                }
                KeyValuePair<string, FileInfo> nft = NameFilteredNFTs[i];
                FileInfo metadataFile;
                // check if the nft contains a metadata attributed based on which should be filtered
                if (CollectionInformation.Information.MetadataFiles.TryGetValue(nft.Key, out metadataFile))
                {
                    Metadata metadata;
                    if (!CollectionInformation.GetMetadataFromCache(nft.Key, out metadata))
                    {
                        continue;
                    }
                    bool include = false;
                    bool exclude = false;
                    // check if the nft should be excluded (~blacklist)
                    // filters in an OR gate fashion
                    foreach (MetadataAttribute filterAttribute in exclusions)
                    {
                        if (cancellation.IsCancellationRequested)
                        {
                            progress.Report(0);
                            return;
                        }
                        if (metadata.AttributeNames.Contains(filterAttribute.trait_type))
                        {
                            MetadataAttribute nftAttribute;
                            if (metadata.AttributesDictionary.TryGetValue(filterAttribute.trait_type, out nftAttribute))
                            {
                                if (nftAttribute.AttributeMatchesFilter(filterAttribute))
                                {
                                    exclude = true;
                                    include = false;
                                    break;
                                }
                            }
                        }
                    }
                    // check if the nft is to be included (~whitelist)
                    // filters in an OR gate fashion
                    if (!exclude)
                    {
                        if (inclusions.Count == 0)
                        {
                            include = true;
                        }
                        else
                        {
                            foreach (MetadataAttribute filterAttribute in inclusions)
                            {
                                if (cancellation.IsCancellationRequested)
                                {
                                    progress.Report(0);
                                    return;
                                }
                                if (metadata.AttributeNames.Contains(filterAttribute.trait_type))
                                {
                                    MetadataAttribute nftAttribute;
                                    if (metadata.AttributesDictionary.TryGetValue(filterAttribute.trait_type, out nftAttribute))
                                    {
                                        if (nftAttribute.AttributeMatchesFilter(filterAttribute))
                                        {
                                            include = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // if the nft is not to be excluded and it is to be included, add it
                    if (include)
                    {
                        MintingItem mintingItem = new MintingItem(nft.Value.FullName);
                        if (CollectionInformation.Information.MintedFiles.ContainsKey(nft.Key))
                        {
                            mintingItem.IsMinting = true;
                        }
                        else if (CollectionInformation.Information.MetadataFiles.ContainsKey(nft.Key))
                        {
                            mintingItem.IsUploaded = true;
                        }
                        AttributeFilteredNFTs.Items.Add(mintingItem);
                    }
                }
                // update Progress
                float stepProgress = (float)i / NameFilteredNFTs.Length;
                stepProgress /= 3f;
                progress.Report(prePprogress + stepProgress);

            }
        }
    }
}
