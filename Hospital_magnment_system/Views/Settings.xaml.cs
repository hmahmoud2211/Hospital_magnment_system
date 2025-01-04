using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace Hospital_magnment_system.Views
{
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load database settings
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var builder = new MySqlConnectionStringBuilder(connectionString);
                
                txtServer.Text = builder.Server;
                txtDatabase.Text = builder.Database;
                txtUsername.Text = builder.UserID;
                txtPassword.Password = builder.Password;

                // Load general settings from database
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM HospitalSettings LIMIT 1";
                    var cmd = new MySqlCommand(query, conn);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtHospitalName.Text = reader["HospitalName"].ToString();
                            txtAddress.Text = reader["Address"].ToString();
                            txtContactNumber.Text = reader["ContactNumber"].ToString();
                            txtEmail.Text = reader["Email"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message);
            }
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = txtServer.Text,
                    Database = txtDatabase.Text,
                    UserID = txtUsername.Text,
                    Password = txtPassword.Password
                };

                using (var conn = new MySqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    MessageBox.Show("Connection successful!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save database settings
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = txtServer.Text,
                    Database = txtDatabase.Text,
                    UserID = txtUsername.Text,
                    Password = txtPassword.Password
                };

                config.ConnectionStrings.ConnectionStrings["DefaultConnection"].ConnectionString = builder.ConnectionString;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");

                // Save general settings to database
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO HospitalSettings 
                                   (HospitalName, Address, ContactNumber, Email) 
                                   VALUES (@Name, @Address, @Contact, @Email)
                                   ON DUPLICATE KEY UPDATE 
                                   HospitalName = @Name,
                                   Address = @Address,
                                   ContactNumber = @Contact,
                                   Email = @Email";

                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", txtHospitalName.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Contact", txtContactNumber.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Settings saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: " + ex.Message);
            }
        }
    }
} 