using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Controls;
using ServerMonitor.DAO;
using ServerMonitor.LogDb;
using ServerMonitor.Models;
using ServerMonitor.Services.RequestServices;
using ServerMonitor.SiteDb;
using ServerMonitor.ViewModels.BLL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Charting;
using Telerik.UI.Xaml.Controls.Chart;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

/// <summary>
/// 创建：xb  创建时间：2018/01/25  修改：xb  最后一次修改时间：2018/05/26
/// 站点详情界面对应的上下文
/// </summary>
namespace ServerMonitor.ViewModels
{
    /// <summary>
    /// 创建：xb 创建时间：2018/01/25 修改：xb
    /// </summary>
    public class SiteDetailViewModel : Template10.Mvvm.ViewModelBase
    {
        #region 变量声明
        /// <summary>
        /// 用来保存由刷新产生的最新的一条记录   属性
        /// </summary>
        private LogModel refresh_log;
        /// <summary>
        /// 整个界面中封装的所有变量   属性
        /// </summary>
        private ViewInfo infos;
        /// <summary>
        /// 传进来的站点id    属性
        /// </summary>
        private string _SiteId = "Default";
        /// <summary>
        /// 接口 -> 联系人操作
        /// </summary>
        private IContactDAO ContactImpl;
        /// <summary>
        /// 点击查看详情的站点的ID   字段  ->  _SiteId
        /// </summary>
        public string SiteId { get { return _SiteId; } set { Set(ref _SiteId, value); RaisePropertyChanged(() => SiteId); } }
        /// <summary>
        /// 界面的信息  字段   ->  infos
        /// </summary>
        public ViewInfo Infos { get => infos; set => infos = value; }
        /// <summary>
        /// 字段  ->  refresh_log
        /// </summary>
        public LogModel Refresh_log { get => refresh_log; set { Set(ref refresh_log, value); RaisePropertyChanged(() => Refresh_log); } }
        /// <summary>
        /// 临时变量  站点id  ->  由字符串转换来的
        /// </summary>
        public int id = 0;
        /// <summary>
        /// 封装的SiteDetailViewModel使用的工具类
        /// </summary>
        private ISiteDetailUtil utilObject;
        /// <summary>
        /// 站点DAO
        /// </summary>
        private ISiteDAO siteDao;
        /// <summary>
        /// 请求记录DAO
        /// </summary>
        private ILogDAO logDao;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public SiteDetailViewModel()
        {
            // 初始化界面变量	
            Infos = new ViewInfo
            {
                // 初始化记录变量	
                Logs = new ObservableCollection<LogModel>(),
                SuccessLogs = new ObservableCollection<LogModel>(),
                FirstLineChartData = new ObservableCollection<LineChartData>(),
                FirstChartAxisProperties = new FirstChartAxisProperties()
            };
            // 初始化工具接口	
            utilObject = new SiteDetailUtilImpl();
            // 初始化Site的DAO操作接口	
            siteDao = new SiteDaoImpl();
            // 初始化Log的DAO操作接口	
            logDao = new LogDaoImpl();
            Debug.WriteLine("Construction function => SiteDetailViewModel();");
        }
        #region 初始化界面的函数部分
        /// <summary>
        /// 初始化生成数据  创建：xb
        /// </summary>
        public async Task InitData()
        {
            // 初始化引用的对象
            utilObject = new SiteDetailUtilImpl();
            siteDao = new SiteDaoImpl();
            logDao = new LogDaoImpl();

            // 计算时间坐标轴的起始时间以及终止时间
            DateTime start = DateTime.Now.AddDays(-1);
            DateTime end = DateTime.Now;
            infos.MaxmumDatetime = end;
            infos.MinmumDatetime = start;

            // 获取关于站点的信息
            Infos.Detail_Site = siteDao.GetSiteById(id);
            Infos.ContactCollection = new ObservableCollection<ContactModel>();
            Infos.IsMonitor = Infos.Detail_Site.Is_Monitor;
            Infos.IsWebSite = !Infos.Detail_Site.Is_server;
            Infos.Site_Address = new Uri(Infos.Detail_Site.Site_address, UriKind.RelativeOrAbsolute);

            // 初始化下面两个图表数据
            ClearSiteRequestCount();

            // 读取站点的记录
            InitLogsData();
            // 读取联系人的记录
            InitContactData();
            // 处理平均值和中位数
            InitAverageMedianValue();
            // 更新图表数据
            InitChartData();
            await Task.CompletedTask;
        }
        /// <summary>
        /// 初始化载入记录数据
        /// </summary>
        public void InitLogsData()
        {
            List<LogModel> l = logDao.GetLogsBySiteId(id);
            if (l.Count == 0)
            {
                Infos.LastRequest = new LogModel();
                Infos.LastRequestWords = "None Data !";
                Infos.PreviousRequestLog = new LogModel();
                Infos.PreviousRequestLogWords = "None Data!";
            }
            else
            {
                DateTime pioneerDate = l.First().Create_Time;
                foreach (var log in l)
                {
                    // 这里加上这句话是为把数据库里的Utc时间转换为LocalTime
                    log.Create_Time = log.Create_Time.ToLocalTime();
                    Infos.Logs.Add(log);
                    if (!log.Is_error)
                    {
                        Infos.SuccessLogs.Add(log);
                        object o = log.Create_Time.Subtract(pioneerDate).TotalMinutes;
                        // 大于两倍的请求周期
                        if (log.Create_Time.Subtract(pioneerDate).TotalMinutes > 30)
                        {
                            Infos.FirstLineChartData.Add(new LineChartData() { RequestTime = log.Create_Time.AddMinutes(-30), ResponseTime = null });
                        }
                        Infos.FirstLineChartData.Add(new LineChartData() { RequestTime = log.Create_Time, ResponseTime = log.TimeCost });
                        pioneerDate = log.Create_Time;
                    }
                }
                Infos.LastRequest = l.Last<LogModel>();
                infos.LastRequestWords = string.Format("{0} in {1} ms", Infos.LastRequest.Status_code, infos.LastRequest.TimeCost);
                if (l.Count > 1)
                {
                    infos.PreviousRequestLog = l[l.Count - 1];
                    Infos.PreviousRequestLogWords = string.Format("{0} in {1} ms", Infos.PreviousRequestLog.Status_code, infos.PreviousRequestLog.TimeCost);
                }
                else
                {
                    Infos.PreviousRequestLog = new LogModel();
                    Infos.PreviousRequestLogWords = "No Data!";
                }
            }
        }
        /// <summary>
        /// 初始化载入联系人数据
        /// </summary>
        public void InitContactData()
        {
            ContactImpl = new ContactDAOImpl();
            // 初始化封装的信息集合
            Infos.ContactCollection = new ObservableCollection<ContactModel>();
            List<ContactModel> contactList = ContactImpl.GetContactModelsBySiteId(infos.Detail_Site.Id);
            if (contactList.Count == 0)
            {
                Debug.WriteLine("无联系人!");
                Infos.ContactCollection.Add(new ContactModel() { Contact_name = "No Data!", Contact_email = "No Data!", Telephone = "No Data!" });
                Infos.IsContactEmpty = true;
            }
            else
            {
                foreach (var contact in contactList)
                {
                    Infos.ContactCollection.Add(contact);
                }
                Infos.IsContactEmpty = false;
            }
        }
        /// <summary>
        /// 初始化平均值与中位数
        /// </summary>
        public void InitAverageMedianValue()
        {
            // 如果有数据
            if (Infos.Logs.Count != 0)
            {
                UpdateBindLine();
            }
            else
            {
                Infos.MedianValue = 0;
                Infos.AverageValue = 0;
            }
        }
        /// <summary>
        /// 更新下面两个图表的数据
        /// </summary>
        public void InitChartData()
        {
            if (Infos.Logs.Count<LogModel>() == 0)
            {
                // 没有数据则显示无数据的提醒
                Infos.LastRequest = new LogModel();
                Infos.LastRequestWords = string.Format("No Datas ! ");
                Infos.PreviousRequestLog = new LogModel();
                Infos.LastRequestWords = string.Format("No Datas ! ");
            }
            else
            {
                #region 清空Re和PieInfo集合每一项的值
                foreach (var item in Infos.Re)
                {
                    item.Count = 0;
                }
                foreach (var item in Infos.Pieinfo)
                {
                    item.Count = 0;
                }
                #endregion
                foreach (var i in Infos.Logs)
                {
                    #region 更新数据
                    UpdateChart(i);
                    #endregion
                }
                // 更新上次请求记录
                Infos.LastRequest = Infos.Logs.Last<LogModel>();
                Infos.LastRequestWords = string.Format("{0} in {1} ms", Infos.LastRequest.Status_code, infos.LastRequest.TimeCost);
                if (Infos.Logs.Count > 1)
                {
                    infos.PreviousRequestLog = Infos.Logs[Infos.Logs.Count - 2];
                    Infos.PreviousRequestLogWords = string.Format("{0} in {1} ms", Infos.PreviousRequestLog.Status_code, infos.PreviousRequestLog.TimeCost);
                }
                else
                {
                    Infos.PreviousRequestLog = new LogModel();
                    Infos.PreviousRequestLogWords = "None Data!";
                }
            }
        }
        /// <summary>
        /// 初始化第二个图表的键值（纵坐标值以及横坐标值）
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<RequestCountInfo> InitRadCartesianChartData()
        {
            ObservableCollection<RequestCountInfo> r = new ObservableCollection<RequestCountInfo>
            {
                new RequestCountInfo() { RequestStatus = "1ms", Count = 0 },
                new RequestCountInfo() { RequestStatus = "30ms", Count = 0 },
                new RequestCountInfo() { RequestStatus = "100ms", Count = 0 },
                new RequestCountInfo() { RequestStatus = ">100ms", Count = 0 },
                new RequestCountInfo() { RequestStatus = "error", Count = 0 }
            };
            return r;
        }
        /// <summary>
        /// 初始化饼图的坐标轴信息
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<PieChartInfo> InitPieChartData()
        {
            ObservableCollection<PieChartInfo> pi = new ObservableCollection<PieChartInfo>
            {
                new PieChartInfo() { Ry = RequestType.SUCCESS, Count = 0 },
                new PieChartInfo() { Ry = RequestType.OVERTIME, Count = 0 },
                new PieChartInfo() { Ry = RequestType.ERROR, Count = 0 }
            };
            return pi;
        }
        #endregion
        #region 更新数据的一些操作方法
        /// <summary>
        /// 计算平均请求时间以及请求时间中位值
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        public Tuple<double, double> CountAverageMax(ObservableCollection<LogModel> logs)
        {
            if (logs.Count == 0)
            {
                return Tuple.Create<double, double>(0, 0);
            }
            // 构建一个存储请求时间的数组
            double[] request_array = new double[logs.Count];
            // 初始化数组
            request_array.Initialize();
            foreach (var l in logs)
            {
                request_array[logs.IndexOf(l)] = l.TimeCost;
            }
            // 对数组进行排序
            utilObject.QuickSort(ref request_array, 0, request_array.Length - 1);

            // 平均值
            double Average = request_array.Average();
            // 中位值
            double Middle = request_array[request_array.Length / 2];

            var t = Tuple.Create<double, double>(Average, Middle);

            return t;
        }
        /// <summary>
        /// 用来清空第二个图表所用的计数数据
        /// </summary>
        public void ClearSiteRequestCount()
        {
            #region 生成第二个图表与第三个图表的信息
            Infos.Re = InitRadCartesianChartData();
            Infos.Pieinfo = new ObservableCollection<PieChartInfo>();
            Infos.Pieinfo = InitPieChartData();
            #endregion
        }
        /// <summary>
        /// 发起请求主体
        /// </summary>
        /// <returns>请求结果Log</returns>
        public async Task<LogModel> MakeRequest()
        {
            LogModel log;
            if (Infos.IsWebSite)
            {
                try
                {
                    log = await utilObject.RequestHTTPSite(infos.Detail_Site, HTTPRequest.Instance);
                }
                catch (Exception ex)
                {
                    DBHelper.InsertErrorLog(ex);
                    log = null;
                }
            }
            else
            {
                try
                {
                    switch (Infos.Detail_Site.Protocol_type)
                    {
                        case "DNS":
                            // 这里需要简单处理下请求的内容
                            log = await utilObject.AccessDNSServer(infos.Detail_Site, DNSRequest.Instance);
                            break;
                        case "FTP":
                            log = await utilObject.AccessFTPServer(infos.Detail_Site, FTPRequest.Instance);
                            break;
                        case "SSH":
                            log = await utilObject.AccessSSHServer(infos.Detail_Site, new SSHRequest(infos.Detail_Site.Site_address, SshLoginType.Anonymous));
                            break;
                        case "SMTP":
                            log = await utilObject.AccessSMTPServer(infos.Detail_Site, new SMTPRequest(infos.Detail_Site.Site_address, infos.Detail_Site.Server_port));
                            break;
                        case "SOCKET":
                            log = await utilObject.ConnectToServerWithSocket(infos.Detail_Site, new SocketRequest());
                            break;
                        case "ICMP":
                            IPAddress ip = await utilObject.GetIPAddressAsync(Infos.Detail_Site.Site_address);
                            ICMPRequest icmp = new ICMPRequest(ip);
                            log = await utilObject.ConnectToServerWithICMP(infos.Detail_Site, icmp);
                            break;
                        default:
                            log = null;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    DBHelper.InsertErrorLog(ex);
                    log = null;
                }
            }
            return log;
        }
        /// <summary>
        /// 更新绑定的中位数以及平均数
        /// </summary>
        public void UpdateBindLine()
        {
            Tuple<double, double> t = CountAverageMax(Infos.SuccessLogs);
            //Infos.AverageValue = Math.Log10(t.Item1);
            //Infos.MedianValue = Math.Log10(t.Item2);
            Infos.AverageValue = t.Item1;
            Infos.MedianValue = t.Item2;
        }
        /// <summary>
        /// 界面数据添加一条新的记录
        /// </summary>
        /// <param name="log"></param>
        public void AddNewLog(LogModel log)
        {
            if (logDao.InsertOneLog(log) == 1)
            {
                Debug.WriteLine("成功插入一条日志数据! 日志内容为：" + log.ToString());
                Infos.Logs.Add(log);
                if (!log.Is_error)
                {
                    Infos.SuccessLogs.Add(log);
                    if (log.Create_Time.Subtract(Infos.SuccessLogs.Last().Create_Time).TotalMinutes >= 30)
                    {
                        Infos.FirstLineChartData.Add(new LineChartData() { RequestTime = log.Create_Time, ResponseTime = null });
                    }
                    Infos.FirstLineChartData.Add(new LineChartData() { RequestTime = log.Create_Time, ResponseTime = log.TimeCost });
                }
            }
            else
            {
                Debug.WriteLine("插入失败，记录失败操作!");
                throw new Exception("插入请求日志操作失败!");
            }
            UpdateChart(log);
        }
        /// <summary>
        /// 插入一条记录的时候更新下面两个图表的信息
        /// </summary>
        /// <param name="log"></param>
        public void UpdateChart(LogModel log)
        {
            #region 添加第二个表格需要的数据
            if (log.Is_error)
            {
                Infos.Re[4].Count++;
            }
            else if (log.TimeCost <= 1)
            {
                Infos.Re[0].Count++;
            }
            else if (log.TimeCost <= 30)
            {
                Infos.Re[1].Count++;
            }
            else if (log.TimeCost <= 100)
            {
                Infos.Re[2].Count++;
            }
            else
            {
                Infos.Re[3].Count++;
            }
            #endregion

            #region 添加第三个表格所需的数据
            if (!log.Is_error)
            {
                Infos.Pieinfo[0].Count++;
            }
            else if ("1002".Equals(log.Status_code))
            {
                Infos.Pieinfo[1].Count++;
            }
            else
            {
                Infos.Pieinfo[2].Count++;
            }
            #endregion
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
                    Infos.FirstChartAxisProperties.Min.MajorStep = 1;
                    Infos.FirstChartAxisProperties.Min.MajorStepUnit1 = TimeInterval.Hour;
                    break;
                case 1:
                    Infos.FirstChartAxisProperties.Min.MajorStep = 4;
                    Infos.FirstChartAxisProperties.Min.MajorStepUnit1 = TimeInterval.Hour;
                    break;
                case 2:
                    Infos.FirstChartAxisProperties.Min.MajorStep = 8;
                    Infos.FirstChartAxisProperties.Min.MajorStepUnit1 = TimeInterval.Hour;
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region UI交互 Method集
        #region 界面跳转的方法
        // 别的界面跳转到这个界面的时候调用的方法
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {

            #region 处理id并存入界面变量
            SiteId = (suspensionState.ContainsKey(nameof(SiteId))) ? suspensionState[nameof(SiteId)]?.ToString() : parameter?.ToString();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                SiteId = "Designtime value";

            }
            int.TryParse(SiteId, out id);
            #endregion
            await InitData();
            await Task.CompletedTask;
            Infos.LoadAsyncStat = true;

            Debug.WriteLine("OnNavigatedToAsync();");
        }

        // 界面跳转，离开界面调用的第二个方法
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(SiteId)] = SiteId;
            }
            await Task.CompletedTask;
        }

        // 界面跳转，离开此界面调用的第一个方法
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }
        #endregion
        /// <summary>
        /// 界面加载的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Load_Loading(FrameworkElement sender, object args)
        {
            ChangeStepUnitStep(0);
            Debug.WriteLine("load_Loading() Excute!");
        }
        /// <summary>
        /// 绑定界面的ToggleSwitch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToggledChanged(object sender, RoutedEventArgs e)
        {
            ToggleSwitch t = sender as ToggleSwitch;

            Infos.Detail_Site.Is_Monitor = t.IsOn;
            siteDao.UpdateSite(Infos.Detail_Site);
        }
        /// <summary>
        /// Pivot 切换的时候触发的事件
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
                    Infos.MaxmumDatetime = DateTime.Now;
                    Infos.MinmumDatetime = DateTime.Now.AddDays(-1);
                    break;
                case 1:
                    Infos.MaxmumDatetime = DateTime.Now;
                    Infos.MinmumDatetime = DateTime.Now.AddDays(-3);
                    break;
                case 2:
                    Infos.MaxmumDatetime = DateTime.Now;
                    Infos.MinmumDatetime = DateTime.Now.AddDays(-7);
                    break;
                default:
                    break;
            }
            ChangeStepUnitStep(_selectedIndex);
        }
        /// <summary>
        /// 清空日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClearLog(object sender, RoutedEventArgs e)
        {
            // 删除数据库中记录
            logDao.DeleteLogsBySite(Infos.Detail_Site.Id);
            // 更新数据库站点的请求
            Infos.Detail_Site.Request_count = 0;
            siteDao.UpdateSite(Infos.Detail_Site);
            // 重新获取界面信息
            Infos.Logs.Clear();
            Infos.SuccessLogs.Clear();
            //Infos.RequestTimeList.Clear();
            #region 清空Re和PieInfo集合每一项的值
            foreach (var item in Infos.Re)
            {
                item.Count = 0;
            }
            foreach (var item in Infos.Pieinfo)
            {
                item.Count = 0;
            }
            #endregion
            Infos.MedianValue = 0;
            Infos.AverageValue = 0;
            InitChartData();
        }
        /// <summary>
        /// 刷新按钮的点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            // P操作 禁用刷新按钮
            Infos.RequestAsyncStat = false;
            LogModel log = await MakeRequest();
            // 确认返回的log有效
            if (null != log)
            {
                AddNewLog(log);
            }

            // 更新绑定数据
            if (Infos.Logs.Count != 0)
            {
                UpdateBindLine();
                // 更新上次请求记录
                Infos.LastRequest = Infos.Logs.Last();
                Infos.LastRequestWords = string.Format("{0} in {1} ms", Infos.LastRequest.Status_code, infos.LastRequest.TimeCost);
                if (Infos.Logs.Count > 1)
                {
                    infos.PreviousRequestLog = Infos.Logs[Infos.Logs.Count - 2];
                    Infos.PreviousRequestLogWords = string.Format("{0} in {1} ms", Infos.PreviousRequestLog.Status_code, infos.PreviousRequestLog.TimeCost);
                }
                else
                {
                    Infos.PreviousRequestLog = new LogModel();
                    Infos.PreviousRequestLogWords = "None Data!";
                }
            }
            else
            {
                Infos.MedianValue = 0;
                Infos.AverageValue = 0;
                Infos.LastRequest = new LogModel();
                Infos.LastRequestWords = "No Data!";
                Infos.PreviousRequestLog = new LogModel();
                Infos.PreviousRequestLogWords = "None Data!";
            }
            // V操作 启用刷新按钮
            Infos.RequestAsyncStat = true;
        }
        /// <summary>
        /// 点击编辑按钮跳转至编辑界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NavigateIntoEditPart(object sender, RoutedEventArgs e)
        {
            if (infos.IsWebSite)
            {
                NavigationService.Navigate(typeof(Views.AddWebsitePage), "2," + infos.Detail_Site.Id);
            }
            else
            {
                NavigationService.Navigate(typeof(Views.AddServerPage), "2," + infos.Detail_Site.Id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #endregion
        #region 没有用到方法集
        ///// <summary>
        ///// 测试提示框的方法  没有用到
        ///// </summary>
        //public async void TestMessageDailog()
        //{
        //    var messageBox = new Windows.UI.Popups.MessageDialog("提示框xxx") { Title = "提示框标题" };
        //    messageBox.Commands.Add(new Windows.UI.Popups.UICommand("第一个按钮的文字", uicommand =>
        //    {
        //        // 执行了第一按钮的点击事件
        //    }));
        //    await messageBox.ShowAsync();
        //}
        ///// <summary>
        ///// 请求服务器状态  没有用到
        ///// </summary>
        ///// <param name="serverProtocol"></param>
        ///// <returns></returns>
        //public async Task<LogModel> RequestServerIcmp(SiteModel site)
        //{
        //    LogModel log = null;
        //    try
        //    {
        //        log = new LogModel();
        //        IPAddress ip = await utilObject.GetIPAddressAsync(site.Site_address);
        //        Dictionary<string, string> datas = Request.IcmpRequest(ip);

        //        if (datas.Count == 1)
        //        {
        //            Debug.WriteLine("异常返回");
        //            throw new Exception("服务器请求失败！");
        //        }
        //        else
        //        {
        //            log.Create_Time = DateTime.Now;
        //            log.Site_id = site.Id;

        //            Debug.WriteLine("");
        //        }
        //    }
        //    catch (ArgumentNullException e)
        //    {
        //        log = null;
        //        Debug.WriteLine("获取服务器状态失败！原因是：未获取到返回信息");
        //        DBHelper.InsertErrorLog(e);
        //    }
        //    // 捕获超时异常!
        //    catch (SocketException e)
        //    {
        //        log = null;
        //        Debug.WriteLine(e.ToString());
        //        DBHelper.InsertErrorLog(e);
        //        return log;
        //    }
        //    catch (Exception e)
        //    {
        //        log = null;
        //        Debug.WriteLine("获取服务器状态失败！原因是：" + e.Message);
        //        DBHelper.InsertErrorLog(e);
        //    }
        //    return log;
        //}
        ///// <summary>
        ///// 请求服务器状态  没有使用!
        ///// </summary>
        ///// <param name="serverProtocol"></param>
        ///// <returns></returns>
        //public async Task<LogModel> RequestDNSServer(SiteModel site)
        //{
        //    LogModel log = null;

        //    if (null != site.Site_address && !("".Equals(site.Site_address)))
        //    {
        //        IPAddress ip = await utilObject.GetIPAddressAsync(site.Site_address);
        //        if (null == ip)
        //        {
        //            try
        //            {
        //                ip = IPAddress.Parse(site.Site_address);
        //            }
        //            catch (ArgumentException e)
        //            {
        //                Debug.WriteLine(e.ToString());
        //                DBHelper.InsertErrorLog(e);
        //                return null;
        //            }
        //        }
        //        IPEndPoint iPEndPoint = new IPEndPoint(ip, 53);
        //        Tuple<string, string, string, string> tuple = await Request.SocketRequest(iPEndPoint);
        //        #region 赋值log
        //        log = new LogModel
        //        {
        //            Site_id = site.Id,
        //            Create_Time = DateTime.Now
        //        };
        //        if ("200".Equals(tuple.Item1))
        //        {
        //            log.Is_error = false;
        //        }
        //        else
        //        {
        //            log.Is_error = true;
        //        }
        //        log.TimeCost = int.Parse(tuple.Item2);
        //        log.Status_code = tuple.Item1;
        //        log.Log_Record = tuple.Item4;
        //        #endregion

        //        // 更新站点信息
        //        Infos.Detail_Site.Status_code = log.Status_code;
        //        Infos.Detail_Site.Update_time = DateTime.Now;
        //        Infos.Detail_Site.Is_success = log.Is_error ? 0 : 1;
        //        Infos.Detail_Site.Request_TimeCost = int.Parse(tuple.Item2);
        //        Infos.Detail_Site.Request_count++;
        //        DBHelper.UpdateSite(infos.Detail_Site);
        //        Debug.WriteLine("请求了一次服务器!");
        //    }
        //    return log;
        //}
        ///// <summary>
        ///// 截取url部分判断是否能转换成ip  没有用到
        ///// </summary>
        ///// <param name="url"></param>
        ///// <returns></returns>
        //public async Task<IPAddress> GetIPAddress(string url)
        //{
        //    if (!IPAddress.TryParse(url, out IPAddress reIP))
        //    {
        //        //如果输入的不是ip地址               
        //        //通过域名解析ip地址
        //        url = url.Substring(url.IndexOf('w'));//网址截取从以第一w
        //        IPAddress[] hostEntry = await Dns.GetHostAddressesAsync(url);
        //        for (int m = 0; m < hostEntry.Length; m++)
        //        {
        //            if (hostEntry[m].AddressFamily == AddressFamily.InterNetwork)
        //            {
        //                reIP = hostEntry[m];
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        reIP = null;
        //    }
        //    return reIP;
        //}
        ///// <summary>
        ///// 请求网站，并存入一条记录  没有使用到
        ///// </summary>
        ///// <returns></returns>
        //public async Task<LogModel> RequestWebsite()
        //{
        //    // 定义需要的变量
        //    LogModel newLog = new LogModel();
        //    JObject result = null;
        //    string httpRequestStatus = "";
        //    int httpRequestInterval = 0;
        //    // 获取JSON格式的请求结果
        //    string httpResult = await Request.HttpRequest(infos.Detail_Site.Site_address);
        //    // 处理请求结果的数据
        //    try
        //    {
        //        result = JObject.Parse(httpResult);
        //        httpRequestStatus = result["StatusCode"].ToString();
        //        httpRequestInterval = int.Parse(result["RequestTime"].ToString());

        //        newLog.Status_code = httpRequestStatus;
        //        newLog.Site_id = id;
        //        newLog.TimeCost = httpRequestInterval;
        //        newLog.Create_Time = DateTime.Now;
        //        // 更新站点信息           
        //        Infos.Detail_Site.Status_code = httpRequestStatus;
        //        // 判断获取到的请求结果是不是请求成功
        //        newLog.Is_error = SuccessCodeMatch(Infos.Detail_Site, Infos.Detail_Site.Status_code);
        //        // 更新站点信息
        //        Infos.Detail_Site.Update_time = DateTime.Now;
        //        Infos.Detail_Site.Is_success = newLog.Is_error ? 0 : 1;
        //        Infos.Detail_Site.Request_TimeCost = httpRequestInterval;
        //        Infos.Detail_Site.Request_count++;
        //    }
        //    catch (JsonReaderException e)
        //    {
        //        DBHelper.InsertErrorLog(e);
        //        Debug.WriteLine(httpResult);
        //        // 返回值为自定义的错误内容
        //        CatchCustomReturned(httpResult);

        //    }
        //    catch (Exception e)
        //    {
        //        DBHelper.InsertErrorLog(e);
        //        Debug.WriteLine(httpResult);
        //        newLog = null;
        //    }
        //    finally
        //    {
        //        // 更新站点
        //        DBHelper.UpdateSite(Infos.Detail_Site);
        //    }

        //    return newLog;
        //}
        ///// <summary>
        ///// 接收自定义的错误返回并新增错误日志信息  没有用到
        ///// </summary>
        ///// <param name="customResult"></param>
        //public void CatchCustomReturned(string customResult)
        //{
        //    switch (customResult)
        //    {
        //        case "请求超时":
        //            Infos.Detail_Site.Is_success = -1;
        //            Infos.Detail_Site.Update_time = DateTime.Now;
        //            Infos.Detail_Site.Request_TimeCost = 5000;
        //            break;
        //        case "请求失败":
        //            Infos.Detail_Site.Is_success = 0;
        //            Infos.Detail_Site.Update_time = DateTime.Now;
        //            Infos.Detail_Site.Request_TimeCost = 5000;
        //            break;
        //        default:
        //            throw new ArgumentException("返回参数不合法!");
        //    }
        //}
        ///// <summary>
        ///// 查看是否满足用户提出的成功Code  没有使用到
        ///// </summary>
        ///// <param name="site"></param>
        ///// <param name="statusCode"></param>
        ///// <returns></returns>
        //public bool SuccessCodeMatch(SiteModel site, string statusCode)
        //{
        //    string[] successCodes = GetSuccStatusCode(site);
        //    foreach (var i in successCodes)
        //    {
        //        if (i.Equals(statusCode))
        //        {
        //            return false;

        //        }
        //    }
        //    return true;
        //}
        ///// <summary>
        ///// 获取服务器状态成功的状态码列表  没有使用到
        ///// </summary>
        ///// <param name="site"></param>
        ///// <returns></returns>
        //public string[] GetSuccStatusCode(SiteModel site)
        //{
        //    if (site.Request_succeed_code.Contains(','))
        //    {
        //        return site.Request_succeed_code.Split(',');
        //    }
        //    else
        //    {
        //        return new string[] { site.Request_succeed_code };
        //    }
        //}
        #endregion
    }

    #region 封装信息
    /// <summary>
    /// 请求类别
    /// </summary>
    public enum RequestType { SUCCESS, ERROR, OVERTIME };

    /// <summary>
    /// 封装用于第三个图表(饼图)的信息结构 创建人：xb 创建时间：2018/03
    /// </summary>
    public class PieChartInfo : ObservableObject
    {
        private RequestType ry;
        private int count;

        public RequestType Ry
        {
            get => ry; set
            {
                ry = value;
                RaisePropertyChanged(() => Ry);
            }
        }
        public int Count
        {
            get => count; set
            {
                count = value;
                RaisePropertyChanged(() => Count);
            }
        }
    }

    /// <summary>
    /// 封装用于第二个BarSeries表的信息结构  创建人：xb 创建时间：2018/03
    /// </summary>
    public class RequestCountInfo : ObservableObject
    {
        // 请求结果类型
        private string requestStatus;
        // 请求次数
        private int count;


        public int Count
        {
            get => count;
            set
            {
                count = value;
                RaisePropertyChanged(() => Count);
            }
        }
        public string RequestStatus
        {
            get => requestStatus;
            set
            {
                requestStatus = value;
                RaisePropertyChanged(() => RequestStatus);
            }
        }
    }

    /// <summary>
    /// 封装页面所需的所有信息 创建人：xb 创建时间：2018/03
    /// </summary>
    public class ViewInfo : ObservableObject
    {
        private bool isMonitor;
        private bool isWebSite;
        private SiteModel site;
        private ObservableCollection<LogModel> logs;
        private ObservableCollection<RequestCountInfo> re;
        private ObservableCollection<PieChartInfo> pieinfo;
        private DateTime maxmumDatetime;
        private DateTime minmumDatetime;
        private LogModel lastRequest;
        private string lastRequestWords;
        private double medianValue;
        private double averageValue;
        private FirstChartAxisProperties firstChartAxisProperties;
        private ObservableCollection<ContactModel> contactCollection;
        private bool loadAsyncStat = false;
        private bool requestAsyncStat = true;
        private bool isContactEmpty = true;
        //private ObservableCollection<ObservableCollection<LogModel>> logCollections;
        private ObservableCollection<LogModel> successLogs;
        private Uri site_Address;
        private LogModel previousRequestLog;
        private string previousRequestLogWords;
        private ObservableCollection<LineChartData> firstLineChartData;

        // 对应界面上的toggledSwitch 按钮的值，表示此站点是否正在监测
        public bool IsMonitor
        {
            get => isMonitor;
            set
            {
                isMonitor = value;
                Detail_Site.Is_Monitor = value;
            }
        }
        // 对应图标站点的详细信息
        public SiteModel Detail_Site
        {
            get => site;
            set
            {
                site = value;
                RaisePropertyChanged(() => Detail_Site);
            }
        }
        // 对应的图表上站点的访问记录
        public ObservableCollection<LogModel> Logs
        {
            get => logs;
            set
            {
                logs = value;
                RaisePropertyChanged(() => Logs);
            }
        }
        // 对应的信息
        public ObservableCollection<RequestCountInfo> Re
        {
            get => re; set
            {
                re = value;
                RaisePropertyChanged(() => Re);
            }
        }

        public ObservableCollection<PieChartInfo> Pieinfo
        {
            get => pieinfo; set
            {
                pieinfo = value;
                RaisePropertyChanged(() => Pieinfo);
            }
        }

        // 第一个表格的时间轴的最大值与最小值
        public DateTime MaxmumDatetime
        {
            get => maxmumDatetime;
            set
            {
                maxmumDatetime = value;
                RaisePropertyChanged(() => MaxmumDatetime);
            }
        }
        public DateTime MinmumDatetime
        {
            get => minmumDatetime;
            set
            {
                minmumDatetime = value;
                RaisePropertyChanged(() => MinmumDatetime);
            }
        }

        // 下面的网址是否能被点击
        public bool IsWebSite
        {
            get => isWebSite;
            set
            {
                isWebSite = value;
            }
        }

        // 记录上次请求的结果
        public LogModel LastRequest
        {
            get => lastRequest;
            set
            {
                lastRequest = value;
                RaisePropertyChanged(() => LastRequest);
            }
        }
        public string LastRequestWords
        {
            get => lastRequestWords;
            set
            {
                lastRequestWords = value;
                RaisePropertyChanged(() => LastRequestWords);
            }
        }

        // 中位数/平均数
        public double MedianValue
        {
            get => medianValue;
            set
            {
                medianValue = value;
                RaisePropertyChanged(() => MedianValue);
            }
        }
        public double AverageValue
        {
            get => averageValue;
            set
            {
                averageValue = value;
                RaisePropertyChanged(() => AverageValue);
            }
        }

        ///// <summary>
        ///// 请求时间列表
        ///// </summary>
        //public ObservableCollection<LogModel> RequestTimeList
        //{
        //    get => requestTimeList;
        //    set
        //    {
        //        requestTimeList = value;
        //        RaisePropertyChanged(() => RequestTimeList);
        //    }
        //}

        /// <summary>
        /// 第一个图表使用的数据
        /// </summary>
        public FirstChartAxisProperties FirstChartAxisProperties
        {
            get => firstChartAxisProperties;
            set
            {
                firstChartAxisProperties = value;
                RaisePropertyChanged(() => FirstChartAxisProperties);
            }
        }

        /// <summary>
        /// 联系人集合
        /// </summary>
        public ObservableCollection<ContactModel> ContactCollection
        {
            get => contactCollection;
            set
            {
                contactCollection = value;
                RaisePropertyChanged(() => ContactCollection);
            }
        }

        /// <summary>
        /// 异步加载的状态
        /// </summary>
        public bool LoadAsyncStat
        {
            get => loadAsyncStat;
            set
            {
                loadAsyncStat = value;
                RaisePropertyChanged(() => LoadAsyncStat);
            }
        }

        /// <summary>
        /// 异部请求状态
        /// </summary>
        public bool RequestAsyncStat
        {
            get => requestAsyncStat;
            set
            {
                requestAsyncStat = value;
                RaisePropertyChanged(() => RequestAsyncStat);
            }
        }
        /// <summary>
        /// 标识站点的Uri地址
        /// </summary>
        public Uri Site_Address { get => site_Address; set => site_Address = value; }
        /// <summary>
        /// 前一次的（上上次的）请求结果
        /// </summary>
        public LogModel PreviousRequestLog
        {
            get => previousRequestLog;
            set
            {
                previousRequestLog = value;
                RaisePropertyChanged(() => PreviousRequestLog);
            }
        }

        public string PreviousRequestLogWords
        {
            get => previousRequestLogWords;
            set
            {
                previousRequestLogWords = value;
                RaisePropertyChanged(() => PreviousRequestLogWords);
            }
        }
        public bool IsContactEmpty
        {
            get => isContactEmpty;
            set
            {
                isContactEmpty = value;
                RaisePropertyChanged(() => IsContactEmpty);
            }
        }
        /// <summary>
        /// 成功记录的集合
        /// </summary>
        public ObservableCollection<LogModel> SuccessLogs
        {
            get => successLogs;
            set
            {
                successLogs = value;
                RaisePropertyChanged(() => SuccessLogs);
            }
        }

        public ObservableCollection<LineChartData> FirstLineChartData
        {
            get => firstLineChartData;
            set
            {
                firstLineChartData = value;
                RaisePropertyChanged(() => FirstLineChartData);
            }
        }
    }

    /// <summary>
    /// 封装第一个图表坐标轴响应式变化属性 创建人：xb 创建时间：2018/04
    /// </summary>
    public class FirstChartAxisProperties : ObservableObject
    {
        private DateTimeContinuousAxisProperties min;
        private DateTimeContinuousAxisProperties mid;
        private DateTimeContinuousAxisProperties max;

        public DateTimeContinuousAxisProperties Min
        {
            get => min;
            set
            {
                min = value;
                RaisePropertyChanged(() => Min);
            }
        }
        public DateTimeContinuousAxisProperties Mid
        {
            get => mid;
            set
            {
                mid = value;
                RaisePropertyChanged(() => Mid);
            }
        }
        public DateTimeContinuousAxisProperties Max
        {
            get => max;
            set
            {
                max = value;
                RaisePropertyChanged(() => Max);
            }
        }

        public FirstChartAxisProperties()
        {
            Min = new DateTimeContinuousAxisProperties();
            Mid = new DateTimeContinuousAxisProperties();
            Max = new DateTimeContinuousAxisProperties();
        }
    }

    /// <summary>
    /// 封装DateTimeContinuousAxis坐标轴使用的属性 创建人：xb 创建时间：2018/04
    /// </summary>
    public class DateTimeContinuousAxisProperties : ObservableObject
    {
        private TimeInterval MajorStepUnit;
        private double majorStep;

        /// <summary>
        /// 第一个表格的横轴单位
        /// </summary>
        public TimeInterval MajorStepUnit1
        {
            get => MajorStepUnit;
            set
            {
                MajorStepUnit = value;
                RaisePropertyChanged(() => MajorStepUnit1);
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

        public DateTimeContinuousAxisProperties()
        {
            MajorStep = 6;
            MajorStepUnit = TimeInterval.Hour;
        }
    }
    #endregion
    #region 图表标签格式化类
    /// <summary>
    /// 对数轴标签格式化 创建人：xb 创建时间：2018/04
    /// </summary>
    public class CustomLogOperatorAxisLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            // The owner parameter is the Axis instance which labels are currently formatted
            var axis = owner as Axis;
            var con = content == null ? 0 : int.Parse(content.ToString());
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
    /// 时间轴标签格式化 创建人：xb 创建时间：2018/04
    /// </summary>
    public class CustomOperatorDateTimeAxisLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            // The owner parameter is the Axis instance which labels are currently formatted
            var axis = owner as DateTimeContinuousAxis;
            var con = Convert.ToDateTime(content);

            if (axis.MajorStepUnit != TimeInterval.Hour || axis.MajorStep >= 4) //当时间间隔为天时，格式化显示天
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
    /// <summary>
    /// 分类轴标签格式化（柱状图）创建人：xb 创建时间：2018/04
    /// </summary>
    public class CustomOperatorCategoricalAxisLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            // The owner parameter is the Axis instance which labels are currently formatted
            var axis = owner as DateTimeContinuousAxis;
            var siteId = Convert.ToInt32(content == null ? "" : content.ToString());
            foreach (var item in ChartPageViewModel.siteModels.Where(i => i.Id == siteId).Select(i => i))
            {
                return item.Site_name;
            }
            return "UnkUtcNownID" + content.ToString();
        }
    }
    #endregion
    #region 转换器
    /// <summary>
    /// 取对数转换器 创建人：xb 创建时间：2018/04
    /// </summary>
    public class Log10Convert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int requestTime = int.Parse(value.ToString());
            double log10RequestTime = Math.Log10(requestTime);
            return log10RequestTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 中位数以及平均数转换器 创建人：xb 创建时间：2018/04
    /// </summary>
    public class StringFormatConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string typeOfValue = parameter as string;
            double _value = double.Parse(value.ToString());
            if (parameter.ToString().Equals("Median"))
            {
                return string.Format("{0}：{1} ms", typeOfValue, (int)_value);
            }
            else
            {
                return string.Format("{0}：{1} ms\t\t\t", typeOfValue, (int)_value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 联系人输出转换器 创建人：xb 创建时间：2018/04
    /// </summary>
    public class ContactFormatConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format("{0}:\t{1}", parameter as string, value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 布尔值->Visibility 创建人：xb 创建时间：2018/04
    /// </summary>
    public class BoolToVisbilityConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool _value = bool.Parse(value.ToString());
            return _value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 布尔值取反 创建人：xb 创建时间：2018/04
    /// </summary>
    public class BoolRevertConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !bool.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
