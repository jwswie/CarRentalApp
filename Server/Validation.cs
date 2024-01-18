using System.Data.SqlClient;
using System.Data;
using System.Windows.Controls;
using System.Windows;

namespace Server
{
    public class Validation
    {
        static public string connectionString = @"Data Source = DESKTOP-BVS5CLQ; Initial Catalog = CarRental; Trusted_Connection=True; TrustServerCertificate = True";

        static public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        static public bool IsValidData(string email)
        {
            string sql = "SELECT * FROM Users WHERE Email = @Email";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@Email", email);

                DataSet ds = new DataSet();
                adapter.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

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
