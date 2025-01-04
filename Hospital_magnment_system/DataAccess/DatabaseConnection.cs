using MySql.Data.MySqlClient;
using System;

namespace Hospital_magnment_system.DataAccess
{
    public class DatabaseConnection
    {
        private static string connectionString = "Server=localhost;Database=hospital_management;Uid=root;Pwd=Hazem@2003;";
        
        public static MySqlConnection GetConnection()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception("Error connecting to database: " + ex.Message);
            }
        }
    }
} 