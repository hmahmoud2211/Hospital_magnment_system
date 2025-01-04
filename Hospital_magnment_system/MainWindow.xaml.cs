using System.Windows;
using System.Windows.Controls;
using Hospital_magnment_system.Views;

namespace Hospital_magnment_system
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button _currentActiveButton;

        public MainWindow()
        {
            InitializeComponent();
            NavigateToPage(btnDashboard, new Dashboard());
        }

        private void NavigateToPage(Button clickedButton, Page page)
        {
            // Reset previous button style
            if (_currentActiveButton != null)
            {
                _currentActiveButton.Background = System.Windows.Media.Brushes.Transparent;
            }

            // Set new button as active
            clickedButton.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#34495e");
            _currentActiveButton = clickedButton;

            // Navigate to new page
            MainFrame.Navigate(page);
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(btnDashboard, new Dashboard());
        }

        private void btnPatients_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(btnPatients, new Patients());
        }

        private void btnDoctors_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(btnDoctors, new Doctors());
        }

        private void btnAppointments_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(btnAppointments, new Appointments());
        }

        private void btnRooms_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(btnRooms, new Rooms());
        }

        private void btnBilling_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(btnBilling, new Billing());
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(btnSettings, new Settings());
        }
    }
}