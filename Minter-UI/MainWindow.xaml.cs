using System;
using System.IO;
using System.Windows;
using Chia_NFT_Minter.CollectionInformation_ns;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
#if DEBUG && PUBLISH
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                System.Diagnostics.Debugger.Launch();
            }

#endif

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                // Log the exception details
                string filePath = @"errors.log";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Unhandled exception: " + ex.Message);
                    writer.WriteLine("Stack trace: " + ex.StackTrace);
                }
            };
            InitializeComponent();
            // load collection information once
            bool caseSensitive = true;
            if (Settings_NS.Settings.All != null)
            {
                caseSensitive = Settings_NS.Settings.All.CaseSensitiveFileHandling;
            }
            CollectionInformation.ReloadAll(caseSensitive);
        }
    }
}
