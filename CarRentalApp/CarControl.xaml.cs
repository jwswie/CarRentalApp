using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace CarRentalApp
{
    /// <summary>
    /// Interaction logic for CarControl.xaml
    /// </summary>
    public partial class CarControl : UserControl
    {
        public CarControl(byte[] img, string model, string ID)
        {
            InitializeComponent();

            CarModelLabel.Content = model;
            IDLabel.Content = ID;

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