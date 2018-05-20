using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using ServerMonitor.Models;
using GalaSoft.MvvmLight;
using ServerMonitor.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Telerik.Charting;
using Telerik.UI.Xaml.Controls.Chart;
using GalaSoft.MvvmLight.Threading;
using ServerMonitor.ViewModels.BLL;

namespace ServerMonitor.ViewModels
{
    /// <summary>
    /// Created by fjl
    /// </summary>
    public class ChartPageViewModel : Template10.Mvvm.ViewModelBase
    {
        #region 变量
        const int MAX_NUMBER_OF_SITE = 5;
        const int ERROR_CODE = 4;

        public IChartUtil ChartDao { get; set; }

        //为柱状图绑定的站点ID提供显示的站点名
        public static List<SiteModel> siteModels = new List<SiteModel>();

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
                    TypeChanged(type);
                    RaisePropertyChanged(() => Type);
                }
            }
        }

        private int pivotIndex;
        public int PivotIndex
        {
            get { return pivotIndex; }
            set
            {
                if (pivotIndex != value)
                {
                    pivotIndex = value;RaisePropertyChanged(() => PivotIndex);
                }
            }
        }

        //图表1数据序列集合（前台绑定的非此属性，而是可变副本）
        private ObservableCollection<ObservableCollection<Chart1>> chart1Collection;

        public ObservableCollection<ObservableCollection<Chart1>> Chart1Collection
        {
            get { return chart1Collection; }
            set { chart1Collection = value; RaisePropertyChanged(() => Chart1Collection); }
        }
        #endregion

        //构造函数
        public ChartPageViewModel()
        {
            RequestResult = new List<string> {"All", "Success", "Error", "OverTime" };
            PivotIndex = 0;
            Infos = new SiteRequestCountInfo();
        }

        //跳转到页面触发
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //对象实例化
            var result = InitAsync();
            //加载数据库数据
            var log = await LoadDbLogAsync();
            var site = await LoadDbSiteAsync();
            var selectResult = await ChartDao.SelectSitesAsync(site);
            Infos.Sites =  selectResult.Item2;
            Infos.Logs = log;
            Infos.SelectSites = selectResult.Item1;

            //计算图表数据
            var getResult = await Task.Run(() => ChartDao.CacuChartAsync(Infos.Sites, Infos.Logs));
            Chart1Collection = getResult.Item1;
            Infos.BarChart = getResult.Item2;
            Lengend = await ChartDao.ChartLengendAsync(Infos.Sites);
            //默认显示全部
            Type = "All";
            //图表加载完毕后切换加载状态
            Infos.State3 = Visibility.Collapsed;
            Infos.State1 = Visibility.Visible;
        }

        #region 实例化及加载数据库数据
        //变量初始化
        public async Task<bool> InitAsync()
        {
            Lengend = new ObservableCollection<ChartLengend>();
            ChartDao = new ChartUtilImpl();
            Chart1Collection = new ObservableCollection<ObservableCollection<Chart1>>();
            Infos.Chart1CollectionCopy = new ObservableCollection<ObservableCollection<Chart1>>();
            Infos.SelectSites = new ObservableCollection<SelectSite>();
            Infos.Sites = new List<SiteModel>();
            Infos.Logs = new List<LogModel>();
            Infos.BarChart = new ObservableCollection<BarChartData>();
            await Task.CompletedTask;
            return true;
        }

        public async Task<List<SiteModel>> LoadDbSiteAsync()
        {
            await Task.CompletedTask;
            siteModels = DBHelper.GetAllSite();
            return siteModels;
        }
        public async Task<List<LogModel>> LoadDbLogAsync()
        {
            await Task.CompletedTask;
            List<LogModel> logs = new List<LogModel>();
            logs = DBHelper.GetAllLog();
            //数据排序，便于图表按序显示
            logs = logs.OrderBy(o => o.Create_time).ToList();
            return logs;
        }

        #endregion

        #region 响应事件
        /// <summary>
        /// 切换类型
        /// </summary>
        /// <param name="type">当前所选请求结果类型</param>
        public bool TypeChanged(string type)
        {
            if (Infos != null)
            {
                //重新计算需清空（前台所绑定属性）
                Infos.Chart1CollectionCopy.Clear();
                //根据所选类型从图表1序列集合中选择符合的数据
                foreach (var items in Chart1Collection)
                {
                    if (type.Equals("All"))
                    {
                        Infos.Chart1CollectionCopy.Add(items);
                    }
                    else
                    {
                        //每个站点序列
                        ObservableCollection<Chart1> dataItem = new ObservableCollection<Chart1>();
                        //请求结果符合当前所选类别时，添加到序列(Linq 查询表达式)
                        foreach (var item in items.Where(i => i.Result.Equals(type)).Select(i => i))
                        {
                            dataItem.Add(item);
                        }
                        //序列集合
                        Infos.Chart1CollectionCopy.Add(dataItem);
                    }  
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 切换至选择站点页面
        /// </summary>
        public void ChartFliterClick()
        {
            Infos.State1 = Visibility.Collapsed;
            Infos.State2 = Visibility.Visible;
        }

        /// <summary>
        /// 确定站点并切换页面
        /// </summary>
        public async Task<bool> AcceptClickAsync()
        {
            //清空数据，重新统计
            Infos.Sites.Clear();
            Lengend.Clear();
            Chart1Collection.Clear();
            Infos.BarChart.Clear();
            foreach (var item in Infos.SelectSites.Where(i => i.IsSelected == true).Select(i => i.Site))
            {
                //获取选择的站点
                Infos.Sites.Add(item);
            }
            //选择站点数量大于上限进行提示
            if (Infos.Sites.Count > MAX_NUMBER_OF_SITE)
            {
                var msgDialog = new Windows.UI.Popups.MessageDialog("站点最多选择五个！！") { Title = "错误提示" };
                await msgDialog.ShowAsync();
                return false;
            }
            else
            {
                Infos.State1 = Visibility.Visible;
                Infos.State2 = Visibility.Collapsed;

                //重新统计数据
                var getResult = await Task.Run(() => ChartDao.CacuChartAsync(Infos.Sites, Infos.Logs));
                Chart1Collection = getResult.Item1;
                Infos.BarChart = getResult.Item2;
                Lengend = await ChartDao.ChartLengendAsync(Infos.Sites);

                //统计完成后触发此方法，计算前台需要显示的数据
                TypeChanged(Type);
                return true;
            }
        }

        /// <summary>
        /// Pivot 切换时间区间时触发（当天，近三天，近一周）
        /// </summary>
        /// <returns>返回SelectedIndex</returns>
        public int PivotSelectionChanged()
        {
            int _selectedIndex = PivotIndex;
            switch (_selectedIndex)
            {
                case 0:
                    Infos.HAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.HAxisProperties.MinnumDateTime = DateTime.Now.AddHours(-23);
                    Infos.HAxisProperties.ChartTitle = "Results within the last 24 hours";
                    Infos.HAxisProperties.MajorStep = 4;
                    Infos.HAxisProperties.MajorStepUnit = TimeInterval.Hour;
                    return _selectedIndex;
                case 1:
                    Infos.HAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.HAxisProperties.MinnumDateTime = DateTime.Now.AddDays(-2);
                    Infos.HAxisProperties.ChartTitle = "Nearly three days of request results";
                    Infos.HAxisProperties.MajorStep = 1;
                    Infos.HAxisProperties.MajorStepUnit = TimeInterval.Day;
                    return _selectedIndex;
                case 2:
                    Infos.HAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.HAxisProperties.MinnumDateTime = DateTime.Now.AddDays(-6);
                    Infos.HAxisProperties.ChartTitle = "Nearly a week of request results";
                    Infos.HAxisProperties.MajorStep = 1;
                    Infos.HAxisProperties.MajorStepUnit = TimeInterval.Day;
                    return _selectedIndex;
                default:
                    break;
            }
            return ERROR_CODE;
        }
        #endregion
    }

    #region 图表标签格式化类
    //对数轴标签格式化
    public class CustomLogarithmicAxisLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            // The owner parameter is the Axis instance which labels are currently formatted
            var axis = owner as Axis;
            var con = Convert.ToInt32(content == null ? "" : content.ToString());
            if (con >= 1000)
            {
                content = con / 1000 + "s";
            }
            else
            {
                content = con + "ms";
            }
            return content.ToString();
        }
    }
    //时间轴标签格式化
    public class CustomDateTimeAxisLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            // The owner parameter is the Axis instance which labels are currently formatted
            var axis = owner as DateTimeContinuousAxis;
            var con = Convert.ToDateTime(content);

            if (axis.MajorStepUnit != TimeInterval.Hour) //当时间间隔为天时，格式化显示天
            {
                var con_str = String.Format("{0:MM-dd HH:mm}", con);
                return con_str;
            }
            else //否则只显示小时
            {
                var con_str = String.Format("{0:HH:mm}", con);
                return con_str;
            }
        }
    }
    //分类轴标签格式化（柱状图）
    public class CustomCategoricalAxisLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            // The owner parameter is the Axis instance which labels are currently formatted
            var axis = owner as DateTimeContinuousAxis;
            var siteId = Convert.ToInt32(content == null ? "" : content.ToString());
            foreach (var item in ChartPageViewModel.siteModels.Where(i => i.Id == siteId).Select(i => i))
            {
                return "#" + item.Id + " " + item.Site_name;
            }
            return "UnknownID" + content.ToString();
        }
    }
    #endregion

    #region 图表页面各项数据类
    /// <summary>
    /// 封装图表1 DateTimeContinuousAxis坐标轴使用的属性
    /// </summary>
    public class Chart1HAxisProperties : ObservableObject
    {
        private TimeInterval majorStepUnit;
        private double majorStep;
        //图表1 坐标轴时间线结束值
        private DateTime maxnumDateTime = DateTime.Now;
        //图表1 坐标轴时间线起始值
        private DateTime minnumDateTime = DateTime.Now.Date;
        private string chartTitle;

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

        public Chart1HAxisProperties()
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

        private SiteModel site;

        public SiteModel Site
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
        //站点id
        private string siteId;

        public string SiteId
        {
            get { return siteId; }
            set
            {
                siteId = value;
                RaisePropertyChanged(() => SiteId);
            }
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
        //以下为站点请求结果
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

        //图表1 时间坐标轴 时隙(两个相邻时间差)及单位(Hour,Day)等属性
        private Chart1HAxisProperties hAxisProperties
            =new Chart1HAxisProperties();

        //数据库日志
        private List<LogModel> logs;
        //选择站点
        private ObservableCollection<SelectSite> selectSites;
        //获取被选中的站点
        private List<SiteModel> sites;
        //柱形图数据
        private ObservableCollection<BarChartData> barChart;
        //图表1所有系列集合
        ObservableCollection<ObservableCollection<Chart1>> chart1CollectionCopy;
        #endregion

        #region 属性

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

        public List<LogModel> Logs
        {
            get => logs;
            set
            {
                logs = value;
                RaisePropertyChanged(() => Logs);
            }
        }

        public List<SiteModel> Sites
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

        public Chart1HAxisProperties HAxisProperties
        {
            get { return hAxisProperties; }
            set { hAxisProperties = value; RaisePropertyChanged(() => HAxisProperties); }
        }
        #endregion
    }
    #endregion
}
