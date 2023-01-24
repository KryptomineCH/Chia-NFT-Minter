using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// This class represents a user control for displaying a minting preview.
    /// It has several dependency properties: Data, IsUploading, IsUploaded, and IsMinting.
    /// Each of these properties has a corresponding callback method that updates the visual state of the control when the property changes.
    /// </summary>
    public partial class MintingPreview_Control : UserControl
    {
        public MintingPreview_Control()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The "Data" property is a dependency property of type string. It is used to set the address of the browser.
        /// When the "Data" property changes, the OnDataChanged callback method is invoked, which updates the address of the browser.
        /// </summary>
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        name: "Data", 
        propertyType: typeof(string), 
        ownerType: typeof(MintingPreview_Control), 
        typeMetadata: new PropertyMetadata(default(string), OnDataChanged));

        /// <summary>
        /// The "Data" property is a dependency property of type string. It is used to set the address of the browser.
        /// When the "Data" property changes, the OnDataChanged callback method is invoked, which updates the address of the browser.
        /// </summary>
        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            set { 
                SetValue(DataProperty, value); 
            }
        }
        /// <summary>
        /// The "Data" property is a dependency property of type string. It is used to set the address of the browser.
        /// When the "Data" property changes, the OnDataChanged callback method is invoked, which updates the address of the browser.
        /// </summary>
        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MintingPreview_Control)d;
            control.Browser.Address = (string)e.NewValue;
        }
        /// <summary>
        /// The "IsUploading" property is a dependency property of type bool. It is used to indicate whether or not the control is currently uploading.
        /// When the "IsUploading" property changes, the OnIsUploadingChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
        public static readonly DependencyProperty IsUploadingProperty = DependencyProperty.Register(
                name: "IsUploading",
                propertyType: typeof(bool),
                ownerType: typeof(MintingPreview_Control),
                typeMetadata: new PropertyMetadata(default(bool), OnIsUploadingChanged));
        /// <summary>
        /// The "IsUploading" property is a dependency property of type bool. It is used to indicate whether or not the control is currently uploading.
        /// When the "IsUploading" property changes, the OnIsUploadingChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
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
        /// <summary>
        /// The "IsUploading" property is a dependency property of type bool. It is used to indicate whether or not the control is currently uploading.
        /// When the "IsUploading" property changes, the OnIsUploadingChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
        public bool IsUploading
        {
            get { return (bool)GetValue(IsUploadingProperty); }
            set { 
                SetValue(IsUploadingProperty, value); 
            }
        }
        /// <summary>
        /// The "IsUploaded" property is a dependency property of type bool. It is used to indicate whether or not the control has finished uploading.
        /// When the "IsUploaded" property changes, the OnIsUploadedChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
        public static readonly DependencyProperty IsUploadedProperty = DependencyProperty.Register(
                name: "IsUploaded",
                propertyType: typeof(bool),
                ownerType: typeof(MintingPreview_Control),
                typeMetadata: new PropertyMetadata(default(bool), OnIsUploadedChanged));
        /// <summary>
        /// The "IsUploaded" property is a dependency property of type bool. It is used to indicate whether or not the control has finished uploading.
        /// When the "IsUploaded" property changes, the OnIsUploadedChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
        private static void OnIsUploadedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MintingPreview_Control;
            if ((bool)e.NewValue)
            {
                control.MarkingBorder.BorderBrush = new SolidColorBrush(Colors.Blue);
            }
            else
            {
                control.MarkingBorder.BorderBrush = new SolidColorBrush(ColorHelper.ColorConverter.FromHex("#1c1c1c"));
            }
        }
        /// <summary>
        /// The "IsUploaded" property is a dependency property of type bool. It is used to indicate whether or not the control has finished uploading.
        /// When the "IsUploaded" property changes, the OnIsUploadedChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
        public bool IsUploaded
        {
            get { return (bool)GetValue(IsUploadingProperty); }
            set
            {
                SetValue(IsUploadingProperty, value);
            }
        }
        /// <summary>
        /// The "IsMinting" property is a dependency property of type bool. It is used to indicate whether or not the control is currently minting.
        /// When the "IsMinting" property changes, the OnIsMintingChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
        public static readonly DependencyProperty IsMintingProperty = DependencyProperty.Register(
            name: "IsMinting",
            propertyType: typeof(bool),
            ownerType: typeof(MintingPreview_Control),
            typeMetadata: new PropertyMetadata(default(bool), OnIsMintingChanged));
        /// <summary>
        /// The "IsMinting" property is a dependency property of type bool. It is used to indicate whether or not the control is currently minting.
        /// When the "IsMinting" property changes, the OnIsMintingChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
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
        /// <summary>
        /// The "IsMinting" property is a dependency property of type bool. It is used to indicate whether or not the control is currently minting.
        /// When the "IsMinting" property changes, the OnIsMintingChanged callback method is invoked, 
        /// which updates the visual state of the control by changing the BorderBrush of the MarkingBorder element.
        /// </summary>
        public bool IsMinting
        {
            get { return (bool)GetValue(IsMintingProperty); }
            set {
                SetValue(IsMintingProperty, value); 
            }
        }
    }
}