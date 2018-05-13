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

        Task<Tuple<ObservableCollection<SelectSite>, List<SiteModel>>> SelectSitesAsync(List<SiteModel> sites);

        Task<ObservableCollection<ChartLengend>> ChartLengendAsync(List<SiteModel> sites);

        Task<Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>> CacuChartAsync(List<SiteModel> sites, List<LogModel> logs);

        Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>> CacuBarChart(List<SiteModel> sites, int[,] requestResults);
    }
}
