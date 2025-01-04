using System;
using System.Windows;
using System.Data;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;

namespace Hospital_magnment_system.Views
{
    public partial class ViewBillWindow : Window
    {
        private readonly DataRowView _bill;

        public ViewBillWindow(DataRowView bill)
        {
            InitializeComponent();
            _bill = bill;
            LoadBillDetails();
        }

        private void LoadBillDetails()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   b.*,
                                   CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                                   p.Phone, p.Address,
                                   CONCAT(d.FirstName, ' ', d.LastName) as DoctorName
                                   FROM Bills b
                                   JOIN Patients p ON b.PatientID = p.PatientID
                                   LEFT JOIN Doctors d ON b.DoctorID = d.DoctorID
                                   WHERE b.BillID = @BillID";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@BillID", _bill["BillID"]);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtBillNumber.Text = reader["BillNumber"].ToString();
                            txtBillDate.Text = Convert.ToDateTime(reader["BillDate"]).ToString("dd/MM/yyyy");
                            txtPatientName.Text = reader["PatientName"].ToString();
                            txtPatientContact.Text = reader["Phone"].ToString();
                            txtPatientAddress.Text = reader["Address"].ToString();
                            txtDoctorName.Text = reader["DoctorName"].ToString();
                            txtRoomCharges.Text = string.Format("{0:C2}", reader["RoomCharges"]);
                            txtMedicineCharges.Text = string.Format("{0:C2}", reader["MedicineCharges"]);
                            txtDoctorFees.Text = string.Format("{0:C2}", reader["DoctorFees"]);
                            txtOtherCharges.Text = string.Format("{0:C2}", reader["OtherCharges"]);
                            txtTotalAmount.Text = string.Format("{0:C2}", reader["TotalAmount"]);
                            txtStatus.Text = reader["Status"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bill details: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            // Implement printing functionality
            MessageBox.Show("Printing functionality to be implemented");
        }
    }
} 