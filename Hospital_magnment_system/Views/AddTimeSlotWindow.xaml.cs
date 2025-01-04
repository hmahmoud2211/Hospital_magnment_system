using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class AddTimeSlotWindow : Window
    {
        private readonly string _doctorId;
        private readonly DateTime _selectedDate;
        private readonly DataRowView _slotToEdit;
        private bool _isEditMode;

        public AddTimeSlotWindow(string doctorId, DateTime selectedDate, DataRowView slot = null)
        {
            InitializeComponent();
            _doctorId = doctorId;
            _selectedDate = selectedDate;
            _slotToEdit = slot;
            _isEditMode = slot != null;

            InitializeWindow();
        }

        private void InitializeWindow()
        {
            LoadTimeSlots();
            LoadPatients();

            if (_isEditMode)
            {
                Title = "Edit Time Slot";
                cmbTime.SelectedItem = _slotToEdit["TimeSlot"].ToString();
                cmbStatus.SelectedValue = _slotToEdit["Status"].ToString();
                // Load other fields...
            }
            else
            {
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void LoadTimeSlots()
        {
            var timeSlots = new List<string>();
            DateTime startTime = new DateTime(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, 9, 0, 0);
            DateTime endTime = new DateTime(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, 17, 0, 0);

            while (startTime <= endTime)
            {
                timeSlots.Add(startTime.ToString("HH:mm"));
                startTime = startTime.AddMinutes(30);
            }

            cmbTime.ItemsSource = timeSlots;
        }

        private void LoadPatients()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT PatientID, CONCAT(FirstName, ' ', LastName) as FullName FROM Patients ORDER BY FirstName";
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
                                    AppointmentDate = @AppointmentDate,
                                    PatientID = @PatientID,
                                    Notes = @Notes,
                                    Status = @Status
                                    WHERE AppointmentID = @AppointmentID";

                            cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@AppointmentID", _slotToEdit["AppointmentID"]);
                        }
                        else
                        {
                            query = @"INSERT INTO Appointments 
                                    (DoctorID, PatientID, AppointmentDate, Notes, Status)
                                    VALUES 
                                    (@DoctorID, @PatientID, @AppointmentDate, @Notes, @Status)";

                            cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@DoctorID", _doctorId);
                        }

                        var selectedTime = cmbTime.SelectedItem.ToString();
                        var appointmentDate = _selectedDate.Date.Add(TimeSpan.Parse(selectedTime));

                        cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);
                        cmd.Parameters.AddWithValue("@PatientID", ((DataRowView)cmbPatient.SelectedItem)["PatientID"]);
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
            if (cmbTime.SelectedItem == null)
            {
                MessageBox.Show("Please select a time slot");
                return false;
            }
            if (cmbPatient.SelectedItem == null)
            {
                MessageBox.Show("Please select a patient");
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