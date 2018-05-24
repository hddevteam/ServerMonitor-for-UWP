
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telerik.UI.Xaml.Controls.Chart;
using Template10.Common;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ServerMonitor.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Chart : Page
    {
        public ChartPalette DefaultPalette { get { return ChartPalettes.DefaultLight; } }

        public Chart()
        {   
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
            CreateSeries();
        }

        public void CreateSeries()
        {
            RadCartesianChart chart = this.RequestTimeLineChar as RadCartesianChart;
            chart.Series.Clear();
            for (int i = 0; i < 5; i++)
            {
                CategoricalSeries series = null;
                if (chart == null)
                {
                    return;
                }
                series = new LineSeries()
                {
                    Stroke = DefaultPalette.FillEntries.Brushes[i],
                    StrokeThickness = 3,
                    PointTemplate = chart.Resources["PointTemplate"] as DataTemplate
                };
                series.CategoryBinding = new PropertyNameDataPointBinding("RequestTime");
                series.ValueBinding = new PropertyNameDataPointBinding("ResponseTime");
                series.SetBinding(ChartSeries.ItemsSourceProperty, new Binding() { Path = new PropertyPath("Infos.Chart1CollectionCopy[" + i + "]") });
                series.ClipToPlotArea = false;

                chart.Series.Add(series);
            }
        }
    }
}
