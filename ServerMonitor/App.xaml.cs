using Windows.UI.Xaml;
using System.Threading.Tasks;
using ServerMonitor.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using Template10.Common;
using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using ServerMonitor.Controls;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using ServerMonitor.Models;
using ServerMonitor.Util;
using ServerMonitor.SiteDb;
using ServerMonitor.ViewModels;
using ServerMonitor.Services.RequestServices;
using ServerMonitor.ViewModels.BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerMonitor
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : BootStrapper
    {
        public MainPageViewModel ViewModel { get; private set; }

        public App()
        {
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);
            #region app settings

            // some settings must be set in app.constructor
            var settings = SettingsService.Instance;
            RequestedTheme = settings.AppTheme;
            CacheMaxDuration = settings.CacheMaxDuration;
            ShowShellBackButton = settings.UseShellBackButton;

            #endregion
        }

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Exclude);
            return new ModalDialog
            {
                DisableBackButtonWhenModal = true,
                Content = new Views.Shell(service),
                ModalContent = new Views.Busy(),
            };
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // TODO: add your long-running task here
            // 翻译下：把你需要长时间加载的代码放在这里
            #region 数据库Init
            // 加载XML文件
            XDocument document = XDocument.Load("Common/Config.xml");
            // 获取XML的根元素进行操作
            XElement root = document.Root;
            // 加载数据库名称
            XElement dbName = root.Element("DBFilename");
            DBHelper.InitDB(dbName.Value);
            DataBaseControlImpl.Instance.SetDBFilename(dbName.Value);
            #endregion
            await NavigationService.NavigateAsync(typeof(Views.MainPage));
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {            
            return base.OnInitializeAsync(args);
        }

		protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
		{
			base.OnBackgroundActivated(args);
			IBackgroundTaskInstance taskInstance = args.TaskInstance;
			BackGroundRequestTask(taskInstance);
		}

		private async void BackGroundRequestTask(IBackgroundTaskInstance taskInstance)
		{
            
            MessageRemind toast = new MessageRemind();//初始化消息提醒
			var sitelist = DBHelper.GetAllSite();
			var len = sitelist.Count;//使用foreach会出现不在期望中的异常
			SiteModel _presite = new SiteModel();
			_presite = DBHelper.GetSiteById(4);//这里是指定了precheck的id为4
			var _precolor = _presite.Last_request_result;//如果percheck为错误 就不进行请求了
			if (_precolor != 0)
            {
                //遍历sitelist 根据协议进行请求
                for (int i = 0; i < len; i++)
                {
                    SiteModel si = sitelist[i];
                    string _protocol = si.Protocol_type;
                    string _address = si.Site_address;
                    string url = _address;
                    bool _is_Monitor = si.Is_Monitor;
                    if (!_is_Monitor)
                    {
                        continue;
                    }
                    if (!IPAddress.TryParse(url, out IPAddress reIP))
                    {
                        //如果输入的不是ip地址               
                        //通过域名解析ip地址
                        //网址简单处理 去除http和https
                        var http = url.StartsWith("http://");
                        var https = url.StartsWith("https://");
                        if (http)
                        {
                            url = url.Substring(7);//网址截取从以第一w
                        }
                        else if (https)
                        {
                            url = url.Substring(8);//网址截取从以第一w
                        }
                        IPAddress[] hostEntry = await Dns.GetHostAddressesAsync(url);
                        for (int m = 0; m < hostEntry.Length; m++)
                        {
                            if (hostEntry[m].AddressFamily == AddressFamily.InterNetwork)
                            {
                                reIP = hostEntry[m];
                                break;
                            }
                        }
                    }//根据地址解析出ipv4地址
                    switch (_protocol)//根据协议请求站点
                    {
                        case "HTTPS":
                            HTTPRequest hTTPs = HTTPRequest.Instance;
                            hTTPs.ProtocolType = TransportProtocol.https;//更改协议类型
                            hTTPs.Uri = _address;
                            bool httpsFlag = await hTTPs.MakeRequest();
                            //请求完毕
                            //处理数据
                            si.Request_interval = hTTPs.TimeCost;
                            si.Request_count += 1;
                            if ("1002".Equals(hTTPs.Status))//定义的超时状态码
                            {
                                //请求超时
                                si.Last_request_result = -1;
                            }
                            else
                            {
                                SiteDetailViewModel util = new SiteDetailViewModel();//用于查看状态码
                                bool match = util.SuccessCodeMatch(si, hTTPs.Status);//匹配用户设定状态码
                                if (match)//匹配为成功  否则为失败
                                {
                                    si.Last_request_result = 1;
                                }
                                else
                                {
                                    si.Last_request_result = 0;
                                    toast.ShowToast(si);
                                }
                            }
                            break;
                        case "HTTP":
                            HTTPRequest hTTP = HTTPRequest.Instance;//默认协议类型http
                            hTTP.Uri = _address;
                            hTTP.ProtocolType = TransportProtocol.http;
                            bool httpFlag = await hTTP.MakeRequest();
                            //请求完毕
                            //处理数据
                            si.Request_interval = hTTP.TimeCost;
                            si.Request_count += 1;
                            if ("1002".Equals(hTTP.Status))
                            {
                                //请求超时
                                si.Last_request_result = -1;
                            }
                            else
                            {
                                SiteDetailUtilImpl util = new SiteDetailUtilImpl();
                                bool match = util.SuccessCodeMatch(si, hTTP.Status);//匹配用户设定状态码
                                if (match)
                                {
                                    si.Last_request_result = 1;
                                }
                                else
                                {
                                    si.Last_request_result = 0;
                                }
                            }
                            if (httpFlag == false)
                            {
                                toast.ShowToast(si);
                                si.Last_request_result = 0;
                            }
                            break;
                        case "DNS":
                            string baiduDomain = "www.baidu.com";//设定一个网站供dns服务器进行解析
                            DNSRequest dNS = new DNSRequest(reIP, baiduDomain);
                            bool dnsFlag = await dNS.MakeRequest();
                            //请求完毕
                            if ("1000".Equals(dNS.Status))
                            {
                                //dns正常
                                si.Last_request_result = 1;
                            }
                            else if ("1001".Equals(dNS.Status))
                            {
                                //unknown
                                si.Last_request_result = 2;
                            }
                            else if ("1002".Equals(dNS.Status))
                            {
                                //timeout
                                si.Last_request_result = -1;
                            }
                            si.Request_interval = dNS.TimeCost;
                            si.Request_count += 1;
                            if (dnsFlag == false)
                            {
                                //消息提醒
                                si.Last_request_result = 0;
                                toast.ShowToast(si);
                            }
                            break;
                        case "ICMP":
                            ICMPRequest icmp = new ICMPRequest(reIP);
                            bool icmpFlag = icmp.DoRequest();
                            //请求完毕
                            RequestObj requestObj;//用于存储icmp请求结果的对象              
                            requestObj = DataHelper.GetProperty(icmp);
                            si.Last_request_result = int.Parse(requestObj.Color);
                            si.Request_count += 1;
                            si.Request_interval = requestObj.TimeCost;
                            if (icmpFlag == false)
                            {
                                si.Last_request_result = 0;
                                toast.ShowToast(si);
                            }
                            break;
                        case "FTP":
                            var json = si.ProtocolIdentification;
                            JObject js = (JObject)JsonConvert.DeserializeObject(json);
                            //在此处加入type类型
                            string username = js["username"].ToString();
                            string password = js["password"].ToString();
                            FTPRequest fTP = new FTPRequest(LoginType.Identify)
                            {
                                FtpServer = reIP
                            };
                            fTP.Identification.Username = username;
                            fTP.Identification.Password = password;
                            bool ftpFlag = await fTP.MakeRequest();
                            //请求完毕
                            if ("1001".Equals(fTP.Status))
                            {
                                //置为错误
                                si.Last_request_result = 0;
                            }
                            else if ("1000".Equals(fTP.Status))
                            {
                                //置为成功
                                si.Last_request_result = 1;
                            }
                            else if ("1002".Equals(fTP.Status))
                            {
                                //超时异常
                                si.Last_request_result = -1;
                            }
                            si.Request_count += 1;
                            si.Request_interval = fTP.TimeCost;
                            if (ftpFlag == false)
                            {
                                si.Last_request_result = 0;
                                toast.ShowToast(si);
                            }
                            break;
                        case "SMTP":
                            SMTPRequest sMTP = new SMTPRequest(_address, si.Server_port);
                            bool smtpFlag = await sMTP.MakeRequest();
                            //请求完毕

                            if ("1000".Equals(sMTP.Status))
                            {
                                si.Last_request_result = 1;
                            }
                            else if ("1001".Equals(sMTP.Status))
                            {
                                si.Last_request_result = 0;
                            }
                            else if ("1002".Equals(sMTP.Status))
                            {
                                si.Last_request_result = -1;
                            }
                            si.Request_count += 1;
                            si.Request_interval = sMTP.TimeCost;
                            if (smtpFlag == false)
                            {
                                si.Last_request_result = 0;
                                toast.ShowToast(si);
                            }
                            break;
                        default:
                            break;
                    }
                    DBHelper.UpdateSite(si);
                }
            }
			
		}
	}
}
