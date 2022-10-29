﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StockTraderRI.Infrastructure.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {       public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is true) ? Visibility.Visible : Visibility.Collapsed;            




    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new System.NotImplementedException();
            }

     
    }
    }

