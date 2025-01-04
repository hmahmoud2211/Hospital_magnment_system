using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using Hospital_magnment_system.DataAccess;
using Hospital_magnment_system.Models;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace Hospital_magnment_system.Views
{
    public partial class AddBillWindow : Window
    {
        private decimal _totalRoomCharges = 0;
        private decimal _totalMedicineCharges = 0;
        private decimal _totalDoctorFees = 0;
        private decimal _totalOtherCharges = 0;
        
        private ObservableCollection<MedicineItem> _medicines;
        private ObservableCollection<DoctorFeeItem> _doctorFees;

        public AddBillWindow()
        {
            try
            {
                InitializeComponent();
                InitializeCollections();
                Loaded += AddBillWindow_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing window: " + ex.Message);
            }
        }

        private void AddBillWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadPatients();
                UpdateBillSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void InitializeCollections()
        {
            try
            {
                _medicines = new ObservableCollection<MedicineItem>();
                _doctorFees = new ObservableCollection<DoctorFeeItem>();

                if (dgMedicines != null)
                {
                    dgMedicines.ItemsSource = _medicines;
                }
                if (dgDoctorFees != null)
                {
                    dgDoctorFees.ItemsSource = _doctorFees;
                }

                // Initialize other charges to 0
                if (txtOtherCharges != null)
                {
                    txtOtherCharges.Text = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing collections: " + ex.Message);
            }
        }

        private void LoadPatients()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT DISTINCT 
                                   p.PatientID,
                                   p.FirstName, 
                                   p.LastName,
                                   CONCAT(p.FirstName, ' ', p.LastName) as FullName,
                                   p.Phone, 
                                   p.Address
                                   FROM Patients p
                                   LEFT JOIN Admissions a ON p.PatientID = a.PatientID
                                   WHERE (a.DischargeDate IS NULL AND a.Status = 'Active')
                                   OR a.AdmissionID IS NULL
                                   ORDER BY p.FirstName";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            MessageBox.Show("No patients found in the database.");
                            return;
                        }

                        if (cmbPatient != null)
                        {
                            cmbPatient.ItemsSource = dt.DefaultView;
                            cmbPatient.DisplayMemberPath = "FullName";
                            cmbPatient.SelectedValuePath = "PatientID";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patients: " + ex.Message);
            }
        }

        private void cmbPatient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbPatient?.SelectedItem is DataRowView selectedPatient)
                {
                    if (txtPatientDetails != null)
                    {
                        txtPatientDetails.Text = $"Name: {selectedPatient["FullName"]}\n" +
                                               $"Phone: {selectedPatient["Phone"]}\n" +
                                               $"Address: {selectedPatient["Address"]}";
                    }
                    LoadPatientRoomDetails(selectedPatient["PatientID"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating patient details: " + ex.Message);
            }
        }

        private void LoadPatientRoomDetails(string patientId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   r.RoomNumber,
                                   r.RatePerDay,
                                   DATEDIFF(CURDATE(), ra.AdmissionDate) as Days,
                                   DATEDIFF(CURDATE(), ra.AdmissionDate) * r.RatePerDay as Total
                                   FROM RoomAllocations ra
                                   JOIN Rooms r ON ra.RoomID = r.RoomID
                                   WHERE ra.PatientID = @PatientID
                                   AND ra.Status = 'Active'";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PatientID", patientId);
                    
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgRoomCharges.ItemsSource = dt.DefaultView;

                    _totalRoomCharges = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        _totalRoomCharges += Convert.ToDecimal(row["Total"]);
                    }
                    UpdateRoomCharges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading room details: " + ex.Message);
            }
        }

        private void btnAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addMedicineWindow = new AddMedicineWindow();
                if (addMedicineWindow.ShowDialog() == true && addMedicineWindow.Result != null)
                {
                    _medicines.Add(addMedicineWindow.Result);
                    UpdateMedicineCharges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding medicine: " + ex.Message);
            }
        }

        private void btnRemoveMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (dgMedicines.SelectedItem is MedicineItem medicine)
            {
                _medicines.Remove(medicine);
                UpdateMedicineCharges();
            }
        }

        private void btnAddDoctorFee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addDoctorFeeWindow = new AddDoctorFeeWindow();
                if (addDoctorFeeWindow.ShowDialog() == true && addDoctorFeeWindow.Result != null)
                {
                    _doctorFees.Add(addDoctorFeeWindow.Result);
                    UpdateDoctorFees();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding doctor fee: " + ex.Message);
            }
        }

        private void btnRemoveDoctorFee_Click(object sender, RoutedEventArgs e)
        {
            if (dgDoctorFees.SelectedItem is DoctorFeeItem fee)
            {
                _doctorFees.Remove(fee);
                UpdateDoctorFees();
            }
        }

        private void txtOtherCharges_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtOtherCharges.Text, out decimal otherCharges))
            {
                _totalOtherCharges = otherCharges;
                UpdateBillSummary();
            }
        }

        private void UpdateRoomCharges()
        {
            txtTotalRoomCharges.Text = string.Format("{0:C2}", _totalRoomCharges);
            UpdateBillSummary();
        }

        private void UpdateMedicineCharges()
        {
            _totalMedicineCharges = 0;
            foreach (var medicine in _medicines)
            {
                _totalMedicineCharges += medicine.Total;
            }
            txtTotalMedicineCharges.Text = string.Format("{0:C2}", _totalMedicineCharges);
            UpdateBillSummary();
        }

        private void UpdateDoctorFees()
        {
            _totalDoctorFees = 0;
            foreach (var fee in _doctorFees)
            {
                _totalDoctorFees += fee.Amount;
            }
            txtTotalDoctorFees.Text = string.Format("{0:C2}", _totalDoctorFees);
            UpdateBillSummary();
        }

        private void UpdateBillSummary()
        {
            try
            {
                if (txtSummaryRoomCharges != null)
                    txtSummaryRoomCharges.Text = string.Format("{0:C2}", _totalRoomCharges);
                
                if (txtSummaryMedicineCharges != null)
                    txtSummaryMedicineCharges.Text = string.Format("{0:C2}", _totalMedicineCharges);
                
                if (txtSummaryDoctorFees != null)
                    txtSummaryDoctorFees.Text = string.Format("{0:C2}", _totalDoctorFees);
                
                if (txtSummaryOtherCharges != null)
                    txtSummaryOtherCharges.Text = string.Format("{0:C2}", _totalOtherCharges);
                
                decimal total = _totalRoomCharges + _totalMedicineCharges + 
                              _totalDoctorFees + _totalOtherCharges;
                
                if (txtSummaryTotal != null)
                    txtSummaryTotal.Text = string.Format("{0:C2}", total);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating bill summary: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                SaveBill();
            }
        }

        private bool ValidateInput()
        {
            if (cmbPatient.SelectedItem == null)
            {
                MessageBox.Show("Please select a patient");
                return false;
            }

            if (_totalRoomCharges + _totalMedicineCharges + 
                _totalDoctorFees + _totalOtherCharges <= 0)
            {
                MessageBox.Show("Total amount cannot be zero");
                return false;
            }

            return true;
        }

        private void SaveBill()
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
                            string billNumber = GenerateBillNumber();
                            
                            string billQuery = @"INSERT INTO Bills 
                                (BillNumber, PatientID, BillDate, RoomCharges, 
                                 MedicineCharges, DoctorFees, OtherCharges, Status) 
                                VALUES 
                                (@BillNumber, @PatientID, @BillDate, @RoomCharges,
                                 @MedicineCharges, @DoctorFees, @OtherCharges, 'Pending')";

                            using (var cmd = new MySqlCommand(billQuery, conn, transaction))
                            {
                                var selectedPatient = (DataRowView)cmbPatient.SelectedItem;
                                cmd.Parameters.AddWithValue("@BillNumber", billNumber);
                                cmd.Parameters.AddWithValue("@PatientID", selectedPatient["PatientID"]);
                                cmd.Parameters.AddWithValue("@BillDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@RoomCharges", _totalRoomCharges);
                                cmd.Parameters.AddWithValue("@MedicineCharges", _totalMedicineCharges);
                                cmd.Parameters.AddWithValue("@DoctorFees", _totalDoctorFees);
                                cmd.Parameters.AddWithValue("@OtherCharges", _totalOtherCharges);
                                
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            MessageBox.Show($"Bill {billNumber} saved successfully!");
                            DialogResult = true;
                            Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("Error saving bill: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GenerateBillNumber()
        {
            return $"BILL-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 