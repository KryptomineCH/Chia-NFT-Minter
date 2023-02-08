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
                progress.Report(1);
                return;
            }
            MetadataAttribute excludeFilterAttribute_temp;
            MetadataAttribute includeFilterAttribute_temp;
            float prePprogress = 2f / 3f;
            KeyValuePair<string, FileInfo>[] NameFilteredNFTs = NameFilter.NameFilteredNFTs.ToArray();
            for (int i = 0; i < NameFilteredNFTs.Length; i++)
            {
                if (cancellation.IsCancellationRequested)
                {
                    progress.Report(0);
                    return;
                }
                KeyValuePair<string, FileInfo> nft = NameFilteredNFTs[i];
                FileInfo metadataFile;
                if (CollectionInformation.Information.MetadataFiles.TryGetValue(nft.Key, out metadataFile))
                {
                    Metadata metadata;
                    if (!CollectionInformation.GetMetadataFromCache(nft.Key, out metadata))
                    {
                        continue;
                    }
                    bool include = false;
                    bool exclude = false;
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
                    if (!exclude)
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
                float stepProgress = (float)i / NameFilteredNFTs.Length;
                stepProgress /= 3f;
                progress.Report(prePprogress + stepProgress);

            }
        }
    }
}
