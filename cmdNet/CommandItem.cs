using System.ComponentModel;

namespace cmdNet
{
    public class CommandItem : INotifyPropertyChanged
    {
        public string Command { get; set; }
        public string ResourceKey { get; set; }

        public string Description => LanguageManager.Instance.GetString(ResourceKey);

        public void RefreshDescription()
        {
            OnPropertyChanged(nameof(Description));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}