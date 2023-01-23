using Microsoft.Web.WebView2.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// Interaction logic for MintingPreview_Control.xaml
    /// </summary>
    public partial class MintingPreview_Control : UserControl
    {
        public MintingPreview_Control()
        {
            InitializeComponent();
        }
        public void Load()
        {
            // This method is called when the control is about to be displayed
            // You can load the image here by setting the source of your image control
            Browser.Address = Data;
        }

        public void Unload()
        {
            // This method is called when the control is no longer needed and can be removed from memory
            // You can unload the image here by setting the source of your image control to null
            Browser.Address = null;
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        name: "Data", 
        propertyType: typeof(string), 
        ownerType: typeof(MintingPreview_Control), 
        typeMetadata: new PropertyMetadata(default(string), OnDataChanged));

        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            set { 
                SetValue(DataProperty, value); 
            }
        }
        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MintingPreview_Control)d;
            //control.WebView2Control.Source = new Uri((string)e.NewValue);
            control.Browser.Address = (string)e.NewValue;
        }

        public static readonly DependencyProperty IsUploadingProperty = DependencyProperty.Register(
                name: "IsUploading",
                propertyType: typeof(bool),
                ownerType: typeof(MintingPreview_Control),
                typeMetadata: new PropertyMetadata(default(bool), OnIsUploadingChanged));

        private static void OnIsUploadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MintingPreview_Control;
            if ((bool)e.NewValue)
            {
                control.MarkingBorder.BorderBrush = new SolidColorBrush(Colors.Cyan);
            }
            else
            {
                control.MarkingBorder.BorderBrush = new SolidColorBrush(ColorHelper.ColorConverter.FromHex("#1c1c1c"));
            }
        }
        public bool IsUploading
        {
            get { return (bool)GetValue(IsUploadingProperty); }
            set { 
                SetValue(IsUploadingProperty, value); 
            }
        }

        public static readonly DependencyProperty IsMintingProperty = DependencyProperty.Register(
            name: "IsMinting",
            propertyType: typeof(bool),
            ownerType: typeof(MintingPreview_Control),
            typeMetadata: new PropertyMetadata(default(bool), OnIsMintingChanged));

        private static void OnIsMintingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MintingPreview_Control;
             if ((bool)e.NewValue)
            {
                control.MarkingBorder.BorderBrush = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                control.MarkingBorder.BorderBrush = new SolidColorBrush(ColorHelper.ColorConverter.FromHex("#1c1c1c"));
            }
        }
        public bool IsMinting
        {
            get { return (bool)GetValue(IsMintingProperty); }
            set {
                SetValue(IsMintingProperty, value); 
            }
        }
    }
}