using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.Models;

namespace Hospital_magnment_system.Views
{
    public partial class AddMedicineWindow : Window
    {
        public MedicineItem Result { get; private set; }

        public AddMedicineWindow()
        {
            InitializeComponent();
            txtQuantity.TextChanged += UpdateTotal;
            txtPrice.TextChanged += UpdateTotal;
        }

        private void UpdateTotal(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(txtQuantity.Text, out int quantity) && 
                decimal.TryParse(txtPrice.Text, out decimal price))
            {
                txtTotal.Text = string.Format("{0:C2}", quantity * price);
            }
            else
            {
                txtTotal.Text = "$0.00";
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                Result = new MedicineItem
                {
                    Name = txtMedicineName.Text,
                    Quantity = int.Parse(txtQuantity.Text),
                    Price = decimal.Parse(txtPrice.Text)
                };

                DialogResult = true;
                Close();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtMedicineName.Text))
            {
                MessageBox.Show("Please enter medicine name");
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity");
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price");
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 