using System;
using System.Windows;
using System.Data;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Windows.Controls;

namespace Hospital_magnment_system.Views
{
    public partial class EditPatientWindow : Window
    {
        private readonly DataRowView _patient;

        public EditPatientWindow(DataRowView patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadPatientData();
        }

        private void LoadPatientData()
        {
            try
            {
                txtFirstName.Text = _patient["FirstName"].ToString();
                txtLastName.Text = _patient["LastName"].ToString();
                txtPhone.Text = _patient["Phone"].ToString();
                txtEmail.Text = _patient["Email"].ToString();
                txtAddress.Text = _patient["Address"].ToString();

                string gender = _patient["Gender"].ToString();
                foreach (ComboBoxItem item in cmbGender.Items)
                {
                    if (item.Content.ToString() == gender)
                    {
                        cmbGender.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patient data: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    UpdatePatient();
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

        private void UpdatePatient()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE Patients 
                                   SET FirstName = @FirstName,
                                       LastName = @LastName,
                                       Gender = @Gender,
                                       Phone = @Phone,
                                       Email = @Email,
                                       Address = @Address
                                   WHERE PatientID = @PatientID";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", _patient["PatientID"]);
                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Gender", ((ComboBoxItem)cmbGender.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Patient updated successfully!");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating patient: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 