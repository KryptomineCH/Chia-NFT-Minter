﻿using Chia_Client_API.Wallet_NS.WalletAPI_NS;
using CHIA_RPC.Objects_NS;
using CHIA_RPC.Wallet_RPC_NS.Wallet_NS;
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
using Minter_UI.Settings_NS;

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
                    // if there are no nfts to be offer, wait for more
                    if (CollectionInformation.Information.ReadyToOffer.IsEmpty)
                    {
                        await Task.Delay(2000, cancle).ConfigureAwait(false);
                        continue;
                    }
                    // get nft to be offered
                    KeyValuePair<string, FileInfo> nftToBeOffered_File = CollectionInformation.Information.ReadyToOffer.First();
                    _ = CollectionInformation.Information.ReadyToOffer.Remove(nftToBeOffered_File.Key, out _);
                    // if the file is not an NFT file, continue
                    if (!nftToBeOffered_File.Value.Name.EndsWith(".nft"))
                    {
                        continue;
                    }
                    Nft nftToBeOffered = Nft.Load(nftToBeOffered_File.Value.FullName);
                    // calculate chia offer price
                    long mojosPrice = (long)(Settings.All.OfferingPrice * 1000000000000);
                    // create offer content
                    Offer_RPC offer_rpc = new Offer_RPC();
                    offer_rpc.offer.Add("1", mojosPrice);
                    offer_rpc.offer.Add(nftToBeOffered.launcher_id, -1);
                    OfferFile offer = await WalletApi.CreateOfferForIds(offer_rpc).ConfigureAwait(false);
                    // save offer file
                    string nftName = Path.GetFileNameWithoutExtension(nftToBeOffered_File.Value.Name);
                    string key = CollectionInformation.GetKeyFromFile(nftToBeOffered_File.Value);
                    // update ui
                    dispatcherObject.Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (MintingItem item in uiView.Items)
                        {
                            if (item.Key == key)
                            {
                                item.IsUploading = true;
                                break;
                            }
                        }
                    }));
                    FileInfo offerFilePath = new FileInfo(Path.Combine(Directories.Offered.FullName, nftName + ".offer"));
                    offer.Export(offerFilePath.FullName);
                    /// add successful mint to collection information
                    CollectionInformation.Information.OfferedFiles[key] = offerFilePath;
                    // update ui
                    dispatcherObject.Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (MintingItem item in uiView.Items)
                        {
                            if (item.Key == key)
                            {
                                item.IsUploading = false;
                                item.IsUploaded = true;
                                break;
                            }
                        }
                    }));
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