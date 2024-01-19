using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for ManagementControl.xaml
    /// </summary>
    public partial class ManagementControl : UserControl
    {
        private SqlConnectionManager sqlConnectionManager;
        private string currentWindow = "Main";
        private int idClass = 0;
        private List<Price> prices = new List<Price>();
        private List<Car> cars = new List<Car>();
        private string Mode;
        private int Id;

        public ManagementControl(SqlConnectionManager sqlConnectionManager)
        {
            InitializeComponent();
            this.sqlConnectionManager = sqlConnectionManager;
        }

        public void SetVisibility(string window)
        {
            switch (window)
            {
                case "Open Update":
                    UpdateControl updateControl = new UpdateControl(Id, sqlConnectionManager);
                    updateControl.Margin = new Thickness(173,40,169,37);
                    MainGrid.Children.Add(updateControl);
                    BackButton.IsEnabled = false; CloseButton.Visibility = Visibility.Visible; CloseButton.IsEnabled = true;
                    Panel.SetZIndex(updateControl, 0);
                    Panel.SetZIndex(CloseButton, 1);
                    currentWindow = "Update";
                    break;

                case "Close Update":
                    UpdateControl updateControlDelete = MainGrid.Children.OfType<UpdateControl>().FirstOrDefault();
                    if (updateControlDelete != null)
                        MainGrid.Children.Remove(updateControlDelete);
                    BackButton.IsEnabled = true;
                    CloseButton.Visibility = Visibility.Hidden; CloseButton.IsEnabled = false;
                    currentWindow = "Panel";
                    break;

                default:
                    break;
            }
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string selectedClass = ClassComboBox.SelectedItem?.ToString();

                if (selectedClass != null)
                {
                    if (selectedClass.Contains("Economy"))
                    {
                        idClass = 1;
                    }
                    else if (selectedClass.Contains("Intermediate"))
                    {
                        idClass = 2;
                    }
                    else if (selectedClass.Contains("Business"))
                    {
                        idClass = 3;
                    }
                    else
                    {
                        idClass = 0;
                    }

                    string sqlExpression = $"SELECT Model FROM Cars WHERE ID_Class = {idClass}";

                    SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection);
                    SqlDataReader reader = command.ExecuteReader();

                    List<string> uniqueModels = new List<string>();

                    ModelComboBox.Items.Clear();

                    while (reader.Read())
                    {
                        string model = reader["Model"].ToString();

                        if (!uniqueModels.Contains(model))
                        {
                            uniqueModels.Add(model);
                            ModelComboBox.Items.Add(model);
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cars.Clear();
            prices.Clear();
            CarDataGrid.ItemsSource = null;
            PriceDataGrid.ItemsSource = null;
            object selectedItem = ModelComboBox.SelectedItem;
            int carID = 0;

            if (selectedItem != null)
            {
                string selectedClass = selectedItem.ToString();

                string sqlExpression1 = "SELECT c.CarID, c.ID_Class, c.Model, c.Color, c.ManufactureYear, c.Transmission, c.FuelType " +
                                        "FROM Cars c " +
                                        "WHERE c.ID_Class = @ClassID AND c.Model = @Model";

                using (SqlCommand command = new SqlCommand(sqlExpression1, sqlConnectionManager.connection))
                {
                    command.Parameters.AddWithValue("@ClassID", idClass);
                    command.Parameters.AddWithValue("@Model", selectedClass);

                    try
                    {

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                carID = reader.GetInt32(reader.GetOrdinal("CarID"));
                                int classID = reader.GetInt32(reader.GetOrdinal("ID_Class"));
                                string model = reader["Model"].ToString();
                                string color = reader["Color"].ToString();
                                int year = reader.GetInt32(reader.GetOrdinal("ManufactureYear"));
                                string transmission = reader["Transmission"].ToString();
                                string fuel = reader["FuelType"].ToString();

                                Car car = new Car
                                {
                                    Property1 = carID,
                                    Property2 = classID,
                                    Property3 = model,
                                    Property4 = color,
                                    Property5 = year,
                                    Property6 = transmission,
                                    Property7 = fuel
                                };

                                cars.Add(car);
                            }
                            CarDataGrid.ItemsSource = cars;
                            CarDataGrid.Items.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                string sqlExpression2 = "SELECT c.CarID, p.RentDays, p.PricePerDay " +
                                            "FROM Cars c " +
                                            "JOIN Prices p ON c.CarID = p.ID_Car " +
                                            "WHERE c.CarID = @CarID";

                using (SqlCommand command = new SqlCommand(sqlExpression2, sqlConnectionManager.connection))
                {
                    command.Parameters.AddWithValue("@CarID", carID);
                    command.Parameters.AddWithValue("@Model", selectedClass);

                    try
                    {
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                int rentDays = reader.GetInt32(reader.GetOrdinal("RentDays"));
                                int pricePerDay = reader.GetInt32(reader.GetOrdinal("PricePerDay"));

                                Price price = new Price
                                {
                                    Property1 = rentDays,
                                    Property2 = pricePerDay
                                };

                                prices.Add(price);
                            }
                            PriceDataGrid.ItemsSource = prices;
                            PriceDataGrid.Items.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if(clickedButton.Tag.ToString() == "Add")
            {
                currentWindow = "Add";
                Mode = "Add";
            }
            else if (clickedButton.Tag.ToString() == "Delete")
            {
                ManagePanel.Visibility = Visibility.Visible; ManagePanel.IsEnabled = true;
                BackButton.Visibility = Visibility.Visible; BackButton.IsEnabled = true;
                Confirm.Visibility = Visibility.Visible; Confirm.IsEnabled = true;
                currentWindow = "Panel";
                Mode = "Delete";
            }
            else if (clickedButton.Tag.ToString() == "Update")
            {
                ManagePanel.Visibility = Visibility.Visible; ManagePanel.IsEnabled = true;
                BackButton.Visibility = Visibility.Visible; BackButton.IsEnabled = true;
                Confirm.Visibility = Visibility.Hidden; Confirm.IsEnabled = false;
                currentWindow = "Panel";
                Mode = "Update";
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (CarDataGrid.SelectedItem == null)
            {
                MessageBox.Show($"Choose CarDataGrid element", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PriceDataGrid.SelectedItem != null)
            {
                MessageBox.Show($"Choose CarDataGrid element", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CarDataGrid.SelectedItem is Car selectedCar)
            {
                int carId = selectedCar.Property1;

                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete car with ID {carId}?", "Deletion confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    cars.Remove(selectedCar);

                    string sqlExpression = $"DELETE FROM Cars WHERE CarID = {carId}; " +
                                           $"DELETE FROM Prices WHERE ID_Car = {carId};";

                    SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Entry deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Could not find an entry to delete", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    CarDataGrid.Items.Refresh();
                    PriceDataGrid.Items.Refresh();
                }
            }

        }

        private void BackButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (currentWindow)
            {
                case "Panel":
                    ManagePanel.Visibility = Visibility.Hidden; ManagePanel.IsEnabled = false;
                    BackButton.Visibility = Visibility.Hidden; BackButton.IsEnabled = false;
                    currentWindow = "Main";
                    break;

                case "Update":
                    SetVisibility("Close Update");
                    break;
                default:
                    break;
            }
        }

        private void CarDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if(Mode == "Update")
            {
                if (CarDataGrid.SelectedItem == null)
                {
                    MessageBox.Show($"Choose CarDataGrid element", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (PriceDataGrid.SelectedItem != null)
                {
                    MessageBox.Show($"Choose CarDataGrid element", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CarDataGrid.SelectedItem is Car selectedCar)
                {
                    Id = selectedCar.Property1;
                    SetVisibility("Open Update");
                }
            }
        }
    }

    public class Car
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public int Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
    }

    public class Price
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
    }
}