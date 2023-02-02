using CHIA_RPC.Wallet_RPC_NS.KeyManagement;
using CHIA_RPC.Wallet_RPC_NS.WalletNode_NS;
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
        internal static OneWrite_MultiRead<GetSyncStatus_Response> SyncStatus = new OneWrite_MultiRead<GetSyncStatus_Response>(new GetSyncStatus_Response(),5000);
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
        internal static OneWrite_MultiRead<LogIn_Response> CurrentlyLoggedInWallet = new OneWrite_MultiRead<LogIn_Response>(new LogIn_Response(), 5000);
        internal static string? PrimaryWalletAdress { get; set; }
        internal static bool Licensed = false;
        /// <summary>
        /// the wallet number of the currently selected nft wallet
        /// </summary>
        internal static ulong NftWallet_ID { get; set; }
    }
}
