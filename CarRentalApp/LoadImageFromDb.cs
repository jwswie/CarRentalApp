using System.Data.SqlClient;

namespace CarRentalApp
{
    public class LoadImageFromDb
    {
        static public string connectionString = @"Data Source = DESKTOP-BVS5CLQ; Initial Catalog = CarRental; Trusted_Connection=True; TrustServerCertificate = True";

        public byte[] imageData = null;

        public byte[] LoadImage(int ID)
        {
            string quwery = "SELECT ImageData FROM ImageTable WHERE ID = @ID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(quwery, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        imageData = (byte[])reader["ImageData"];
                    }
                }
                return imageData;
            }
        }
    }
}
