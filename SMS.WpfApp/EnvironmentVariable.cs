using System.ComponentModel;

namespace SMS.WpfApp
{
    public class EnvironmentVariable : INotifyPropertyChanged
    {
        private string _field;
        private string _value;
        private string _comment;

        public string Field
        {
            get => _field;
            set
            {
                _field = value;
                OnPropertyChanged(nameof(Field));
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
