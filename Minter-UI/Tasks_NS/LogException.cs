using System;
using System.IO;
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
        internal static void Log(Exception logException, string? devMessage = null ,bool unhandled = true)
        {
            try
            {

                lock (FileLock)
                {
                    using (StreamWriter writer = new StreamWriter(LogFile.FullName, true))
                    {
                        if (unhandled)
                        {
                            writer.WriteLine($"Unhandled exception: {logException.Message}");
                        }
                        else
                        {
                            writer.WriteLine($"Exception: {logException.Message}");
                        }
                        writer.WriteLine("Time: " + DateTime.Now.ToString());
                        if (!string.IsNullOrEmpty(devMessage))
                        {
                            writer.WriteLine($"Dev Message: {devMessage}");
                        }
                        writer.WriteLine("Stack trace: {logException.StackTrace}");
                        writer.WriteLine();
                    }
                }
            }
            catch (Exception ex){
                MessageBox.Show($"An error occurred which the application tried to log to: {Environment.NewLine}" +
                    $"{LogFile.FullName}{Environment.NewLine}{Environment.NewLine}" +
                    $"however, the error lo'gging failed because of:{Environment.NewLine}" +
                    $"{ex.Message}");
            }
        }
    }
}
