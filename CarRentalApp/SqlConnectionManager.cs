using System;
using System.Data.SqlClient;
using System.Windows;

namespace CarRentalApp
{
    public class SqlConnectionManager
    {
        private string connectionString;
        public SqlConnection connection;
        private bool IsConnected;

        public SqlConnectionManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void OpenConnection()
        {
            try
            {
                if (!IsConnected) // Если подключение закрыто - то открываем его
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    IsConnected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening connection: {ex.Message}");
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (IsConnected) // Если подключение открыто - то закрываем его
                {
                    connection.Close();
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing connection: {ex.Message}");
            }
        }
    }
}