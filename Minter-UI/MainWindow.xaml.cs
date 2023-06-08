using Minter_UI.CollectionInformation_ns;
using Minter_UI.Tasks_NS;
using System;
using System.IO;
using System.Windows;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            #if DEBUG
            //if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioVersion"))
            //    && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VS_VERSION_INFO")))
            //{
                if (System.Diagnostics.Debugger.IsAttached == false)
                {
                    System.Diagnostics.Debugger.Launch();
                }
            //}
            #endif
            // register event handler of uncaught exceptions in order to log tem in errors.log
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                // Log the exception details
                LogException.LogAsync(ex);
            };
            // initialize the form
            InitializeComponent();
            // load collection information once
            CollectionInformation.ReloadAll();
        }
    }
}
