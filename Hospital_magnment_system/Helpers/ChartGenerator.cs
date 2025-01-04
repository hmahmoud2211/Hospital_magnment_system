using System.Windows.Controls;
using System.Data;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;
using System.Linq;
using System.Windows;

namespace Hospital_magnment_system.Helpers
{
    public class ChartGenerator
    {
        public UIElement GenerateChart(ReportType reportType, DataTable data)
        {
            return reportType switch
            {
                ReportType.RevenueSummary => GenerateRevenueSummaryChart(data),
                ReportType.RoomOccupancy => GenerateRoomOccupancyChart(data),
                ReportType.PatientStatistics => GeneratePatientStatisticsChart(data),
                ReportType.DoctorPerformance => GenerateDoctorPerformanceChart(data),
                _ => new TextBlock { Text = "No chart available for this report type" }
            };
        }

        private CartesianChart GenerateRevenueSummaryChart(DataTable data)
        {
            var chart = new CartesianChart
            {
                Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Revenue",
                        Values = new ChartValues<decimal>(
                            data.AsEnumerable().Select(row => row.Field<decimal>("Revenue"))
                        )
                    },
                    new LineSeries
                    {
                        Title = "Bill Count",
                        Values = new ChartValues<int>(
                            data.AsEnumerable().Select(row => row.Field<int>("BillCount"))
                        )
                    }
                },
                AxisX = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Month",
                        Labels = data.AsEnumerable().Select(row => row.Field<string>("Month")).ToList()
                    }
                },
                Height = 400
            };

            return chart;
        }

        private PieChart GenerateRoomOccupancyChart(DataTable data)
        {
            var chart = new PieChart
            {
                Series = new SeriesCollection(
                    data.AsEnumerable().Select(row => new PieSeries
                    {
                        Title = row.Field<string>("RoomType"),
                        Values = new ChartValues<double> { row.Field<double>("OccupancyRate") },
                        DataLabels = true
                    })
                ),
                Height = 400
            };

            return chart;
        }

        private CartesianChart GeneratePatientStatisticsChart(DataTable data)
        {
            var chart = new CartesianChart
            {
                Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Admissions",
                        Values = new ChartValues<int>(
                            data.AsEnumerable().Select(row => row.Field<int>("Admissions"))
                        )
                    },
                    new LineSeries
                    {
                        Title = "Average Stay Duration",
                        Values = new ChartValues<double>(
                            data.AsEnumerable().Select(row => row.Field<double>("AvgStayDuration"))
                        )
                    }
                },
                AxisX = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Month",
                        Labels = data.AsEnumerable().Select(row => row.Field<string>("Month")).ToList()
                    }
                },
                Height = 400
            };

            return chart;
        }

        private CartesianChart GenerateDoctorPerformanceChart(DataTable data)
        {
            var chart = new CartesianChart
            {
                Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Total Appointments",
                        Values = new ChartValues<int>(
                            data.AsEnumerable().Select(row => row.Field<int>("TotalAppointments"))
                        )
                    },
                    new ColumnSeries
                    {
                        Title = "Completed Appointments",
                        Values = new ChartValues<int>(
                            data.AsEnumerable().Select(row => row.Field<int>("CompletedAppointments"))
                        )
                    }
                },
                AxisX = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Doctor",
                        Labels = data.AsEnumerable().Select(row => row.Field<string>("DoctorName")).ToList()
                    }
                },
                Height = 400
            };

            return chart;
        }
    }
} 