using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace Server
{
    /// <summary>
    /// Interaction logic for CategoryControl.xaml
    /// </summary>
    public partial class CategoryControl : UserControl
    {
        public CategoryControl(byte[] img, string category, string price)
        {
            InitializeComponent();

            Category.Content = category;
            Price.Content = price;

            if (img != null)
            {
                using (MemoryStream stream = new MemoryStream(img))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    CarImage.Source = bitmap;
                }
            }
            else
            {
                MessageBox.Show("Image not loaded", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}