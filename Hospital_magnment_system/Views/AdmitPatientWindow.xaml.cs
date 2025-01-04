using System;
using System.Windows;
using System.Data;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;

namespace Hospital_magnment_system.Views
{
    public partial class AdmitPatientWindow : Window
    {
        private readonly DataRowView _patient;

        public AdmitPatientWindow(DataRowView patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadPatientInfo();
            LoadRooms();
            LoadDoctors();
            dpAdmissionDate.SelectedDate = DateTime.Today;
        }

        private void LoadPatientInfo()
        {
            txtPatientName.Text = $"{_patient["FirstName"]} {_patient["LastName"]}";
            txtPatientDetails.Text = $"Gender: {_patient["Gender"]}\n" +
                                   $"Phone: {_patient["Phone"]}\n" +
                                   $"Email: {_patient["Email"]}";
        }

        private void LoadRooms()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT RoomID, RoomNumber, RatePerDay 
                                   FROM Rooms 
                                   WHERE Status = 'Available'
                                   ORDER BY RoomNumber";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        cmbRoom.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading rooms: " + ex.Message);
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
                                   CONCAT(FirstName, ' ', LastName) as FullName,
                                   Specialization
                                   FROM Doctors 
                                   WHERE Status = 'Active'
                                   ORDER BY FirstName";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        cmbDoctor.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message);
            }
        }

        private void btnAdmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    AdmitPatient();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error admitting patient: " + ex.Message);
            }
        }

        private bool ValidateInput()
        {
            if (cmbRoom.SelectedItem == null)
            {
                MessageBox.Show("Please select a room");
                return false;
            }

            if (cmbDoctor.SelectedItem == null)
            {
                MessageBox.Show("Please select a doctor");
                return false;
            }

            if (!dpAdmissionDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select admission date");
                return false;
            }

            return true;
        }

        private void AdmitPatient()
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
                            // Create admission record
                            string admissionQuery = @"INSERT INTO Admissions 
                                (PatientID, DoctorID, AdmissionDate, Notes, Status) 
                                VALUES 
                                (@PatientID, @DoctorID, @AdmissionDate, @Notes, 'Active')";

                            using (var cmd = new MySqlCommand(admissionQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PatientID", _patient["PatientID"]);
                                cmd.Parameters.AddWithValue("@DoctorID", ((DataRowView)cmbDoctor.SelectedItem)["DoctorID"]);
                                cmd.Parameters.AddWithValue("@AdmissionDate", dpAdmissionDate.SelectedDate.Value);
                                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text.Trim());
                                cmd.ExecuteNonQuery();
                            }

                            // Update room status
                            string roomQuery = @"UPDATE Rooms 
                                               SET Status = 'Occupied' 
                                               WHERE RoomID = @RoomID";

                            using (var cmd = new MySqlCommand(roomQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@RoomID", ((DataRowView)cmbRoom.SelectedItem)["RoomID"]);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            MessageBox.Show("Patient admitted successfully!");
                            DialogResult = true;
                            Close();
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
                throw new Exception("Error admitting patient: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 