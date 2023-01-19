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
using System.Runtime;
using System.Windows.Input;
using System.Windows;
using Minter_UI.CollectionInformation_ns;

namespace Minter_UI.Tasks_NS
{
    internal class MintNftFiles
    {
        internal static bool MintingInProgress = false;
        private static object MintingInProgressLock = new object();
        internal static async Task MintNfts_Task(CancellationToken cancle)
        {
            lock (MintingInProgressLock)
            {
                if (MintingInProgress) return;
                MintingInProgress = true;
            }
            string royaltyAdress = "xch10fjlp8nv5ru5pfl4wad9gqpk9350anggum6vqemuhmwlmy54pnlskcq2aj";
            if (GlobalVar.Licensed && GlobalVar.PrimaryWalletAdress != null)
            {
                royaltyAdress = GlobalVar.PrimaryWalletAdress;
            }
            else 
            {
                MessageBox.Show($"Software not licensed, not in sync or royalty address couldnt be found{Environment.NewLine}" +
                    $"1.9% Fees will go to KryptoMine");
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
                // balance check
                GetWalletBalance_Response walletBalance = await WalletApi.GetWalletBalance_Async(new WalletID_RPC() { wallet_id = 1 });
                if (walletBalance.wallet_balance.spendable_balance > Settings.All.MintingFee+1) 
                {
                    // start mint task
                    _ = Task.Run(() => MintNft(nftToBeMinted, royaltyAdress, cancle)).ConfigureAwait(false);
                }
                else if (walletBalance.wallet_balance.unconfirmed_wallet_balance != 0)
                {
                    // there seem to be transactions ongoing which block cash
                    int ongoingTransactions = await CheckPendingTransactions();
                    if (ongoingTransactions == 0)
                    { 
                        // no ongoing transactions, all transactions seem stuck!
                        await WalletApi.DeleteUnconfirmedTransactions_Async(1);
                    }
                    await Task.Delay(1000);
                }
                else
                {
                    // not enough balance
                    MessageBox.Show($"you do not seem to have enough balance to mint an NFT!" +
                        $"{Environment.NewLine}" +
                        $"{Environment.NewLine}" +
                        $"Minting has been stopped");
                    goto CancleJump;
                }
                
            }
            CancleJump:;
            lock (MintingInProgressLock)
            {
                MintingInProgress = false;
            }
        }
        /// <summary>
        /// This function checks for pending transactions related to NFT minting. 
        /// It looks for files with a ".mint" extension in the "PendingTransactions" directory, 
        /// and verifies if the NFT was minted successfully using the "VerifyMint" function from the "WalletApi" class. 
        /// If the NFT was minted successfully, 
        /// the function adds the NFT's information to the "MintedFiles" dictionary in the "CollectionInformation" class
        /// and deletes the corresponding pending transaction file. 
        /// If the transaction has been pending for longer than the specified time (default 0.25 hours) 
        /// it is considered stuck and the file is deleted and removed from the "PendingTransactions" dictionary.
        /// </summary>
        /// <param name="hoursAfterTransactionConsideredStuck">The amount of hours after which a pending transaction is considered stuck. Default is 0.25 hours.</param>
        /// <returns>The number of still pending transactions.</returns>
        private static async Task<int> CheckPendingTransactions(double hoursAfterTransactionConsideredStuck = 0.25)
        {
            FileInfo[] files = Directories.PendingTransactions.GetFiles("*.mint");
            int stillPendingTransactions = 0;
            foreach (FileInfo file in files)
            {
                // load spend bundle
                NftMintNFT_Response spendBundle = NftMintNFT_Response.Load(file.FullName);
                // check if nft was minted
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
                    FileInfo nftFilePath = new FileInfo(Path.Combine(Directories.Minted.FullName, nftName));
                    nft.nft_info.Save(nftFilePath.FullName);
                    /// add successful mint to collection information
                    CollectionInformation.Information.MintedFiles[key] = nftFilePath;
                    /// delete pending transaction
                    File.Delete(file.FullName);
                    /// remove pending transaction
                    _ = CollectionInformation.Information.PendingTransactions.Remove(key, out _);
                }
                else if (file.CreationTime < DateTime.Now - TimeSpan.FromHours(hoursAfterTransactionConsideredStuck))
                {
                    File.Delete(file.FullName);
                    // nft was minted sucessfully
                    string nftFullName = Path.GetFileNameWithoutExtension(file.FullName);
                    string nftName = Path.GetFileNameWithoutExtension(file.Name);
                    string key = nftFullName;
                    if (!Settings.All.CaseSensitiveFileHandling)
                    {
                        key = key.ToLower();
                    }
                    _ = CollectionInformation.Information.PendingTransactions.Remove(key, out _);
                }
                else
                {
                    stillPendingTransactions++;
                }
            }
            return stillPendingTransactions;
        }
        private static async Task MintNft(KeyValuePair<string, FileInfo> nftToBeMinted, string royaltyAdress, CancellationToken cancel)
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
            // send mint transaction
            NftMintNFT_Response response = await WalletApi.NftMintNft_Async(rpc);
            /// save spend bundle to validate transaction completeness
            string transactionPath = Path.Combine(Directories.PendingTransactions.FullName, nftFullName,".mint");
            File.WriteAllText(transactionPath, response.ToString());
            // wait for mint to complete
            NftGetInfo_Response nftInfo = await WalletApi.NftAwaitMintComplete_Async(response, cancel);
            /// validate mint
            if (nftInfo.success)
            { /// mint was successful
                FileInfo nftFilePath = new FileInfo(Path.Combine(Directories.Minted.FullName, nftName));
                nftInfo.nft_info.Save(nftFilePath.FullName);
                /// add successful mint to collection information
                CollectionInformation.Information.MintedFiles[key] = nftFilePath;
                /// delete pending transaction
                File.Delete(transactionPath);
                /// remove pending transaction
                _ = CollectionInformation.Information.PendingTransactions.Remove(key,out _);
            }
        }
    }
}