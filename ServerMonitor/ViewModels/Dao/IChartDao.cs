using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;

namespace ServerMonitor.ViewModels
{
    public interface IChartDao
    {
        ChartPalette DefaultPalette { get; }

        ObservableCollection<ChartLengend> Lengend { get; set; }

        ObservableCollection<BarChartData> BarChart { get; set; }

        ObservableCollection<BarChartData> GridChart { get; set; }

        Task<Tuple<ObservableCollection<SelectSite>, List<Site>>> SelectSitesAsync(List<Site> sites);

        Task<ObservableCollection<ChartLengend>> ChartLengendAsync(List<Site> sites);

        Task<Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>> CacuChartAsync(List<Site> sites, List<Log> logs);

        Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>> CacuBarChart(List<Site> sites, int[,] requestResults);
    }
}
