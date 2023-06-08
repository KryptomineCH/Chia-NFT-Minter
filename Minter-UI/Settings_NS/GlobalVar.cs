using Chia_Client_API.WalletAPI_NS;
using CHIA_RPC.General_NS;
using CHIA_RPC.Wallet_NS.WalletNode_NS;
using Multithreading_Library.DataTransfer;
using System.Windows.Media;

namespace Minter_UI
{
    /// <summary>
    /// Globalvar contains a set op application wide variables / settings
    /// </summary>
    internal static class GlobalVar
    {
        /// <summary>
        /// The sync status is beeing updated every couple of seconds
        /// </summary>
        /// <remarks>
        /// This variable is managed from within Footer.xaml.cs
        /// </remarks>
        internal static OneWrite_MultiRead<GetWalletSyncStatus_Response> SyncStatus = new OneWrite_MultiRead<GetWalletSyncStatus_Response>(new GetWalletSyncStatus_Response(),5000);
        /// <summary>
        /// this variable is determined in order to check if a wallet was fully synced already (for one-Time checks and tasks)</br>
        /// once the wallet switchs, FullSync will reset
        /// </summary>
        /// <remarks>
        /// This variable is managed from within Footer.xaml.cs
        /// </remarks>
        internal static bool FullSync = false;
        /// <summary>
        /// The currently logged in wallet. This variable is updated from within Footer.xaml.cs
        /// </summary>
        internal static OneWrite_MultiRead<FingerPrint_Response> CurrentlyLoggedInWallet = new OneWrite_MultiRead<FingerPrint_Response>(new FingerPrint_Response(), 5000);
        /// <summary>
        /// the primary wallet adress to specify where minting fees go to
        /// </summary>
        internal static string? PrimaryWalletAdress { get; set; }
        /// <summary>
        /// is updated fro mthe footer and used in various locations to determine license only material (custom receive adress when minting)
        /// </summary>
        internal static bool Licensed = false;
        /// <summary>
        /// the wallet number of the currently selected nft wallet
        /// </summary>
        internal static ulong NftWallet_ID { get; set; }
        internal static Wallet_RPC_Client WalletApi = new Wallet_RPC_Client();
    }
}
