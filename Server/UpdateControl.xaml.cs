using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    /// <summary>
    /// Interaction logic for UpdateControl.xaml
    /// </summary>
    public partial class UpdateControl : UserControl
    {
        private int Id;
        private SqlConnectionManager sqlConnectionManager;

        public UpdateControl(int ID, SqlConnectionManager sqlConnectionManager)
        {
            InitializeComponent();
            Update.Background = (Brush)new BrushConverter().ConvertFromString("White");
            Id = ID;
            this.sqlConnectionManager = sqlConnectionManager;

            string sqlExpression1 = "SELECT c.CarID, c.ID_Class, c.Model, c.Color, c.ManufactureYear, c.Transmission, c.FuelType " +
                                    "FROM Cars c " +
                                    "WHERE c.CarID = @CarID";

            using (SqlCommand command = new SqlCommand(sqlExpression1, sqlConnectionManager.connection))
            {
                command.Parameters.AddWithValue("@CarID", Id);

                try
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string classID = reader["ID_Class"].ToString();
                            string model = reader["Model"].ToString();
                            string color = reader["Color"].ToString();
                            string year = reader["ManufactureYear"].ToString();
                            string transmission = reader["Transmission"].ToString();
                            string fuel = reader["FuelType"].ToString();

                            txtClass.Text = classID;
                            txtModel.Text = model;
                            txtColor.Text = color;
                            txtYear.Text = year;
                            txtTransmission.Text = transmission;
                            txtFuel.Text = fuel;

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            string sqlExpresion2 = "SELECT c.CarID, p.RentDays, p.PricePerDay " +
                                               "FROM Cars c " +
                                               "JOIN Prices p ON c.CarID = p.ID_Car " +
                                               "WHERE c.CarID = @CarID"; // Получаем цены на конкретное авто

            using (SqlCommand command = new SqlCommand(sqlExpresion2, sqlConnectionManager.connection))
            {
                command.Parameters.AddWithValue("@CarID", Id);

                int rentDays1 = 3; int rentDays2 = 9; int rentDays3 = 26;
                int pricePerDay1 = 0; int pricePerDay2 = 0; int pricePerDay3 = 0;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int rentDays = reader.GetInt32(reader.GetOrdinal("RentDays"));
                        int pricePerDay = reader.GetInt32(reader.GetOrdinal("PricePerDay"));

                        if (rentDays == rentDays1)
                            pricePerDay1 = pricePerDay;
                        else if (rentDays == rentDays2)
                            pricePerDay2 = pricePerDay;
                        else if (rentDays == rentDays3)
                            pricePerDay3 = pricePerDay;
                    }
                }
                txt3.Text = $"{pricePerDay1}";
                txt9.Text = $"{pricePerDay2}";
                txt26.Text = $"{pricePerDay3}";
            }

        }

    }
}
