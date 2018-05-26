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
using Windows.UI.Xaml.Data;
using ServerMonitor.SiteDb;
using ServerMonitor.LogDb;
using System.ComponentModel;
using System.Diagnostics;
/// <summary>
/// 创建：fjl  创建时间：2018/05/26 
/// </summary>
namespace ServerMonitor.ViewModels
{
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26 
    /// 图表界面的数据上下文
    /// </summary>
    public class ChartPageViewModel : Template10.Mvvm.ViewModelBase
    {
        #region 变量
        //最大选择站点数目
        const int MAX_NUMBER_OF_SITE = 5;
        //操作错误返回代码
        const int ERROR_CODE = 4;

        public IChartUtil ChartDao { get; set; }
        public ISiteDAO siteDao { get; set; }
        public ILogDAO logDao { get; set; }

        //为柱状图绑定的站点ID提供显示的站点名
        public static List<SiteModel> siteModels = new List<SiteModel>();

        private ChartSiteRequestCountInfo infos;

        public ChartSiteRequestCountInfo Infos
        {
            get { return infos; }
            set
            {
                infos = value;
                RaisePropertyChanged(() => Infos);
            }
        }
        private ObservableCollection<LineChartLengend> lengend;
        public ObservableCollection<LineChartLengend> Lengend
        {
            get { return lengend; }

            set { lengend = value; RaisePropertyChanged(() => Lengend); }
        }
        //请求结果类型
        public List<string> RequestResult { get; set; }
        //所选结果类型
        private string type;
        public string RequestResultType
        {
            get { return type; }
            set
            {
                if (value != null && value != type)
                {
                    type = value;
                    TypeChanged(type);
                    RaisePropertyChanged(() => RequestResultType);
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

        //线性表数据序列集合（前台绑定的非此属性，而是可变副本）
        private ObservableCollection<ObservableCollection<LineChartData>> lineChartCollection;

        public ObservableCollection<ObservableCollection<LineChartData>> LineChartCollection
        {
            get { return lineChartCollection; }
            set { lineChartCollection = value; RaisePropertyChanged(() => LineChartCollection); }
        }
        #endregion

        //构造函数
        public ChartPageViewModel()
        {
            RequestResult = new List<string> {"All", "Success", "Error", "OverTime" };
            PivotIndex = 0;
            Infos = new ChartSiteRequestCountInfo();
        }

        //跳转到页面触发
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //对象实例化
            var result = InitAsync();
            //加载数据库数据
            var log = await LoadDbLogAsync();
            var site = await LoadDbSiteAsync();
            var selectResult = await ChartDao.AddInfoForSiteAsync(site);
            Infos.SiteSelected = selectResult.Item2;
            Infos.Logs = log;
            Infos.SiteInfoCompleted = selectResult.Item1;

            //计算图表数据
            var getResult = await Task.Run(() => ChartDao.StatisticsSiteRequestResultAsync(Infos.SiteSelected, Infos.Logs));
            LineChartCollection = getResult.Item1;
            Infos.BarChart = getResult.Item2;
            Lengend = await ChartDao.SetLineChartLengendAsync(Infos.SiteSelected);
            //默认显示全部
            RequestResultType = "All";
            //图表加载完毕后切换加载状态
            Infos.State3 = Visibility.Collapsed;
            Infos.State1 = Visibility.Visible;
        }

        #region 实例化及加载数据库数据
        /// <summary>
        /// 变量初始化 创建：fjl
        /// </summary>
        /// <returns>返回true</returns>
        public async Task<bool> InitAsync()
        {
            Lengend = new ObservableCollection<LineChartLengend>();
            ChartDao = new ChartUtilImpl();
            siteDao = new SiteDaoImpl();
            logDao = new LogDaoImpl();
            LineChartCollection = new ObservableCollection<ObservableCollection<LineChartData>>();
            Infos.LineChartCollectionCopy = new ObservableCollection<ObservableCollection<LineChartData>>();
            Infos.SiteInfoCompleted = new ObservableCollection<AddSiteInfo>();
            Infos.SiteSelected = new List<SiteModel>();
            Infos.Logs = new List<LogModel>();
            Infos.BarChart = new ObservableCollection<BarChartData>();
            await Task.CompletedTask;
            return true;
        }
        /// <summary>
        /// 加载数据库站点 创建: fjl
        /// </summary>
        /// <returns>返回站点列表</returns>
        public async Task<List<SiteModel>> LoadDbSiteAsync()
        {
            await Task.CompletedTask;
            siteModels = siteDao.GetAllSite();
            return siteModels;
        }
        /// <summary>
        /// 加载数据库站点记录 创建: fjl
        /// </summary>
        /// <returns>返回记录列表</returns>
        public async Task<List<LogModel>> LoadDbLogAsync()
        {
            await Task.CompletedTask;
            List<LogModel> logs = new List<LogModel>();
            logs = logDao.GetAllLog();
            //数据排序，便于图表按序显示
            logs = logs.OrderBy(o => o.Create_Time).ToList();
            return logs;
        }

        #endregion

        #region 响应事件
        /// <summary>
        /// 切换类型 创建: fjl
        /// </summary>
        /// <param name="type">当前所选请求结果类型</param>
        public bool TypeChanged(string type)
        {
            if (Infos != null)
            {
                //重新计算需清空（前台所绑定属性）
                Infos.LineChartCollectionCopy.Clear();
                //根据所选类型从图表1序列集合中选择符合的数据
                foreach (var items in LineChartCollection)
                {
                    if (type.Equals("All"))
                    {
                        Infos.LineChartCollectionCopy.Add(items);
                    }
                    else
                    {
                        //每个站点序列
                        ObservableCollection<LineChartData> dataItem = new ObservableCollection<LineChartData>();
                        //请求结果符合当前所选类别时，添加到序列(Linq 查询表达式)
                        foreach (var item in items.Where(i => i.Result.Equals(type)).Select(i => i))
                        {
                            dataItem.Add(item);
                        }
                        //序列集合
                        Infos.LineChartCollectionCopy.Add(dataItem);
                    }  
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 切换至选择站点页面 创建: fjl
        /// </summary>
        public void ChartFliterClick()
        {
            Infos.State1 = Visibility.Collapsed;
            Infos.State2 = Visibility.Visible;
        }

        /// <summary>
        /// 确定站点并切换页面 创建: fjl
        /// </summary>
        public async Task<bool> AcceptClickAsync()
        {
            //清空数据，重新统计
            Infos.SiteSelected.Clear();
            Lengend.Clear();
            LineChartCollection.Clear();
            Infos.BarChart.Clear();
            foreach (var item in Infos.SiteInfoCompleted.Where(i => i.IsSelected == true).Select(i => i.Site))
            {
                //获取选择的站点
                Infos.SiteSelected.Add(item);
            }
            //选择站点数量大于上限进行提示
            if (Infos.SiteSelected.Count > MAX_NUMBER_OF_SITE)
            {
                var msgDialog = new Windows.UI.Popups.MessageDialog("站点最多选择五个！！") { Title = "错误提示" };
                await msgDialog.ShowAsync();
                return false;
            }
            else
            {
                Infos.State2 = Visibility.Collapsed;
                Infos.State3 = Visibility.Visible;
                Lengend = await ChartDao.SetLineChartLengendAsync(Infos.SiteSelected);
                //重新统计数据
                var getResult = await ChartDao.StatisticsSiteRequestResultAsync(Infos.SiteSelected, Infos.Logs);
                LineChartCollection = getResult.Item1;
                Infos.BarChart = getResult.Item2;
                Infos.State3 = Visibility.Collapsed;
                Infos.State1 = Visibility.Visible;
                //统计完成后触发此方法，计算前台需要显示的数据
                TypeChanged(RequestResultType);
                return true;
            }
        }

        /// <summary>
        /// Pivot 切换时间区间时触发（当天，近三天，近一周）创建: fjl
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
                    return _selectedIndex;
                case 1:
                    Infos.HAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.HAxisProperties.MinnumDateTime = DateTime.Now.AddDays(-2);
                    Infos.HAxisProperties.ChartTitle = "Nearly three days of request results";
                    return _selectedIndex;
                case 2:
                    Infos.HAxisProperties.MaxnumDateTime = DateTime.Now;
                    Infos.HAxisProperties.MinnumDateTime = DateTime.Now.AddDays(-6);
                    Infos.HAxisProperties.ChartTitle = "Nearly a week of request results";
                    return _selectedIndex;
                default:
                    break;
            }
            return ERROR_CODE;
        }
        #endregion
    }

    #region 图表标签格式化，颜色转换等工具类
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 图表数据点颜色转换器
    /// </summary>
    public class DataPointToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DataPoint point = value as DataPoint;
            if (point == null)
            {
                return value;
            }

            var series = point.Presenter as LineSeries;
            if (point.Parent == null || series == null)
            {
                return value;
            }

            return series.Stroke;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 对数轴标签格式化
    /// </summary>
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
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 时间轴标签格式化
    /// </summary>
    public class CustomDateTimeAxisLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            // The owner parameter is the Axis instance which labels are currently formatted
            var axis = owner as DateTimeContinuousAxis;
            var con = Convert.ToDateTime(content);

            if(axis.Title.ToString().Contains("three days") || axis.Title.ToString().Contains("week"))
            {
                var con_str = String.Format("{0:MM-dd HH:mm}", con);
                return con_str;
            }
            else
            {
                var con_str = String.Format("{0:HH:mm}", con);
                return con_str;
            }
        }
    }
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 分类轴标签格式化（柱状图）
    /// </summary>
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
    /// /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 封装线形图 DateTimeContinuousAxis坐标轴使用的属性
    /// </summary>
    public class LineChartHAxisProperties : ObservableObject
    {
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
    }
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 线形图的图例
    /// </summary>
    public class LineChartLengend : ObservableObject
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
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 完善站点信息
    /// </summary>
    public class AddSiteInfo : ObservableObject
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
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 线性图数据类
    /// </summary>
    public class LineChartData:ObservableObject
    {

        //发起请求时间
        private DateTime requestTime;

        public DateTime RequestTime
        {
            get { return requestTime; }
            set { requestTime = value; RaisePropertyChanged(() => RequestTime); }
        }
        //站点请求回复时间
        private Double? responseTime;

        public Double? ResponseTime
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
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 柱形图（列表）数据类
    /// </summary>
    public class BarChartData : ObservableObject
    {
        //站点类型
        private string address;

        public string Address
        {
            get { return address; }
            set { address = value; RaisePropertyChanged(() => Address); }
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
    /// <summary>
    /// 创建：fjl  创建时间：2018/05/26
    /// 页面主要数据封装类
    /// </summary>
    public class ChartSiteRequestCountInfo : ObservableObject
    {
        #region 变量
        //切换页面显示状态
        private Visibility state1 = Visibility.Collapsed;
        private Visibility state2 = Visibility.Collapsed;
        private Visibility state3 = Visibility.Visible;

        //图表1 时间坐标轴 时隙(两个相邻时间差)及单位(Hour,Day)等属性
        private LineChartHAxisProperties hAxisProperties
            =new LineChartHAxisProperties();

        //数据库日志
        private List<LogModel> logs;
        //完善站点信息
        private ObservableCollection<AddSiteInfo> siteInfoCompleted;
        //获取被选中的站点
        private List<SiteModel> siteSelected;
        //柱形图数据
        private ObservableCollection<BarChartData> barChart;
        //图表1所有系列集合
        ObservableCollection<ObservableCollection<LineChartData>> lineChartCollectionCopy;
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

        public List<SiteModel> SiteSelected
        {
            get { return siteSelected; }
            set { siteSelected = value; RaisePropertyChanged(() => SiteSelected); }
        }

        public ObservableCollection<AddSiteInfo> SiteInfoCompleted
        {
            get { return siteInfoCompleted; }
            set { siteInfoCompleted = value; RaisePropertyChanged(() => SiteInfoCompleted); }
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

        public ObservableCollection<ObservableCollection<LineChartData>> LineChartCollectionCopy
        {
            get { return lineChartCollectionCopy; }
            set
            {
                lineChartCollectionCopy = value;
                RaisePropertyChanged(() => LineChartCollectionCopy);
            }
        }

        public LineChartHAxisProperties HAxisProperties
        {
            get { return hAxisProperties; }
            set { hAxisProperties = value; RaisePropertyChanged(() => HAxisProperties); }
        }
        #endregion
    }
    #endregion
}
