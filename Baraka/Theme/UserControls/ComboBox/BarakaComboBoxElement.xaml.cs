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

namespace Baraka.Theme.UserControls.ComboBox
{
    /// <summary>
    /// Logique d'interaction pour BarakaComboBoxElement.xaml
    /// </summary>
    public partial class BarakaComboBoxElement : UserControl
    {
        public BarakaComboBoxElement()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            SideBarPath.Stroke = (SolidColorBrush)App.Current.Resources["MediumBrush"];
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            SideBarPath.Stroke = Brushes.LightGray;
        }
    }
}
