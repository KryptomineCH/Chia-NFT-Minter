using Minter_UI.CollectionInformation_ns;
using Minter_UI.UI_NS;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System;
using System.Linq;

namespace Minter_UI.Tasks_NS
{
    /// <summary>
    /// This class will upload offers to decentralized exchanges
    /// </summary>
    internal class PublishOffers
    {
        /// <summary>
        /// A static boolean variable to track if offering is in progress.
        /// </summary>
        internal static bool OfferingInProgress = false;
        /// <summary>
        /// A static object used to lock the `OfferingInProgress` variable.
        /// </summary>
        private static object OfferingInProgressLock = new object();
        /// <summary>
        /// The `OfferNfts_Task` method is used to offer NFTs for sale.
        /// </summary>
        /// <param name="cancle">The `CancellationToken` used to cancel the task.</param>
        /// <param name="uiView">The `MintingPreview_ViewModel` instance used for UI updates.</param>
        /// <param name="dispatcherObject">The `DispatcherObject` used for UI updates.</param>
        /// <returns>A `Task` that represents the asynchronous operation.</returns>
        internal static async Task PublishOffers_Task(
            CancellationToken cancle,
            MintingPreview_ViewModel uiView,
            DispatcherObject dispatcherObject)
        {
            lock (OfferingInProgressLock)
            {
                // if offering is already in progress, dont start the task twice, instead, return
                if (OfferingInProgress) return;
                OfferingInProgress = true;
            }
            try
            {
                // repeat until the task is cancelled
                while (!cancle.IsCancellationRequested)
                {
                    // if there are no nfts to be offered, wait for more
                    if (CollectionInformation.Information.ReadyToPublishOffer.IsEmpty)
                    {
                        await Task.Delay(2000, cancle).ConfigureAwait(false);
                        continue;
                    }
                    // get nft to be offered
                    KeyValuePair<string, FileInfo> nftToBeOffered_File = CollectionInformation.Information.ReadyToPublishOffer.First();
                    _ = CollectionInformation.Information.ReadyToPublishOffer.Remove(nftToBeOffered_File.Key, out _);
                    // if the file is not an offer file, continue
                    if (!nftToBeOffered_File.Value.Name.EndsWith(".offer"))
                    {
                        continue;
                    }
                    string exchange = nftToBeOffered_File.Key.Split("_").Last();
                    string offerText = File.ReadAllText(nftToBeOffered_File.Value.FullName);
                    if (exchange == "dexie")
                    {
                        if (!Settings_NS.Settings.All.PublishOffersTo_DexieSpace)
                        {
                            continue;
                        }
                        Dexie.Space.Net.Offers_NS.Response_NS.PostOffer_Response publish =  await Dexie.Space.Net.Offers_NS.Offers_Client.PostOffer_Async(offerText).ConfigureAwait(false);
                        Dexie.Space.Net.Offers_NS.Response_NS.GetOffer_Response publishedOffer = await Dexie.Space.Net.Offers_NS.Offers_Client.GetOffer_Async(publish.id).ConfigureAwait(false);
                        FileInfo targetPath = new FileInfo(Path.Combine(Directories.PublishedOffers.FullName, nftToBeOffered_File.Key));
                        publishedOffer.offer.Save(targetPath.FullName);
                        CollectionInformation.Information.PublishedOffers[nftToBeOffered_File.Key] = targetPath;
                    }
                    else if(exchange == "spacescan")
                    {
                        if (!Settings_NS.Settings.All.PublishOffersTo_SpaceScan)
                        {
                            continue;
                        }
                        Spacescan.IO.Net.Offering_NS.Responses_NS.PostOffer_Response publishedOffer = await Spacescan.IO.Net.Offering_NS.Offers_Client.PostOffer_Async(offerText).ConfigureAwait(false);
                        FileInfo targetPath = new FileInfo(Path.Combine(Directories.PublishedOffers.FullName, nftToBeOffered_File.Key));
                        publishedOffer.offer.Save(targetPath.FullName);
                        CollectionInformation.Information.PublishedOffers[nftToBeOffered_File.Key] = targetPath;
                    }
                    else
                    {
                        throw new Exception($"Exchange {exchange} is not recognized!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There has been an error while offering!" +
                            $"{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            ex.ToString());
            }
            finally
            {
                // WrapDirection up the offering process
                lock (OfferingInProgressLock)
                {
                    OfferingInProgress = false;
                }
            }
        }
    }
}