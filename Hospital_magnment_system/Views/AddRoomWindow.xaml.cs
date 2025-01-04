using System;
using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.DataAccess;
using MySql.Data.MySqlClient;
using System.Data;

namespace Hospital_magnment_system.Views
{
    public partial class AddRoomWindow : Window
    {
        private readonly DataRowView _roomToEdit;
        private bool _isEditMode;

        public AddRoomWindow()
        {
            InitializeComponent();
            cmbStatus.SelectedIndex = 0;
            cmbRoomType.SelectedIndex = 0;
        }

        public AddRoomWindow(DataRowView room)
        {
            InitializeComponent();
            _roomToEdit = room;
            _isEditMode = true;
            LoadRoomData();
        }

        private void LoadRoomData()
        {
            Title = "Edit Room";
            txtRoomNumber.Text = _roomToEdit["RoomNumber"].ToString();
            txtFloor.Text = _roomToEdit["Floor"].ToString();
            txtPricePerDay.Text = _roomToEdit["PricePerDay"].ToString();
            txtDescription.Text = _roomToEdit["Description"].ToString();

            // Set Room Type
            foreach (ComboBoxItem item in cmbRoomType.Items)
            {
                if (item.Content.ToString() == _roomToEdit["RoomType"].ToString())
                {
                    cmbRoomType.SelectedItem = item;
                    break;
                }
            }

            // Set Status
            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == _roomToEdit["Status"].ToString())
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }

            // Disable room number editing in edit mode
            txtRoomNumber.IsEnabled = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    using (var conn = DatabaseConnection.GetConnection())
                    {
                        conn.Open();
                        string query;
                        MySqlCommand cmd;

                        if (_isEditMode)
                        {
                            query = @"UPDATE Rooms SET 
                                    RoomType = @RoomType,
                                    Floor = @Floor,
                                    PricePerDay = @PricePerDay,
                                    Status = @Status,
                                    Description = @Description
                                    WHERE RoomID = @RoomID";

                            cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@RoomID", _roomToEdit["RoomID"]);
                        }
                        else
                        {
                            query = @"INSERT INTO Rooms 
                                    (RoomNumber, RoomType, Floor, PricePerDay, Status, Description)
                                    VALUES 
                                    (@RoomNumber, @RoomType, @Floor, @PricePerDay, @Status, @Description)";

                            cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@RoomNumber", txtRoomNumber.Text);
                        }

                        cmd.Parameters.AddWithValue("@RoomType", ((ComboBoxItem)cmbRoomType.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@Floor", int.Parse(txtFloor.Text));
                        cmd.Parameters.AddWithValue("@PricePerDay", decimal.Parse(txtPricePerDay.Text));
                        cmd.Parameters.AddWithValue("@Status", ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString());
                        cmd.Parameters.AddWithValue("@Description", txtDescription.Text);

                        cmd.ExecuteNonQuery();
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving room: " + ex.Message);
                }
            }
        }

        private bool ValidateInput()
        {
            if (!_isEditMode && string.IsNullOrWhiteSpace(txtRoomNumber.Text))
            {
                MessageBox.Show("Please enter Room Number");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtFloor.Text) || !int.TryParse(txtFloor.Text, out _))
            {
                MessageBox.Show("Please enter a valid Floor number");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPricePerDay.Text) || !decimal.TryParse(txtPricePerDay.Text, out _))
            {
                MessageBox.Show("Please enter a valid Price per Day");
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