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
    public class ChartManger : IChartDao
    {
        public ChartPalette DefaultPalette{ get{ return ChartPalettes.DefaultLight;} }

        public ObservableCollection<ChartLengend> Lengend { get; set; }

        public ObservableCollection<BarChartData> BarChart { get; set; }

        public ObservableCollection<BarChartData> GridChart { get; set; }

        public ChartManger()
        {
            Lengend = new ObservableCollection<ChartLengend>();
            BarChart = new ObservableCollection<BarChartData>();
            GridChart = new ObservableCollection<BarChartData>();
        }
        /// <summary>
        /// 对从数据库获取的site做初始处理，初始化统计站点，初始化站点选中状态
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public async Task<Tuple<ObservableCollection<SelectSite>, List<Site>>> SelectSitesAsync(List<Site> sites)
        {
            var selectSites = new ObservableCollection<SelectSite>();
            var Sites = new List<Site>();
            foreach (var item in sites)
            {
                //初始时默认选中前五条
                if (!item.Is_pre_check)
                {
                    if (selectSites.Count < 5)
                    {
                        Sites.Add(item);
                        selectSites.Add(new SelectSite() { Site = item, IsSelected = true });
                    }
                    else
                    {
                        selectSites.Add(new SelectSite() { Site = item, IsSelected = false });
                    }
                }
            }
            await Task.CompletedTask;
            return new Tuple<ObservableCollection<SelectSite>, List<Site>>(selectSites, Sites);
        }

        /// <summary>
        /// 站点信息补全，添加图片及类型说明
        /// </summary>
        /// <param name="selectSites"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<SelectSite>> SelectSiteInfoAsync(ObservableCollection<SelectSite> selectSites)
        {
            foreach (var item in selectSites)
            {
                if (item.Site.Is_server)
                {
                    item.ImagePath = "../images/ic_server.png";
                    item.SiteType = "SERVER";
                }
                else
                {
                    item.ImagePath = "../images/ic_website.png";
                    item.SiteType = "WEBSITE";
                }
            }
            await Task.CompletedTask;
            return selectSites;
        }

        /// <summary>
        /// 线性图图例
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<ChartLengend>> ChartLengendAsync(List<Site> sites)
        {
            int i = 0;
            foreach (var item in sites)
            {
                Lengend.Add(new ChartLengend() { Title = item.Site_name, Fill = DefaultPalette.FillEntries.Brushes[i] });
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
        public async Task<Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>> CacuChartAsync(List<Site> sites, List<Log> logs)
        {
            var chart1Collection = new ObservableCollection<ObservableCollection<Chart1>>();
            //对每个站点进行统计
            DateTime time = DateTime.Now;
            var count = sites.Count;
            int[,] siteResultCount = new int[count, 3];
            int index = 0;//二维数组的行序
            foreach (var site in sites)
            {
                //该站点的数据序列,若站点序列只有一条数据，线性表表现为不显示
                ObservableCollection<Chart1> chart1Series = new ObservableCollection<Chart1>();
                //站点各项请求结果统计
                int successCount = 0, errorCount = 0, overtimeCount = 0;
                foreach (var log in logs)
                {
                    #region 统计站点信息
                    if (log.Site_id == site.Id)
                    {
                        //该条记录结果统计
                        string result = ""; Double responseTime = 0;
                        //判断并记录该条log是成功，失败，还是超时
                        if (!log.Is_error)
                        {
                            //成功
                            successCount++; result = "Success";
                            responseTime = Math.Log10(log.Request_time);
                        }
                        else if (log.Status_code == "1002") //状态码为1002时表示请求超时
                        {
                            //超时
                            overtimeCount++; result = "OverTime";
                            responseTime = Math.Log10(log.Request_time);
                        }
                        else
                        {
                            //失败
                            errorCount++; result = "Error";
                            responseTime = 0;
                        }
                        chart1Series.Add(new Chart1() { RequestTime = log.Create_time, Result = result, ResponseTime = responseTime });
                    }
                    #endregion
                }
                //将统计好的结果加入到序列集合
                chart1Collection.Add(chart1Series);

                siteResultCount[index, 0] = successCount;
                siteResultCount[index, 1] = errorCount;
                siteResultCount[index, 2] = overtimeCount;
                index++;
            }
            await Task.CompletedTask;
            return new Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>(chart1Collection, siteResultCount);
        }

        /// <summary>
        /// 计算柱状图结果，列表数据同
        /// </summary>
        /// <param name="sites"></param>
        /// <param name="requestResults"></param>
        /// <returns></returns>
        public Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>> CacuBarChart(List<Site> sites, int[,] requestResults)
        {
            int index = 0;
            foreach (var item in sites)
            {
                //添加图片
                string type;
                if (item.Is_server)
                {
                    type = "SERVER"; //+(SERVER)区分不同站点类型同名情况
                    BarChart.Add(new BarChartData()
                    {
                        SiteName = item.Site_name + "(SERVER)",
                        Success = requestResults[index, 0],
                        Error = requestResults[index, 1],
                        Overtime = requestResults[index, 2]
                    });
                }
                else
                {
                    type = "WEBSITE";
                    BarChart.Add(new BarChartData()
                    {
                        SiteName = item.Site_name,
                        Success = requestResults[index, 0],
                        Error = requestResults[index, 1],
                        Overtime = requestResults[index, 2]
                    });
                }
                //第三个图表（grid1）数据
                GridChart.Add(new BarChartData()
                {
                    SiteName = item.Site_name,
                    Success = requestResults[index, 0],
                    Error = requestResults[index, 1],
                    Overtime = requestResults[index, 2],
                    Type = type
                });
                index++;
            }
            return new Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>>(BarChart, GridChart);
        }
    }
}
