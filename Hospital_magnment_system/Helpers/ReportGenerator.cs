using System;
using System.Data;
using MySql.Data.MySqlClient;
using Hospital_magnment_system.DataAccess;

namespace Hospital_magnment_system.Helpers
{
    public class ReportGenerator
    {
        public DataTable GenerateReport(ReportType reportType, DateTime startDate, DateTime endDate)
        {
            string query = GetReportQuery(reportType);
            return ExecuteReport(query, startDate, endDate);
        }

        public bool IsChartReport(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.RevenueSummary => true,
                ReportType.RoomOccupancy => true,
                ReportType.PatientStatistics => true,
                ReportType.DoctorPerformance => true,
                _ => false
            };
        }

        private string GetReportQuery(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.RevenueSummary => @"
                    SELECT 
                        DATE_FORMAT(BillDate, '%Y-%m') as Month,
                        SUM(TotalAmount) as Revenue,
                        COUNT(*) as BillCount
                    FROM Bills
                    WHERE BillDate BETWEEN @StartDate AND @EndDate
                    GROUP BY DATE_FORMAT(BillDate, '%Y-%m')
                    ORDER BY Month",

                ReportType.BillingStatus => @"
                    SELECT 
                        b.BillNumber,
                        CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                        b.BillDate,
                        b.TotalAmount,
                        b.Status
                    FROM Bills b
                    JOIN Patients p ON b.PatientID = p.PatientID
                    WHERE b.BillDate BETWEEN @StartDate AND @EndDate
                    ORDER BY b.BillDate DESC",

                ReportType.OutstandingPayments => @"
                    SELECT 
                        b.BillNumber,
                        CONCAT(p.FirstName, ' ', p.LastName) as PatientName,
                        b.BillDate,
                        b.TotalAmount,
                        DATEDIFF(CURRENT_DATE, b.BillDate) as DaysOutstanding
                    FROM Bills b
                    JOIN Patients p ON b.PatientID = p.PatientID
                    WHERE b.Status = 'Pending'
                    AND b.BillDate BETWEEN @StartDate AND @EndDate
                    ORDER BY DaysOutstanding DESC",

                ReportType.RoomOccupancy => @"
                    SELECT 
                        r.RoomType,
                        COUNT(*) as TotalRooms,
                        SUM(CASE WHEN r.Status = 'Occupied' THEN 1 ELSE 0 END) as OccupiedRooms,
                        (SUM(CASE WHEN r.Status = 'Occupied' THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) as OccupancyRate
                    FROM Rooms r
                    GROUP BY r.RoomType",

                ReportType.PatientStatistics => @"
                    SELECT 
                        DATE_FORMAT(AdmissionDate, '%Y-%m') as Month,
                        COUNT(*) as Admissions,
                        AVG(DATEDIFF(COALESCE(DischargeDate, CURRENT_DATE), AdmissionDate)) as AvgStayDuration
                    FROM RoomAllocations
                    WHERE AdmissionDate BETWEEN @StartDate AND @EndDate
                    GROUP BY DATE_FORMAT(AdmissionDate, '%Y-%m')
                    ORDER BY Month",

                ReportType.DoctorPerformance => @"
                    SELECT 
                        CONCAT(d.FirstName, ' ', d.LastName) as DoctorName,
                        COUNT(a.AppointmentID) as TotalAppointments,
                        SUM(CASE WHEN a.Status = 'Completed' THEN 1 ELSE 0 END) as CompletedAppointments,
                        AVG(CASE WHEN a.Status = 'Completed' THEN 1 ELSE 0 END) * 100 as CompletionRate
                    FROM Doctors d
                    LEFT JOIN Appointments a ON d.DoctorID = a.DoctorID
                    AND a.AppointmentDate BETWEEN @StartDate AND @EndDate
                    GROUP BY d.DoctorID, d.FirstName, d.LastName
                    ORDER BY CompletedAppointments DESC",

                _ => throw new ArgumentException("Invalid report type")
            };
        }

        private DataTable ExecuteReport(string query, DateTime startDate, DateTime endDate)
        {
            using var conn = DatabaseConnection.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            var adapter = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }
    }
} 