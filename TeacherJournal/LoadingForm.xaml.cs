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
using System.Windows.Shapes;

namespace TeacherJournal
{
    public partial class LoadingForm : Window
    {
        public static String DefaultProcessDescription = "Почекайте, виконуються дії.";
        private String _processDesription;
        public bool WorkEnded;

        public LoadingForm()
        {
            InitializeComponent();
            _processDesription = DefaultProcessDescription;
        }
        public LoadingForm(String processDescription)
        {
            InitializeComponent();
            _processDesription = processDescription;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WorkEnded = false;
            tbDescription.Text = _processDesription;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!WorkEnded)
            {
                e.Cancel = true;
            }
        }
    }
}
