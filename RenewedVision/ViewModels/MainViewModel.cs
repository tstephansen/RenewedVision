using RenewedVision.Core;
using System;

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
            set
            {
                Console.WriteLine($"VM Text Changing from {Text}");
                Set(ref _text, value);
                Console.WriteLine($"VM Text Changed to: {Text}");
            }
        }
        private string _text;
    }
}
