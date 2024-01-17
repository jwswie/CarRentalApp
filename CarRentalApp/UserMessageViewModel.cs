using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CarRentalApp
{
    internal class UserMessageViewModel : INotifyPropertyChanged
    {
        public User currentUser { get; }
        private ObservableCollection<UserMessage> messages; // список сообщений
        public ObservableCollection<UserMessage> Messages { get => messages; set { messages = value; OnPropertyChanged(nameof(messages)); } } // доступ к списку сообщений + обработчик изменений

        public UserMessageViewModel(User currentUser)
        {
            this.currentUser = currentUser;
            messages = new ObservableCollection<UserMessage>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                Application.Current.Dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))); // если добавилось новое сообщение - уведомить об этом
            } 
        }

        public void AddMessage(User user, string messageText, StackPanel MessageContainer, ScrollViewer MyScrollViewer, string curr)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageControl messageControl = new MessageControl();

                messageControl.Content = messageText;
                messageControl.Margin = new Thickness(10, 10, 0, 0);
                messageControl.Width = 170;

                HorizontalAlignment horizontalAlignment;

                if (curr == "server")
                {
                    horizontalAlignment = HorizontalAlignment.Left;
                    messageControl.HorizontalAlignment = HorizontalAlignment.Left;
                    messageControl.MyBorder.Background = (Brush)new BrushConverter().ConvertFromString("#FFFCA590");
                }
                else {
                    horizontalAlignment = HorizontalAlignment.Right;
                    messageControl.HorizontalAlignment = HorizontalAlignment.Right;
                    messageControl.MyBorder.Background = (Brush)new BrushConverter().ConvertFromString("#FFFFFFA0");
                } 
                UserMessage message = new UserMessage(user, horizontalAlignment) { Message = messageText };

                messages.Add(message);

                MessageContainer.Children.Add(messageControl);

                MyScrollViewer.ScrollToBottom();
            });
        }
    }
}