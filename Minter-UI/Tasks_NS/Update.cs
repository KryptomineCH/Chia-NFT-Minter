using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Minter_UI.Tasks_NS
{
    internal static class Update
    {
        internal static void DownloadUpdate(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            var progress = new Progress<double>(p => worker.ReportProgress((int)(p * 100)));
            string uri = (string)e.Argument;
            e.Result = NFT.Storage.Net.API.DownloadClient.DownloadAsync(uri, progress).Result;
        }

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
