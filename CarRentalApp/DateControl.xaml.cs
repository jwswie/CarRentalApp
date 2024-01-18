using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarRentalApp
{
    /// <summary>
    /// Interaction logic for DateControl.xaml
    /// </summary>
    public partial class DateControl : UserControl
    {
        private int userID, carID;
        SqlConnectionManager sqlConnectionManager;
        Label StatusLabel;

        public DateControl(SqlConnectionManager sqlConnectionManager, Label StatusLabel, int userID, int carID)
        {
            InitializeComponent();
            Date.Background = (Brush)new BrushConverter().ConvertFromString("White");
            this.sqlConnectionManager = sqlConnectionManager;
            this.StatusLabel = StatusLabel;
            this.userID = userID;
            this.carID = carID;

            SubDate.DisplayDateStart = DateTime.Today;
            RetDate.DisplayDateStart = DateTime.Today;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime SubmissionDate = SubDate.SelectedDate ?? DateTime.MinValue;
            DateTime ReturnDate = RetDate.SelectedDate ?? DateTime.MinValue;
            string SR = txtSR.Text;

            TimeSpan difference = ReturnDate.Subtract(SubmissionDate);
            int rentDays = (int)difference.TotalDays;
            int PricePerDay = 0;

            if (string.IsNullOrWhiteSpace(SR) || SubDate.SelectedDate == null || RetDate.SelectedDate == null)
            {
                MessageBox.Show("All fields must be filled", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (RetDate.SelectedDate < SubDate.SelectedDate)
            {
                MessageBox.Show("Return date cannot be earlier than submission date", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                RetDate.SelectedDate = SubDate.SelectedDate;
            }

            if (userID == -1 || userID == 0)
            {
                MessageBox.Show("Log In before renting", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sqlExpresion1 = "SELECT c.CarID, p.RentDays, p.PricePerDay " +
                                               "FROM Cars c " +
                                               "JOIN Prices p ON c.CarID = p.ID_Car " +
                                               "WHERE c.CarID = @targetCarID"; // Получаем цены на конкретное авто

            using (SqlCommand command = new SqlCommand(sqlExpresion1, sqlConnectionManager.connection))
            {
                command.Parameters.AddWithValue("@targetCarID", carID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int renDays = reader.GetInt32(reader.GetOrdinal("RentDays"));
                        int pricePerDay = reader.GetInt32(reader.GetOrdinal("PricePerDay"));

                        if (rentDays >= 1 && rentDays <= 3)
                            PricePerDay = pricePerDay;
                        else if (rentDays >= 4 && rentDays <= 9)
                            PricePerDay = pricePerDay;
                        else if (rentDays >= 10)
                            PricePerDay = pricePerDay;
                    }
                }
            }

            int TotalPrice = rentDays * PricePerDay;

            string sqlExpression2 = "INSERT INTO BookedCars (ID_User, ID_Car, RentDays, TotalPrice) VALUES (@user, @car, @days, @price)"; // Записываем данные об аренде

            using (SqlCommand command = new SqlCommand(sqlExpression2, sqlConnectionManager.connection))
            {
                command.Parameters.AddWithValue("@user", userID);
                command.Parameters.AddWithValue("@car", carID);
                command.Parameters.AddWithValue("@days", rentDays);
                command.Parameters.AddWithValue("@price", TotalPrice);

                int number = command.ExecuteNonQuery();
                MessageBox.Show("Payment successful", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            string sqlExpression3 = "UPDATE Cars SET CurrentStatus = 'Rented' WHERE CarID = @targetCarID"; // Меняем статус

            using (SqlCommand command = new SqlCommand(sqlExpression3, sqlConnectionManager.connection))
            {
                command.Parameters.AddWithValue("@targetCarID", carID);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Successful", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            string sqlExpression4 = "SELECT a.CurrentStatus FROM Cars a WHERE a.CarID = @CarID"; // Обновляем текущий статус

            using (SqlCommand command = new SqlCommand(sqlExpression4, sqlConnectionManager.connection))
            {
                command.Parameters.AddWithValue("@CarID", carID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StatusLabel.Content = reader["CurrentStatus"].ToString();
                    }
                }
            }
        }
    }
}
