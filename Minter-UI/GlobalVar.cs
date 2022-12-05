namespace Minter_UI
{
    /// <summary>
    /// Globalvar contains a set op application wide variables / settings
    /// </summary>
    internal static class GlobalVar
    {
        static GlobalVar()
        {
            Settings.Initialize();
        }
        /// <summary>
        /// this setting specifies if files should be handeled casesensitive.
        /// </summary>
        /// <remarks>
        /// it is recommended to be on for freshly created collections where metadata and rpc files are created automatically.<br/>
        /// however, collections which have been minted by hand beforehands might contain invalid namings (case sensitivity wrong)
        /// </remarks>
        internal static bool CaseSensitiveFilehandling = true;
    }
}
