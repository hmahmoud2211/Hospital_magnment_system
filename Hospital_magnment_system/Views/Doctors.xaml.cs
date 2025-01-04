using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class Doctors : Page
    {
        public Doctors()
        {
            InitializeComponent();
            LoadDoctors();
            LoadSpecializations();
        }

        private void LoadDoctors()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT DoctorID, FirstName, LastName, 
                                   Specialization, Department, Phone, Email, Status 
                                   FROM Doctors";
                    
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    
                    dgDoctors.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message);
            }
        }

        private void LoadSpecializations()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT DISTINCT Specialization FROM Doctors ORDER BY Specialization";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbSpecialization.ItemsSource = dt.DefaultView;
                    cmbSpecialization.DisplayMemberPath = "Specialization";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading specializations: " + ex.Message);
            }
        }

        private void btnAddDoctor_Click(object sender, RoutedEventArgs e)
        {
            var addDoctorWindow = new AddDoctorWindow();
            if (addDoctorWindow.ShowDialog() == true)
            {
                LoadDoctors(); // Refresh the list
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var doctor = ((FrameworkElement)sender).DataContext as DataRowView;
            if (doctor != null)
            {
                var editDoctorWindow = new AddDoctorWindow(doctor);
                if (editDoctorWindow.ShowDialog() == true)
                {
                    LoadDoctors(); // Refresh the list
                }
            }
        }

        private void btnSchedule_Click(object sender, RoutedEventArgs e)
        {
            var doctor = ((FrameworkElement)sender).DataContext as DataRowView;
            if (doctor != null)
            {
                var scheduleWindow = new DoctorScheduleWindow(doctor);
                scheduleWindow.ShowDialog();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var doctor = ((FrameworkElement)sender).DataContext as DataRowView;
            if (doctor != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this doctor?", 
                    "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DatabaseConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM Doctors WHERE DoctorID = @DoctorID";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@DoctorID", doctor["DoctorID"]);
                            cmd.ExecuteNonQuery();
                            
                            LoadDoctors(); // Refresh the list
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting doctor: " + ex.Message);
                    }
                }
            }
        }
    }
} 