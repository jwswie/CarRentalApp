using System.Windows;

namespace Server
{
    public class UserMessage // Информация о сообщении
    {
        private string message; 
        private User sender;
        private HorizontalAlignment horizontalAlignment;
        

        public string Message { get => message; set { message = value; } } // чтение и запись сообщения
        public User Sender { get => sender; } // получаем пользователя
        public HorizontalAlignment HorizontalAlignment { get => horizontalAlignment; } // получаем расположение

        public UserMessage(User sender, HorizontalAlignment horizontalAlignment)
        {
            this.sender = sender;
            this.horizontalAlignment = horizontalAlignment;
        }
    }
}