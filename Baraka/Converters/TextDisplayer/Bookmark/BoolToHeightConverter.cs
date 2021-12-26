﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Baraka.Converters.TextDisplayer.Bookmark
{
    public class BoolToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return double.NaN;
            }
            else
            {
                return 60;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}