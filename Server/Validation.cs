using System.Data.SqlClient;
using System.Data;
using System.Windows.Controls;
using System.Windows;

namespace Server
{
    public class Validation
    {
        static public string connectionString = @"Data Source = DESKTOP-BVS5CLQ; Initial Catalog = CarRental; Trusted_Connection=True; TrustServerCertificate = True";

        static public bool IsModelAlreadyAdded(string model, Grid CatalogGrid) // Проверка на уникальность модели
        {
            foreach (UIElement element in CatalogGrid.Children)
            {
                if (element is CarControl carControl)
                {
                    if (carControl.CarModelLabel.Content.ToString() == model)
                    {
                        return true; // Модель уже добавлена
                    }
                }
            }
            return false; // Модель не найдена
        }
    }
}
