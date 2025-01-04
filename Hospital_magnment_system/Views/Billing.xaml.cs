using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using Hospital_magnment_system.DataAccess;
using Hospital_magnment_system.Models;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace Hospital_magnment_system.Views
{
    public partial class Billing : Page
    {
        public Billing()
        {
            InitializeComponent();
            Loaded += Billing_Loaded;
        }

        private void Billing_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set up event handlers
                dpStartDate.SelectedDateChanged += DateRange_Changed;
                dpEndDate.SelectedDateChanged += DateRange_Changed;
                cmbStatus.SelectionChanged += Filter_Changed;
                dgBills.SelectionChanged += dgBills_SelectionChanged;

                // Initialize values
                dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
                dpEndDate.SelectedDate = DateTime.Today;
                cmbStatus.SelectedIndex = 0;

                // Load initial data
                LoadBills();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing page: " + ex.Message);
            }
        }

        private void LoadBills()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    // Simplified query for testing
                    string query = @"SELECT 
                                   b.*, 
                                   CONCAT(p.FirstName, ' ', p.LastName) as PatientName
                                   FROM Bills b
                                   INNER JOIN Patients p ON b.PatientID = p.PatientID";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);

                        // Debug information
                        MessageBox.Show($"Date Range: {dpStartDate.SelectedDate:dd/MM/yyyy} to {dpEndDate.SelectedDate:dd/MM/yyyy}\n" +
                                      $"Status Filter: {cmbStatus.SelectedValue?.ToString() ?? "All"}\n" +
                                      $"Found {dt.Rows.Count} bills");

                        dgBills.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bills: " + ex.Message);
            }
        }

        private void LoadBillDetails(string billNumber)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   b.BillNumber,
                                   b.BillDate,
                                   b.RoomCharges,
                                   b.MedicineCharges,
                                   b.DoctorFees,
                                   b.OtherCharges,
                                   b.Status,
                                   CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                                   p.Phone, 
                                   p.Address
                                   FROM Bills b
                                   JOIN Patients p ON b.PatientID = p.PatientID
                                   WHERE b.BillNumber = @BillNumber";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BillNumber", billNumber);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Update bill details panel
                                txtPatientName.Text = reader["PatientName"].ToString();
                                txtBillDate.Text = Convert.ToDateTime(reader["BillDate"]).ToString("dd/MM/yyyy");
                                txtRoomCharges.Text = string.Format("{0:C2}", reader["RoomCharges"]);
                                txtMedicineCharges.Text = string.Format("{0:C2}", reader["MedicineCharges"]);
                                txtDoctorFees.Text = string.Format("{0:C2}", reader["DoctorFees"]);
                                txtOtherCharges.Text = string.Format("{0:C2}", reader["OtherCharges"]);
                                
                                decimal totalAmount = Convert.ToDecimal(reader["RoomCharges"]) +
                                                   Convert.ToDecimal(reader["MedicineCharges"]) +
                                                   Convert.ToDecimal(reader["DoctorFees"]) +
                                                   Convert.ToDecimal(reader["OtherCharges"]);
                                txtTotalAmount.Text = string.Format("{0:C2}", totalAmount);

                                // Enable/disable Mark as Paid button based on status
                                btnMarkAsPaid.IsEnabled = reader["Status"].ToString() == "Pending";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bill details: " + ex.Message);
            }
        }

        private void ClearBillDetails()
        {
            txtPatientName.Text = "-";
            txtBillDate.Text = "-";
            txtRoomCharges.Text = "-";
            txtMedicineCharges.Text = "-";
            txtDoctorFees.Text = "-";
            txtOtherCharges.Text = "-";
            txtTotalAmount.Text = "-";
            btnMarkAsPaid.IsEnabled = false;
        }

        private void DateRange_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadBills();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadBills();
        }

        private void dgBills_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBill = dgBills.SelectedItem as DataRowView;
            if (selectedBill != null)
            {
                LoadBillDetails(selectedBill["BillNumber"].ToString());
            }
        }

        private void btnNewBill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addBillWindow = new AddBillWindow();
                if (addBillWindow.ShowDialog() == true)
                {
                    LoadBills(); // Refresh the grid after adding a new bill
                    ClearBillDetails(); // Clear the details panel
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating new bill: " + ex.Message);
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            var bill = ((FrameworkElement)sender).DataContext as DataRowView;
            if (bill != null)
            {
                var viewBillWindow = new ViewBillWindow(bill);
                viewBillWindow.ShowDialog();
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var bill = ((FrameworkElement)sender).DataContext as DataRowView;
            if (bill != null)
            {
                // Implement printing functionality
                MessageBox.Show("Printing functionality to be implemented");
            }
        }

        private void btnMarkAsPaid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedBill = dgBills.SelectedItem as DataRowView;
                if (selectedBill == null) return;

                string billNumber = selectedBill["BillNumber"].ToString();

                if (MessageBox.Show("Are you sure you want to mark this bill as paid?", 
                    "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = DatabaseConnection.GetConnection())
                    {
                        conn.Open();
                        string query = "UPDATE Bills SET Status = 'Paid' WHERE BillNumber = @BillNumber";

                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@BillNumber", billNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Bill marked as paid successfully!");
                    LoadBills(); // Refresh the grid
                    LoadBillDetails(billNumber); // Refresh details
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error marking bill as paid: " + ex.Message);
            }
        }
    }
} 