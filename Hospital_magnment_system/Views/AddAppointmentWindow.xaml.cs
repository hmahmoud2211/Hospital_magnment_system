using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class AddAppointmentWindow : Window
    {
        private readonly DataRowView _appointmentToEdit;
        private bool _isEditMode;

        public AddAppointmentWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        public AddAppointmentWindow(DataRowView appointment)
        {
            InitializeComponent();
            _appointmentToEdit = appointment;
            _isEditMode = true;
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            LoadPatients();
            LoadDoctors();
            dpAppointmentDate.SelectedDate = DateTime.Today;

            if (_isEditMode)
            {
                Title = "Edit Appointment";
                LoadAppointmentData();
            }
            else
            {
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void LoadPatients()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT PatientID, 
                                   CONCAT(FirstName, ' ', LastName) as FullName 
                                   FROM Patients 
                                   ORDER BY FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbPatient.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patients: " + ex.Message);
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
                    cmbDoctor.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message);
            }
        }

        private void LoadAppointmentData()
        {
            // Set the selected patient
            foreach (DataRowView item in cmbPatient.Items)
            {
                if (item["FullName"].ToString() == _appointmentToEdit["PatientName"].ToString())
                {
                    cmbPatient.SelectedItem = item;
                    break;
                }
            }

            // Set the selected doctor
            foreach (DataRowView item in cmbDoctor.Items)
            {
                if (item["FullName"].ToString() == _appointmentToEdit["DoctorName"].ToString())
                {
                    cmbDoctor.SelectedItem = item;
                    break;
                }
            }

            DateTime appointmentDate = Convert.ToDateTime(_appointmentToEdit["AppointmentDate"]);
            dpAppointmentDate.SelectedDate = appointmentDate.Date;
            LoadAvailableTimeSlots();
            cmbTime.SelectedItem = appointmentDate.ToString("HH:mm");
            cmbStatus.Text = _appointmentToEdit["Status"].ToString();
        }

        private void LoadAvailableTimeSlots()
        {
            if (cmbDoctor.SelectedItem == null || !dpAppointmentDate.SelectedDate.HasValue)
                return;

            try
            {
                var timeSlots = new List<string>();
                DateTime startTime = new DateTime(dpAppointmentDate.SelectedDate.Value.Year,
                                               dpAppointmentDate.SelectedDate.Value.Month,
                                               dpAppointmentDate.SelectedDate.Value.Day, 9, 0, 0);
                DateTime endTime = startTime.AddHours(8); // 9 AM to 5 PM

                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT TIME_FORMAT(AppointmentDate, '%H:%i') as BookedTime 
                                   FROM Appointments 
                                   WHERE DoctorID = @DoctorID 
                                   AND DATE(AppointmentDate) = @AppointmentDate 
                                   AND Status != 'Cancelled'";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@DoctorID", ((DataRowView)cmbDoctor.SelectedItem)["DoctorID"]);
                    cmd.Parameters.AddWithValue("@AppointmentDate", dpAppointmentDate.SelectedDate.Value.Date);

                    var bookedSlots = new HashSet<string>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookedSlots.Add(reader["BookedTime"].ToString());
                        }
                    }

                    while (startTime <= endTime)
                    {
                        string timeSlot = startTime.ToString("HH:mm");
                        if (!bookedSlots.Contains(timeSlot))
                        {
                            timeSlots.Add(timeSlot);
                        }
                        startTime = startTime.AddMinutes(30);
                    }
                }

                cmbTime.ItemsSource = timeSlots;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading time slots: " + ex.Message);
            }
        }

        private void cmbDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAvailableTimeSlots();
        }

        private void AppointmentDate_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadAvailableTimeSlots();
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
                            query = @"UPDATE Appointments SET 
                                    PatientID = @PatientID,
                                    DoctorID = @DoctorID,
                                    AppointmentDate = @AppointmentDate,
                                    Reason = @Reason,
                                    Notes = @Notes,
                                    Status = @Status
                                    WHERE AppointmentID = @AppointmentID";

                            cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@AppointmentID", _appointmentToEdit["AppointmentID"]);
                        }
                        else
                        {
                            query = @"INSERT INTO Appointments 
                                    (PatientID, DoctorID, AppointmentDate, Reason, Notes, Status)
                                    VALUES 
                                    (@PatientID, @DoctorID, @AppointmentDate, @Reason, @Notes, @Status)";

                            cmd = new MySqlCommand(query, conn);
                        }

                        var selectedTime = cmbTime.SelectedItem.ToString();
                        var appointmentDate = dpAppointmentDate.SelectedDate.Value.Date.Add(TimeSpan.Parse(selectedTime));

                        cmd.Parameters.AddWithValue("@PatientID", ((DataRowView)cmbPatient.SelectedItem)["PatientID"]);
                        cmd.Parameters.AddWithValue("@DoctorID", ((DataRowView)cmbDoctor.SelectedItem)["DoctorID"]);
                        cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);
                        cmd.Parameters.AddWithValue("@Reason", txtReason.Text);
                        cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);
                        cmd.Parameters.AddWithValue("@Status", ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString());

                        cmd.ExecuteNonQuery();
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving appointment: " + ex.Message);
                }
            }
        }

        private bool ValidateInput()
        {
            if (cmbPatient.SelectedItem == null)
            {
                MessageBox.Show("Please select a patient");
                return false;
            }
            if (cmbDoctor.SelectedItem == null)
            {
                MessageBox.Show("Please select a doctor");
                return false;
            }
            if (!dpAppointmentDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a date");
                return false;
            }
            if (cmbTime.SelectedItem == null)
            {
                MessageBox.Show("Please select a time slot");
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