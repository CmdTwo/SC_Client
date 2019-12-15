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

namespace SC_Client.view
{
    /// <summary>
    /// Логика взаимодействия для Alert.xaml
    /// </summary>
    public partial class Alert : Window
    {
        public Alert(string text, string header = "Alert")
        {
            InitializeComponent();
            ContentTextBox.Text = text;
            AlertWin.Title = header;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
