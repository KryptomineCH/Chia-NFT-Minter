using Chia_NFT_Minter.CollectionInformation_ns;
using Chia_NFT_Minter;
using CHIA_RPC.Wallet_RPC_NS.NFT;
using Minter_UI.Settings_NS;
using NFT.Storage.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chia_Client_API.Wallet_NS.WalletAPI_NS;
using CHIA_RPC.General;
using CHIA_RPC.Wallet_RPC_NS.Wallet_NS;
using System.Data;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;

namespace Minter_UI.Tasks_NS
{
    internal class MintNftFiles
    {
        internal static bool MintingInProgress = false;
        private static object MintingInProgressLock = new object();
        internal static async Task UploadAndGenerateRpcs_Task(CancellationToken cancle)
        {
            lock (MintingInProgressLock)
            {
                if (MintingInProgress) return;
                MintingInProgress = true;
            }
            string royaltyAdress = "xch10fjlp8nv5ru5pfl4wad9gqpk9350anggum6vqemuhmwlmy54pnlskcq2aj";
            if (GlobalVar.Licensed)
            {
                royaltyAdress = GlobalVar.PrimaryWalletAdress;
            }
            while (!cancle.IsCancellationRequested && 
                (!CollectionInformation.Information.ReadyToMint.IsEmpty || UploadNftFiles.UploadingInProgress))
            {
                // if there are no nfts to be minted, wait for more
                if (CollectionInformation.Information.ReadyToMint.IsEmpty)
                {
                    await Task.Delay(1000);
                    continue;
                }
                // get nft to be minted
                KeyValuePair<string, FileInfo> nftToBeMinted = CollectionInformation.Information.MissingRPCs.First();
                _ = CollectionInformation.Information.MissingRPCs.Remove(nftToBeMinted.Key, out _);
                // stat mint task
            }
            CancleJump:;
            lock (MintingInProgressLock)
            {
                MintingInProgress = false;
            }
        }
        private static async Task CheckPendingTransactions()
        {
            FileInfo[] files = Directories.PendingTransactions.GetFiles("*.mint");
            foreach (FileInfo file in files)
            {
                NftMintNFT_Response spendBundle = JsonSerializer.Deserialize<NftMintNFT_Response>(File.ReadAllText(file.FullName));
                NftGetInfo_Response nft = await WalletApi.VerifyMint(spendBundle);
                if (nft.success)
                {
                    // nft was minted sucessfully
                    string nftFullName = Path.GetFileNameWithoutExtension(file.FullName);
                    string nftName = Path.GetFileNameWithoutExtension(file.Name);
                    string key = nftFullName;
                    if (!Settings.All.CaseSensitiveFileHandling)
                    {
                        key = key.ToLower();
                    }
                    // copy nft mint rpc file
                    FileInfo sourceRpcFile = CollectionInformation.Information.RpcFiles[key];
                    FileInfo targetRpcFile = new FileInfo(Path.Combine(Directories.Rpcs.FullName, sourceRpcFile.Name));
                    File.Copy(sourceRpcFile.FullName, targetRpcFile.FullName);
                    // delete 
                }
            }
        }
        private static async Task UploadNft(KeyValuePair<string, FileInfo> nftToBeMinted, string royaltyAdress, CancellationToken cancel)
        {
            // get nft name and identifier key
            string nftFullName = Path.GetFileNameWithoutExtension(nftToBeMinted.Value.FullName);
            string nftName = Path.GetFileNameWithoutExtension(nftToBeMinted.Value.Name);
            string key = nftFullName;
            if (!Settings.All.CaseSensitiveFileHandling)
            {
                key = key.ToLower();
            }
            // update rpc
            string rpcSourcePath = Path.Combine(Directories.Rpcs.FullName, nftName, ".rpc");
            NftMintNFT_RPC rpc = NftMintNFT_RPC.Load(rpcSourcePath);
            rpc.wallet_id = GlobalVar.NftWallet_ID;
            rpc.royalty_address = royaltyAdress;
            /// Mint!
            if (!Settings.All.CaseSensitiveFileHandling)
            {
                key = key.ToLower();
            }
            /// send mint transaction
            NftMintNFT_Response response = await WalletApi.NftMintNft_Async(rpc);
            /// save spend bundle to validate transaction completeness
            string transactionPath = Path.Combine(Directories.PendingTransactions.FullName, nftFullName,".mint");
            File.WriteAllText(transactionPath, response.ToString());
            /// wait for mint to complete
            NftGetInfo_Response nftInfo = await WalletApi.NftAwaitMintComplete_Async(response, cancel);
            /// save rpc
            if (nftInfo.success)
            {
                FileInfo nftFilePath = new FileInfo(Path.Combine(Directories.Minted.FullName, nftFullName));
                nftInfo.nft_info.Save(nftFilePath.FullName);
                /// manage collection information
                CollectionInformation.Information.MintedFiles[key] = nftToBeMinted.Value;
                /// delete pending transaction
                File.Delete(transactionPath);
            }
        }
    }
}