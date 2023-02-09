using Minter_UI.CollectionInformation_ns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Minter_UI.Tasks_NS
{
    /// <summary>
    /// this class contains the status filter which is the initial filter
    /// It is the fastest filter so it should be executed first. 
    /// </summary>
    /// <remarks>it does so by utilizing the collection information dictionaries</remarks>
    internal class StatusFilter
    {
        /// <summary>
        /// this collection will containe the filtered collection
        /// </summary>
        internal Dictionary<string, FileInfo> StatusFilteredNFTs = new Dictionary<string, FileInfo>();
        /// <summary>
        ///  this variable is required as it is an iterativ filter.
        ///  The step count can be calculated in advance
        /// </summary>
        internal int StatusFilterSteps = 0;
        /// <summary>
        /// this task refreshed the filter by using the collectioninformation dictionaries. 
        /// It is an iterative process
        /// </summary>
        /// <param name="includeAllImages"></param>
        /// <param name="includeExistingMetadataImages"></param>
        /// <param name="includeUploadedImages"></param>
        /// <param name="includePendingMints"></param>
        /// <param name="includeMintedImages"></param>
        /// <param name="includeOfferedImages"></param>
        /// <param name="progress"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        internal async Task RefreshStatusFilter(
            bool includeAllImages,
            bool includeExistingMetadataImages,
            bool includeUploadedImages,
            bool includePendingMints,
            bool includeMintedImages,
            bool includeOfferedImages,
            IProgress<float> progress,
            CancellationToken cancellation)
        {
            // initialize progress 
            progress.Report(0);
            // calculate the amount of steps the filter will take to report progress.
            CalculateStatusFilterSteps(
                includeAllImages,
                includeExistingMetadataImages,
                includeUploadedImages,
                includePendingMints,
                includeMintedImages,
                includeOfferedImages);
            // clear old collection
            StatusFilteredNFTs.Clear();
            // check if all nfts should be included
            int step = 0;
            if (includeAllImages)
            {
                // all nfts should be included. dhat is the easiest case
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.NftFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs.Add(nft.Key, nft.Value);
                    step++;
                    progress.Report(((float)step/StatusFilterSteps) / 3f);
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
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
                }
            }
            else if (!includeExistingMetadataImages && includeAllImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.MetadataFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
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
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
                }
            }
            else if (!includeUploadedImages && includeExistingMetadataImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.RpcFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
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
                            progress.Report(0);
                            return;
                        }
                        StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    }
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
                }
            }
            else if (!includePendingMints && includeUploadedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.PendingTransactions.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
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
                        progress.Report(0);
                        return;
                    }
                    if (!StatusFilteredNFTs.ContainsKey(nft.Key))
                    {
                        StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    }
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
                }
            }
            else if (!includeMintedImages && includeUploadedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.MintedFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
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
                        progress.Report(0);
                        return;
                    }
                    if (!StatusFilteredNFTs.ContainsKey(nft.Key))
                    {
                        StatusFilteredNFTs[nft.Key] = CollectionInformation.Information.NftFiles[nft.Key];
                    }
                    step++;
                    progress.Report(((float)step / StatusFilterSteps) / 3f);
                }
            }
            else if (!includeOfferedImages && includeMintedImages)
            {
                KeyValuePair<string, FileInfo>[] selection = CollectionInformation.Information.OfferedFiles.ToArray();
                foreach (KeyValuePair<string, FileInfo> nft in selection)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    StatusFilteredNFTs.Remove(nft.Key);
                    step++;
                    progress.Report(((float)step / StatusFilterSteps)/3f);
                }
            }
        }
        /// <summary>
        /// this function calculates the steps the filter needs to take.
        /// it is an iterative process so the step count cannot be known in advance without calculation but the calculation
        /// is very straight forward.
        /// </summary>
        /// <param name="includeAllImages"></param>
        /// <param name="includeExistingMetadataImages"></param>
        /// <param name="includeUploadedImages"></param>
        /// <param name="includePendingMints"></param>
        /// <param name="includeMintedImages"></param>
        /// <param name="includeOfferedImages"></param>
        internal void CalculateStatusFilterSteps(
            bool includeAllImages,
            bool includeExistingMetadataImages,
            bool includeUploadedImages,
            bool includePendingMints,
            bool includeMintedImages,
            bool includeOfferedImages)
        {
            int steps = 0;
            int nftSelectionCount = 0;
            /// calculation of the steps and final nft count from status selector
            // all nfts
            if (includeAllImages)
            {
                steps += CollectionInformation.Information.NftFiles.Count;
                nftSelectionCount += CollectionInformation.Information.NftFiles.Count;

            }
            // metadata
            steps += CollectionInformation.Information.MetadataFiles.Count;
            if (!includeExistingMetadataImages && includeAllImages)
            {
                nftSelectionCount -= CollectionInformation.Information.MetadataFiles.Count;
            }
            else if (includeExistingMetadataImages && !includeAllImages)
            {
                nftSelectionCount += CollectionInformation.Information.MetadataFiles.Count;
            }
            // uploaded (rpcs)
            if (!includeExistingMetadataImages && includeUploadedImages)
            {
                steps += CollectionInformation.Information.RpcFiles.Count;
                nftSelectionCount -= CollectionInformation.Information.RpcFiles.Count;
            }
            if (includeExistingMetadataImages && !includeUploadedImages)
            {
                steps += CollectionInformation.Information.RpcFiles.Count;
                nftSelectionCount += CollectionInformation.Information.RpcFiles.Count;
            }
            // pending mints
            if (!includePendingMints && includeUploadedImages)
            {
                steps += CollectionInformation.Information.PendingTransactions.Count;
                nftSelectionCount -= CollectionInformation.Information.PendingTransactions.Count;
            }
            if (includePendingMints && !includeUploadedImages)
            {
                steps += CollectionInformation.Information.PendingTransactions.Count;
                nftSelectionCount += CollectionInformation.Information.PendingTransactions.Count;
            }
            // minted
            if (!includeMintedImages && includePendingMints)
            {
                steps += CollectionInformation.Information.MintedFiles.Count;
                nftSelectionCount -= CollectionInformation.Information.MintedFiles.Count;
            }
            if (includeMintedImages && !includePendingMints)
            {
                steps += CollectionInformation.Information.MintedFiles.Count;
                nftSelectionCount += CollectionInformation.Information.MintedFiles.Count;
            }
            // offered
            if (!includeMintedImages && includeOfferedImages)
            {
                steps += CollectionInformation.Information.OfferedFiles.Count;
                nftSelectionCount -= CollectionInformation.Information.OfferedFiles.Count;
            }
            if (includeMintedImages && !includeOfferedImages)
            {
                steps += CollectionInformation.Information.OfferedFiles.Count;
                nftSelectionCount += CollectionInformation.Information.OfferedFiles.Count;
            }
            StatusFilterSteps = steps;
        }
    }
}
