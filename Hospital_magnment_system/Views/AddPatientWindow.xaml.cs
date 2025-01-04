using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;

namespace Hospital_magnment_system.Views
{
    public partial class AddPatientWindow : Window
    {
        public AddPatientWindow()
        {
            InitializeComponent();
            cmbGender.SelectedIndex = 0; // Set default gender selection
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    SavePatient();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving patient: " + ex.Message);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Please enter first name");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Please enter last name");
                return false;
            }

            if (cmbGender.SelectedItem == null)
            {
                MessageBox.Show("Please select gender");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter phone number");
                return false;
            }

            return true;
        }

        private void SavePatient()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO Patients 
                                   (FirstName, LastName, Gender, Phone, Email, Address) 
                                   VALUES 
                                   (@FirstName, @LastName, @Gender, @Phone, @Email, @Address)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Gender", ((ComboBoxItem)cmbGender.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Patient added successfully!");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving patient: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 