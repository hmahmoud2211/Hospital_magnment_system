using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;

namespace Hospital_magnment_system.Views
{
    public partial class Reports : Page
    {
        public Reports()
        {
            InitializeComponent();
            Loaded += Reports_Loaded;
        }

        private void Reports_Loaded(object sender, RoutedEventArgs e)
        {
            dpStartDate.SelectedDate = DateTime.Today.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Today;
        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (GetSelectedReportType())
                {
                    case "Revenue Summary":
                        GenerateRevenueSummary();
                        break;
                    case "Billing Status":
                        GenerateBillingStatus();
                        break;
                    case "Outstanding Payments":
                        GenerateOutstandingPayments();
                        break;
                    // Add other report types here
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + ex.Message);
            }
        }

        private void GenerateRevenueSummary()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   b.BillDate,
                                   COUNT(*) as BillCount,
                                   SUM(b.RoomCharges) as RoomCharges,
                                   SUM(b.MedicineCharges) as MedicineCharges,
                                   SUM(b.DoctorFees) as DoctorFees,
                                   SUM(b.OtherCharges) as OtherCharges,
                                   SUM(b.RoomCharges + b.MedicineCharges + 
                                       b.DoctorFees + b.OtherCharges) as TotalRevenue,
                                   b.Status
                                   FROM Bills b
                                   WHERE b.BillDate BETWEEN @StartDate AND @EndDate
                                   GROUP BY DATE(b.BillDate), b.Status
                                   ORDER BY b.BillDate DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate ?? DateTime.Today);

                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgReportData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating revenue summary: " + ex.Message);
            }
        }

        private void GenerateBillingStatus()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   b.Status,
                                   COUNT(*) as BillCount,
                                   SUM(b.RoomCharges + b.MedicineCharges + 
                                       b.DoctorFees + b.OtherCharges) as TotalAmount
                                   FROM Bills b
                                   WHERE b.BillDate BETWEEN @StartDate AND @EndDate
                                   GROUP BY b.Status";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate ?? DateTime.Today);

                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgReportData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating billing status: " + ex.Message);
            }
        }

        private void GenerateOutstandingPayments()
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
                                   (b.RoomCharges + b.MedicineCharges + 
                                    b.DoctorFees + b.OtherCharges) as TotalAmount
                                   FROM Bills b
                                   INNER JOIN Patients p ON b.PatientID = p.PatientID
                                   WHERE b.Status = 'Pending'
                                   AND b.BillDate BETWEEN @StartDate AND @EndDate
                                   ORDER BY b.BillDate DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate ?? DateTime.Today);

                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgReportData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating outstanding payments: " + ex.Message);
            }
        }

        private void ReportType_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            btnGenerateReport_Click(sender, e);
        }

        private string GetSelectedReportType()
        {
            if (btnRevenueSummary?.IsChecked == true) return "Revenue Summary";
            if (btnBillingStatus?.IsChecked == true) return "Billing Status";
            if (btnOutstandingPayments?.IsChecked == true) return "Outstanding Payments";
            if (btnRoomOccupancy?.IsChecked == true) return "Room Occupancy";
            if (btnPatientStatistics?.IsChecked == true) return "Patient Statistics";
            if (btnDoctorPerformance?.IsChecked == true) return "Doctor Performance";
            
            throw new Exception("Please select a report type");
        }

        private void btnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            // Implement Excel export
            MessageBox.Show("Excel export functionality to be implemented");
        }

        private void btnExportToPDF_Click(object sender, RoutedEventArgs e)
        {
            // Implement PDF export
            MessageBox.Show("PDF export functionality to be implemented");
        }

        private void btnRoomOccupancy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GenerateRoomOccupancyReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating room occupancy report: " + ex.Message);
            }
        }

        private void GenerateRoomOccupancyReport()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   r.RoomNumber,
                                   r.RatePerDay,
                                   r.Status,
                                   CASE 
                                       WHEN r.Status = 'Occupied' THEN 
                                           CONCAT(p.FirstName, ' ', p.LastName)
                                       ELSE 'N/A'
                                   END as CurrentPatient,
                                   CASE 
                                       WHEN r.Status = 'Occupied' THEN 
                                           ra.AdmissionDate
                                       ELSE NULL
                                   END as OccupiedSince
                                   FROM Rooms r
                                   LEFT JOIN RoomAllocations ra ON r.RoomID = ra.RoomID 
                                        AND ra.Status = 'Active'
                                   LEFT JOIN Patients p ON ra.PatientID = p.PatientID
                                   ORDER BY r.RoomNumber";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgReportData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating room occupancy report: " + ex.Message);
            }
        }

        private void btnPatientStatistics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GeneratePatientStatisticsReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating patient statistics: " + ex.Message);
            }
        }

        private void GeneratePatientStatisticsReport()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   COUNT(DISTINCT p.PatientID) as TotalPatients,
                                   COUNT(DISTINCT CASE WHEN a.Status = 'Active' THEN p.PatientID END) as CurrentlyAdmitted,
                                   COUNT(DISTINCT CASE WHEN b.Status = 'Pending' THEN p.PatientID END) as PendingBills,
                                   AVG(DATEDIFF(COALESCE(a.DischargeDate, CURRENT_DATE), a.AdmissionDate)) as AvgStayDuration
                                   FROM Patients p
                                   LEFT JOIN Admissions a ON p.PatientID = a.PatientID
                                   LEFT JOIN Bills b ON p.PatientID = b.PatientID
                                   WHERE a.AdmissionDate BETWEEN @StartDate AND @EndDate
                                   OR a.AdmissionDate IS NULL";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate ?? DateTime.Today);

                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgReportData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating patient statistics: " + ex.Message);
            }
        }

        private void btnDoctorPerformance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GenerateDoctorPerformanceReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating doctor performance report: " + ex.Message);
            }
        }

        private void GenerateDoctorPerformanceReport()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                   CONCAT(d.FirstName, ' ', d.LastName) as DoctorName,
                                   d.Specialization,
                                   COUNT(DISTINCT a.PatientID) as TotalPatients,
                                   SUM(b.DoctorFees) as TotalFees,
                                   COUNT(DISTINCT CASE WHEN a.Status = 'Active' THEN a.PatientID END) as CurrentPatients
                                   FROM Doctors d
                                   LEFT JOIN Admissions a ON d.DoctorID = a.DoctorID
                                   LEFT JOIN Bills b ON a.PatientID = b.PatientID
                                   WHERE a.AdmissionDate BETWEEN @StartDate AND @EndDate
                                   GROUP BY d.DoctorID, d.FirstName, d.LastName, d.Specialization
                                   ORDER BY TotalPatients DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@EndDate", dpEndDate.SelectedDate ?? DateTime.Today);

                        var adapter = new MySqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgReportData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating doctor performance report: " + ex.Message);
            }
        }
    }
} 
