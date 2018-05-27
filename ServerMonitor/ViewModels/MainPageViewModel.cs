using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using ServerMonitor.Models;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using ServerMonitor.Views;
using GalaSoft.MvvmLight;
using ServerMonitor.Controls;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Util;
using ServerMonitor.Services.RequestServices;
using ServerMonitor.ViewModels.BLL;
using ServerMonitor.SiteDb;
using Windows.UI.Xaml.Data;
using ServerMonitor.LogDb;

namespace ServerMonitor.ViewModels
{
    public class MainPageViewModel : Template10.Mvvm.ViewModelBase
    {
        private int rightTapped_SiteId;  //右击站点id
        List<SiteModel> sites; //只在GetListSite()增加和删除其元素个数
        int order = 1;  //1:id As 2:id De 3:Al As 4:Al De
        int filter = 2; //0:Error  1:Normal  2:All Servers,

        //MainPage 界面的弹出框
        ContentDialog termsOfUseContentDialog = null;
        private SiteModel preCheck;  //preCheck站点
        public MainPageViewModel()
        {
            T = 200;
        }
        #region 绑定数据
        private string preCheckName;  //preCheck的名字
        public string PreCheckName
        {
            get => preCheckName;
            set
            {
                preCheckName = value;
                RaisePropertyChanged(() => PreCheckName);
            }
        }

        private string preCheckColor;  //preCheck的颜色
        public string PreCheckColor
        {
            get => preCheckColor;
            set
            {
                preCheckColor = value;
                RaisePropertyChanged(() => PreCheckColor);
            }
        }
        private string preCheckResult;  //preCheck的最近一次请求信息
        public string PreCheckResult  
        {
            get => preCheckResult;
            set
            {
                preCheckResult = value;
                RaisePropertyChanged(() => PreCheckResult);
            }
        }

        //只在GetListSite()增加和删除其元素个数
        //监听的站点列表
        private ObservableCollection<SiteItem> siteItems = new ObservableCollection<SiteItem>();
        public ObservableCollection<SiteItem> SiteItems { get => siteItems; set => siteItems = value; }

        //只在GetListSite()修改元素值 #D13438 #4682B4 #5D5A58 #f7630c，红蓝灰橙
        //站点列表的最近一次请求信息的分类统计信息 显出在站点列表下方
        private List<SiteResult> siteResults = new List<SiteResult>()//Red：错误，Orange：警告，Gray：未知，Blue：成功
        {
            new SiteResult{Color="#4682B4", Name="Success:"},
            new SiteResult{Color="#D13438", Name="Error  :"},
            new SiteResult{Color="#f7630c", Name="Warning:"},
            new SiteResult{Color="#5D5A58", Name="Unknown:"}
        };
        public List<SiteResult> SiteResults { get => siteResults; set => siteResults = value; }

        //站点的宕机信息列表，显示在右上方
        private ObservableCollection<OutageSite> outageSites = new ObservableCollection<OutageSite>();
        public ObservableCollection<OutageSite> OutageSites { get => outageSites; set => outageSites = value; }

        //站点性能信息列表 显示在右下方
        private ObservableCollection<SitePerformance> sitePerformanceList = new ObservableCollection<SitePerformance>();
        public ObservableCollection<SitePerformance> SitePerformanceList { get => sitePerformanceList; set => sitePerformanceList = value; }

        //Set Apdex T，<=T是顾客满意的请求时间单位ms
        private int t;
        public int T
        {
            get => t;
            set
            {
                t = value;
                RaisePropertyChanged(() => T);
            }
        }

        private bool requestAsyncStat = false;
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
        #endregion 绑定数据

        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GetListSite();
            //DispatcherTimeSetup();
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }
        #endregion 系统函数

        #region 响应事件
        /// <summary>
        /// 进行过滤的按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Filter_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menu = (MenuFlyoutItem)e.OriginalSource;
            filter = int.Parse(menu.Tag.ToString());
            GetListSite();
        }

        /// <summary>
        /// 进行排序的按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Order_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menu = (MenuFlyoutItem)e.OriginalSource;
            order = int.Parse(menu.Tag.ToString());
            GetListSite();
        }
        /// <summary>
        /// 用于precheck方法
        /// </summary>
        public async Task<bool> Pre_Check()
        {
            SiteModel _preCheckSite = DBHelper.GetSiteById(4);//初始化precheck记录，目前是ID 4 
            MessageRemind toast = new MessageRemind();
            //对google dns进行pre check (8.8.8.8)
            //IcmpRequest request = new IcmpRequest(IPAddress.Parse("8.8.8.8"));
            //var data = request.DoRequest();//发起请求 data可以指示值
            //RequestObj requestObj;//用于存储icmp请求结果的对象              
            //requestObj = DataHelper.GetProperty(request);
            //_preCheckSite.Last_request_result = int.Parse(requestObj.Color);//更新color
            //_preCheckSite.Request_count = _preCheckSite.Request_count + 1;//更新请求次数
            //_preCheckSite.Request_interval = requestObj.TimeCost;
            //if (data == false)
            //{
            //    toast.ShowToast(_preCheckSite);
            //    _preCheckSite.Last_request_result = int.Parse("0");//更新color 
            //}
            //int up = -1;
            //while (up == -1)
            //{
            //    up = DBHelper.UpdateSite(_preCheckSite);//更新站点
            //}
            string baiduDomain = "www.baidu.com";//设定一个网站供dns服务器进行解析
            DNSRequest pre = new DNSRequest(IPAddress.Parse(_preCheckSite.Site_address), baiduDomain);
            bool dnsFlag = await pre.MakeRequest();
            //请求完毕
            if ("1000".Equals(pre.Status))
            {
                //dns正常
                _preCheckSite.Is_success = 1;
            }
            else if ("1001".Equals(pre.Status))
            {
                //unknown
                _preCheckSite.Is_success = 2;
            }
            else if ("1002".Equals(pre.Status))
            {
                //timeout
                _preCheckSite.Is_success = -1;
            }
            _preCheckSite.Request_TimeCost = pre.TimeCost;
            _preCheckSite.Request_count += 1;
            if (dnsFlag == false)
            {
                SiteDaoImpl impl = new SiteDaoImpl();
                impl.SetAllSiteStatus(2);///所有站点置为unknown
                //消息提醒
                _preCheckSite.Is_success = 0;
                toast.ShowToast(_preCheckSite);                          
            }
            DBHelper.UpdateSite(_preCheckSite);
            GetListSite();//更新ui
            return dnsFlag;
        }
        /// <summary>
        /// 请求所有服务的按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void RequestAll_Click(object sender, RoutedEventArgs e)
        {
            // P操作 禁用刷新按钮
            (sender as AppBarButton).IsEnabled = false;
            RequestAsyncStat = true;
            MessageRemind toast = new MessageRemind();
            //首先进行precheck 
            bool pre =  await Pre_Check();
            if (pre)
            {
                // 引入封装的工具类  --xb
                SiteDetailUtilImpl util = new SiteDetailUtilImpl();
                // 引入封装的SiteDAO
                SiteDaoImpl siteDao = new SiteDaoImpl();
                var sitelist = SiteItems;//获取sitelist
                int leng = sitelist.Count;
                //遍历sitelist 根据协议进行请求
                for (int i = 0; i < leng; i++)
                {
                    // 获取站点对象   --xb
                    SiteModel siteElement = siteDao.GetSiteById(sitelist[i].Id);
                    // 创建用于记录此次请求的Log对象   --xb
                    LogModel log = null;
                    if (!siteElement.Is_Monitor)
                    {
                        continue;
                    }
                    IPAddress _siteAddress_redress = await util.GetIPAddressAsync(siteElement.Site_address);
                    switch (siteElement.Protocol_type)//根据协议请求站点
                    {
                        // HTTP/HTTPS协议请求   --xb
                        // 目前HTTP与HTTPS没有做协议请求上的区分  --xb
                        case "HTTPS":
                        case "HTTP":
                            try
                            {
                                // 发起HTTP请求，生成请求记录并更新站点信息  --xb
                                log = await util.RequestHTTPSite(siteElement, HTTPRequest.Instance);
                            }
                            catch (Exception ex)
                            {
                                DBHelper.InsertErrorLog(ex);
                                log = null;
                            }
                            break;
                        // DNS协议请求   --xb
                        case "DNS":
                            // 发起DNS请求，生成请求记录并更新站点信息  --xb
                            log = await util.AccessDNSServer(siteElement, DNSRequest.Instance);
                            break;
                        // ICMP协议请求   --xb
                        case "ICMP":
                            ICMPRequest icmp = new ICMPRequest(_siteAddress_redress);
                            // 发起ICMP请求，生成请求记录并更新站点信息  --xb
                            log = await util.ConnectToServerWithICMP(siteElement, icmp);
                            break;
                        // FTP协议请求   --xb
                        case "FTP":
                            // 发起FTP请求，生成请求记录并更新站点信息  --xb
                            log = await util.AccessFTPServer(siteElement, FTPRequest.Instance);
                            break;
                        // SMTP协议请求   --xb
                        case "SMTP":
                            // 发起SMTP请求，生成请求记录并更新站点信息  --xb
                            SMTPRequest _smtpRequest = new SMTPRequest(siteElement.Site_address, siteElement.Server_port);
                            log = await util.AccessSMTPServer(siteElement, _smtpRequest);
                            break;
                        // 补充之前欠缺的Socket服务器请求   --xb
                        case "SOCKET":                            
                            // 初始化Socket请求对象
                            SocketRequest _socketRequest = new SocketRequest
                            {
                                TargetEndPoint = new IPEndPoint(_siteAddress_redress, siteElement.Server_port)
                            };
                            // 请求指定终端，并生成对应的请求记录，最后更新站点信息
                            log = await util.ConnectToServerWithSocket(siteElement, _socketRequest);
                            break;
                        // 补充之前欠缺的SSH服务器请求   --xb
                        case "SSH":
                            log = await util.AccessSSHServer(siteElement, new SSHRequest(siteElement.Site_address, SshLoginType.Anonymous));
                            break;
                        default:
                            break;
                    }
                    if (null != log)
                    {
                        // 将请求的记录插入数据库  --xb
                        LogDaoImpl logDao = new LogDaoImpl();
                        logDao.InsertOneLog(log);
                        // 如果请求失败，提醒用户  --xb
                        if (log.Is_error)
                        {
                            // 消息提醒  --xb
                            toast.ShowToast(siteElement);
                        }
                    }
                    // 说明此次请求处于异常状态，记录进数据库中
                    else
                    {
                        DBHelper.InsertErrorLog(new Exception("Insert Log failed!Beacuse log to insert is null"));
                    }
                }
                GetListSite();
            }
            // V操作 启用刷新按钮
            (sender as AppBarButton).IsEnabled = true;
            RequestAsyncStat = false;
        }

        /// <summary>
        /// add server点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Add_Server(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.AddServerPage), "1,-1"); //1MainPage, 2 AllServerPage; -1没有id是新建site
        }

        /// <summary>
        /// add website点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Add_Website(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.AddWebsitePage), "1,-1"); //1MainPage, 2 AllServerPage; -1没有id是新建site
        }

        /// <summary>
        /// 站点列表点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SiteList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int x = (sender as GridView).SelectedIndex;
            if (x >= 0)
            {
                string siteId = ((SiteItem)((sender as GridView).Items[x])).Id + "";
                NavigationService.Navigate(typeof(Views.SiteDetail), siteId);
                (sender as GridView).SelectedIndex = -1;
            }
        }
        /// <summary>
        /// 站点列表右击列表 得到右击的站点id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool SiteList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            string siteId;
            Grid grid;
            //右击时，点击的地点不同，触发的事件源的相关控件也不同。要进行判断
            if (e.OriginalSource is Grid)
            {
                grid = (Grid)(e.OriginalSource);
                if (!(grid.Children[1] is TextBlock))
                {
                    return false;
                }
            }
            else if (e.OriginalSource is TextBlock)
            {
                grid = (Grid)((TextBlock)e.OriginalSource).Parent;
            }
            else
            {
                grid = (Grid)((Image)e.OriginalSource).Parent;
            }
            siteId = ((TextBlock)(grid.Children[1])).Text.ToString();
            rightTapped_SiteId = int.Parse(siteId);
            return true;
        }

        /// <summary>
        /// 站点列表右击后的点击复制事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CopyFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var q1 = from t in sites
                     where t.Id == rightTapped_SiteId
                     select t;
            SiteModel site = q1.First();
            site.Site_name = site.Site_name + " Copy";
            site.Is_success = 2;

            site.Create_time = DateTime.Now;
            site.Update_time = DateTime.Now;
            site.Is_pre_check = false;

            if (DBHelper.InsertOneSite(site) == 1)
            {
                GetListSite();
            }
        }

        /// <summary>
        /// 站点列表右击后的关闭站点监听事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CloseFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var q1 = from t in sites
                     where t.Id == rightTapped_SiteId
                     select t;
            q1.First().Is_Monitor = false;

            if (DBHelper.UpdateSite(q1.First()) == 1)
            {
                GetListSite();
            }
        }

        /// <summary>
        /// 站点列表右击后的点击删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeleteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var q = from t in SiteItems
                    where t.Id == rightTapped_SiteId
                    select t;
            var q1 = from t in sites
                     where t.Id == rightTapped_SiteId
                     select t;

            if (DBHelper.DeleteOneSite(q1.First().Id) == 1)
            {
                GetListSite();
            }
        }
        
        /// <summary>
        /// 弹出对话框，设置应用响应时间的最优门槛T 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async Task TextBlock_TappedAsync()
        {
            int temp = T;
            ContentDialogResult result = await termsOfUseContentDialog.ShowAsync();
            //点击Accept
            if (result == ContentDialogResult.Primary)
            {
                GetSitePerformance(sites); //刷新站点性能列表
            }
            else  //点击 Cancel执行
            {
                T = temp;
            }
        }
        #endregion 响应事件

        #region 辅助函数
        /// <summary>
        /// 在界面加载完毕，可以交互时，被MainPage.xaml.cs的Loaded方法调用
        /// </summary>
        public void Loaded(ContentDialog contentDialog)
        {
            termsOfUseContentDialog = contentDialog;
        }

        /// <summary>
        /// 获取数据 并统一刷新界面数据
        /// </summary>
        private void GetListSite()   //不可测
        {
            SiteItems.Clear();  //清空其数据
            for (int i = 0; i < 4; i++)
            {
                SiteResults[i].Number = 0;
            }
            sites = DBHelper.GetAllSite();
            List<SiteModel> q = ProcessSite(sites);
            GetOutageSite(sites);
            GetSitePerformance(sites);

            //PreCheck：针对其显示做处理
            PreCheckColor = GetSiteItemColor(preCheck.Is_success);
            PreCheckResult = GetSiteItemLastResult(preCheck);
            PreCheckName = preCheck.Site_name;

            //循环把数据添加在SiteItems（列表）中
            for (int i = 0; i < q.Count; i++)
            {
                string color = GetSiteItemColor(q[i].Is_success); //得到在UI上对应站点颜色
                string result = GetSiteItemLastResult(q[i]);              //得到在UI上对应站点信息
                if (q[i].Is_server)
                {
                    SiteItems.Add(new SiteItem()
                    {
                        Id = q[i].Id,
                        Site_name = q[i].Site_name,
                        Is_server = q[i].Is_server,
                        Image_path = "/Images/ic_server.png",
                        ResultColor = color,
                        Last_result = result,
                        Protocol_type = q[i].Protocol_type,
                        Site_address = q[i].Site_address,
                    });
                    ChangeSiteResult(q[i].Is_success);//在for循环内累加站点统计信息
                }
                else
                {
                    SiteItems.Add(new SiteItem()
                    {
                        Id = q[i].Id,
                        Site_name = q[i].Site_name,
                        Is_server = q[i].Is_server,
                        Image_path = "/Images/ic_website.png",
                        ResultColor = color,
                        Last_result = result,
                        Protocol_type = q[i].Protocol_type,
                        Site_address = q[i].Site_address,
                    });
                    ChangeSiteResult(q[i].Is_success);
                }
            }
        }

        /// <summary>
        /// 加工处理从数据库得到的站点信息并返回 --筛选，排序  得到并筛选掉 pre-Check
        /// </summary>
        /// <param name="list">站点列表</param>
        /// <returns>有序的站点列表</returns>
        private List<SiteModel> ProcessSite(List<SiteModel> list) //不可测
        {
            List<SiteModel> q;
            if (filter == 2)  //0:Error  1:Normal  2:All Servers, 筛选
            {
                q = (from t in list
                     where t.Is_Monitor == true
                     select t).ToList();
            }
            else if (filter == 1)//正常1
            {
                q = (from t in list
                     where t.Is_Monitor == true
                     where t.Is_success == 1
                     select t).ToList();
            }
            else
            {
                q = (from t in list
                     where t.Is_Monitor == true
                     where t.Is_success != 1
                     select t).ToList();
            }
            switch (order)  //1:id As 2:id De 3:字母 As 4:字母 De 排序
            {
                case 1:
                    q = (from t in q
                         orderby t.Id ascending
                         select t).ToList();
                    break;
                case 2:
                    q = (from t in q
                         orderby t.Id descending
                         select t).ToList();
                    break;
                case 3:
                    q = (from t in q
                         orderby t.Site_name ascending
                         select t).ToList();
                    break;
                case 4:
                    q = (from t in q
                         orderby t.Site_name descending
                         select t).ToList();
                    break;
                default:
                    break;
            }

            // 从list（数据库） 得到 pre-Check
            preCheck = (from t in list
                        where t.Is_pre_check == true
                        select t).ToList()[0];

            // 筛选掉 pre-Check
            q = (from t in q
                 where t.Is_pre_check == false
                 select t).ToList();

            return q;
        }

        /// <summary>
        /// 得到宕机的站点信息并按时间先后排序，显示在右上方 保存在OutageSites中
        /// </summary>
        /// <param name="list">站点列表</param>
        private void GetOutageSite(List<SiteModel> list) //不可测
        {
            OutageSites.Clear();

            //排除不监听和precheck，再只留错误和超时的
            List<SiteModel> q = (from t in list
                            where t.Is_Monitor == true && t.Is_pre_check == false && (t.Is_success == 0 || t.Is_success == -1)
                            orderby t.Update_time descending
                            select t).ToList();
            // 循环判断站点超时还是错误
            for (int i = 0; i < q.Count && i < 7; i++)
            {
                //Red：0错误，Orange：-1超时，Gray：2未知，Blue：1成功
                //#D13438 #4682B4 #5D5A58 #f7630c，红蓝灰橙
                if (q[i].Is_success == 0)
                {
                    OutageSites.Add(new OutageSite()
                    {
                        LastTime = string.Format("{0:MM-dd HH:mm:ss}", q[i].Update_time),
                        Site_name = q[i].Site_name,
                        Color = "#D13438",
                    });
                }
                else
                {
                    OutageSites.Add(new OutageSite()
                    {
                        LastTime = string.Format("{0:MM-dd HH:mm:ss}", q[i].Update_time),
                        Site_name = q[i].Site_name,
                        Color = "#f7630c",
                    });
                }
            }
        }

        /// <summary>
        /// 得到站点性能信息并按性能排序，从log表中取数据分析 显示在右下方 保存在SitePerformanceList中
        /// </summary>
        /// <param name="list">站点列表</param>
        private void GetSitePerformance(List<SiteModel> list) //不可测
        {
            SitePerformanceList.Clear();

            //得到监听的站点信息（除pre_check）
            List<SiteModel> q = (from t in list
                            where t.Is_Monitor == true && t.Is_pre_check == false
                            select t).ToList();
            List<SitePerformance> container = new List<SitePerformance>();
            //循环计算并记录站点性能信息
            for (int i = 0; i < q.Count; i++)
            {
                List<LogModel> logList = DBHelper.GetLogsBySiteId(q[i].Id);
                if (logList.Count == 0)
                {
                    container.Add(new SitePerformance()
                    {
                        Site_name = q[i].Site_name,
                        Apdex = 0.0,
                    });
                    continue;
                }
                var w = from t in logList   //总的采样 距现在3天内
                        where DateTime.Now.Subtract(t.Create_Time).Days < 3
                        select t;
                int total = w.Count(); //总的采样个数
                if (total == 0)
                {
                    container.Add(new SitePerformance()
                    {
                        Site_name = q[i].Site_name,
                        Apdex = 0.0,
                    });
                    continue;
                }

                var x = from t in w
                        where t.TimeCost <= T
                        select t;
                int satis = x.Count();   //满意的样本个数

                var y = from t in w
                        where t.TimeCost <= 4 * T && t.TimeCost > T
                        select t;
                int toler = y.Count();          //可忍受样本个数
                container.Add(new SitePerformance()
                {
                    Site_name = q[i].Site_name,
                    Apdex = (satis + toler / 2.0) / total,
                });
            }
            container = (from t in container   //对记录排序
                         orderby t.Apdex descending
                         select t).ToList();
            for (int i = 0; i < container.Count; i++)  //记录排序结果
            {
                container[i].Ranking = "No." + (i + 1);
                SitePerformanceList.Add(container[i]);
            }
        }
        
        /// <summary>
        /// 深度克隆站点 （大部分克隆）
        /// </summary>
        /// <param name="site">站点</param>
        /// <returns>克隆站点</returns>
        private SiteModel CloneSite(SiteModel site)   //可测
        {
            SiteModel cs = new SiteModel()
            {
                Site_name = site.Site_name,
                Site_address = site.Site_address,
                Is_server = site.Is_server,
                Protocol_type = site.Protocol_type,
                Server_port = site.Server_port,
                Monitor_interval = site.Monitor_interval,
                Is_Monitor = site.Is_Monitor,
                Create_time = DateTime.Now,
                Update_time = DateTime.Now,
                Is_pre_check = false,
                Request_succeed_code = site.Request_succeed_code
            };
            return cs;
        }
        /// <summary>
        /// 修改站点情况统计信息 相应站点情况（如Success）数量 +1
        /// </summary>
        /// <param name="last_request_result">最近站点请求结果</param>
        private void ChangeSiteResult(int last_request_result)   //不可测
        {
            switch (last_request_result)
            {
                case 1:
                    SiteResults[0].Number ++;
                    break;
                case 0:
                    SiteResults[1].Number ++;
                    break;
                case -1:
                    SiteResults[2].Number ++;
                    break;
                case 2:
                    SiteResults[3].Number ++;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 返回站点在UI上对应颜色
        /// </summary>
        /// <param name="last_request_result">最近站点请求结果</param>
        /// <returns>颜色</returns>
        private string GetSiteItemColor(int last_request_result)    //可测
        {
            string color = "#5D5A58"; //#D13438 #4682B4 #5D5A58 #f7630c，红蓝灰橙
            switch (last_request_result) //Red：0错误，Orange：-1超时，Gray：2未知，4682B4：1成功  
            {
                case 0:
                    color = "#D13438";
                    break;
                case 1:
                    color = "#4682B4";
                    break;
                case 2:
                    color = "#5D5A58";
                    break;
                case -1:
                    color = "#f7630c";
                    break;
                default:       //取默认
                    break;
            }
            return color;
        }


        /// <summary>
        /// 返回最近站点访问信息
        /// </summary>
        /// <param name="site">站点</param>
        /// <returns>站点信息</returns>
        private string GetSiteItemLastResult(SiteModel site) //  能测 但有点复杂
        {
            string result = "";
            if (site.Is_server)  //判断是不是服务器
            {
                switch (site.Is_success) //0错误，-1超时，2未知，1成功
                {
                    case 0:
                        result = "Error in " + site.Request_TimeCost + "ms";
                        break;
                    case 1:
                        result = "Port" + site.Server_port + " (open) in " + site.Request_TimeCost + "ms";
                        break;
                    case 2:
                        result = "Unknown";
                        break;
                    case -1:
                        result = "Timeout in " + site.Request_TimeCost + "ms";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (site.Is_success)
                {
                    case 0:
                        result = "Error in " + site.Request_TimeCost + "ms";
                        break;
                    case 1:
                        result = site.Status_code + " (OK) in " + site.Request_TimeCost + "ms";
                        break;
                    case 2:
                        result = "Unknown";
                        break;
                    case -1:
                        result = "Timeout in " + site.Request_TimeCost + "ms";
                        break;
                    default:
                        break;
                }
            }

            return result;
        }
        #endregion 辅助函数
    }

    //站点情况统计
    public class SiteResult : ObservableObject
    {
        string name;
        string color;
        int number;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(() => Name);
            }
        }
        public string Color
        {
            get => color;
            set
            {
                color = value;
                RaisePropertyChanged(() => Color);
            }
        }
        public int Number
        {
            get => number;
            set
            {
                number = value;
                RaisePropertyChanged(() => Number);
            }
        }
        public SiteResult()
        {
            number = 0;
        }
    }

    //宕机站点信息
    public class OutageSite : ObservableObject
    {
        string site_name;
        string color;    //文本字体颜色
        string lastTime;  //最近更新时间
        public string Site_name
        {
            get => site_name;
            set
            {
                site_name = value;
                RaisePropertyChanged(() => Site_name);
            }
        }
        public string Color
        {
            get => color;
            set
            {
                color = value;
                RaisePropertyChanged(() => Color);
            }
        }
        public string LastTime
        {
            get => lastTime;
            set
            {
                lastTime = value;
                RaisePropertyChanged(() => LastTime);
            }
        }
    }

    //站点详细信息
    public class SiteItem : ObservableObject
    {
        int id;
        string site_name;
        bool is_server;
        string last_result;
        string image_path;
        string resultColor;
        string protocol_type;
        string site_address;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                RaisePropertyChanged(() => Id);
            }
        }
        public string Site_name
        {
            get => site_name;
            set
            {
                site_name = value;
                RaisePropertyChanged(() => Site_name);
            }
        }
        public bool Is_server
        {
            get => is_server;
            set
            {
                is_server = value;
                RaisePropertyChanged(() => Is_server);
            }
        }
        public string Last_result
        {
            get => last_result;
            set
            {
                last_result = value;
                RaisePropertyChanged(() => Last_result);
            }
        }
        public string Image_path
        {
            get => image_path;
            set
            {
                image_path = value;
                RaisePropertyChanged(() => Image_path);
            }
        }
        public string ResultColor
        {
            get => resultColor;
            set
            {
                resultColor = value;
                RaisePropertyChanged(() => ResultColor);
            }
        }


        public string Protocol_type
        {
            get => protocol_type;
            set
            {
                protocol_type = value;
                RaisePropertyChanged(() => Protocol_type);
            }
        }
        public string Site_address
        {
            get => site_address;
            set
            {
                site_address = value;
                RaisePropertyChanged(() => Site_address);
            }
        }
        public SiteItem()
        {
            Id = 1;
            Site_name = "google";
            Is_server = true;
            Last_result = "Unknown";
            Image_path = "/Images/ic_server.png";
            ResultColor = "#5D5A58";
        }
    }

    //站点性能信息
    public class SitePerformance : ObservableObject
    {
        string site_name;
        double apdex;  //性能指标得分
        string ranking;  //排名
        public string Site_name
        {
            get => site_name;
            set
            {
                site_name = value;
                RaisePropertyChanged(() => Site_name);
            }
        }
        public double Apdex
        {
            get => apdex;
            set
            {
                apdex = value;
                RaisePropertyChanged(() => Apdex);
            }
        }
        public string Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                RaisePropertyChanged(() => Ranking);
            }
        }
    }
}
