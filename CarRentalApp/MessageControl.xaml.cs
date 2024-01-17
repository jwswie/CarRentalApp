using System.Windows.Controls;

namespace CarRentalApp
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public MessageControl()
        {
            InitializeComponent();
        }

        public new string Content
        {
            get { return ContentTextBlock.Text; } set { ContentTextBlock.Text = value; }
        }
    }
}
