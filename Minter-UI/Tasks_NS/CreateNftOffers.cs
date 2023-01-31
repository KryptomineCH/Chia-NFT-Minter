using Chia_Client_API.Wallet_NS.WalletAPI_NS;
using Chia_NFT_Minter.CollectionInformation_ns;
using CHIA_RPC.Wallet_RPC_NS.Wallet_NS;
using Minter_UI.CollectionInformation_ns;
using Minter_UI.UI_NS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using CHIA_RPC.Objects_NS;
using Minter_UI.Settings_NS;

namespace Minter_UI.Tasks_NS
{
    internal class CreateNftOffers
    {
        internal static bool OfferingInProgress = false;
        private static object OfferingInProgressLock = new object();
        internal static async Task OfferNfts_Task(
            CancellationToken cancle,
            MintingPreview_ViewModel uiView,
            DispatcherObject dispatcherObject)
        {
            lock (OfferingInProgressLock)
            {
                if (OfferingInProgress) return;
                OfferingInProgress = true;
            }
            try
            {
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
                    string key = nftName;
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
                    if (!Settings.All.CaseSensitiveFileHandling)
                    {
                        key = key.ToLower();
                    }
                    FileInfo offerFilePath = new FileInfo(Path.Combine(Directories.Offered.FullName, nftName + ".offer"));
                    offer.Save(offerFilePath.FullName);
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
                lock (OfferingInProgressLock)
                {
                    OfferingInProgress = false;
                }
            }
        }
    }
}