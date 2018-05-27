using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;
/// <summary>
/// 创建：fjl  创建时间：2018/05/25 
/// Chart页面计算逻辑接口
/// </summary>
namespace ServerMonitor.ViewModels.BLL
{
    public interface IChartUtil
    {
        ChartPalette DefaultPalette { get; }

        ObservableCollection<LineChartLengend> Lengend { get; set; }

        ObservableCollection<BarChartData> BarChart { get; set; }

        Task<Tuple<ObservableCollection<AddSiteInfo>, List<SiteModel>>> AddInfoForSiteAsync(List<SiteModel> sites);

        Task<ObservableCollection<LineChartLengend>> SetLineChartLengendAsync(List<SiteModel> sites);

        Task<Tuple<ObservableCollection<ObservableCollection<LineChartData>>, ObservableCollection<BarChartData>>> StatisticsSiteRequestResultAsync(List<SiteModel> sites, List<LogModel> logs);
    }
}
