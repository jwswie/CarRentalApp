using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Security.Cryptography;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;
        static private string currentWindow, category;
        static public string connectionString = @"Data Source = DESKTOP-BVS5CLQ; Initial Catalog = CarRental; Trusted_Connection=True; TrustServerCertificate = True";
        static public SqlConnectionManager sqlConnectionManager = new SqlConnectionManager(connectionString);
        static private string fullName, login, password;
        static private int userID, carID;

        public MainWindow()
        {
            InitializeComponent();
            Closed += Window_Closed;
            sqlConnectionManager.OpenConnection();
            GridFilling.FillCategory(sqlConnectionManager, CatalogGrid);
            //StartServer();
        }

        private void ClearTextBoxes()
        {
            txtLoginL.Clear();
            pwdPasswordL.Clear();
        }

        public void SetVisibility(string window)
        {
            switch (window)
            {
                case "Open Log In Window":
                    LogIn.Visibility = Visibility.Visible; LogIn.IsEnabled = true;
                    currentWindow = "Log In";
                    break;

                case "Close Log In Window":
                    LogIn.Visibility = Visibility.Hidden; LogIn.IsEnabled = false;
                    currentWindow = "Category";
                    break;

                case "Open View":
                    CarView.Visibility = Visibility.Visible; CarView.IsEnabled = true;
                    BackButton.Visibility = Visibility.Visible; BackButton.IsEnabled = true;
                    currentWindow = "View";
                    break;

                case "Close View":
                    CarView.Visibility = Visibility.Hidden; CarView.IsEnabled = false;
                    BackButton.Visibility = Visibility.Hidden; BackButton.IsEnabled = false;
                    currentWindow = "Catalog";
                    break;

                case "Open Chat":
                    Chat.Visibility = Visibility.Visible; Chat.IsEnabled = true;
                    currentWindow = "Chat";
                    break;

                case "Close Chat":
                    Chat.Visibility = Visibility.Hidden; Chat.IsEnabled = false;
                    currentWindow = "Chat";
                    break;

                default:
                    break;
            }
        }

        private async void StartServer()
        {
            server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            client = await server.AcceptTcpClientAsync();
            stream = client.GetStream();

            User user = new User() { Username = "admin" };
            UserMessageViewModel userMessageViewModel = new UserMessageViewModel(user);
            
            // Start a task to continuously read from the server
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        byte[] buffer = new byte[1024];
                        int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string responseData = Encoding.UTF8.GetString(buffer, 0, byteCount);

                        // Dispatcher.Invoke(() => receivedTextBox.AppendText(responseData + Environment.NewLine));
                        Dispatcher.Invoke(() => userMessageViewModel.AddMessage(user, responseData, MessageContainer, MyScrollViewer, "client"));
                    }
                }
                catch (IOException ex)
                {
                    // Handle IOException when the server disconnects
                    MessageBox.Show("Server disconnected: " + ex);
                }
            });
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility("Close Log In Window"); SetVisibility("Close Sign In Window"); SetVisibility("Close Profile"); SetVisibility("Close View"); SetVisibility("Close Date"); SetVisibility("Close Chat");
            currentWindow = "Category";
            GridFilling.FillCategory(sqlConnectionManager, CatalogGrid);
            Color1.Fill = (Brush)new BrushConverter().ConvertFromString("White");
            Color2.Fill = (Brush)new BrushConverter().ConvertFromString("White");
            Color3.Fill = (Brush)new BrushConverter().ConvertFromString("White");
        }

        private void Grid_Click(object sender, MouseButtonEventArgs e)
        {
            if (currentWindow == "Category")
            {
                CategoryControl clickedControl = e.Source as CategoryControl;

                if (clickedControl != null)
                {
                    category = clickedControl.Category.Content.ToString();
                    currentWindow = "Catalog";
                    GridFilling.FillCatalogGrid(category, sqlConnectionManager, CatalogGrid);
                }
            }
            else if (currentWindow == "Catalog")
            {
                CarControl clickedControl = e.Source as CarControl;
                List<string> colors = new List<string>();

                if (clickedControl != null)
                {
                    carID = int.Parse(clickedControl.IDLabel.Content.ToString());
                    SetVisibility("Open View");

                    try
                    {
                        string sqlExpression1 = "SELECT a.Model, a.ManufactureYear, a.CurrentStatus, a.Transmission, a.FuelType, b.ImageData " +
                                                "FROM Cars a " +
                                                "JOIN ImageTable b ON a.CarId = b.ImageID " +
                                                "WHERE a.CarID = @CarID"; // Получаем информацию о конкретном авто

                        using (SqlCommand command = new SqlCommand(sqlExpression1, sqlConnectionManager.connection))
                        {
                            command.Parameters.AddWithValue("@CarID", carID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    CarNameLabel.Content = reader["Model"].ToString();
                                    YearLabel.Content = $"({reader["ManufactureYear"]})";
                                    FuelLabel.Content = reader["FuelType"].ToString();
                                    StatusLabel.Content = reader["CurrentStatus"].ToString();
                                    TransmissionLabel.Content = reader["Transmission"].ToString();
                                    byte[] img = (byte[])reader["ImageData"];

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
                            }
                        }

                        string sqlExpression2 = "SELECT c.CarID, c.Color, i.ID_Car " +
                                                "FROM Cars c " +
                                                "JOIN ImageTable i ON c.CarID = i.ID_Car " +
                                                "WHERE c.Model = @targetCarModel " +
                                                "GROUP BY c.CarID, c.Color, i.ID_Car"; // Получаем цвет и изображение для конкретного авто

                        using (SqlCommand command = new SqlCommand(sqlExpression2, sqlConnectionManager.connection))
                        {
                            command.Parameters.AddWithValue("@targetCarModel", CarNameLabel.Content.ToString());

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string color = reader["Color"].ToString();
                                    colors.Add(color);
                                }
                            }

                            string[] colorsArray = colors.ToArray();

                            for (int i = 0; i < colorsArray.Length; i++)
                            {
                                string color = colorsArray[i];

                                switch (color)
                                {
                                    case "White":
                                        Color1.Fill = (Brush)new BrushConverter().ConvertFromString("#FFD4D4D4");
                                        break;
                                    case "Black":
                                        Color2.Fill = (Brush)new BrushConverter().ConvertFromString("Black");
                                        break;
                                    case "Blue":
                                        Color3.Fill = (Brush)new BrushConverter().ConvertFromString("#FF4479A9");
                                        break;
                                    case "Orange":
                                        Color1.Fill = (Brush)new BrushConverter().ConvertFromString("#FFF9772B");
                                        break;
                                    case "Grey":
                                        Color2.Fill = (Brush)new BrushConverter().ConvertFromString("#FF8F949B");
                                        break;
                                    case "Metallic":
                                        Color3.Fill = (Brush)new BrushConverter().ConvertFromString("#FF545559");
                                        break;
                                    case "Brown":
                                        Color2.Fill = (Brush)new BrushConverter().ConvertFromString("#FFD4C6BB");
                                        break;
                                    case "Red":
                                        Color2.Fill = (Brush)new BrushConverter().ConvertFromString("#FF970100");
                                        break;
                                    default:
                                        Console.WriteLine($"Unknown color: {color}");
                                        break;
                                }
                            }
                        }

                        string sqlExpresion3 = "SELECT c.CarID, p.RentDays, p.PricePerDay " +
                                               "FROM Cars c " +
                                               "JOIN Prices p ON c.CarID = p.ID_Car " +
                                               "WHERE c.CarID = @targetCarID"; // Получаем цены на конкретное авто

                        using (SqlCommand command = new SqlCommand(sqlExpresion3, sqlConnectionManager.connection))
                        {
                            command.Parameters.AddWithValue("@targetCarID", carID);

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
                            Price3.Content = $"{pricePerDay1}$";
                            Price9.Content = $"{pricePerDay2}$";
                            Price26.Content = $"{pricePerDay3}$";
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ERROR: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }

        private void ManagementButton_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility("Close Date"); SetVisibility("Close Chat");
            string LabelContent = LogInLabel.Content.ToString();

            if (LabelContent == "Log In")
            {
                MessageBox.Show("You can't open your profile because you haven't logged in yet", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                SetVisibility("Open Profile");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User user = new User() { Username = "admin" };
                UserMessageViewModel userMessageViewModel = new UserMessageViewModel(user);
                string dataString = EnterTextBox.Text;
                byte[] data = Encoding.UTF8.GetBytes(dataString);
                await stream.WriteAsync(data, 0, data.Length);
                EnterTextBox.Clear();
                await Dispatcher.InvokeAsync(() =>
                {
                    userMessageViewModel.AddMessage(user, dataString, MessageContainer, MyScrollViewer, "server");
                    DataContext = userMessageViewModel;
                });
            }
            catch (IOException ex)
            {
                // Handle IOException when the server disconnects
                MessageBox.Show("Server disconnected: " + ex);
            }
        }

        private void LogInLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetVisibility("Close Main Window"); SetVisibility("Open Log In Window"); SetVisibility("Close Profile"); SetVisibility("Close View"); SetVisibility("Close Date"); SetVisibility("Close Chat");
            CatalogGrid.Children.Clear();
            string LabelContent = LogInLabel.Content.ToString();

            if (LabelContent == "Log Out")
            {
                LogInLabel.Content = "Log In";
                userID = -1;
            }
        }

        private void logInButton_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLoginL.Text;

            string sqlExpression = "SELECT * FROM Administrators WHERE AdminLogin = @AdminLogin";

            try
            {
                using (SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection))
                {
                    command.Parameters.AddWithValue("@AdminLogin", login);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userID = reader.GetInt32(0);
                            string storedHashedPassword = reader.GetString(2);

                            byte[] storedHashBytes = Convert.FromBase64String(storedHashedPassword); //Декодирование хеша пароля из БД в массив байтов
                            byte[] salt = new byte[16];
                            Array.Copy(storedHashBytes, 0, salt, 0, 16); //Первые 16 байтов декодированного хеша копируются в новый массив salt, который представляет собой соль, использованную при хешировании пароля

                            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(pwdPasswordL.Password, salt, 10000))
                            { //Используем Rfc2898DeriveBytes, чтобы повторно создать хеш для введенного пароля с использованием той же соли, которая была использована при создании оригинального хеша
                                byte[] hash = pbkdf2.GetBytes(20);

                                byte[] hashBytes = new byte[36];
                                Array.Copy(salt, 0, hashBytes, 0, 16);
                                Array.Copy(hash, 0, hashBytes, 16, 20);
                                string enteredPasswordHash = Convert.ToBase64String(hashBytes); //Создаем новый массив байтов hashBytes, в котором сначала идут байты соли, а затем байты хеша. Затем этот массив конвертируется в строку

                                if (enteredPasswordHash == storedHashedPassword)
                                {
                                    SetVisibility("Close Log In Window"); SetVisibility("Open Main Window");

                                    LogInLabel.Content = "Log Out";
                                    ClearTextBoxes();
                                    reader.Close();
                                    GridFilling.FillCategory(sqlConnectionManager, CatalogGrid);
                                }
                                else
                                {
                                    MessageBox.Show("Incorrect Password", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Incorrect Login", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility("Open Chat"); CatalogGrid.Children.Clear(); SetVisibility("Close Profile");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.Close();
        }

        private void ColorButton_Click(object sender, MouseButtonEventArgs e)
        {
            Brush fillBrush = Color1.Fill;

            Rectangle clickedControl = e.Source as Rectangle;

            switch (clickedControl.Tag.ToString())
            {
                case "1":
                    fillBrush = Color1.Fill;
                    break;
                case "2":
                    fillBrush = Color2.Fill;
                    break;
                case "3":
                    fillBrush = Color3.Fill;
                    break;
            }

            if (fillBrush is SolidColorBrush solidColorBrush)
            {
                Color color = solidColorBrush.Color;
                string colorString = color.ToString();

                switch (colorString)
                {
                    case "#FFD4D4D4":
                        colorString = "White";
                        break;
                    case "#FF000000":
                        colorString = "Black";
                        break;
                    case "#FF4479A9":
                        colorString = "Blue";
                        break;
                    case "#FFF9772B":
                        colorString = "Orange";
                        break;
                    case "#FF8F949B":
                        colorString = "Grey";
                        break;
                    case "#FF545559":
                        colorString = "Metallic";
                        break;
                    case "#FFD4C6BB":
                        colorString = "Brown";
                        break;
                    case "#FF970100":
                        colorString = "Red";
                        break;
                    default:
                        MessageBox.Show($"Unknown color: {color}");
                        break;
                }

                string sqlExpression = "SELECT c.CarID, c.Color, c.CurrentStatus, i.ImageData " +
                                        "FROM Cars c " +
                                        "JOIN ImageTable i ON c.CarID = i.ID_Car " +
                                        "WHERE c.Model = @targetCarModel AND c.Color = @targetColor"; // Выбираем цвет, изображение и статус для конкретной модели

                SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection);
                command.Parameters.AddWithValue("@targetCarModel", CarNameLabel.Content.ToString());
                command.Parameters.AddWithValue("@targetColor", colorString);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byte[] img = reader["ImageData"] as byte[];
                        carID = reader.GetInt32(reader.GetOrdinal("CarID"));
                        StatusLabel.Content = reader["CurrentStatus"].ToString();

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
        }

        private void BackButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (currentWindow)
            {
                case "Sign In":
                    SetVisibility("Close Sign In Window"); SetVisibility("Open Log In Window");
                    break;
                case "View":
                    SetVisibility("Close View");
                    CatalogGrid.Children.Clear();
                    GridFilling.FillCatalogGrid(category, sqlConnectionManager, CatalogGrid);
                    Color1.Fill = (Brush)new BrushConverter().ConvertFromString("White");
                    Color2.Fill = (Brush)new BrushConverter().ConvertFromString("White");
                    Color3.Fill = (Brush)new BrushConverter().ConvertFromString("White");
                    break;
                case "Edit Profile":
                    SetVisibility("Close Edit Profile");
                    SetVisibility("Open Profile");
                    break;
                case "Date":
                    SetVisibility("Close Date");
                    break;

                case "Book List":
                    SetVisibility("Close Book List");
                    SetVisibility("Open Profile");
                    break;
                default:
                    break;
            }
        }
    }
}
