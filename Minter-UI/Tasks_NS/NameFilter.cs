using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Minter_UI.Tasks_NS
{
    /// <summary>
    /// this class filters the nfts by their name
    /// </summary>
    internal class NameFilter
    {
        /// <summary>
        /// this collection will contain the final collection of filtered nfts
        /// </summary>
        internal Dictionary<string, FileInfo> NameFilteredNFTs = new Dictionary<string, FileInfo>();
        /// <summary>
        /// task to refresh the filters which also reports progress to the filtering progress bar
        /// </summary>
        /// <param name="nameFilter"></param>
        /// <param name="statusFilter"></param>
        /// <param name="progress"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        internal async Task RefreshNameFilter(
            string nameFilter,
            StatusFilter statusFilter,
            IProgress<float> progress,
            CancellationToken cancellation)
        {
            // set the progress to its beginning state.
            // as the statuses are filtered first, it is at 1/3rd of the progress
            float preProgress = 1f / 3f;
            // clear collection
            NameFilteredNFTs.Clear();
            // no filter is set, just copy the input collection
            if (nameFilter == null)
            {
                NameFilteredNFTs = new Dictionary<string, FileInfo>(statusFilter.StatusFilteredNFTs);
                progress.Report(2f / 3f);
                return;
            }
            // prepare name filter and futher checks for include all filters
            nameFilter = nameFilter.Trim();
            if (nameFilter == "" || nameFilter == "*" || nameFilter == ".*")
            {
                NameFilteredNFTs = new Dictionary<string, FileInfo>(statusFilter.StatusFilteredNFTs);
                progress.Report(2f / 3f);
            }
            else
            {
                // prepare filter string for regex
                nameFilter = nameFilter.Replace("*", ".*");
                nameFilter = nameFilter.Replace("..*", ".*");
                if (Settings_NS.Settings.All.CaseSensitiveFileHandling!)
                {
                    nameFilter = nameFilter.ToLower();
                }
                // prepare input dictionary into an array so that a for loop can be used and the progress calculated
                KeyValuePair<string, FileInfo>[] statusFilteredNFTs = statusFilter.StatusFilteredNFTs.ToArray();
                for(int i = 0; i < statusFilteredNFTs.Length; i++)
                {
                    // get next nft to check for filter
                    KeyValuePair<string, FileInfo> nft = statusFilteredNFTs[i];
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    // match the filter
                    if (Regex.IsMatch(nft.Key, nameFilter))
                    {
                        NameFilteredNFTs[nft.Key] = nft.Value;
                    }
                    // report progress
                    float stepProgress = (float) i / statusFilteredNFTs.Length;
                    stepProgress /= 3f;
                    progress.Report(preProgress + stepProgress);
                }
            }
        }
    }
}
