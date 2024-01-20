using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace CarRentalApp
{
    public class GridFilling
    {
        public static void FillCatalogGrid(string category, SqlConnectionManager sqlConnectionManager, Grid CatalogGrid)
        {
            CatalogGrid.Children.Clear();
            int ClassID = 0;

            switch (category)
            {
                case "Economy":
                    ClassID = 1;
                    break;
                case "Intermediate":
                    ClassID = 2;
                    break;
                case "Business":
                    ClassID = 3;
                    break;
            }

            try
            {
                string sqlExpression = "SELECT CarId, Model, ImageData FROM Cars a, ImageTable b WHERE a.CarId = b.ImageID AND a.ID_Class = @targetClass";

                using (SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection))
                {
                    command.Parameters.AddWithValue("@targetClass", ClassID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int row = 0;
                        int col = 0;

                        while (reader.Read())
                        {
                            byte[] imageData = (byte[])reader["ImageData"];
                            string model = reader["Model"].ToString();
                            string ID = reader["CarId"].ToString();

                            if (!Validation.IsModelAlreadyAdded(model, CatalogGrid))
                            {
                                CarControl UC = new CarControl(imageData, model, ID);

                                Grid.SetRow(UC, row);
                                Grid.SetColumn(UC, col);
                                CatalogGrid.Children.Add(UC);

                                col++;

                                if (col == 3)
                                {
                                    col = 0;
                                    row++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FillCategory(SqlConnectionManager sqlConnectionManager, Grid CatalogGrid)
        {
            CatalogGrid.Children.Clear();

            try
            {
                string sqlExpression = "WITH RankedCars AS (" +
                                "SELECT a.CarId, a.ID_Class, b.ImageData, c.ClassName, ROW_NUMBER() OVER(PARTITION BY c.ClassName ORDER BY a.CarId) AS RowNum " + // Номер строки отдельно для каждого класса
                                "FROM Cars a " +
                                "JOIN ImageTable b ON a.CarId = b.ImageID " +
                                "JOIN CarClass c ON a.ID_Class = c.ID" +
                                ") " +
                                "SELECT CarId, ID_Class, ImageData, ClassName " +
                                "FROM RankedCars " +
                                "WHERE RowNum = 1"; //Получаем первое авто в каждой категории

                using (SqlCommand command = new SqlCommand(sqlExpression, sqlConnectionManager.connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int row = 0;
                        int col = 0;

                        while (reader.Read())
                        {
                            byte[] imageData = (byte[])reader["ImageData"];
                            string category = reader["ClassName"].ToString();
                            string price = GetPriceByCategory(category);

                            CategoryControl UC = new CategoryControl(imageData, category, price);

                            Grid.SetRow(UC, row);
                            Grid.SetColumn(UC, col);
                            CatalogGrid.Children.Add(UC);

                            col++;

                            if (col == 4)
                            {
                                col = 0;
                                row++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string GetPriceByCategory(string category)
        {
            switch (category)
            {
                case "Economy":
                    return "14$";
                case "Intermediate":
                    return "23$";
                case "Business":
                    return "30$";
                default:
                    return "";
            }
        }
    }
}