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
                // Set up event handlers after controls are initialized
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
                    string query = @"SELECT 
                                   b.BillNumber as 'Bill #',
                                   DATE_FORMAT(b.BillDate, '%Y-%m-%d') as Date,
                                   CONCAT(p.FirstName, ' ', p.LastName) as Patient,
                                   (b.RoomCharges + b.MedicineCharges + b.DoctorFees + b.OtherCharges) as 'Total Amount',
                                   b.Status,
                                   b.BillNumber as BillID
                                   FROM Bills b
                                   INNER JOIN Patients p ON b.PatientID = p.PatientID
                                   WHERE DATE(b.BillDate) BETWEEN @StartDate AND @EndDate
                                   AND (@Status = 'All' OR b.Status = @Status)
                                   ORDER BY b.BillDate DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate?.Date ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate?.Date ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedValue?.ToString() ?? "All");

                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
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
                                   CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                                   b.RoomCharges,
                                   b.MedicineCharges,
                                   b.DoctorFees,
                                   b.OtherCharges,
                                   (b.RoomCharges + b.MedicineCharges + b.DoctorFees + b.OtherCharges) as TotalAmount,
                                   b.Status
                                   FROM Bills b
                                   INNER JOIN Patients p ON b.PatientID = p.PatientID
                                   WHERE b.BillNumber = @BillNumber";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BillNumber", billNumber);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtPatientName.Text = reader["PatientName"].ToString();
                                txtBillDate.Text = Convert.ToDateTime(reader["BillDate"]).ToString("MM/dd/yyyy");
                                txtRoomCharges.Text = string.Format("{0:C2}", reader["RoomCharges"]);
                                txtMedicineCharges.Text = string.Format("{0:C2}", reader["MedicineCharges"]);
                                txtDoctorFees.Text = string.Format("{0:C2}", reader["DoctorFees"]);
                                txtOtherCharges.Text = string.Format("{0:C2}", reader["OtherCharges"]);
                                txtTotalAmount.Text = string.Format("{0:C2}", reader["TotalAmount"]);

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
            if (!IsInitialized || sender == null) return;
            
            if (dpStartDate?.SelectedDate != null && dpEndDate?.SelectedDate != null)
            {
                LoadBills();
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized || sender == null) return;
            
            if (cmbStatus?.SelectedItem != null)
            {
                LoadBills();
            }
        }

        private void dgBills_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedBill = dgBills.SelectedItem as DataRowView;
                if (selectedBill != null)
                {
                    // Get the bill number from the selected row
                    string billNumber = selectedBill["Bill #"].ToString();
                    LoadBillDetails(billNumber);
                }
                else
                {
                    ClearBillDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bill details: " + ex.Message);
            }
        }

        private void btnNewBill_Click(object sender, RoutedEventArgs e)
        {
            var addBillWindow = new AddBillWindow();
            if (addBillWindow.ShowDialog() == true)
            {
                LoadBills();
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

                string billNumber = selectedBill["Bill #"].ToString();

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
                catch (Exception ex)
                {
                MessageBox.Show("Error marking bill as paid: " + ex.Message);
            }
        }
    }
} 