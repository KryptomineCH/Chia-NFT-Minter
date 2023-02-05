using Minter_UI.CollectionInformation_ns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minter_UI.Tasks_NS
{
    internal class StatusFilter
    {
        internal Dictionary<string, FileInfo> StatusFilteredNFTs = new Dictionary<string, FileInfo>();
        internal async Task RefreshStatusFilter(
            bool includeAllImages,
            bool includeExistingMetadataImages,
            bool includeUploadedImages,
            bool includePendingMints,
            bool includeMintedImages,
            bool includeOfferedImages,
            IProgress<int> progress,
            CancellationToken cancellation)
        {
            StatusFilteredNFTs.Clear();
            // check if all nfts should be included
            if (includeAllImages)
            {
                // all nfts should be included. dhat is the easiest case
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.NftFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs.Add(nft.Key, nft.Value);
                }
            }

            // add metadata if selected
            if (includeExistingMetadataImages && !includeAllImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.MetadataFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                }
            }
            else if (!includeExistingMetadataImages && includeAllImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.MetadataFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                }
            }
            // filter by upload status
            if (includeUploadedImages && !includeExistingMetadataImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.RpcFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                }
            }
            else if (!includeUploadedImages && includeExistingMetadataImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.RpcFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                }
            }
            // filter by Pending Mint status
            if (includePendingMints && !includeUploadedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.PendingTransactions.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (!StatusFilteredNFTs.ContainsKey(nft.Key))
                    {
                        if (cancellation.IsCancellationRequested)
                        {
                            return;
                        }
                        StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    }
                }
            }
            else if (!includePendingMints && includeUploadedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.PendingTransactions.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                }
            }
            // filter by minted status
            if (includeMintedImages && !includeUploadedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.MintedFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    if (!StatusFilteredNFTs.ContainsKey(nft.Key))
                    {
                        StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    }
                }
            }
            else if (!includeMintedImages && includeUploadedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.MintedFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                }
            }
            // filter by offered status
            if (includeOfferedImages && !includeMintedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.OfferedFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    if (!StatusFilteredNFTs.ContainsKey(nft.Key))
                    {
                        StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    }
                }
            }
            else if (!includeOfferedImages && includeMintedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.OfferedFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                }
            }
        }
    }
}
