using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class Appointments : Page
    {
        public Appointments()
        {
            InitializeComponent();
            InitializePage();
        }

        private void InitializePage()
        {
            try
            {
                dpStartDate.SelectedDate = DateTime.Today;
                dpEndDate.SelectedDate = DateTime.Today.AddDays(7);
                LoadDoctors();
                if (cmbStatus.Items.Count > 0)
                {
                    cmbStatus.SelectedIndex = 0;
                }
                LoadAppointments();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing page: " + ex.Message);
            }
        }

        private void LoadDoctors()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT DoctorID, 
                                   CONCAT(FirstName, ' ', LastName) as FullName 
                                   FROM Doctors 
                                   WHERE Status = 'Active'
                                   ORDER BY FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    var allRow = dt.NewRow();
                    allRow["DoctorID"] = DBNull.Value;
                    allRow["FullName"] = "All Doctors";
                    dt.Rows.InsertAt(allRow, 0);

                    cmbDoctor.ItemsSource = dt.DefaultView;
                    cmbDoctor.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message);
            }
        }

        private void LoadAppointments()
        {
            try
            {
                if (dpStartDate?.SelectedDate == null || dpEndDate?.SelectedDate == null || 
                    cmbStatus?.SelectedItem == null)
                {
                    return;
                }

                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   a.AppointmentID,
                                   a.AppointmentDate,
                                   CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                                   CONCAT(d.FirstName, ' ', d.LastName) as DoctorName,
                                   a.Status
                                   FROM Appointments a
                                   JOIN Patients p ON a.PatientID = p.PatientID
                                   JOIN Doctors d ON a.DoctorID = d.DoctorID
                                   WHERE (@DoctorID IS NULL OR a.DoctorID = @DoctorID)
                                   AND (@Status = 'All' OR a.Status = @Status)
                                   AND a.AppointmentDate BETWEEN @StartDate AND @EndDate
                                   ORDER BY a.AppointmentDate DESC";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    
                    cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate.Value);
                    cmd.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate.Value.AddDays(1));
                    
                    object doctorId = DBNull.Value;
                    if (cmbDoctor?.SelectedItem is DataRowView selectedDoctor && 
                        selectedDoctor["DoctorID"] != DBNull.Value)
                    {
                        doctorId = selectedDoctor["DoctorID"];
                    }
                    cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    
                    string status = "All";
                    if (cmbStatus.SelectedItem is ComboBoxItem selectedStatus)
                    {
                        status = selectedStatus.Content.ToString();
                    }
                    cmd.Parameters.AddWithValue("@Status", status);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgAppointments.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointments: " + ex.Message);
            }
        }

        private void DateRange_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized && dpStartDate?.SelectedDate != null && dpEndDate?.SelectedDate != null)
            {
                LoadAppointments();
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized && sender != null)
            {
                LoadAppointments();
            }
        }

        private void btnAddAppointment_Click(object sender, RoutedEventArgs e)
        {
            var addAppointmentWindow = new AddAppointmentWindow();
            if (addAppointmentWindow.ShowDialog() == true)
            {
                LoadAppointments();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var appointment = ((FrameworkElement)sender).DataContext as DataRowView;
            if (appointment != null)
            {
                var editWindow = new AddAppointmentWindow(appointment);
                if (editWindow.ShowDialog() == true)
                {
                    LoadAppointments();
                }
            }
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            var appointment = ((FrameworkElement)sender).DataContext as DataRowView;
            if (appointment != null)
            {
                if (MessageBox.Show("Are you sure you want to mark this appointment as completed?",
                    "Confirm Completion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    UpdateAppointmentStatus(appointment["AppointmentID"].ToString(), "Completed");
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var appointment = ((FrameworkElement)sender).DataContext as DataRowView;
            if (appointment != null)
            {
                if (MessageBox.Show("Are you sure you want to cancel this appointment?",
                    "Confirm Cancellation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    UpdateAppointmentStatus(appointment["AppointmentID"].ToString(), "Cancelled");
                }
            }
        }

        private void UpdateAppointmentStatus(string appointmentId, string status)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Appointments SET Status = @Status WHERE AppointmentID = @AppointmentID";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                    cmd.ExecuteNonQuery();
                    LoadAppointments();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating appointment status: {ex.Message}");
            }
        }
    }
} 