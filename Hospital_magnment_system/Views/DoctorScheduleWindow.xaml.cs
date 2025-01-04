using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class DoctorScheduleWindow : Window
    {
        private readonly DataRowView _doctor;

        public DoctorScheduleWindow(DataRowView doctor)
        {
            InitializeComponent();
            _doctor = doctor;
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            txtDoctorName.Text = $"Dr. {_doctor["FirstName"]} {_doctor["LastName"]}";
            txtSpecialization.Text = _doctor["Specialization"].ToString();
            dpScheduleDate.SelectedDate = DateTime.Today;
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   a.AppointmentID,
                                   TIME_FORMAT(a.AppointmentDate, '%H:%i') as TimeSlot,
                                   CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                                   a.Status
                                   FROM Appointments a
                                   LEFT JOIN Patients p ON a.PatientID = p.PatientID
                                   WHERE a.DoctorID = @DoctorID 
                                   AND DATE(a.AppointmentDate) = @ScheduleDate
                                   ORDER BY a.AppointmentDate";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@DoctorID", _doctor["DoctorID"]);
                    cmd.Parameters.AddWithValue("@ScheduleDate", dpScheduleDate.SelectedDate);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgSchedule.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading schedule: " + ex.Message);
            }
        }

        private void dpScheduleDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSchedule();
        }

        private void btnAddTimeSlot_Click(object sender, RoutedEventArgs e)
        {
            var addSlotWindow = new AddTimeSlotWindow(_doctor["DoctorID"].ToString(), dpScheduleDate.SelectedDate.Value);
            if (addSlotWindow.ShowDialog() == true)
            {
                LoadSchedule();
            }
        }

        private void btnEditSlot_Click(object sender, RoutedEventArgs e)
        {
            var slot = ((FrameworkElement)sender).DataContext as DataRowView;
            if (slot != null)
            {
                var editSlotWindow = new AddTimeSlotWindow(_doctor["DoctorID"].ToString(), dpScheduleDate.SelectedDate.Value, slot);
                if (editSlotWindow.ShowDialog() == true)
                {
                    LoadSchedule();
                }
            }
        }

        private void btnCancelSlot_Click(object sender, RoutedEventArgs e)
        {
            var slot = ((FrameworkElement)sender).DataContext as DataRowView;
            if (slot != null)
            {
                if (MessageBox.Show("Are you sure you want to cancel this appointment?", 
                    "Confirm Cancellation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DatabaseConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "UPDATE Appointments SET Status = 'Cancelled' WHERE AppointmentID = @AppointmentID";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@AppointmentID", slot["AppointmentID"]);
                            cmd.ExecuteNonQuery();
                            LoadSchedule();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error cancelling appointment: " + ex.Message);
                    }
                }
            }
        }
    }
} 