using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;

namespace ServerMonitor.ViewModels.BLL
{
    public class ChartUtilImpl : IChartUtil
    {
        const int OVERTIME = 5000;//超时时间

        public ChartPalette DefaultPalette{ get{ return ChartPalettes.DefaultLight;} }

        public ObservableCollection<ChartLengend> Lengend { get; set; }

        public ObservableCollection<BarChartData> BarChart { get; set; }

        public ChartUtilImpl()
        {
            Lengend = new ObservableCollection<ChartLengend>();
            BarChart = new ObservableCollection<BarChartData>();
        }
        /// <summary>
        /// 对从数据库获取的site做初始处理，初始化统计站点，初始化站点选中状态
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public async Task<Tuple<ObservableCollection<SelectSite>, List<SiteModel>>> SelectSitesAsync(List<SiteModel> sites)
        {
            var selectSites = new ObservableCollection<SelectSite>();
            var Sites = new List<SiteModel>();
            foreach (var item in sites.Where(i => i.Is_pre_check == false).Select(i => i))
            {
                //初始时默认选中前五条
                if (selectSites.Count < 5)
                {
                    Sites.Add(item);
                    selectSites.Add(new SelectSite()
                    {
                        Site = item,
                        IsSelected =true,
                        ImagePath = item.Is_server ? "../images/ic_server.png" : "../images/ic_website.png",
                        SiteType = item.Is_server ? "SERVER" : "WEBSITE"
                    });
                }
                else
                {
                    selectSites.Add(new SelectSite()
                    {
                        Site = item,
                        IsSelected = false,
                        ImagePath = item.Is_server ? "../images/ic_server.png" : "../images/ic_website.png",
                        SiteType = item.Is_server ? "SERVER" : "WEBSITE"
                    });
                }
            }
            await Task.CompletedTask;
            return new Tuple<ObservableCollection<SelectSite>, List<SiteModel>>(selectSites, Sites);
        }

        /// <summary>
        /// 线性图图例
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<ChartLengend>> ChartLengendAsync(List<SiteModel> sites)
        {
            int i = 0;
            foreach (var item in sites)
            {
                Lengend.Add(new ChartLengend() { Title = "#" + item.Id + " " + item.Site_name, Fill = DefaultPalette.FillEntries.Brushes[i] });
                i++;
            }
            await Task.CompletedTask;
            return Lengend;
        }

        /// <summary>
        /// 计算线性图结果
        /// </summary>
        /// <param name="sites"></param>
        /// <param name="logs"></param>
        /// <returns></returns>
        public async Task<Tuple<ObservableCollection<ObservableCollection<Chart1>>, ObservableCollection<BarChartData>>> 
            CacuChartAsync(List<SiteModel> sites, List<LogModel> logs)
        {
            var chart1Collection = new ObservableCollection<ObservableCollection<Chart1>>();
            //对每个站点进行统计
            foreach (var site in sites)
            {
                ObservableCollection<Chart1> chart1Series = new ObservableCollection<Chart1>();
                //站点各项请求结果统计
                int successCount = 0, errorCount = 0, overtimeCount = 0;
                foreach (var log in logs)
                {
                    #region 统计站点信息
                    if (log.Site_id == site.Id)
                    {
                        //该条记录结果统计
                        string result = "";
                        Double responseTime = 0;
                        //判断并记录该条log是成功，失败，还是超时
                        if (!log.Is_error)
                        {
                            //成功
                            successCount++; result = "Success";
                            responseTime = log.TimeCost;
                            chart1Series.Add(new Chart1() { RequestTime = log.Create_Time, Result = result, ResponseTime = responseTime });
                        }
                        else if (log.Status_code == "1002") //状态码为1002时表示请求超时
                        {
                            //超时
                            overtimeCount++; result = "OverTime";
                            responseTime = log.TimeCost;
                            chart1Series.Add(new Chart1() { RequestTime = log.Create_Time, Result = result, ResponseTime = responseTime });
                        }
                        else
                        {
                            //失败
                            errorCount++; result = "Error";
                            chart1Series.Add(new Chart1() { RequestTime = log.Create_Time, Result = result, ResponseTime = null });
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
            return new Tuple<ObservableCollection<ObservableCollection<Chart1>>, ObservableCollection<BarChartData>>(chart1Collection, BarChart);
        }
    }
}
