using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Minter_UI.Tasks_NS
{
    /// <summary>
    /// this class contains the application update check. It validates against the latest github release
    /// </summary>
    internal static class Update
    {
        /// <summary>
        /// downloads the file from github and reports progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void DownloadUpdate(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            string uri = (string)e.Argument;
            var progress = new Progress<float>(p =>
            {
                worker.ReportProgress((int)(p * 100));
            });
            e.Result = NFT.Storage.Net.API.DownloadClient.DownloadAsync(uri, (IProgress<float>) progress).Result;
        }
        /// <summary>
        /// when the update is completed, the old.exe is renamed (cant be deleted)
        /// then the new exe is renamed accordingly. the new exe is started and this application is terminated.
        /// The new versions update check will validate update complete and delete the old file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void UpdateCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            byte[] fileData = (byte[])e.Result;
            File.Move("Minter-UI.exe", "Minter-UI-old.exe");
            File.WriteAllBytes("Minter-UI.exe", fileData);
            Process updatedApp = new Process();
            updatedApp.StartInfo.FileName = "Minter-UI.exe";
            updatedApp.Start();
            Environment.Exit(0);
        }
    }
}
