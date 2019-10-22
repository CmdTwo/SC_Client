﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SC_Client.control
{
    /// <summary>
    /// Логика взаимодействия для EventAlert.xaml
    /// </summary>
    public partial class EventAlert : UserControl
    {
        public EventAlert(string message, bool temp)
        {
            InitializeComponent();
            HorizontalAlignment = HorizontalAlignment.Left;

            MessageText.Text = (temp) 
                ? "User " + message + " has joined!" 
                : "User " + message + " has left!";
        }
    }
}
