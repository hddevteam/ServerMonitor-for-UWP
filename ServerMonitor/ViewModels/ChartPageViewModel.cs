using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using System.Threading;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ServerMonitor.Models;
using GalaSoft.MvvmLight;
using ServerMonitor.Controls;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ServerMonitor.Views;
using Windows.UI.Xaml.Data;
using System.Globalization;
using Telerik.UI.Xaml.Controls.Chart;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using GalaSoft.MvvmLight.Threading;

namespace ServerMonitor.ViewModels
{
    /// <summary>
    /// Created by fjl
    /// </summary>
    public class ChartPageViewModel : Template10.Mvvm.ViewModelBase
    {
        #region 变量
        public ChartPalette DefaultPalette
        {
            get
            {
                return ChartPalettes.DefaultLight;
            }
        }

        private SiteRequestCountInfo infos;

        public SiteRequestCountInfo Infos
        {
            get { return infos; }
            set
            {
                infos = value;
                RaisePropertyChanged(() => Infos);
            }
        }
        private ObservableCollection<ChartLengend> lengend;
        public ObservableCollection<ChartLengend> Lengend
        {
            get { return lengend; }

            set { lengend = value; RaisePropertyChanged(() => Lengend); }
        }
        //请求结果类型
        public List<string> RequestResult { get; set; }
        //所选结果类型
        private string type;
        public string Type
        {
            get { return type; }
            set
            {
                if (value != null && value != type)
                {
                    type = value;
                    TypeChanged_Data(type);
                    RaisePropertyChanged(() => Type);
                }
            }
        }
        #endregion

        //构造函数
        public ChartPageViewModel()
        {
            RequestResult = new List<string> { "Success", "Error", "OverTime" };
            Type = "Success";
            Infos = new SiteRequestCountInfo();
        }

        //跳转到页面触发
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            await Task.CompletedTask;
            //对象实例化
            initAsync();
            //加载数据库数据
            loadDbDataAsync();
            //加载图表数据
            await LoadChartDataAsync();
            //图表加载完毕后切换加载状态
            Infos.State3 = Visibility.Collapsed;
            Infos.State1 = Visibility.Visible;

        }

        #region 实例化及加载数据库数据
        //变量初始化
        public async void initAsync()
        {

            Lengend = new ObservableCollection<ChartLengend>();

            Infos.Collection = new ObservableCollection<ObservableCollection<LineCount>>();
            Infos.SelectSites = new ObservableCollection<SelectSite>();
            Infos.Sites = new ObservableCollection<Site>();
            Infos.Logs = new ObservableCollection<Log>();
            Infos.BarChart = new ObservableCollection<BarChartData>();
            Infos.GridChart = new ObservableCollection<BarChartData>();
            await Task.CompletedTask;

        }

        //加载数据库数据
        public async void loadDbDataAsync()
        {

            //加载日志记录
            List<Log> l = DBHelper.GetAllLog();
            foreach (var log in l)
            {
                Infos.Logs.Add(log);
            }
            //加载所有站点
            List<Site> s = DBHelper.GetAllSite();

            foreach (var item in s)
            {
                //初始时默认选中前五条
                if (!item.Is_pre_check)
                {
                    if (Infos.SelectSites.Count < 5)
                    {
                        Infos.Sites.Add(item);
                        Infos.SelectSites.Add(new SelectSite() { Site = item, IsSelected = true });
                    }
                    else
                    {
                        Infos.SelectSites.Add(new SelectSite() { Site = item, IsSelected = false });
                    }
                }

            }
            await SelectSiteInfoAsync();
            await ChartLengendAsync();
            await Task.CompletedTask;
        }

        //站点信息补全，添加图片及类型说明
        public async Task SelectSiteInfoAsync()
        {
            foreach (var item in Infos.SelectSites)
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

        }

        //线性图图例
        public async Task ChartLengendAsync()
        {
            int i = 0;

            foreach (var item in Infos.Sites)
            {

                Lengend.Add(new ChartLengend() { Title = item.Site_name, Fill = DefaultPalette.FillEntries.Brushes[i] });
                i++;
            }
            await Task.CompletedTask;
        }
        #endregion

        #region 数据统计

        //开始进行图表数据计算
        public async Task LoadChartDataAsync()
        {
            //数据库是否有日志记录
            if (Infos.Logs.Count != 0)
            {
                await CacuLineChartAsync();

            }
        }

        /// <summary>
        /// 计算线性图结果
        /// </summary>
        /// <param name="sites"></param>
        /// <param name="logs"></param>
        public async Task CacuLineChartAsync()
        {
            //对每个站点进行统计
            DateTime time = DateTime.Now;
            foreach (var site in Infos.Sites)
            {
                //该站点的数据序列
                ObservableCollection<LineCount> series = new ObservableCollection<LineCount>();
                if (site.Is_server) { site.Site_name = site.Site_name + "(SERVER)"; } //避免不同站点类型同名情况

                //近一周
                for (int i = 6; i >= 0; i--)
                {
                    //Debug.WriteLine(time.AddDays(-i));
                    series.Add(new LineCount() { MonitorTime = time.AddDays(-i), SiteName = site.Site_name });
                }
                Cacu(site, series);
                //将统计好的结果加入到序列集合
                Infos.Collection.Add(series);

            }
            await Task.CompletedTask;
        }
        /// <summary>
        /// 站点请求结果的统计过程
        /// </summary>
        /// <param name="site"></param>
        /// <param name="series"></param>
        public void Cacu(Site site, ObservableCollection<LineCount> series)
        {
            //站点各项请求结果统计
            int successCount = 0;
            int errorCount = 0, overtimeCount = 0;
            foreach (var log in Infos.Logs)
            {
                #region 统计站点信息
                if (log.Site_id == site.Id)
                {
                    //该条记录结果统计
                    int success = 0;
                    int error = 0, overtime = 0;
                    //判断并记录该条log是成功，失败，还是超时
                    if (!log.Is_error)
                    {
                        //成功
                        success++;
                        successCount++;
                    }
                    else if (log.Status_code == "1002") //状态码为1002时表示请求超时
                    {
                        //超时
                        overtime++;
                        overtimeCount++;
                    }
                    else
                    {
                        //失败
                        error++;
                        errorCount++;
                    }

                    TimeSpan timeSpan = DateTime.Now - log.Create_time;

                    //若该记录时间在最近一周内
                    if (series != null && timeSpan.Days <= 6)
                    {
                        series[6 - timeSpan.Days].Success += success;
                        series[6 - timeSpan.Days].Error += error;
                        series[6 - timeSpan.Days].Overtime += overtime;
                        //根据选择的类型来显示不同结果
                        if (Type == "Success") { series[6 - timeSpan.Days].Count += success; }
                        else if (Type == "Error") { series[6 - timeSpan.Days].Count += error; }
                        else { series[6 - timeSpan.Days].Count += overtime; }
                        //Debug.WriteLine("SiteName:{0}+Success:{1}+Error:{2}+OverTime:{3}+MonitorTime:{4}+6 - timeSpan.Days:{5}", site.Site_name, success, error, overtime, log.Create_time,(6 - timeSpan.Days));
                    }

                }

                #endregion
            }
            //Debug.WriteLine("SiteName:{0}+Success:{1}+Error:{2}+OverTime:{3}", site.Site_name, successCount, errorCount, overtimeCount);
            CacuBarChart(site, successCount, errorCount, overtimeCount);
        }
        /// <summary>
        /// 计算柱状图结果，列表数据同
        /// </summary>
        /// <param name="site"></param>
        /// <param name="success"></param>
        /// <param name="error"></param>
        /// <param name="overtime"></param>
        ///
        public void CacuBarChart(Site site, int success, int error, int overtime)
        {
            //添加图片
            string type;
            if (site.Is_server)
            {
                type = "SERVER";
                Infos.BarChart.Add(new BarChartData() { Site = site.Site_name, Success = success, Error = error, Overtime = overtime });
                //此处去掉之前的添加的显示类别
                site.Site_name = site.Site_name.Substring(0, site.Site_name.Length - 8);
            }
            else
            {
                type = "WEBSITE";
                Infos.BarChart.Add(new BarChartData() { Site = site.Site_name, Success = success, Error = error, Overtime = overtime });
            }

            Infos.GridChart.Add(new BarChartData() { Site = site.Site_name, Success = success, Error = error, Overtime = overtime, Type = type });
        }
        #endregion

        #region 响应事件

        /// <summary>
        /// 切换类型
        /// </summary>
        /// <param name="type"></param>
        public void TypeChanged_Data(string type)
        {
            //Debug.WriteLine("TypeChanged:" + Type);
            if (Infos != null)
            {
                foreach (var items in Infos.Collection)
                {
                    foreach (var item in items)
                    {
                        if (type == "Success") { item.Count = item.Success; }
                        else if (type == "Error") { item.Count = item.Error; }
                        else
                        {
                            item.Count = item.Overtime;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 选择站点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChartFliter_Click(object sender, RoutedEventArgs e)
        {
            Infos.State1 = Visibility.Collapsed;
            Infos.State2 = Visibility.Visible;
        }

        /// <summary>
        /// 确定站点并切换页面
        /// </summary>
        public async void Accept_ClickAsync()
        {
            //清空数据，重新统计
            Infos.Sites.Clear();
            Infos.Collection.Clear();
            Infos.GridChart.Clear();
            Infos.BarChart.Clear();
            foreach (var item in Infos.SelectSites.Where(i => i.IsSelected == true).Select(i => i.Site))
            {
                //获取选择的站点
                Infos.Sites.Add(item);
            }
            //选择站点数量大于上限进行提示
            if (Infos.Sites.Count > 5)
            {
                ShowMessageDialog();
            }
            else
            {
                Infos.State1 = Visibility.Visible;
                Infos.State2 = Visibility.Collapsed;
                //重新统计数据
                await CacuLineChartAsync();
                Lengend.Clear();
                await ChartLengendAsync();
            }
        }

        //选择站点上限提示对话框
        private async void ShowMessageDialog()
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog("站点最多选择五个！！") { Title = "提示" };
            await msgDialog.ShowAsync();
        }
        #endregion

    }
    #region 图表页面各项数据类
    //图表图例
    public class ChartLengend : ObservableObject
    {
        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }
        private Brush fill;

        public Brush Fill
        {
            get { return fill; }
            set { fill = value; RaisePropertyChanged(() => Fill); }
        }

    }
    //站点选择类
    public class SelectSite : ObservableObject
    {
        private string imagepath;

        public string ImagePath
        {
            get { return imagepath; }
            set { imagepath = value; RaisePropertyChanged(() => ImagePath); }
        }
        private string siteType;

        public string SiteType
        {
            get { return siteType; }
            set { siteType = value; RaisePropertyChanged(() => SiteType); }
        }

        private Site site;

        public Site Site
        {
            get { return site; }
            set
            {
                site = value;
                RaisePropertyChanged(() => Site);
            }
        }
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

    }

    //线性图数据类
    public class LineCount : ObservableObject
    {

        private int count;

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                RaisePropertyChanged(() => Count);
            }
        }

        private string siteName;

        public string SiteName
        {
            get { return siteName; }
            set
            {
                siteName = value;
                RaisePropertyChanged(() => SiteName);
            }
        }

        private int number;

        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                RaisePropertyChanged(() => Number);
            }
        }

        private int error;

        public int Error
        {
            get { return error; }
            set
            {
                error = value;
                RaisePropertyChanged(() => Error);
            }
        }
        private int success;

        public int Success
        {
            get { return success; }
            set
            {
                success = value;
                RaisePropertyChanged(() => Success);
            }
        }
        private int overtime;

        public int Overtime
        {
            get { return overtime; }
            set
            {
                overtime = value;
                RaisePropertyChanged(() => Overtime);
            }
        }

        private DateTime monitorTime;

        public DateTime MonitorTime
        {
            get { return monitorTime; }
            set
            {
                monitorTime = value;
                RaisePropertyChanged(() => MonitorTime);
            }
        }
    }
    //柱形图（列表）数据类
    public class BarChartData : ObservableObject
    {
        private int count;

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                RaisePropertyChanged(() => Count);
            }
        }

        private string image;

        public string Image
        {
            get { return image; }
            set
            {
                image = value;
                RaisePropertyChanged(() => Image);
            }
        }
        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged(() => Type); }
        }


        private string site;

        public string Site
        {
            get { return site; }
            set
            {
                site = value;
                RaisePropertyChanged(() => Site);
            }
        }
        private int error;

        public int Error
        {
            get { return error; }
            set
            {
                error = value;
                RaisePropertyChanged(() => Error);
            }
        }
        private int success;

        public int Success
        {
            get { return success; }
            set
            {
                success = value;
                RaisePropertyChanged(() => Success);
            }
        }
        private int overtime;

        public int Overtime
        {
            get { return overtime; }
            set
            {
                overtime = value;
                RaisePropertyChanged(() => Overtime);
            }
        }
    }
    //页面所有数据
    public class SiteRequestCountInfo : ObservableObject
    {
        #region 变量
        //切换页面显示状态
        private Visibility state1 = Visibility.Collapsed;
        private Visibility state2 = Visibility.Collapsed;
        private Visibility state3 = Visibility.Visible;

        //数据库日志
        private ObservableCollection<Log> logs;
        //选择站点
        private ObservableCollection<SelectSite> selectSites;
        //获取被选中的站点
        private ObservableCollection<Site> sites;
        //柱形图数据
        private ObservableCollection<BarChartData> barChart;
        //线性图数据
        private ObservableCollection<LineCount> lineChart;
        //线形图所有系列集合
        ObservableCollection<ObservableCollection<LineCount>> collection;
        //页面1表格数据
        private ObservableCollection<BarChartData> gridChart;
        #endregion

        #region 属性

        public ObservableCollection<BarChartData> GridChart
        {
            get { return gridChart; }
            set { gridChart = value; RaisePropertyChanged(() => GridChart); }
        }

        public Visibility State1
        {
            get { return state1; }
            set { state1 = value; RaisePropertyChanged(() => State1); }
        }

        public Visibility State2
        {
            get { return state2; }
            set { state2 = value; RaisePropertyChanged(() => State2); }
        }

        public Visibility State3
        {
            get { return state3; }
            set { state3 = value; RaisePropertyChanged(() => State3); }
        }

        public ObservableCollection<Log> Logs
        {
            get => logs;
            set
            {
                logs = value;
                RaisePropertyChanged(() => Logs);
            }
        }

        public ObservableCollection<Site> Sites
        {
            get { return sites; }
            set { sites = value; RaisePropertyChanged(() => Sites); }
        }

        public ObservableCollection<SelectSite> SelectSites
        {
            get { return selectSites; }
            set { selectSites = value; RaisePropertyChanged(() => SelectSites); }
        }

        public ObservableCollection<BarChartData> BarChart
        {
            get { return barChart; }
            set
            {
                barChart = value;
                RaisePropertyChanged(() => BarChart);
            }
        }

        public ObservableCollection<LineCount> LineChart
        {
            get { return lineChart; }
            set
            {
                lineChart = value;
                RaisePropertyChanged(() => LineChart);
            }
        }

        public ObservableCollection<ObservableCollection<LineCount>> Collection
        {
            get { return collection; }
            set
            {
                collection = value;
                RaisePropertyChanged(() => Collection);
            }
        }
        #endregion
    }
    #endregion
}
