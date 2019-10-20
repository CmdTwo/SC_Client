using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SC_Client.src;

namespace SC_Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client ClientObj;

        public MainWindow()
        {
            InitializeComponent();

            ClientObj = new Client();
            ClientObj.debugBox = DebugTextBox;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ClientObj.Connect();            
            ConnectButton.IsEnabled = !(SendTestDataButton.IsEnabled = DisconnectButton.IsEnabled = true);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ClientObj.Disconnect();
            DisconnectButton.IsEnabled = SendTestDataButton.IsEnabled = !(ConnectButton.IsEnabled = true);
        }

        private void TestDataButton_Click(object sender, RoutedEventArgs e)
        {
            ClientObj.SendTestData();
        }     
    }
}
