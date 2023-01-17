using Chia_NFT_Minter.CollectionInformation_ns;
using Chia_NFT_Minter;
using CHIA_RPC.Wallet_RPC_NS.NFT;
using Minter_UI.Settings_NS;
using NFT.Storage.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Minter_UI.Tasks_NS
{
    internal class UploadNftFiles
    {
        internal static bool UploadingInProgress = false;
        private static object UploadingInProgressLock = new object();
        internal static async Task UploadAndGenerateRpcs_Task(CancellationToken cancle)
        {
            lock(UploadingInProgressLock)
            {
                if (UploadingInProgress) return;
                UploadingInProgress = true;
            }
            string royaltyAdress = "xch10fjlp8nv5ru5pfl4wad9gqpk9350anggum6vqemuhmwlmy54pnlskcq2aj";
            if (GlobalVar.Licensed)
            {
                royaltyAdress = GlobalVar.PrimaryWalletAdress;
            }
            while(!cancle.IsCancellationRequested && !CollectionInformation.Information.MissingRPCs.IsEmpty)
            {
                KeyValuePair<string, FileInfo> nftToBeUploaded = CollectionInformation.Information.MissingRPCs.First();
                // get nft name and identifier key
                string nftFullName = Path.GetFileNameWithoutExtension(nftToBeUploaded.Value.FullName);
                string nftName = Path.GetFileNameWithoutExtension(nftToBeUploaded.Value.Name);
                string key = nftFullName;
                if (!Settings.All.CaseSensitiveFileHandling)
                {
                    key = key.ToLower();
                }
                // check if this nft has metadata and is not yet uploaded
                if (CollectionInformation.Information.MetadataFiles.ContainsKey(key))
                {
                    // upload files
                    /// upload nft file
                    List<string> nftlinkList = new List<string>();
                    Task<NFT_File> nftUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(nftFullName));
                    nftUploadTask.Wait(cancle);
                    if (cancle.IsCancellationRequested)
                    {
                        goto CancleJump;
                    }
                    nftlinkList.Add(nftUploadTask.Result.URL);
                    /// upload metadata
                    List<string> metalinkList = new List<string>();
                    Task<NFT_File> metaUploadTask =
                        Task.Run(() => NftStorageAccount.Client.Upload(CollectionInformation.Information.MetadataFiles[key]));
                    metaUploadTask.Wait(cancle);
                    if (cancle.IsCancellationRequested)
                    {
                        goto CancleJump;
                    }
                    metalinkList.Add(metaUploadTask.Result.URL);
                    // build link lists for rpc
                    if (Settings.All.CustomServerURL != null)
                    {
                        /// prepare custom url (~prefix)
                        string customLink = Settings.All.CustomServerURL;
                        if (!customLink.EndsWith("/")) customLink += "/";
                        /// finalize custom uri
                        nftlinkList.Add(customLink + Directories.Nfts.Name + "/" + nftName);
                        metalinkList.Add(
                            customLink + Directories.Metadata.Name + "/" + CollectionInformation.Information.MetadataFiles[key].Name);
                    }
                    /// add license url (is static)
                    List<string> licenseLinks = new List<string> { Settings.All.LicenseURL };
                    if (Settings.All.LicenseURL_Backup != null)
                    {
                        licenseLinks.Add(Settings.All.LicenseURL_Backup);
                    }
                    // create rpc
                    NftMintNFT_RPC rpc = new NftMintNFT_RPC(
                        walletID: GlobalVar.NftWallet_ID,
                        nftLinks: nftlinkList.ToArray(),
                        metadataLinks: metalinkList.ToArray(),
                        licenseLinks: licenseLinks.ToArray(),
                        mintingFee_Mojos: (ulong)Settings.All.MintingFee,
                        royaltyAddress: royaltyAdress
                        );
                    /// save rpc
                    rpc.Save(Path.Combine(Directories.Rpcs.FullName, (nftFullName + ".rpc")));
                    /// manage collection information
                    CollectionInformation.Information.MissingRPCs.Remove(nftToBeUploaded.Key, out _);
                    CollectionInformation.Information.ReadyToMint[nftToBeUploaded.Key] = nftToBeUploaded.Value;
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
