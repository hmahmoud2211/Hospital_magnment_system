using System;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;

namespace Hospital_magnment_system.Views
{
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    LoadStatistics(conn);
                    LoadRecentAppointments(conn);
                    LoadRecentAdmissions(conn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard data: " + ex.Message);
            }
        }

        private void LoadStatistics(MySqlConnection conn)
        {
            // Total Patients
            string query = "SELECT COUNT(*) FROM Patients";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            txtTotalPatients.Text = cmd.ExecuteScalar().ToString();

            // Total Doctors
            query = "SELECT COUNT(*) FROM Doctors";
            cmd = new MySqlCommand(query, conn);
            txtTotalDoctors.Text = cmd.ExecuteScalar().ToString();

            // Today's Appointments
            query = "SELECT COUNT(*) FROM Appointments WHERE DATE(AppointmentDate) = CURDATE()";
            cmd = new MySqlCommand(query, conn);
            txtTodayAppointments.Text = cmd.ExecuteScalar().ToString();

            // Available Rooms
            query = "SELECT COUNT(*) FROM Rooms WHERE Status = 'Available'";
            cmd = new MySqlCommand(query, conn);
            txtAvailableRooms.Text = cmd.ExecuteScalar().ToString();
        }

        private void LoadRecentAppointments(MySqlConnection conn)
        {
            string query = @"SELECT 
                            CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                            CONCAT(d.FirstName, ' ', d.LastName) as DoctorName,
                            a.AppointmentDate
                           FROM Appointments a
                           JOIN Patients p ON a.PatientID = p.PatientID
                           JOIN Doctors d ON a.DoctorID = d.DoctorID
                           ORDER BY a.AppointmentDate DESC
                           LIMIT 5";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            dgRecentAppointments.ItemsSource = dt.DefaultView;
        }

        private void LoadRecentAdmissions(MySqlConnection conn)
        {
            string query = @"SELECT 
                            CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                            r.RoomNumber,
                            ra.AdmissionDate
                           FROM RoomAllocations ra
                           JOIN Patients p ON ra.PatientID = p.PatientID
                           JOIN Rooms r ON ra.RoomID = r.RoomID
                           WHERE ra.Status = 'Active'
                           ORDER BY ra.AdmissionDate DESC
                           LIMIT 5";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            dgRecentAdmissions.ItemsSource = dt.DefaultView;
        }
    }
} 