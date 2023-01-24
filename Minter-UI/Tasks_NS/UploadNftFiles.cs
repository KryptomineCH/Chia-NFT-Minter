using Chia_NFT_Minter.CollectionInformation_ns;
using CHIA_RPC.Wallet_RPC_NS.NFT;
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

namespace Minter_UI.Tasks_NS
{
    internal class UploadNftFiles
    {
        internal static bool UploadingInProgress = false;
        private static object UploadingInProgressLock = new object();
        internal static async Task UploadAndGenerateRpcs_Task(
            CancellationToken cancle, 
            MintingPreview_ViewModel uiView,
            DispatcherObject dispatcherObject)
        {
            if (NftStorageAccount.Client == null)
            {
                MessageBox.Show("cannot upload pics because nft.storage account is not set!");
                return;
            }
            lock(UploadingInProgressLock)
            {
                if (UploadingInProgress)
                {
                    return;
                }
                UploadingInProgress = true;
            }
            string royaltyAdress = "xch10fjlp8nv5ru5pfl4wad9gqpk9350anggum6vqemuhmwlmy54pnlskcq2aj";
            if (GlobalVar.Licensed && GlobalVar.PrimaryWalletAdress != null)
            {
                royaltyAdress = GlobalVar.PrimaryWalletAdress;
            }
            while(!cancle.IsCancellationRequested && !CollectionInformation.Information.MissingRPCs.IsEmpty)
            {
                KeyValuePair<string, FileInfo> nftToBeUploaded = CollectionInformation.Information.MissingRPCs.First();
                // get nft name and identifier key
                string nftFullName = nftToBeUploaded.Value.FullName;
                string nftName = Path.GetFileNameWithoutExtension(nftToBeUploaded.Value.Name);
                string key = nftName;
                if (Settings.All != null && !Settings.All.CaseSensitiveFileHandling)
                {
                    key = key.ToLower();
                }
                // check if this nft has metadata and is not yet uploaded
                if (CollectionInformation.Information.MetadataFiles.ContainsKey(key))
                {
                    // upload files
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
                    /// upload nft file
                    List<string> nftlinkList = new List<string>();
                    FileInfo nft = CollectionInformation.Information.NftFiles[key];
                    Task<NFT_File> nftUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(nft.FullName));
                    await nftUploadTask.ConfigureAwait(false); ;
                    if (cancle.IsCancellationRequested)
                    {
                        goto CancleJump;
                    }
                    nftlinkList.Add(nftUploadTask.Result.URL);
                    /// upload metadata
                    List<string> metalinkList = new List<string>();
                    Task<NFT_File> metaUploadTask =
                        Task.Run(() => NftStorageAccount.Client.Upload(CollectionInformation.Information.MetadataFiles[key]));
                    await metaUploadTask.ConfigureAwait(false); ;
                    if (cancle.IsCancellationRequested)
                    {
                        goto CancleJump;
                    }
                    metalinkList.Add(metaUploadTask.Result.URL);
                    // build link lists for rpc
                    if (Settings.All != null && Settings.All.CustomServerURL != null)
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
                    if (Settings.All != null && Settings.All.LicenseURL != null)
                    {
                        licenseLinks.Add(Settings.All.LicenseURL);
                    }
                    if (Settings.All != null && Settings.All.LicenseURL_Backup != null)
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
                    rpc.Save(Path.Combine(Directories.Rpcs.FullName, (nftName + ".rpc")));
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
            lock (UploadingInProgressLock)
            {
                UploadingInProgress = false;
            }
        }
    }
}
