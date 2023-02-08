using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Minter_UI.Tasks_NS
{
    internal class NameFilter
    {
        internal Dictionary<string, FileInfo> NameFilteredNFTs = new Dictionary<string, FileInfo>();
        internal int NameFilterSteps = 0;
        internal async Task RefreshNameFilter(
            string nameFilter,
            StatusFilter statusFilter,
            IProgress<float> progress,
            CancellationToken cancellation)
        {
            float preProgress = 1f / 3f;
            NameFilteredNFTs.Clear();
            if (nameFilter == null)
            {
                NameFilteredNFTs = new Dictionary<string, FileInfo>(statusFilter.StatusFilteredNFTs);
                progress.Report(2f / 3f);
                return;
            }
            nameFilter = nameFilter.Trim();
            if (nameFilter == "" || nameFilter == "*" || nameFilter == ".*")
            {
                NameFilteredNFTs = new Dictionary<string, FileInfo>(statusFilter.StatusFilteredNFTs);
                progress.Report(2f / 3f);
            }
            else
            {
                nameFilter = nameFilter.Replace("*", ".*");
                nameFilter = nameFilter.Replace("..*", ".*");
                if (Settings_NS.Settings.All.CaseSensitiveFileHandling!)
                {
                    nameFilter = nameFilter.ToLower();
                }
                KeyValuePair<string, FileInfo>[] statusFilteredNFTs = statusFilter.StatusFilteredNFTs.ToArray();
                for(int i = 0; i < statusFilteredNFTs.Length; i++)
                {
                    KeyValuePair<string, FileInfo> nft = statusFilteredNFTs[i];
                    if (cancellation.IsCancellationRequested)
                    {
                        progress.Report(0);
                        return;
                    }
                    if (Regex.IsMatch(nft.Key, nameFilter))
                    {
                        NameFilteredNFTs[nft.Key] = nft.Value;
                    }
                    float stepProgress = (float) i / statusFilteredNFTs.Length;
                    stepProgress /= 3f;
                    progress.Report(preProgress + stepProgress);
                }
            }
        }
    }
}
