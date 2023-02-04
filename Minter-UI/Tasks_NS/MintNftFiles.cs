using CHIA_RPC.Wallet_RPC_NS.NFT;
using Minter_UI.Settings_NS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chia_Client_API.Wallet_NS.WalletAPI_NS;
using CHIA_RPC.General;
using CHIA_RPC.Wallet_RPC_NS.Wallet_NS;
using System.Windows;
using Minter_UI.CollectionInformation_ns;
using Minter_UI.UI_NS;
using System.Windows.Threading;

namespace Minter_UI.Tasks_NS
{
    internal class MintNftFiles
    {
        internal static bool MintingInProgress = false;
        private static object MintingInProgressLock = new object();
        internal static async Task MintNfts_Task(
            CancellationToken cancle, 
            MintingPreview_ViewModel uiView,
            DispatcherObject dispatcherObject)
        {
            lock (MintingInProgressLock)
            {
                if (MintingInProgress) return;
                MintingInProgress = true;
            }
            try
            {
                CheckPendingTransactions(cancle);
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
                while (!cancle.IsCancellationRequested)
                {
                    // if there are no nfts to be minted, wait for more
                    if (CollectionInformation.Information.ReadyToMint.IsEmpty)
                    {
                        await Task.Delay(2000, cancle).ConfigureAwait(false);
                        continue;
                    }
                    // get nft to be minted
                    KeyValuePair<string, FileInfo> nftToBeMinted = CollectionInformation.Information.ReadyToMint.First();
                    _ = CollectionInformation.Information.ReadyToMint.Remove(nftToBeMinted.Key, out _);
                    // balance check
                    GetWalletBalance_Response walletBalance = await WalletApi.GetWalletBalance_Async(new WalletID_RPC() { wallet_id = 1 })
                        .ConfigureAwait(false);
                    if (walletBalance.wallet_balance.spendable_balance > Settings.All.MintingFee + 1)
                    {
                        // start mint task
                        Task mint = Task.Run(() => MintNft(
                            nftToBeMinted,
                            royaltyAdress,
                            cancle,
                            uiView,
                            dispatcherObject
                            ));
                        await mint.ConfigureAwait(false);
                        await Task.Delay(500, cancle).ConfigureAwait(false);
                    }
                    else if (walletBalance.wallet_balance.unconfirmed_wallet_balance != 0)
                    {
                        // there seem to be transactions ongoing which block cash
                        int ongoingTransactions = await CheckPendingTransactions(cancle).ConfigureAwait(false);
                        if (ongoingTransactions == 0)
                        {
                            // no ongoing transactions, all transactions seem stuck!
                            await WalletApi.DeleteUnconfirmedTransactions_Async(1).ConfigureAwait(false);
                        }
                        await Task.Delay(1000, cancle).ConfigureAwait(false);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There has been an error while minting!" +
                            $"{Environment.NewLine}" +
                            $"{Environment.NewLine}" + 
                            ex.ToString());
            }
            finally
            {
                lock (MintingInProgressLock)
                {
                    MintingInProgress = false;
                }
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
        internal static async Task<int> CheckPendingTransactions(CancellationToken cancle, double hoursAfterTransactionConsideredStuck = 0.25)
        {
            FileInfo[] files = Directories.PendingTransactions.GetFiles("*.mint");
            int stillPendingTransactions = 0;
            foreach (FileInfo file in files)
            {
                if (cancle.IsCancellationRequested) return -1;
                // load spend bundle
                NftMintNFT_Response spendBundle = NftMintNFT_Response.Load(file.FullName);
                // check if nft was minted
                NftGetInfo_Response nft = await WalletApi.VerifyMint(spendBundle).ConfigureAwait(false);
                if (nft.success)
                {
                    // nft was minted sucessfully
                    //string nftFullName = file.FullName;
                    string nftName = Path.GetFileNameWithoutExtension(file.Name);
                    string key = CollectionInformation.GetKeyFromFile(file);
                    FileInfo nftFilePath = new FileInfo(Path.Combine(Directories.Minted.FullName, nftName+".nft"));
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
                    string key = CollectionInformation.GetKeyFromFile(file);
                    _ = CollectionInformation.Information.PendingTransactions.Remove(key, out _);
                }
                else
                {
                    stillPendingTransactions++;
                }
            }
            return stillPendingTransactions;
        }
        private static async Task MintNft(
            KeyValuePair<string, FileInfo> nftToBeMinted, 
            string royaltyAdress,
            CancellationToken cancel, 
            MintingPreview_ViewModel uiView,
            DispatcherObject dispatcherObject)
        {
            // get nft name and identifier key
            //string nftFullName = nftToBeMinted.Value.FullName;
            string nftName = Path.GetFileNameWithoutExtension(nftToBeMinted.Value.Name);
            string key = CollectionInformation.GetKeyFromFile(nftToBeMinted.Value);
            // update rpc
            string rpcSourcePath = Path.Combine(Directories.Rpcs.FullName, nftName+ ".rpc");
            NftMintNFT_RPC rpc = NftMintNFT_RPC.Load(rpcSourcePath);
            rpc.wallet_id = GlobalVar.NftWallet_ID;
            rpc.royalty_address = royaltyAdress;
            rpc.fee = Settings.All.MintingFee;
            // send mint transaction
            dispatcherObject.Dispatcher.Invoke(new Action(() =>
            {
                foreach (MintingItem item in uiView.Items)
                {
                    if (item.Key == key)
                    {
                        item.IsMinting = true;
                        break;
                    }
                }
            }));
            NftMintNFT_Response response = await WalletApi.NftMintNft_Async(rpc).ConfigureAwait(false);
            /// save spend bundle to validate transaction completeness
            string transactionPath = Path.Combine(Directories.PendingTransactions.FullName, nftName+".mint");
            response.Save(transactionPath);
            // wait for mint to complete
            NftGetInfo_Response nftInfo = await WalletApi.NftAwaitMintComplete_Async(response, cancel,refreshInterwallSeconds:30).ConfigureAwait(false);
            /// validate mint
            if (nftInfo.success)
            { /// mint was successful
                FileInfo nftFilePath = new FileInfo(Path.Combine(Directories.Minted.FullName, nftName+".nft"));
                nftInfo.nft_info.Save(nftFilePath.FullName);
                /// add successful mint to collection information
                CollectionInformation.Information.MintedFiles[key] = nftFilePath;
                /// delete pending transaction
                File.Delete(transactionPath);
                /// remove pending transaction
                _ = CollectionInformation.Information.PendingTransactions.Remove(key,out _);
                dispatcherObject.Dispatcher.Invoke(new Action(() =>
                {
                    MintingItem? itemToRemove = null;
                    foreach (MintingItem item in uiView.Items)
                    {
                        if (item.Key == key)
                        {
                            itemToRemove = item;
                            break;
                        }
                    }
                    if (itemToRemove != null)
                    {
                        uiView.Items.Remove(itemToRemove);
                    }
                })); 
            }
        }
    }
}