using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;


namespace FindlayBikeShop
{
    /// <summary>
    /// Interaction logic for BikeDetails.xaml
    /// </summary>
    public partial class BikeDetails : Window
    {

        private string connectionString = "Data Source=BikeDatabase.db";

        // store the selected bike that is being displayed (used for passing to edit window)
        private Bike currentBike;

        public BikeDetails(Bike bike)
        {
            InitializeComponent();

            currentBike = bike;

            this.DataContext = bike;
        }


        private void Home_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var inventoryWindow = new Inventory();
            inventoryWindow.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            var maintenanceWindow = new MaintenanceHistory();
            maintenanceWindow.Show();
        }

        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            // open the addBike window, but pass current bike in case of editing an existing bike instead of adding a new one
            var editWindow = new AddBike(currentBike);
            editWindow.ShowDialog();

            // refresh the bike page after editing details to display edited information
            var refreshedWindow = new BikeDetails(currentBike);
            refreshedWindow.Show();
            this.Close();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
