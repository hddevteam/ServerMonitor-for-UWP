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
using Telerik.Charting;

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
        //图表1数据序列集合（前台绑定的非此属性，而是可变副本）
        private static ObservableCollection<ObservableCollection<Chart1>> chart1Collection;

        public ObservableCollection<ObservableCollection<Chart1>> Chart1Collection
        {
            get { return chart1Collection; }
            set { chart1Collection = value; RaisePropertyChanged(() => Chart1Collection); }
        }
        #endregion

        //构造函数
        public ChartPageViewModel()
        {
            RequestResult = new List<string> {"显示全部", "Success", "Error", "OverTime" };
            Type = "显示全部";
            Infos = new SiteRequestCountInfo();
            Chart1Collection = new ObservableCollection<ObservableCollection<Chart1>>();
        }

        //跳转到页面触发
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //对象实例化
            var result = InitAsync();
            //加载数据库数据
            var log = await LoadDbLogAsync();
            var site = await LoadDbSiteAsync();

            var selectResult = await SelectSitesAsync(site);
            Infos.Sites =  selectResult.Item2;
            Infos.Logs = log;
            Infos.SelectSites = selectResult.Item1;
            
            //数据库是否有日志记录
            if (Infos.Logs.Count != 0)
            {
                //计算图表数据
                var getResult= await Task.Run(()=>CacuChartAsync(Infos.Sites,Infos.Logs));
                foreach (var item in getResult.Item1)
                {
                    Chart1Collection.Add(item);
                }
                //此处不能直接赋值，图表绑定会有问题??
                //Chart1Collection = getResult.Item1;
                CacuBarChart(Infos.Sites, getResult.Item2);
            }

            await ChartLengendAsync(Infos.Sites);
            Infos.SelectSites = await SelectSiteInfoAsync(Infos.SelectSites);

            //图表加载完毕后切换加载状态
            Infos.State3 = Visibility.Collapsed;
            Infos.State1 = Visibility.Visible;
        }

        #region 实例化及加载数据库数据
        //变量初始化
        public async Task<bool> InitAsync()
        {
            Lengend = new ObservableCollection<ChartLengend>();
            Infos.Chart1CollectionCopy = new ObservableCollection<ObservableCollection<Chart1>>();
            Infos.SelectSites = new ObservableCollection<SelectSite>();
            Infos.Sites = new List<Site>();
            Infos.Logs = new List<Log>();
            Infos.BarChart = new ObservableCollection<BarChartData>();
            Infos.GridChart = new ObservableCollection<BarChartData>();
            await Task.CompletedTask;
            return true;
        }

        //加载数据库数据
        public async Task<List<Site>> LoadDbSiteAsync()
        {
            var sites = DBHelper.GetAllSite();

            await Task.CompletedTask;
            return DBHelper.GetAllSite();
        }
        public async Task<List<Log>> LoadDbLogAsync()
        {
            await Task.CompletedTask;
            return DBHelper.GetAllLog();
        }

        /// <summary>
        /// 对从数据库获取的site做初始处理，初始化统计站点，初始化站点选中状态
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public async Task<Tuple<ObservableCollection<SelectSite>,List<Site>>> SelectSitesAsync(List<Site> sites)
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
        #endregion

        #region 数据统计
        /// <summary>
        /// 计算线性图结果
        /// </summary>
        /// <param name="sites"></param>
        /// <param name="logs"></param>
        /// <returns></returns>
        public async Task<Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>> CacuChartAsync(List<Site> sites,List<Log> logs)
        {
            var chart1Collection = new ObservableCollection<ObservableCollection<Chart1>>();
            //对每个站点进行统计
            DateTime time = DateTime.Now;
            var count = Infos.Sites.Count;
            int[,] siteResultCount = new int[count, 3];
            int index = 0;//二维数组的行序
            foreach (var site in Infos.Sites)
            {
                //该站点的数据序列,若站点序列只有一条数据，线性表表现为不显示
                ObservableCollection<Chart1> chart1Series = new ObservableCollection<Chart1>();
                //站点各项请求结果统计
                int successCount = 0, errorCount = 0, overtimeCount = 0;
                foreach (var log in Infos.Logs)
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

                siteResultCount[index,0] = successCount;
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
        public Tuple<ObservableCollection<BarChartData>,ObservableCollection<BarChartData>> CacuBarChart(List<Site> sites, int[,] requestResults)
        {
            int index = 0;
            foreach (var item in sites)
            {
                //添加图片
                string type;
                if (item.Is_server)
                {
                    type = "SERVER"; //+(SERVER)区分不同站点类型同名情况
                    Infos.BarChart.Add(new BarChartData() { SiteName = item.Site_name + "(SERVER)", Success = requestResults[index,0],
                        Error = requestResults[index, 1], Overtime = requestResults[index, 2]
                    });
                }
                else
                {
                    type = "WEBSITE";
                    Infos.BarChart.Add(new BarChartData() { SiteName = item.Site_name, Success = requestResults[index, 0],
                        Error = requestResults[index, 1], Overtime = requestResults[index, 2]
                    });
                }
                //第三个图表（grid1）数据
                Infos.GridChart.Add(new BarChartData() { SiteName = item.Site_name, Success = requestResults[index, 0],
                    Error = requestResults[index, 1], Overtime = requestResults[index, 2], Type = type });
                index++;
            }
            return new Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>>(Infos.BarChart, Infos.GridChart);
        }
        #endregion

        #region 响应事件
        /// <summary>
        /// 切换类型
        /// </summary>
        /// <param name="type"></param>
        public void TypeChanged_Data(string type)
        {
            Debug.WriteLine("TypeChanged:" + Type);
            if (Infos != null)
            {
                //重新计算需清空（前台所绑定属性）
                Infos.Chart1CollectionCopy.Clear();
                //根据所选类型从图表1序列集合中选择符合的数据
                foreach (var items in Chart1Collection)
                {
                    if (Type.Equals("显示全部"))
                    {
                        Infos.Chart1CollectionCopy.Add(items);
                    }
                    else
                    {
                        //每个站点序列
                        ObservableCollection<Chart1> dataItem = new ObservableCollection<Chart1>();
                        //请求结果符合当前所选类别时，添加到序列(Linq 查询表达式)
                        foreach (var item in items.Where(i => i.Result.Equals(Type)).Select(i => i))
                        {
                            dataItem.Add(item);
                        }
                        //序列集合
                        Infos.Chart1CollectionCopy.Add(dataItem);
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
            Lengend.Clear();
            Chart1Collection.Clear();
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
                var getResult = await Task.Run(() => CacuChartAsync(Infos.Sites, Infos.Logs));
                foreach (var item in getResult.Item1)
                {
                    Chart1Collection.Add(item);
                }
                //Chart1Collection = getResult.Item1;
                CacuBarChart(Infos.Sites, getResult.Item2);
                //统计完成后触发此方法，计算前台需要显示的数据
                TypeChanged_Data(Type);
                await ChartLengendAsync(Infos.Sites);
            }
        }

        //选择站点上限提示对话框
        private async void ShowMessageDialog()
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog("站点最多选择五个！！") { Title = "提示" };
            await msgDialog.ShowAsync();
        }

        /// <summary>
        /// Pivot 切换时间区间时触发（当天，近三天，近一周）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Pivot _p = sender as Pivot;
            int _selectedIndex = _p.SelectedIndex;
            Debug.WriteLine("Pivot_SelectionChanged();");
            switch (_selectedIndex)
            {
                case 0:
                    Infos.DateTimeContinuousAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.DateTimeContinuousAxisProperties.MinnumDateTime = DateTime.Now.AddHours(-23);
                    Infos.DateTimeContinuousAxisProperties.ChartTitle = "Results within the last 24 hours";
                    break;
                case 1:
                    Infos.DateTimeContinuousAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.DateTimeContinuousAxisProperties.MinnumDateTime = DateTime.Now.AddDays(-2);
                    Infos.DateTimeContinuousAxisProperties.ChartTitle = "Nearly three days of request results";
                    break;
                case 2:
                    Infos.DateTimeContinuousAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.DateTimeContinuousAxisProperties.MinnumDateTime = DateTime.Now.AddDays(-6);
                    Infos.DateTimeContinuousAxisProperties.ChartTitle = "Nearly a week of request results";
                    break;
                default:
                    break;
            }
            ChangeStepUnitStep(_selectedIndex);
            TypeChanged_Data(Type);
        }

        /// <summary>
        /// 响应式修改第一个图表的属性
        /// </summary>
        /// <param name="index"></param>
        public void ChangeStepUnitStep(int index)
        {
            switch (index)
            {
                case 0:
                    Infos.DateTimeContinuousAxisProperties.MajorStep = 6;
                    Infos.DateTimeContinuousAxisProperties.MajorStepUnit = TimeInterval.Hour;
                    break;
                case 1:
                    Infos.DateTimeContinuousAxisProperties.MajorStep = 1;
                    Infos.DateTimeContinuousAxisProperties.MajorStepUnit = TimeInterval.Day;
                    break;
                case 2:
                    Infos.DateTimeContinuousAxisProperties.MajorStep = 1;
                    Infos.DateTimeContinuousAxisProperties.MajorStepUnit = TimeInterval.Day;
                    break;
                default:
                    break;
            }
        }
        #endregion

    }
    #region 图表页面各项数据类

    /// <summary>
    /// 封装图表1 DateTimeContinuousAxis坐标轴使用的属性
    /// </summary>
    public class Chart1DateTimeContinuousAxisProperties : ObservableObject
    {
        private TimeInterval majorStepUnit;
        private double majorStep;
        //图表1 坐标轴时间线结束值
        private DateTime maxnumDateTime = DateTime.Now;
        //图表1 坐标轴时间线起始值
        private DateTime minnumDateTime = DateTime.Now.Date;
        private string chartTitle = "Today's request results";

        public string ChartTitle
        {
            get { return chartTitle; }
            set { chartTitle = value; RaisePropertyChanged(() => ChartTitle); }
        }
        
        /// <summary>
        /// 第一个图表的横轴单位
        /// </summary>
        public TimeInterval MajorStepUnit
        {
            get => majorStepUnit;
            set
            {
                majorStepUnit = value;
                RaisePropertyChanged(() => MajorStepUnit);
            }
        }
        /// <summary>
        /// 第一个图表的横轴坐标之间的间隔
        /// </summary>
        public double MajorStep
        {
            get => majorStep;
            set
            {
                majorStep = value;
                RaisePropertyChanged(() => MajorStep);
            }
        }

        public DateTime MaxnumDateTime
        {
            get { return maxnumDateTime; }
            set { maxnumDateTime = value; RaisePropertyChanged(() => MaxnumDateTime); }
        }

        public DateTime MinnumDateTime
        {
            get { return minnumDateTime; }
            set { minnumDateTime = value; RaisePropertyChanged(() => MinnumDateTime); }
        }

        public Chart1DateTimeContinuousAxisProperties()
        {
            MajorStep = 1;
            MajorStepUnit = TimeInterval.Hour;
        }
    }

    //第一个图表的图例
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

    //第一个图表,线性图数据类
    public class Chart1:ObservableObject
    {

        //发起请求时间
        private DateTime requestTime;

        public DateTime RequestTime
        {
            get { return requestTime; }
            set { requestTime = value; RaisePropertyChanged(() => RequestTime); }
        }
        //站点请求回复时间
        private Double responseTime;

        public Double ResponseTime
        {
            get { return responseTime; }
            set { responseTime = value; RaisePropertyChanged(() => ResponseTime); }
        }
        //记录请求结果
        private string result;

        public string Result
        {
            get { return result; }
            set { result = value; RaisePropertyChanged(() => Result); }
        }
    }
    //柱形图（列表）数据类
    public class BarChartData : ObservableObject
    {
        //站点类型
        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged(() => Type); }
        }
        //站点名
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
        //以下为站点请求结果
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

        //图表1 时间坐标轴 时隙(两个相邻时间差)及单位(Hour,Day)等属性
        private Chart1DateTimeContinuousAxisProperties dateTimeContinuousAxisProperties
            =new Chart1DateTimeContinuousAxisProperties();

        //数据库日志
        private List<Log> logs;
        //选择站点
        private ObservableCollection<SelectSite> selectSites;
        //获取被选中的站点
        private List<Site> sites;
        //柱形图数据
        private ObservableCollection<BarChartData> barChart;
        //图表1所有系列集合
        ObservableCollection<ObservableCollection<Chart1>> chart1CollectionCopy;
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

        public List<Log> Logs
        {
            get => logs;
            set
            {
                logs = value;
                RaisePropertyChanged(() => Logs);
            }
        }

        public List<Site> Sites
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

        public ObservableCollection<ObservableCollection<Chart1>> Chart1CollectionCopy
        {
            get { return chart1CollectionCopy; }
            set
            {
                chart1CollectionCopy = value;
                RaisePropertyChanged(() => Chart1CollectionCopy);
            }
        }

        public Chart1DateTimeContinuousAxisProperties DateTimeContinuousAxisProperties
        {
            get { return dateTimeContinuousAxisProperties; }
            set { dateTimeContinuousAxisProperties = value; RaisePropertyChanged(() => DateTimeContinuousAxisProperties); }
        }
        #endregion
    }
    #endregion
}
