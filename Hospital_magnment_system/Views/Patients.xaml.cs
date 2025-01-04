using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class Patients : Page
    {
        public Patients()
        {
            InitializeComponent();
            LoadPatients();
        }

        private void LoadPatients()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT p.PatientID, 
                                   p.FirstName, 
                                   p.LastName,
                                   CONCAT(p.FirstName, ' ', p.LastName) as FullName, 
                                   p.Gender, 
                                   p.Phone, 
                                   p.Email, 
                                   p.Address
                                   FROM Patients p
                                   ORDER BY p.FirstName";
                    
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    
                    dgPatients.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patients: " + ex.Message);
            }
        }

        private void btnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addPatientWindow = new AddPatientWindow();
                if (addPatientWindow.ShowDialog() == true)
                {
                    LoadPatients(); // Refresh the grid
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding patient: " + ex.Message);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedPatient = dgPatients.SelectedItem as DataRowView;
                if (selectedPatient == null)
                {
                    MessageBox.Show("Please select a patient to edit");
                    return;
                }

                var editPatientWindow = new EditPatientWindow(selectedPatient);
                if (editPatientWindow.ShowDialog() == true)
                {
                    LoadPatients(); // Refresh the grid
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error editing patient: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedPatient = dgPatients.SelectedItem as DataRowView;
                if (selectedPatient == null)
                {
                    MessageBox.Show("Please select a patient to delete");
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this patient?",
                    "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DeletePatient(selectedPatient["PatientID"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting patient: " + ex.Message);
            }
        }

        private void btnAdmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedPatient = dgPatients.SelectedItem as DataRowView;
                if (selectedPatient == null)
                {
                    MessageBox.Show("Please select a patient to admit");
                    return;
                }

                var admitPatientWindow = new AdmitPatientWindow(selectedPatient);
                if (admitPatientWindow.ShowDialog() == true)
                {
                    LoadPatients(); // Refresh the grid
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error admitting patient: " + ex.Message);
            }
        }

        private void DeletePatient(string patientId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // First check if patient can be deleted
                            string checkQuery = @"SELECT COUNT(*) FROM Admissions 
                                                WHERE PatientID = @PatientID 
                                                AND Status = 'Active'";
                            
                            using (var cmd = new MySqlCommand(checkQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PatientID", patientId);
                                int activeAdmissions = Convert.ToInt32(cmd.ExecuteScalar());
                                
                                if (activeAdmissions > 0)
                                {
                                    throw new Exception("Cannot delete patient with active admission");
                                }
                            }

                            // If no active admissions, update patient status to 'Inactive'
                            string updateQuery = @"UPDATE Patients 
                                                 SET Status = 'Inactive' 
                                                 WHERE PatientID = @PatientID";
                            
                            using (var cmd = new MySqlCommand(updateQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PatientID", patientId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            MessageBox.Show("Patient deleted successfully");
                            LoadPatients(); // Refresh the grid
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting patient: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (dgPatients.ItemsSource is DataView dataView)
                {
                    string filter = txtSearch.Text.Trim();
                    dataView.RowFilter = string.IsNullOrEmpty(filter) ? "" :
                        $"FirstName LIKE '%{filter}%' OR LastName LIKE '%{filter}%' OR " +
                        $"Phone LIKE '%{filter}%' OR Email LIKE '%{filter}%'";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering patients: " + ex.Message);
            }
        }
    }
} 