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
        /// <summary>
        /// 对从数据库获取的site完善其信息，初始化统计站点 创建：fjl
        /// </summary>
        /// <param name="sites"></param>
        /// <returns>返回信息完善的站点列表和已选站点</returns>
        Task<Tuple<ObservableCollection<AddSiteInfo>, List<SiteModel>>> AddInfoForSiteAsync(List<SiteModel> sites);
        /// <summary>
        /// 生成线性图图例 创建：fjl
        /// </summary>
        /// <param name="sites">站点列表</param>
        /// <returns>返回图例集合</returns>
        Task<ObservableCollection<LineChartLengend>> SetLineChartLengendAsync(List<SiteModel> sites);
        /// <summary>
        /// 对站点请求记录统计其结果 创建：fjl
        /// </summary>
        /// <param name="sites">站点列表</param>
        /// <param name="logs">记录列表</param>
        /// <returns>返回统计的单个站点详细结果和结果总计</returns>
        Task<Tuple<ObservableCollection<ObservableCollection<LineChartData>>, ObservableCollection<BarChartData>>> StatisticsSiteRequestResultAsync(List<SiteModel> sites, List<LogModel> logs);
        /// <summary>
        /// 判断两个时间的时间间隔是否大于30分钟 创建：fjl
        /// </summary>
        /// <param name="t1">第一个时间</param>
        /// <param name="t2">第二个时间</param>
        /// <returns>大于返回true，其他返回false</returns>
        Task<bool> CompareTimeInterval(DateTime t1, DateTime t2);
    }
}
