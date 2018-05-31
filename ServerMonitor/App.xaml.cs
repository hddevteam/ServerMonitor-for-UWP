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
using ServerMonitor.LogDb;

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
            //DBHelper.InitDB(dbName.Value);
            DBHelper.SetDBFilename(dbName.Value);
            DataBaseControlImpl.Instance.InitDB(dbName.Value);
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
			var _precolor = _presite.Is_success;//如果percheck为错误 就不进行请求了
            // 引入封装的工具类  --xb
            SiteDetailUtilImpl util = new SiteDetailUtilImpl();
            if (_precolor != 0)
            {
                //遍历sitelist 根据协议进行请求
                for (int i = 0; i < len; i++)
                {
                    // 获取站点对象   --xb
                    SiteModel siteElement =sitelist[i];
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
                            SocketRequest _socketRequest = new SocketRequest();
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
            }
			
		}
	}
}
