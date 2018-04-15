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

namespace ServerMonitor.ViewModels
{
    public class MainPageViewModel : Template10.Mvvm.ViewModelBase
    {
        private int rightTapped_SiteId;
        List<Site> sites; //只在GetListSite()增加和删除其元素个数
        int order = 1;  //1:id As 2:id De 3:Al As 4:Al De
        int filter = 2; //0:Error  1:Normal  2:All Servers,

        private Site preCheck;
        public MainPageViewModel()
        {
            
        }
        #region 绑定数据
        private string preCheckName;
        public string PreCheckName
        {
            get => preCheckName;
            set
            {
                preCheckName = value;
                RaisePropertyChanged(() => PreCheckName);
            }
        }

        private string preCheckColor = "Red";
        public string PreCheckColor
        {
            get => preCheckColor;
            set
            {
                preCheckColor = value;
                RaisePropertyChanged(() => PreCheckColor);
            }
        }
        private string preCheckResult;
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
        private ObservableCollection<SiteItem> siteItems = new ObservableCollection<SiteItem>();
        public ObservableCollection<SiteItem> SiteItems { get => siteItems; set => siteItems = value; }

        //只在GetListSite()修改元素值 #D13438 #4682B4 #5D5A58 #f7630c，红蓝灰橙
        private List<SiteResult> siteResults = new List<SiteResult>()//Red：错误，Orange：警告，Gray：未知，Blue：成功
        {
            new SiteResult{Color="#4682B4", Name="Success:"},
            new SiteResult{Color="#D13438", Name="Error  :"},
            new SiteResult{Color="#f7630c", Name="Warning:"},
            new SiteResult{Color="#5D5A58", Name="Unknown:"}
        };
        public List<SiteResult> SiteResults { get => siteResults; set => siteResults = value; }
        
        private ObservableCollection<OutageSite> outageSites = new ObservableCollection<OutageSite>();
        public ObservableCollection<OutageSite> OutageSites { get => outageSites; set => outageSites = value; }

        private ObservableCollection<SitePerformance> sitePerformanceList = new ObservableCollection<SitePerformance>();
        public ObservableCollection<SitePerformance> SitePerformanceList { get => sitePerformanceList; set => sitePerformanceList = value; }
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
        public static void Pre_Check()
        {
            //对google dns进行pre check (8.8.8.8)
            var _googleDnsBack =  Request.IcmpRequest(IPAddress.Parse("8.8.8.8"));
            string _resultcolor = DataHelper.GetColor(_googleDnsBack);//得到返回值
            Site _preCheckSite = DBHelper.GetSiteById(4);//初始化precheck记录，目前是ID 4 
            _preCheckSite.Last_request_result = int.Parse(_resultcolor);//更新color
            _preCheckSite.Request_count = _preCheckSite.Request_count + 1;//更新请求次数
            DBHelper.UpdateSite(_preCheckSite);//更新站点
            MainPageViewModel _getlist = new MainPageViewModel();
            _getlist.GetListSite();//更新ui

        }       
        /// <summary>
        /// 进行刷新的按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Refresh_Click()
        {
            GetListSite();
        }
        /// <summary>
        /// 请求所有服务的按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void RequestAll_Click(object sender, RoutedEventArgs e)
        {
            var sitelist = SiteItems;//获取sitelist
            int leng = sitelist.Count;
            for (int i = 0; i < leng; i++)
            {
                var item = sitelist[i];
                var _siteid = item.Id;
                var _sitetype = item.Is_server;
                var _siteprotocol = item.Protocol_type;
                var _address = item.Site_address;
                if (_sitetype == true)
                {
                    //如果是服务器 需要发起ICMP请求 测试连通性
                    string url = _address;
                    IPAddress reIP;
                    //var debug = IPAddress.Parse(url);
                    //var test = IPAddress.TryParse(url, out reIP);
                    if (!IPAddress.TryParse(url, out reIP))
                    {
                        //如果输入的不是ip地址               
                        //通过域名解析ip地址
                        url = url.Substring(url.IndexOf('w'));//网址截取从以第一w
                        IPAddress[] hostEntry = await Dns.GetHostAddressesAsync(url);
                        for (int m = 0; m < hostEntry.Length; m++)
                        {
                            if (hostEntry[m].AddressFamily == AddressFamily.InterNetwork)
                            {
                                reIP = hostEntry[m];
                                break;
                            }
                        }
                    }
                    Dictionary<string, string> backData = new Dictionary<string, string>();

                    backData = Request.IcmpRequest(reIP);
                    Site upSite = new Site();
                    upSite = DBHelper.GetSiteById(item.Id);
                    var color = DataHelper.GetColor(backData);
                    var dictionary = backData;
                    var time = DataHelper.GetTime(backData);
                    
                    try
                    {
                        upSite.Last_request_result = int.Parse(color);
                        upSite.Request_interval = int.Parse(time);
                        DBHelper.UpdateSite(upSite);
                    }
                    catch { }                        
                    GetListSite();//更新站点列表              
                }
                else
                {
                    //不是服务器
                    if (Convert.ToBoolean(string.Compare("_siteprotocol", "HTTPS", true)) || Convert.ToBoolean(string.Compare("_siteprotocol", "HTTP", true)))
                    {
                        //需要发起HTTP请求
                        //需要域名传入
                        //发起http请求示例，传入网址，返回状态码和请求时间
                        string reback = await Request.HttpRequest(_address);
                        var color = DataHelper.GetHttpColor(reback);
                        var time = DataHelper.GetHttpTime(reback);
                        var status = DataHelper.GetHttpStatus(reback);
                        Site upSite = new Site();
                        upSite = DBHelper.GetSiteById(item.Id);
                        try
                        {
                            upSite.Last_request_result = int.Parse(color);
                            upSite.Status_code = status;
                            upSite.Request_interval = int.Parse(time);
                            DBHelper.UpdateSite(upSite);
                        }
                        catch { }
                        GetListSite();
                        //Request.GetHttpResponse(infos.Detail_Site.Site_address);
                    }
                }
            }
            var sitelist1 = SiteItems;
        }
        //add server点击事件
        public void Add_Server(object sender, RoutedEventArgs e)
        {
            ShowAddServerPage();
        }
        private void ShowAddServerPage()
        {
            var msgPopup = new AddServerPage();
            AddServerPage.ShowWindow();
        }
        //add website点击事件
        public void Add_Website(object sender, RoutedEventArgs e)
        {
            ShowAddWebsitePage();
        }
        private void ShowAddWebsitePage()
        {
            var msgPopup = new AddWebsitePage();
            AddWebsitePage.ShowWindow();
        }
        public void SiteList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int x = (sender as GridView).SelectedIndex;
            if(x>=0)
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
            if (e.OriginalSource is Grid)
            {
                grid = (Grid)(e.OriginalSource);
                if(!(grid.Children[1] is TextBlock))
                {
                    return false;
                }
            }
            else if(e.OriginalSource is TextBlock)
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

        public void CopyFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var q = from t in SiteItems
                    where t.Id == rightTapped_SiteId
                    select t;
            var q1 = from t in sites
                     where t.Id == rightTapped_SiteId
                     select t;
            Site site = CloneSite(q1.First());
            site.Site_name = site.Site_name + " Copy";
            site.Last_request_result = 2;
            if (DBHelper.InsertOneSite(site)==1)
            {
                GetListSite();
            }
        }
        public void CloseFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var q = from t in SiteItems
                    where t.Id == rightTapped_SiteId
                    select t;
            var q1 = from t in sites
                     where t.Id == rightTapped_SiteId
                     select t;
            q1.First().Is_Monitor = false;

            if (DBHelper.UpdateSite(q1.First()) == 1)
            {
                GetListSite();
            }
        }
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


        //DispatcherTimer dispatcherTimer;
        //int Timecount = 60000 * 5;//5分钟 定义一个计时器5分钟
        //public void DispatcherTimeSetup()
        //{
        //    dispatcherTimer = new DispatcherTimer();
        //    dispatcherTimer.Tick += DispatcherTimer_Tick;
        //    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);//设置方法间隔1s
        //    dispatcherTimer.Start();
        //}

        //private void DispatcherTimer_Tick(object sender, object e)
        //{
        //    //倒计时
        //    _countdown = "Next Refresh : "+((Timecount / 60000) % 60).ToString() + "分 " + ((Timecount / 1000) % 60).ToString() + "秒";
        //    RaisePropertyChanged(() => CountDown);
        //    Timecount -= 1000;
        //    if (Timecount == 0)
        //    {
        //        Timecount = 5 * 60000;
        //    }
        //}
        //private string _countdown = "Next Refresh : ";

        //public string CountDown
        //{
        //    get => _countdown;
        //    set
        //    {
        //        _countdown = value;
        //        RaisePropertyChanged(() => CountDown);
        //    }
        //}


        #endregion 响应事件

        #region 辅助函数
        //获取数据
        private void GetListSite()
        {
            SiteItems.Clear();
            for (int i = 0; i < 4; i++)
            {
                SiteResults[i].Number = 0;
            }
            sites = DBHelper.GetAllSite();
            List<Site> q = ProcessSite(sites);  //加工处理从数据库得到的站点信息  --筛选，排序
            GetOutageSite(sites);    //加工处理从数据库得到的站点信息，显示在右上方

            GetSitePerformance(sites);

            PreCheckColor = GetSiteItemColor(preCheck.Last_request_result);
            PreCheckResult = GetSiteItemLastResult(preCheck);
            PreCheckName = preCheck.Site_name;

            for (int i = 0; i < q.Count; i++)
            {
                string color = GetSiteItemColor(q[i].Last_request_result); //得到在UI上对应站点颜色
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
                    ChangeSiteResult(q[i].Last_request_result, 1);//在for循环内累加站点统计信息
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
                    ChangeSiteResult(q[i].Last_request_result, 1);
                }
            }
        }
        //加工处理从数据库得到的站点信息，显示在右上方
        private void GetOutageSite(List<Site> list)
        {
            OutageSites.Clear();

            //排除不监听和precheck，再只留错误和超时的
            List<Site> q = (from t in list
                            where t.Is_Monitor == true && t.Is_pre_check == false && (t.Last_request_result==0|| t.Last_request_result == -1)
                            orderby t.Update_time descending
                            select t).ToList();
            // 循环判断站点超时还是错误
            for (int i = 0; i < q.Count && i < 7; i++)
            {
                //Red：0错误，Orange：-1超时，Gray：2未知，Blue：1成功
                //#D13438 #4682B4 #5D5A58 #f7630c，红蓝灰橙
                if (q[i].Last_request_result==0)
                {
                    OutageSites.Add(new OutageSite()
                    {
                        LastTime = string.Format("{0:MM-dd HH:mm:ss}", q[i].Update_time),
                        Site_name =q[i].Site_name,
                        Color= "#D13438",
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

        //得到站点性能信息，从log表中取数据分析
        private void GetSitePerformance(List<Site> list)
        {
            int T = 200;
            SitePerformanceList.Clear();

            //得到监听的站点信息（除pre_check）
            List<Site> q = (from t in list
                            where t.Is_Monitor == true && t.Is_pre_check == false
                            select t).ToList();
            List<SitePerformance> container = new List<SitePerformance>();
            //循环计算并记录站点性能信息
            for (int i = 0; i < q.Count; i++)
            {
                List<Log> logList = DBHelper.GetLogsBySiteId(q[i].Id);
                if (logList.Count==0)
                {
                    container.Add(new SitePerformance()
                    {
                        Site_name = q[i].Site_name,
                        Apdex = 0.0,
                    });
                    continue;
                }
                var w = from t in logList   //总的采样
                        where DateTime.Now.Subtract(t.Create_time).Days < 3
                        select t;
                int total = w.Count(); //总的采样个数
                if (total==0)
                {
                    container.Add(new SitePerformance()
                    {
                        Site_name = q[i].Site_name,
                        Apdex = 0.0,
                    });
                    continue;
                }

                var x = from t in w
                        where t.Request_time <= T
                        select t;
                int satis = x.Count();   //满意的样本个数

                var y = from t in w
                        where t.Request_time <= 4 * T && t.Request_time > T
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

        //加工处理从数据库得到的站点信息  --筛选，排序
        private List<Site> ProcessSite(List<Site> list)
        {
            List<Site> q;
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
                     where t.Last_request_result == 1
                     select t).ToList();
            }
            else
            {
                q = (from t in list
                     where t.Is_Monitor == true
                     where t.Last_request_result != 1
                     select t).ToList();
            }
            switch (order)  //1:id As 2:id De 3:Al As 4:Al De 排序
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

        //深度克隆站点
        private Site CloneSite(Site site)
        {
            Site cs = new Site()
            {
                Site_name = site.Site_name,
                Site_address = site.Site_address,
                Is_server = site.Is_server,
                Protocol_type = site.Protocol_type,
                Server_port = site.Server_port,
                Monitor_interval = site.Monitor_interval,
                Is_Monitor = site.Is_Monitor,
                //Status_code = site.Status_code,
                //Request_interval = site.Request_interval,
                Create_time = DateTime.Now,
                Update_time = DateTime.Now,
                Is_pre_check = false,
                Request_succeed_code = site.Request_succeed_code
            };
            return cs;
        }
        //修改站点统计信息 一般是相应站点情况（如Success）数量 +1
        private void ChangeSiteResult(int last_request_result, int ch)
        {
            switch (last_request_result)
            {
                case 1:
                    SiteResults[0].Number += ch;
                    break;
                case 0:
                    SiteResults[1].Number += ch;
                    break;
                case -1:
                    SiteResults[2].Number += ch;
                    break;
                case 2:
                    SiteResults[3].Number += ch;
                    break;
                default:
                    break;
            }
        }
        //处理站点信息last_request_result 
        ////Red：0错误，Orange：-1超时，Gray：2未知，4682B4：1成功  
        //#D13438 #4682B4 #5D5A58 #f7630c，红蓝灰橙
        private string GetSiteItemColor(int last_request_result)
        {
            string color = "#5D5A58";
            switch (last_request_result)
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
                default:
                    break;
            }
            return color;
        }
        //处理站点信息last_request_result
        private string GetSiteItemLastResult(Site site)
        {
            string result = "";
            if(site.Is_server)
            {
                switch (site.Last_request_result)
                {
                    case 0:
                        result = "Error in " + site.Request_interval + "ms";
                        break;
                    case 1:
                        result = "Port"+site.Server_port + " (open) in " + site.Request_interval + "ms";
                        break;
                    case 2:
                        result = "Unknown";
                        break;
                    case -1:
                        result = "Timeout in " + site.Request_interval + "ms";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (site.Last_request_result)
                {
                    case 0:
                        result = "Error in " + site.Request_interval + "ms";
                        break;
                    case 1:
                        result = site.Status_code+" (OK) in " + site.Request_interval + "ms";
                        break;
                    case 2:
                        result = "Unknown";
                        break;
                    case -1:
                        result = "Timeout in " + site.Request_interval + "ms";
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
