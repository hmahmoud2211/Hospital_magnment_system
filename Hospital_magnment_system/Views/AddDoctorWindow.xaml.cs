using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Data;
using Hospital_magnment_system.DataAccess;
using System.Windows.Controls;

namespace Hospital_magnment_system.Views
{
    public partial class AddDoctorWindow : Window
    {
        private DataRowView _doctorToEdit;
        private bool _isEditMode;

        public AddDoctorWindow()
        {
            InitializeComponent();
            dpJoinDate.SelectedDate = DateTime.Today;
            cmbStatus.SelectedIndex = 0;
        }

        public AddDoctorWindow(DataRowView doctor)
        {
            InitializeComponent();
            _doctorToEdit = doctor;
            _isEditMode = true;
            LoadDoctorData();
        }

        private void LoadDoctorData()
        {
            txtFirstName.Text = _doctorToEdit["FirstName"].ToString();
            txtLastName.Text = _doctorToEdit["LastName"].ToString();
            txtSpecialization.Text = _doctorToEdit["Specialization"].ToString();
            txtDepartment.Text = _doctorToEdit["Department"].ToString();
            txtPhone.Text = _doctorToEdit["Phone"].ToString();
            txtEmail.Text = _doctorToEdit["Email"].ToString();
            txtLicenseNumber.Text = _doctorToEdit["LicenseNumber"].ToString();
            dpJoinDate.SelectedDate = Convert.ToDateTime(_doctorToEdit["JoinDate"]);
            cmbStatus.Text = _doctorToEdit["Status"].ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    using (var conn = DatabaseConnection.GetConnection())
                    {
                        conn.Open();
                        string query;
                        MySqlCommand cmd;

                        if (_isEditMode)
                        {
                            query = @"UPDATE Doctors SET 
                                    FirstName = @FirstName,
                                    LastName = @LastName,
                                    Specialization = @Specialization,
                                    Department = @Department,
                                    Phone = @Phone,
                                    Email = @Email,
                                    LicenseNumber = @LicenseNumber,
                                    JoinDate = @JoinDate,
                                
                                    WHERE DoctorID = @DoctorID";

                            cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@DoctorID", _doctorToEdit["DoctorID"]);
                        }
                        else
                        {
                            query = @"INSERT INTO Doctors 
                                    (FirstName, LastName, Specialization, Department, 
                                     Phone, Email, LicenseNumber, JoinDate)
                                    VALUES 
                                    (@FirstName, @LastName, @Specialization, @Department,
                                     @Phone, @Email, @LicenseNumber, @JoinDate, @Status)";

                            cmd = new MySqlCommand(query, conn);
                        }

                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@Specialization", txtSpecialization.Text);
                        cmd.Parameters.AddWithValue("@Department", txtDepartment.Text);
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@LicenseNumber", txtLicenseNumber.Text);
                        cmd.Parameters.AddWithValue("@JoinDate", dpJoinDate.SelectedDate);

                        cmd.ExecuteNonQuery();
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving doctor: " + ex.Message);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Please enter First Name");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Please enter Last Name");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtSpecialization.Text))
            {
                MessageBox.Show("Please enter Specialization");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter Phone Number");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please enter Email");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtLicenseNumber.Text))
            {
                MessageBox.Show("Please enter License Number");
                return false;
            }
            if (!dpJoinDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select Join Date");
                return false;
            }
            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 