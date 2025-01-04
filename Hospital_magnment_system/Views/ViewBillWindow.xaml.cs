using System;
using System.Windows;
using System.Data;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Documents; // Correct namespace for printing
using System.Windows.Controls;

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
                        cmd.Parameters.AddWithValue("@BillNumber", _bill["BillNumber"]);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Update bill details
                                txtBillNumber.Text = reader["BillNumber"].ToString();
                                txtBillDate.Text = Convert.ToDateTime(reader["BillDate"]).ToString("dd/MM/yyyy");
                                txtPatientName.Text = reader["PatientName"].ToString();
                                txtPatientContact.Text = reader["Phone"].ToString();
                                txtPatientAddress.Text = reader["Address"].ToString();
                                
                                // Update charges
                                txtRoomCharges.Text = string.Format("{0:C2}", reader["RoomCharges"]);
                                txtMedicineCharges.Text = string.Format("{0:C2}", reader["MedicineCharges"]);
                                txtDoctorFees.Text = string.Format("{0:C2}", reader["DoctorFees"]);
                                txtOtherCharges.Text = string.Format("{0:C2}", reader["OtherCharges"]);

                                // Calculate and display total
                                decimal totalAmount = Convert.ToDecimal(reader["RoomCharges"]) +
                                                   Convert.ToDecimal(reader["MedicineCharges"]) +
                                                   Convert.ToDecimal(reader["DoctorFees"]) +
                                                   Convert.ToDecimal(reader["OtherCharges"]);
                                txtTotalAmount.Text = string.Format("{0:C2}", totalAmount);

                                // Update status
                                txtStatus.Text = reader["Status"].ToString();
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create the print dialog
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Create the FlowDocument
                    FlowDocument doc = new FlowDocument();
                    doc.PagePadding = new Thickness(50);
                    doc.ColumnWidth = printDialog.PrintableAreaWidth;

                    // Add content to the document
                    doc.Blocks.Add(CreateHeader());
                    doc.Blocks.Add(CreateBillInfo());
                    doc.Blocks.Add(CreatePatientInfo());
                    doc.Blocks.Add(CreateChargesTable());
                    
                    // Create IDocumentPaginatorSource from FlowDocument
                    IDocumentPaginatorSource idpSource = doc;

                    // Print the document
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Bill Print");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing bill: " + ex.Message);
            }
        }

        private Section CreateHeader()
        {
            Section section = new Section();
            
            Paragraph header = new Paragraph();
            header.TextAlignment = TextAlignment.Center;
            
            Run hospitalName = new Run("Hospital Management System\n");
            hospitalName.FontSize = 20;
            hospitalName.FontWeight = FontWeights.Bold;
            
            Run invoice = new Run("INVOICE\n");
            invoice.FontSize = 16;
            invoice.FontWeight = FontWeights.Bold;
            
            header.Inlines.Add(hospitalName);
            header.Inlines.Add(invoice);
            
            section.Blocks.Add(header);
            return section;
        }

        private Section CreateBillInfo()
        {
            Section section = new Section();
            
            Table table = new Table();
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            
            TableRow row1 = new TableRow();
            row1.Cells.Add(CreateTableCell("Bill Number:", true));
            row1.Cells.Add(CreateTableCell(txtBillNumber.Text));
            
            TableRow row2 = new TableRow();
            row2.Cells.Add(CreateTableCell("Bill Date:", true));
            row2.Cells.Add(CreateTableCell(txtBillDate.Text));
            
            TableRow row3 = new TableRow();
            row3.Cells.Add(CreateTableCell("Status:", true));
            row3.Cells.Add(CreateTableCell(txtStatus.Text));
            
            table.RowGroups.Add(new TableRowGroup());
            table.RowGroups[0].Rows.Add(row1);
            table.RowGroups[0].Rows.Add(row2);
            table.RowGroups[0].Rows.Add(row3);
            
            section.Blocks.Add(table);
            return section;
        }

        private Section CreatePatientInfo()
        {
            Section section = new Section();
            
            Paragraph header = new Paragraph(new Run("Patient Information"));
            header.FontWeight = FontWeights.Bold;
            header.Margin = new Thickness(0, 20, 0, 10);
            
            Table table = new Table();
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            
            TableRow row1 = new TableRow();
            row1.Cells.Add(CreateTableCell("Name:", true));
            row1.Cells.Add(CreateTableCell(txtPatientName.Text));
            
            TableRow row2 = new TableRow();
            row2.Cells.Add(CreateTableCell("Contact:", true));
            row2.Cells.Add(CreateTableCell(txtPatientContact.Text));
            
            TableRow row3 = new TableRow();
            row3.Cells.Add(CreateTableCell("Address:", true));
            row3.Cells.Add(CreateTableCell(txtPatientAddress.Text));
            
            table.RowGroups.Add(new TableRowGroup());
            table.RowGroups[0].Rows.Add(row1);
            table.RowGroups[0].Rows.Add(row2);
            table.RowGroups[0].Rows.Add(row3);
            
            section.Blocks.Add(header);
            section.Blocks.Add(table);
            return section;
        }

        private Section CreateChargesTable()
        {
            Section section = new Section();
            
            Paragraph header = new Paragraph(new Run("Charges Breakdown"));
            header.FontWeight = FontWeights.Bold;
            header.Margin = new Thickness(0, 20, 0, 10);
            
            Table table = new Table();
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            
            TableRow row1 = new TableRow();
            row1.Cells.Add(CreateTableCell("Room Charges:", true));
            row1.Cells.Add(CreateTableCell(txtRoomCharges.Text));
            
            TableRow row2 = new TableRow();
            row2.Cells.Add(CreateTableCell("Medicine Charges:", true));
            row2.Cells.Add(CreateTableCell(txtMedicineCharges.Text));
            
            TableRow row3 = new TableRow();
            row3.Cells.Add(CreateTableCell("Doctor Fees:", true));
            row3.Cells.Add(CreateTableCell(txtDoctorFees.Text));
            
            TableRow row4 = new TableRow();
            row4.Cells.Add(CreateTableCell("Other Charges:", true));
            row4.Cells.Add(CreateTableCell(txtOtherCharges.Text));
            
            TableRow row5 = new TableRow();
            row5.Cells.Add(CreateTableCell("Total Amount:", true));
            row5.Cells.Add(CreateTableCell(txtTotalAmount.Text));
            row5.FontWeight = FontWeights.Bold;
            
            table.RowGroups.Add(new TableRowGroup());
            table.RowGroups[0].Rows.Add(row1);
            table.RowGroups[0].Rows.Add(row2);
            table.RowGroups[0].Rows.Add(row3);
            table.RowGroups[0].Rows.Add(row4);
            table.RowGroups[0].Rows.Add(row5);
            
            section.Blocks.Add(header);
            section.Blocks.Add(table);
            return section;
        }

        private TableCell CreateTableCell(string text, bool isBold = false)
        {
            TableCell cell = new TableCell(new Paragraph(new Run(text)));
            if (isBold)
            {
                cell.FontWeight = FontWeights.Bold;
            }
            cell.Padding = new Thickness(5);
            return cell;
        }
    }
} 