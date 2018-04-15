using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ServerMonitor
{
    public class CustomConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format("{0} is {1}", parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomConverterBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BarSeries series = (value as DataPointInfo).Series as BarSeries;
            RadCartesianChart chart = series.Chart as RadCartesianChart;
            var dataPoint = (value as DataPointInfo).DataPoint;
            Border border = series.GetDataPointVisual(dataPoint) as Border;
            return border.Background;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
