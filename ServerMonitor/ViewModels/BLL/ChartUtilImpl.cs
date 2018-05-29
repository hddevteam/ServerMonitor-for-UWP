using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;
/// <summary>
/// 创建：fjl  创建时间：2018/05/25 
/// Chart页面计算逻辑接口实现类
/// </summary>
namespace ServerMonitor.ViewModels.BLL
{
    public class ChartUtilImpl : IChartUtil
    {
        const int OVERTIME = 5000;//超时时间
        const double MAXTIMEINTERVAL = 1800;//最大时间间隔 30min*60=1800s

        public ChartPalette DefaultPalette{ get{ return ChartPalettes.DefaultLight;} }

        public ObservableCollection<LineChartLengend> Lengend { get; set; }

        public ObservableCollection<BarChartData> BarChart { get; set; }

        public ChartUtilImpl()
        {
            Lengend = new ObservableCollection<LineChartLengend>();
            BarChart = new ObservableCollection<BarChartData>();
        }
        /// <summary>
        /// 对从数据库获取的site完善其信息，初始化统计站点 创建：fjl
        /// </summary>
        /// <param name="sites"></param>
        /// <returns>返回信息完善的站点列表和已选站点</returns>
        public async Task<Tuple<ObservableCollection<AddSiteInfo>, List<SiteModel>>> AddInfoForSiteAsync(List<SiteModel> sites)
        {
            var selectSites = new ObservableCollection<AddSiteInfo>();
            var Sites = new List<SiteModel>();
            foreach (var item in sites.Where(i => i.Is_pre_check == false).Select(i => i))
            {
                //初始时默认选中前五条
                if (Sites.Count < 5 && item.Is_Monitor)
                {
                    Sites.Add(item);
                    selectSites.Add(new AddSiteInfo()
                    {
                        Site = item,
                        IsSelected =true,
                        ImagePath = item.Is_server ? "../images/ic_server.png" : "../images/ic_website.png",
                        SiteType = item.Is_server ? "SERVER" : "WEBSITE"
                    });
                }
                else
                {
                    selectSites.Add(new AddSiteInfo()
                    {
                        Site = item,
                        IsSelected = false,
                        ImagePath = item.Is_server ? "../images/ic_server.png" : "../images/ic_website.png",
                        SiteType = item.Is_server ? "SERVER" : "WEBSITE"
                    });
                }
            }
            await Task.CompletedTask;
            return new Tuple<ObservableCollection<AddSiteInfo>, List<SiteModel>>(selectSites, Sites);
        }

        /// <summary>
        /// 生成线性图图例 创建：fjl
        /// </summary>
        /// <param name="sites">站点列表</param>
        /// <returns>返回图例集合</returns>
        public async Task<ObservableCollection<LineChartLengend>> SetLineChartLengendAsync(List<SiteModel> sites)
        {
            int i = 0;
            foreach (var item in sites)
            {
                Lengend.Add(new LineChartLengend() { Title = "#" + item.Id + " " + item.Site_name, Fill = DefaultPalette.FillEntries.Brushes[i] });
                i++;
            }
            await Task.CompletedTask;
            return Lengend;
        }

        /// <summary>
        /// 对站点请求记录统计其结果 创建：fjl
        /// </summary>
        /// <param name="sites">站点列表</param>
        /// <param name="logs">记录列表</param>
        /// <returns>返回统计的单个站点详细结果和结果总计</returns>
        public async Task<Tuple<ObservableCollection<ObservableCollection<LineChartData>>, ObservableCollection<BarChartData>>> 
            StatisticsSiteRequestResultAsync(List<SiteModel> sites, List<LogModel> logs)
        {
            var chart1Collection = new ObservableCollection<ObservableCollection<LineChartData>>();
            //对每个站点进行统计
            foreach (var site in sites)
            {
                //为站点创建序列数据
                ObservableCollection<LineChartData> chart1Series = new ObservableCollection<LineChartData>();
                //站点各项请求结果统计
                int successCount = 0, errorCount = 0, overtimeCount = 0;

                foreach (var log in logs)
                {
                    #region 统计站点信息
                    if (log.Site_id == site.Id)
                    {
                        //判断并记录该条log是成功，失败，还是超时
                        if (!log.Is_error)
                        {
                            //成功
                            successCount++;
                            //把数据库里的Utc时间转换为LocalTime
                            log.Create_Time = log.Create_Time.ToLocalTime();
                            //若相邻两个记录时间差大于30分钟，且最近一条记录不是错误记录，则中间插入一条错误记录，使图表在此处断开
                            if (chart1Series.Count != 0 && chart1Series[chart1Series.Count - 1].ResponseTime != null 
                                && await CompareTimeInterval(chart1Series[chart1Series.Count - 1].RequestTime, log.Create_Time))
                            {
                                //在当前记录的前三十分钟添加一条错误记录
                                chart1Series.Add(new LineChartData() { RequestTime = log.Create_Time.AddMinutes(-30), ResponseTime = null });
                            }
                            chart1Series.Add(new LineChartData() { RequestTime = log.Create_Time, ResponseTime = log.TimeCost });
                        }
                        else if (log.Status_code == "1002") //状态码为1002时表示请求超时
                        {
                            //超时
                            overtimeCount++;
                            //把数据库里的Utc时间转换为LocalTime
                            log.Create_Time = log.Create_Time.ToLocalTime();
                            //若相邻两个记录时间差大于30分钟，且最近一条记录不是错误记录，则中间插入一条错误记录，使图表在此处断开
                            if (chart1Series.Count != 0 && chart1Series[chart1Series.Count - 1].ResponseTime != null
                                && await CompareTimeInterval(chart1Series[chart1Series.Count - 1].RequestTime, log.Create_Time))
                            {
                                //在当前记录的前三十分钟添加一条错误记录
                                chart1Series.Add(new LineChartData() { RequestTime = log.Create_Time.AddMinutes(-30), ResponseTime = null });
                            }
                            chart1Series.Add(new LineChartData() { RequestTime = log.Create_Time, ResponseTime = OVERTIME });
                        }
                        else
                        {
                            //失败
                            errorCount++;
                            //把数据库里的Utc时间转换为LocalTime
                            log.Create_Time = log.Create_Time.ToLocalTime();
                            chart1Series.Add(new LineChartData() { RequestTime = log.Create_Time, ResponseTime = null });
                        }
                    }
                    #endregion
                }
                //将统计好的结果加入到序列集合
                chart1Collection.Add(chart1Series);
                BarChart.Add(new BarChartData()
                {
                    SiteId = site.Id.ToString(),
                    SiteName = "#" + site.Id + " " + site.Site_name,
                    Success = successCount,
                    Error = errorCount,
                    Overtime = overtimeCount,
                    Address = site.Site_address
                });
            }
            await Task.CompletedTask;
            return new Tuple<ObservableCollection<ObservableCollection<LineChartData>>, ObservableCollection<BarChartData>>(chart1Collection, BarChart);
        }

        /// <summary>
        /// 判断两个时间的时间间隔是否大于30分钟 创建：fjl
        /// </summary>
        /// <param name="t1">第一个时间</param>
        /// <param name="t2">第二个时间</param>
        /// <returns>大于返回true，其他返回false</returns>
        public async Task<bool> CompareTimeInterval(DateTime t1,DateTime t2)
        {
            await Task.CompletedTask;
            TimeSpan timeSpan = t2.Subtract(t1);
            if (timeSpan.TotalSeconds > MAXTIMEINTERVAL)
            {
                return true;
            }
            return false;
        }
    }
}
