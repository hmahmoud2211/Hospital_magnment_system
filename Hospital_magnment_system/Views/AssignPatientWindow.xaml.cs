using System;
using System.Windows;
using System.Data;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;

namespace Hospital_magnment_system.Views
{
    public partial class AssignPatientWindow : Window
    {
        private readonly DataRowView _room;

        public AssignPatientWindow(DataRowView room)
        {
            InitializeComponent();
            _room = room;
            LoadRoomInfo();
            LoadPatients();
            LoadDoctors();
            dpAdmissionDate.SelectedDate = DateTime.Today;
        }

        private void LoadRoomInfo()
        {
            txtRoomNumber.Text = _room["RoomNumber"].ToString();
            txtRoomType.Text = _room["RoomType"].ToString();
            txtRatePerDay.Text = string.Format("{0:C2}", _room["RatePerDay"]);
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
                                   WHERE PatientID NOT IN 
                                   (SELECT PatientID FROM RoomAllocations 
                                    WHERE Status = 'Active')
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

        private void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    using (var conn = DatabaseConnection.GetConnection())
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // Create room allocation
                                string query = @"INSERT INTO RoomAllocations 
                                               (RoomID, PatientID, DoctorID, 
                                                AdmissionDate, Status) 
                                               VALUES 
                                               (@RoomID, @PatientID, @DoctorID, 
                                                @AdmissionDate, 'Active')";

                                using (var cmd = new MySqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@RoomID", _room["RoomID"]);
                                    cmd.Parameters.AddWithValue("@PatientID", ((DataRowView)cmbPatient.SelectedItem)["PatientID"]);
                                    cmd.Parameters.AddWithValue("@DoctorID", ((DataRowView)cmbDoctor.SelectedItem)["DoctorID"]);
                                    cmd.Parameters.AddWithValue("@AdmissionDate", dpAdmissionDate.SelectedDate.Value);
                                    cmd.ExecuteNonQuery();
                                }

                                // Update room status
                                query = "UPDATE Rooms SET Status = 'Occupied' WHERE RoomID = @RoomID";
                                using (var cmd = new MySqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@RoomID", _room["RoomID"]);
                                    cmd.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Patient assigned successfully!");
                                DialogResult = true;
                                Close();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error assigning patient: " + ex.Message);
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
            if (!dpAdmissionDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select admission date");
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