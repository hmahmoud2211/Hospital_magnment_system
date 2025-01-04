using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using Hospital_magnment_system.DataAccess;
using Hospital_magnment_system.Models;
using MySql.Data.MySqlClient;

namespace Hospital_magnment_system.Views
{
    public partial class AddDoctorFeeWindow : Window
    {
        public DoctorFeeItem Result { get; private set; }

        public AddDoctorFeeWindow()
        {
            try
            {
                InitializeComponent();
                Loaded += AddDoctorFeeWindow_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing window: " + ex.Message);
            }
        }

        private void AddDoctorFeeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDoctors();
                InitializeConsultationTypes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void InitializeConsultationTypes()
        {
            try
            {
                if (cmbConsultationType != null && cmbConsultationType.Items.Count > 0)
                {
                    cmbConsultationType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing consultation types: " + ex.Message);
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
                                   CONCAT(FirstName, ' ', LastName) as FullName,
                                   Specialization
                                   FROM Doctors 
                                   WHERE Status = 'Active'
                                   ORDER BY FirstName";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);

                        if (cmbDoctor != null)
                        {
                            cmbDoctor.ItemsSource = dt.DefaultView;
                            cmbDoctor.DisplayMemberPath = "FullName";
                            
                            if (dt.Rows.Count > 0)
                            {
                                cmbDoctor.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    var selectedDoctor = cmbDoctor.SelectedItem as DataRowView;
                    var selectedConsultation = (cmbConsultationType.SelectedItem as ComboBoxItem)?.Content?.ToString();

                    if (selectedDoctor != null && !string.IsNullOrEmpty(selectedConsultation))
                    {
                        Result = new DoctorFeeItem
                        {
                            DoctorID = Convert.ToInt32(selectedDoctor["DoctorID"]),
                            DoctorName = selectedDoctor["FullName"].ToString(),
                            ConsultationType = selectedConsultation,
                            Amount = decimal.Parse(txtFeeAmount.Text),
                            Notes = txtNotes.Text ?? string.Empty
                        };

                        DialogResult = true;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding doctor fee: " + ex.Message);
            }
        }

        private bool ValidateInput()
        {
            if (cmbDoctor?.SelectedItem == null)
            {
                MessageBox.Show("Please select a doctor");
                return false;
            }

            if (cmbConsultationType?.SelectedItem == null)
            {
                MessageBox.Show("Please select consultation type");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFeeAmount?.Text) || 
                !decimal.TryParse(txtFeeAmount.Text, out decimal amount) || 
                amount <= 0)
            {
                MessageBox.Show("Please enter a valid fee amount");
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error closing window: " + ex.Message);
            }
        }
    }
} 