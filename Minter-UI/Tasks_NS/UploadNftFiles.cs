using Chia_NFT_Minter.CollectionInformation_ns;
using Chia_NFT_Minter;
using CHIA_RPC.Wallet_RPC_NS.NFT;
using Minter_UI.Settings_NS;
using NFT.Storage.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Minter_UI.Tasks_NS
{
    internal class UploadNftFiles
    {
        internal static bool UploadingInProgress = false;
        internal static async Task UploadAndGenerateRpcs_Task(CancellationToken cancle)
        {
            if (UploadingInProgress) return;
            string royaltyAdress = "xch10fjlp8nv5ru5pfl4wad9gqpk9350anggum6vqemuhmwlmy54pnlskcq2aj";
            if (GlobalVar.Licensed)
            {
                royaltyAdress = GlobalVar.PrimaryWalletAdress;
            }
            foreach (FileInfo nftFile in CollectionInformation.Information.NftFiles.Values)
            {
                if (cancle.IsCancellationRequested) return;
                // get nft name and identifier key
                string nftName = Path.GetFileNameWithoutExtension(nftFile.FullName);
                string key = nftName;
                if (!Settings.All.CaseSensitiveFileHandling)
                {
                    key = key.ToLower();
                }
                // check if this nft has metadata and is not yet uploaded
                if (CollectionInformation.Information.MetadataFiles.ContainsKey(key)
                    && !CollectionInformation.Information.RpcFiles.ContainsKey(key)
                    && !CollectionInformation.Information.MintedFiles.ContainsKey(key))
                {
                    // upload files
                    /// upload nft file
                    List<string> nftlinkList = new List<string>();
                    Task<NFT_File> nftUploadTask = Task.Run(() => NftStorageAccount.Client.Upload(nftFile.FullName));
                    nftUploadTask.Wait();
                    nftlinkList.Add(nftUploadTask.Result.URL);
                    /// upload metadata
                    List<string> metalinkList = new List<string>();
                    Task<NFT_File> metaUploadTask =
                        Task.Run(() => NftStorageAccount.Client.Upload(CollectionInformation.Information.MetadataFiles[key]));
                    metaUploadTask.Wait();
                    metalinkList.Add(metaUploadTask.Result.URL);
                    // build link lists for rpc
                    if (Settings.All.CustomServerURL != null)
                    {
                        /// prepare custom url (~prefix)
                        string customLink = Settings.All.CustomServerURL;
                        if (!customLink.EndsWith("/")) customLink += "/";
                        /// finalize custom uri
                        nftlinkList.Add(customLink + Directories.Nfts.Name + "/" + nftFile.Name);
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
                    rpc.Save(Path.Combine(Directories.Rpcs.FullName, (nftName + ".rpc")));
                }
            }
        }
    }
}
