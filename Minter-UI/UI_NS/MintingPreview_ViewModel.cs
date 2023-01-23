using CHIA_RPC.FullNode_RPC_NS;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Minter_UI.UI_NS
{
    public class MintingItem
    {
        public MintingItem() { 
            IsUploading = false;
            IsMinting = false;
        }
        public MintingItem(string imageURI)
        {
            Data = imageURI;
            IsUploading = false;
            IsMinting = false;
        }
        private string _data;
        public string Data { get
            {
                return _data;
            }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }
        private bool isUploading;
        public bool IsUploading
        {
            get { return isUploading; }
            set
            {
                isUploading = value;
                OnPropertyChanged();
            }
        }
        private bool isMinting;
        public bool IsMinting
        {
            get { return isMinting; }
            set
            {
                isMinting = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
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
