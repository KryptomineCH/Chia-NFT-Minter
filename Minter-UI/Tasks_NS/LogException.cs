using CefSharp.DevTools.Browser;
using Minter_UI.Settings_NS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Minter_UI.Tasks_NS
{
    internal class LogException
    {
        private static object FileLock = new object();
        private static FileInfo LogFile = new FileInfo(@"errors.log");
        /// <summary>
        /// this function logs an exception in a tread safe manner
        /// </summary>
        /// <param name="logException"></param>
        /// <param name="unhandled"></param>
        internal static async Task LogAsync(Exception logException, string? devMessage = null, bool unhandled = true)
        {
            try
            {
                // generate log lines
                List<string> lines = new List<string>();
                if (unhandled)
                {
                    lines.Add($"Unhandled exception: {logException.Message}");
                }
                else
                {
                    lines.Add($"Exception: {logException.Message}");
                }
                lines.Add("Time: " + DateTime.Now.ToString());
                lines.Add("Client Version: " + Assembly.GetExecutingAssembly().GetName().Version);
                if (!string.IsNullOrEmpty(devMessage))
                {
                    lines.Add($"Dev Message: {devMessage}");
                }
                lines.Add($"Stack trace: {logException.StackTrace}");
                Exception? innerEx = logException.InnerException;
                while (innerEx != null)
                {
                    lines.Add("#----------------- inner exception -----------------#");
                    if (unhandled)
                    {
                        lines.Add($"Unhandled exception: {logException.Message}");
                    }
                    else
                    {
                        lines.Add($"Exception: {logException.Message}");
                    }
                    lines.Add($"Stack trace: {logException.StackTrace}");
                    innerEx = innerEx.InnerException;
                }
                lines.Add("");
                
                if (Settings_NS.Settings.All.AutoUploadAnonymousErrorReport == null)
                {
                    MessageBoxResult result = MessageBox.Show("An error occurred. Do you want to upload errors automatically?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        Settings_NS.Settings.All.AutoUploadAnonymousErrorReport = true;
                    }
                    else
                    {
                        Settings_NS.Settings.All.AutoUploadAnonymousErrorReport = false;
                    }
                    Settings_NS.Settings.Save();
                }
                if (Settings_NS.Settings.All.AutoUploadAnonymousErrorReport == true)
                {
                    string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}-{Guid.NewGuid().ToString()}-{Assembly.GetExecutingAssembly().GetName().Version}.txt";
                    UploadFileAsync(lines.ToArray(), $"ftp://julianbechtold.ch:50000/{fileName}", "chiaminter_ftp", "chiaminter_ftp");
                }
                
                
                lock (FileLock)
                {
                    using (StreamWriter writer = new StreamWriter(LogFile.FullName, true))
                    {
                        foreach (string line in lines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred which the application tried to log to: {Environment.NewLine}" +
                    $"{LogFile.FullName}{Environment.NewLine}{Environment.NewLine}" +
                    $"however, the error lo'gging failed because of:{Environment.NewLine}" +
                    $"{ex.Message}");
            }
        }
        private static async Task UploadFileAsync(string[] contentLines, string targetUri, string ftpUser, string ftpPass)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(targetUri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UsePassive = true;
            request.Credentials = new NetworkCredential(ftpUser, ftpPass);

            // Convert string[] to a single string, then to a byte array.
            string content = String.Join(Environment.NewLine, contentLines);
            byte[] fileContents = Encoding.UTF8.GetBytes(content);

            request.ContentLength = fileContents.Length;

            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
            }

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                if (response.StatusDescription.Contains("226"))
                {
                    // success
                }
            }
        }
    }
}
