using RenewedVision.Core;

namespace RenewedVision.ViewModels
{
    public class MainViewModel : Observable
    {
        public MainViewModel()
        {
            ClearRichTextCommand = new RelayCommand(() => Text = string.Empty);
            ShowMessageBoxCommand = new RelayCommand(ShowTweetPopup);
        }

        public IRelayCommand ClearRichTextCommand { get; }
        public IRelayCommand ShowMessageBoxCommand { get; }

        private void ShowTweetPopup()
        {
            System.Windows.MessageBox.Show(Text, "Message");
        }

        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }
        private string _text;
    }
}
