using Minter_UI.CollectionInformation_ns;
using Minter_UI.Settings_NS;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// thisitem contains the data for an nftpreview. it is beeing added to the viewmodel
    /// </summary>
    public class MintingItem : INotifyPropertyChanged
    {
        public MintingItem() { 
            IsUploading = false;
            IsUploaded = false;
            IsMinting = false;
        }
        public MintingItem(string imageURI)
        {
            Data = imageURI;
            IsUploading = false;
            IsUploaded = false;
            IsMinting = false;
        }
        /// <summary>
        /// data is the image uri of the nft
        /// </summary>
        private string _data;
        /// <summary>
        /// this is the imageURI of the Item
        /// </summary>
        public string Data { get
            {
                return _data;
            }
            set
            {
                _data = value;
                string key = CollectionInformation.GetKeyFromFile(Data);
                bool caseSensitive = true;
                if (Settings.All != null)
                {
                    caseSensitive = Settings.All.CaseSensitiveFileHandling;
                }
                if (!caseSensitive)
                {
                    key = key.ToLower();
                }
                this.Key = key;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// this bool specifies if the nft is currently in uploading stage, giving it a light blue border
        /// </summary>
        private bool isUploading;
        /// <summary>
        /// this bool specifies if the nft is currently in uploading stage, giving it a light blue border
        /// </summary>
        public bool IsUploading
        {
            get { return isUploading; }
            set
            {
                isUploading = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// this bool specifies if the nft is uploaded stage, giving it a dark blue border
        /// </summary>
        private bool isUploaded;
        /// <summary>
        /// this bool specifies if the nft is uploaded stage, giving it a dark blue border
        /// </summary>
        public bool IsUploaded
        {
            get { return isUploaded; }
            set
            {
                isUploaded = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// this bool specifies if the nft is currently beeing minted, giving it a yellow border
        /// </summary>
        private bool isMinting;
        /// <summary>
        /// this bool specifies if the nft is currently beeing minted, giving it a yellow border
        /// </summary>
        public bool IsMinting
        {
            get { return isMinting; }
            set
            {
                isMinting = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// the key of the nft
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// event handler which is beeing hooked onto by ui controls
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// raises the event
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// the viewmodel contains an observable collection which ui elements can hook onto in order to automatically
    /// update their content (gallery)
    /// </summary>
    public class MintingPreview_ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<MintingItem> _items;
        public ObservableCollection<MintingItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
