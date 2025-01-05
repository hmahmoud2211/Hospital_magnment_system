using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class Rooms : Page
    {
        public Rooms()
        {
            InitializeComponent();
            InitializePage();
        }

        private void InitializePage()
        {
            LoadFloors();
            cmbRoomType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbFloor.SelectedIndex = 0;
            LoadRooms();
        }

        private void LoadFloors()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT DISTINCT Floor FROM Rooms ORDER BY Floor";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataRow allRow = dt.NewRow();
                    allRow["Floor"] = DBNull.Value;
                    dt.Rows.InsertAt(allRow, 0);

                    cmbFloor.ItemsSource = dt.DefaultView;
                    cmbFloor.DisplayMemberPath = "Floor";
                    cmbFloor.SelectedValuePath = "Floor";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading floors: " + ex.Message);
            }
        }

        private void LoadRooms()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT RoomID, 
                                   RoomNumber,
                                   RoomType,
                                   Floor,
                                   Status,
                                   RatePerDay
                                   FROM Rooms 
                                   WHERE 1=1";

                    if (cmbRoomType.SelectedIndex > 0)
                    {
                        query += " AND RoomType = @RoomType";
                    }
                    if (cmbStatus.SelectedIndex > 0)
                    {
                        query += " AND Status = @Status";
                    }
                    if (cmbFloor.SelectedValue != null && cmbFloor.SelectedValue != DBNull.Value)
                    {
                        query += " AND Floor = @Floor";
                    }

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        if (cmbRoomType.SelectedIndex > 0)
                        {
                            cmd.Parameters.AddWithValue("@RoomType", ((ComboBoxItem)cmbRoomType.SelectedItem).Content.ToString());
                        }
                        if (cmbStatus.SelectedIndex > 0)
                        {
                            cmd.Parameters.AddWithValue("@Status", ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString());
                        }
                        if (cmbFloor.SelectedValue != null && cmbFloor.SelectedValue != DBNull.Value)
                        {
                            cmd.Parameters.AddWithValue("@Floor", cmbFloor.SelectedValue);
                        }

                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgRooms.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading rooms: " + ex.Message);
            }
        }

        private void LoadRoomDetails(DataRowView room)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                                   ra.AdmissionDate,
                                   CONCAT(d.FirstName, ' ', d.LastName) as DoctorName,
                                   DATEDIFF(CURRENT_DATE, ra.AdmissionDate) as TotalDays,
                                   r.RatePerDay,
                                   DATEDIFF(CURRENT_DATE, ra.AdmissionDate) * r.RatePerDay as CurrentCharges
                                   FROM Rooms r
                                   LEFT JOIN RoomAllocations ra ON r.RoomID = ra.RoomID AND ra.Status = 'Active'
                                   LEFT JOIN Patients p ON ra.PatientID = p.PatientID
                                   LEFT JOIN Doctors d ON ra.DoctorID = d.DoctorID
                                   WHERE r.RoomID = @RoomID";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoomID", room["RoomID"]);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtCurrentPatient.Text = reader["PatientName"].ToString() ?? "No patient";
                                txtAdmissionDate.Text = reader["AdmissionDate"] != DBNull.Value ? 
                                    Convert.ToDateTime(reader["AdmissionDate"]).ToString("dd/MM/yyyy") : "-";
                                txtAttendingDoctor.Text = reader["DoctorName"]?.ToString() ?? "-";
                                txtTotalDays.Text = reader["TotalDays"] != DBNull.Value ? 
                                    reader["TotalDays"].ToString() : "0";
                                txtCurrentCharges.Text = reader["CurrentCharges"] != DBNull.Value ? 
                                    string.Format("{0:C2}", reader["CurrentCharges"]) : "$0.00";

                                // Show discharge button only if there's an active patient
                                btnDischarge.Visibility = !string.IsNullOrEmpty(reader["PatientName"]?.ToString()) ? 
                                    Visibility.Visible : Visibility.Collapsed;
                            }
                            else
                            {
                                ClearRoomDetails();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading room details: " + ex.Message);
                ClearRoomDetails();
            }
        }

        private void ClearRoomDetails()
        {
            txtCurrentPatient.Text = "No patient";
            txtAdmissionDate.Text = "-";
            txtAttendingDoctor.Text = "-";
            txtTotalDays.Text = "0";
            txtCurrentCharges.Text = "$0.00";
            btnDischarge.Visibility = Visibility.Collapsed;
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadRooms();
        }

        private void dgRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRoom = dgRooms.SelectedItem as DataRowView;
            if (selectedRoom != null)
            {
                LoadRoomDetails(selectedRoom);
            }
            else
            {
                ClearRoomDetails();
            }
        }

        private void btnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            var addRoomWindow = new AddRoomWindow();
            if (addRoomWindow.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var room = ((FrameworkElement)sender).DataContext as DataRowView;
            if (room != null)
            {
                var editRoomWindow = new AddRoomWindow(room);
                if (editRoomWindow.ShowDialog() == true)
                {
                    LoadRooms();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var room = ((FrameworkElement)sender).DataContext as DataRowView;
            if (room != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this room?",
                    "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DatabaseConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM Rooms WHERE RoomID = @RoomID";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@RoomID", room["RoomID"]);
                            cmd.ExecuteNonQuery();
                            LoadRooms();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting room: " + ex.Message);
                    }
                }
            }
        }

        private void btnDischarge_Click(object sender, RoutedEventArgs e)
        {
            var selectedRoom = dgRooms.SelectedItem as DataRowView;
            if (selectedRoom != null)
            {
                if (MessageBox.Show("Are you sure you want to discharge the patient?",
                    "Confirm Discharge", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = DatabaseConnection.GetConnection())
                        {
                            conn.Open();
                            string query = @"UPDATE RoomAllocations 
                                           SET Status = 'Discharged', 
                                           DischargeDate = CURRENT_TIMESTAMP 
                                           WHERE RoomID = @RoomID 
                                           AND Status = 'Active'";

                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@RoomID", selectedRoom["RoomID"]);
                            cmd.ExecuteNonQuery();

                            // Update room status
                            query = "UPDATE Rooms SET Status = 'Available' WHERE RoomID = @RoomID";
                            cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@RoomID", selectedRoom["RoomID"]);
                            cmd.ExecuteNonQuery();

                            LoadRooms();
                            ClearRoomDetails();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error discharging patient: " + ex.Message);
                    }
                }
            }
        }

        private void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedRoom = ((FrameworkElement)sender).DataContext as DataRowView;
                if (selectedRoom != null)
                {
                    if (selectedRoom["Status"].ToString() != "Available")
                    {
                        MessageBox.Show("Only available rooms can be assigned to patients.");
                        return;
                    }

                    var assignWindow = new AssignPatientWindow(selectedRoom);
                    if (assignWindow.ShowDialog() == true)
                    {
                        LoadRooms();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error assigning room: " + ex.Message);
            }
        }
    }
} 