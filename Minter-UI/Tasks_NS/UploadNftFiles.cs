using Minter_UI.Settings_NS;
using NFT.Storage.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Windows;
using Minter_UI.CollectionInformation_ns;
using Minter_UI.UI_NS;
using System.Windows.Threading;
using System;
using CollectionInformation = Minter_UI.CollectionInformation_ns.CollectionInformation;
using CHIA_RPC.Wallet_NS.NFT_NS;

namespace Minter_UI.Tasks_NS
{
    /// <summary>
    /// This clas contains the fire & forget task which uploads the NFT Files
    /// </summary>
    internal class UploadNftFiles
    {
        /// <summary>
        /// variable which specifies if the uploading is currently running or not
        /// </summary>
        internal static bool UploadingInProgress = false;
        /// <summary>
        /// lock is beeing used to update the UploadingInProgress variable in a threadsafe manner and to
        /// block the task from running twice
        /// </summary>
        private static object UploadingInProgressLock = new object();
        /// <summary>
        /// infinite loop which can be cancelled which uploads the NFT media
        /// </summary>
        /// <param name="cancel">cancellation token to stop the process</param>
        /// <param name="uiView">the viewmodel which is beeing used to update the display status</param>
        /// <param name="dispatcherObject">making sure the correct process updates the ui</param>
        /// <returns></returns>
        internal static async Task UploadAndGenerateRpcs_Task(
            CancellationToken cancel, 
            MintingPreview_ViewModel uiView,
            DispatcherObject dispatcherObject)
        {
            // if no nft.storage api key is set the media cant be uploaded
            if (NftStorageAccount.Client == null)
            {
                MessageBox.Show("cannot upload pics because nft.storage account is not set!");
                return;
            }
            // make sure the task is only started once
            lock(UploadingInProgressLock)
            {
                if (UploadingInProgress)
                {
                    return;
                }
                UploadingInProgress = true;
            }
            // the default cryptomine royalty address (if the software is not licensed)
            string royaltyAdress = "xch10fjlp8nv5ru5pfl4wad9gqpk9350anggum6vqemuhmwlmy54pnlskcq2aj";
            try
            {
                // infinite loop unless cancelled
                while (!cancel.IsCancellationRequested)
                {
                    // if there are no nfts to be offer, wait for more
                    if (CollectionInformation.Information.MissingRPCs.IsEmpty)
                    {
                        try
                        {
                            await Task.Delay(2000, cancel).ConfigureAwait(false);
                        }
                        catch (TaskCanceledException)
                        {
                            // Log the exception or perform any necessary cleanup
                            break;
                        }
                        continue;
                    }
                    if (GlobalVar.Licensed && GlobalVar.PrimaryWalletAdress != null)
                    {
                        royaltyAdress = GlobalVar.PrimaryWalletAdress;
                    }
                    KeyValuePair<string, FileInfo> nftToBeUploaded = CollectionInformation.Information.MissingRPCs.First();
                    // get nft name and identifier key
                    string nftFullName = nftToBeUploaded.Value.FullName;
                    string nftName = Path.GetFileNameWithoutExtension(nftToBeUploaded.Value.Name);
                    string key = CollectionInformation.GetKeyFromFile(nftToBeUploaded.Value);
                    // check if this nft has metadata and is not yet uploaded
                    if (CollectionInformation.Information.MetadataFiles.ContainsKey(key))
                    { // upload files
                        /// do not upload files which are uploaded already
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
                        /// upload nft (image) file
                        List<string> nftlinkList = new List<string>();
                        FileInfo nft = CollectionInformation.Information.NftFiles[key];
                        Task<NFT_File> nftUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(nft.FullName));
                        await nftUploadTask.ConfigureAwait(false); ;
                        if (cancel.IsCancellationRequested)
                        {
                            goto CancleJump;
                        }
                        nftlinkList.Add(nftUploadTask.Result.URL);
                        /// upload metadata
                        List<string> metalinkList = new List<string>();
                        Task<NFT_File> metaUploadTask =
                            Task.Run(() => NftStorageAccount.Client.Upload(CollectionInformation.Information.MetadataFiles[key]));
                        await metaUploadTask.ConfigureAwait(false); ;
                        if (cancel.IsCancellationRequested)
                        {
                            goto CancleJump;
                        }
                        metalinkList.Add(metaUploadTask.Result.URL);
                        // build link lists for rpc
                        if (Settings.All != null && !string.IsNullOrEmpty(Settings.All.CustomServerURL))
                        {
                            /// prepare custom url (~prefix)
                            string customLink = Settings.All.CustomServerURL;
                            if (!customLink.EndsWith("/")) customLink += "/";
                            /// finalize custom uri
                            nftlinkList.Add(customLink + Directories.Nfts.Name + "/" + nft.Name);
                            metalinkList.Add(
                                customLink + Directories.Metadata.Name + "/" + CollectionInformation.Information.MetadataFiles[key].Name);
                        }
                        /// add license url (is static)
                        List<string> licenseLinks = new List<string>();
                        if (Settings.All != null && !string.IsNullOrEmpty(Settings.All.LicenseURL))
                        {
                            licenseLinks.Add(Settings.All.LicenseURL);
                        }
                        if (Settings.All != null && !string.IsNullOrEmpty(Settings.All.LicenseURL_Backup))
                        {
                            licenseLinks.Add(Settings.All.LicenseURL_Backup);
                        }
                        // create rpc
                        NftMintNFT_RPC rpc = new NftMintNFT_RPC(
                            walletID: GlobalVar.NftWallet_ID,
                            nftLinks: nftlinkList.ToArray(),
                            metadataLinks: metalinkList.ToArray(),
                            licenseLinks: licenseLinks.ToArray(),
                            royaltyAddress: royaltyAdress
                            );
                        if (Settings.All != null)
                        {
                            rpc.fee = Settings.All.MintingFee;
                        }
                        /// save rpc
                        rpc.SaveRpcToFile(Path.Combine(Directories.Rpcs.FullName, (nftName + ".rpc")));
                        /// manage collection information
                        CollectionInformation.Information.MissingRPCs.Remove(nftToBeUploaded.Key, out _);
                        CollectionInformation.Information.ReadyToMint[nftToBeUploaded.Key] = nftToBeUploaded.Value;
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
                    else
                    { // no metadata found!
                        await Task.Delay(1000);
                    }
                }
            CancleJump:;
            }
            catch (TaskCanceledException ex)
            {
                /* expected behaviour */
            }
            catch (Exception ex)
            {
                LogException.Log(ex, unhandled: false);
                MessageBox.Show($"There has been an error while uploading!" +
                            $"{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            ex.ToString());


            }
            finally
            {
                // Wrap up the uploading process
                lock (UploadingInProgressLock)
                {
                    UploadingInProgress = false;
                }
            }
        }
    }
}
