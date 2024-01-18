using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;

namespace CarRentalApp
{
    /// <summary>
    /// Interaction logic for ProfileControl.xaml
    /// </summary>
    public partial class ProfileControl : UserControl
    {
        private string currentWindow = "Profile";
        private string currrentMode;
        private SqlConnectionManager sqlConnectionManager;
        private string fullName;
        private int userID;

        public ProfileControl(SqlConnectionManager sqlConnectionManager, string fullName, int userID)
        {
            InitializeComponent();
            this.sqlConnectionManager = sqlConnectionManager;
            this.fullName = fullName;
            this.userID = userID;
            ProfileUsernameLabel.Content = fullName;
        }

        public void SetVisibility(string window)
        {
            switch (window)
            {
                case "Open Profile":
                    UserProfile.Visibility = Visibility.Visible; UserProfile.IsEnabled = true;
                    currentWindow = "Profile";
                    break;

                case "Close Profile":
                    UserProfile.Visibility = Visibility.Hidden; UserProfile.IsEnabled = false;
                    break;

                case "Open Edit Profile":
                    EditUserProfile.Visibility = Visibility.Visible; EditUserProfile.IsEnabled = true;
                    BackButton.Visibility = Visibility.Visible; BackButton.IsEnabled = true;
                    UserNameButton.Visibility = Visibility.Visible; UserNameButton.IsEnabled = true;
                    PasswordButton.Visibility = Visibility.Visible; PasswordButton.IsEnabled = true;
                    currentWindow = "Edit Profile";
                    break;

                case "Close Edit Profile":
                    EditUserProfile.Visibility = Visibility.Hidden; EditUserProfile.IsEnabled = false;
                    BackButton.Visibility = Visibility.Hidden; BackButton.IsEnabled = false;
                    UserNameButton.Visibility = Visibility.Hidden; UserNameButton.IsEnabled = false;
                    PasswordButton.Visibility = Visibility.Hidden; PasswordButton.IsEnabled = false;
                    break;

                case "Open Book List":
                    BookList.Visibility = Visibility.Visible; BookList.IsEnabled = true;
                    BackButton.Visibility = Visibility.Visible; BackButton.IsEnabled = true;
                    currentWindow = "Book List";
                    break;

                case "Close Book List":
                    BookList.Visibility = Visibility.Hidden; BookList.IsEnabled = false;
                    BackButton.Visibility = Visibility.Hidden; BackButton.IsEnabled = false;
                    currentWindow = "Profile";
                    break;

                default:
                    break;
            }
        }

        private void BackButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (currentWindow)
            {
                case "Edit Profile":
                    SetVisibility("Close Edit Profile");
                    SetVisibility("Open Profile");
                    break;

                case "Book List":
                    SetVisibility("Close Book List");
                    SetVisibility("Open Profile");
                    break;
                default:
                    break;
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility("Close Profile"); SetVisibility("Open Edit Profile");

            UserNameButton.Visibility = Visibility.Visible; UserNameButton.IsEnabled = true;
            PasswordButton.Visibility = Visibility.Visible; PasswordButton.IsEnabled = true;
            NewLabel.Visibility = Visibility.Hidden;
            txtNew.IsEnabled = false;
            EditConfirmButton.Visibility = Visibility.Hidden; EditConfirmButton.IsEnabled = false;
        }

        private void UserNameButton_Click(object sender, RoutedEventArgs e)
        {
            UserNameButton.Visibility = Visibility.Hidden; UserNameButton.IsEnabled = false;
            PasswordButton.Visibility = Visibility.Hidden; PasswordButton.IsEnabled = false;
            NewLabel.Content = "Enter New Name:"; NewLabel.Visibility = Visibility.Visible;
            txtNew.IsEnabled = true;
            EditConfirmButton.Visibility = Visibility.Visible; EditConfirmButton.IsEnabled = true;
            currrentMode = "Edit Name";
        }

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            UserNameButton.Visibility = Visibility.Hidden; UserNameButton.IsEnabled = false;
            PasswordButton.Visibility = Visibility.Hidden; PasswordButton.IsEnabled = false;
            NewLabel.Content = "Enter New Password:"; NewLabel.Visibility = Visibility.Visible;
            txtNew.IsEnabled = true;
            EditConfirmButton.Visibility = Visibility.Visible; EditConfirmButton.IsEnabled = true;
            currrentMode = "Edit Password";
        }

        private void EditConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNew.Text))
            {
                MessageBox.Show("Field must be filled", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (currrentMode == "Edit Name")
            {
                string NewName = txtNew.Text;

                try
                {
                    string sqlExpression = "UPDATE Users SET Username = @Username " + "WHERE ID = @ID";

                    using (SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection))
                    {
                        command.Parameters.AddWithValue("@ID", userID);
                        command.Parameters.AddWithValue("@Username", NewName);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            fullName = NewName;
                            MessageBox.Show("Name updated successfully");

                            SetVisibility("Close Edit Profile"); SetVisibility("Open Profile");
                            txtNew.Clear();
                            ProfileUsernameLabel.Content = NewName;
                        }
                        else
                        {
                            MessageBox.Show("Could not find an entry to update", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating data: {ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (currrentMode == "Edit Password")
            {
                string NewPassword = txtNew.Text;


                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                var pbkdf2 = new Rfc2898DeriveBytes(NewPassword, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);

                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

                string passwordHash = Convert.ToBase64String(hashBytes);

                try
                {
                    string sqlExpression = "UPDATE Users SET HashedPassword = @HashedPassword, Salt = @Salt " + "WHERE ID = @ID";

                    using (SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection))
                    {
                        command.Parameters.AddWithValue("@ID", userID);
                        command.Parameters.AddWithValue("@HashedPassword", passwordHash);
                        command.Parameters.AddWithValue("@Salt", Convert.ToBase64String(salt));

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Password updated successfully");

                            SetVisibility("Close Edit Profile"); SetVisibility("Open Profile");
                            txtNew.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Could not find an entry to update", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating data: {ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BookListButton_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility("Close Profile"); SetVisibility("Open Book List");

            cardListBox.Items.Clear();

            try
            {
                string sqlExpression = "SELECT c.CarID, c.Model, b.RentDays, i.ImageData " +
                                        "FROM Cars c " +
                                        "JOIN ImageTable i ON c.CarID = i.ID_Car " +
                                        "JOIN BookedCars b ON c.CarID = b.ID_Car " +
                                        "WHERE b.ID_User = @ID";

                SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection);

                command.Parameters.AddWithValue("@ID", userID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string CarId = reader["CarID"].ToString();
                        string CarModel = reader["Model"].ToString();

                        cardListBox.Items.Add($"{CarId}: {CarModel}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
