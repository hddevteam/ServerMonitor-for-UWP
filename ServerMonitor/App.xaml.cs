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
using ServerMonitor.DAO;
using ServerMonitor.SiteDb;
using GalaSoft.MvvmLight.Threading;

namespace ServerMonitor
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : BootStrapper
    {
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
			var _sitelist = DBHelper.GetAllSite();
			var len = _sitelist.Count;//使用foreach会出现不在期望中的异常

			SiteModel _presite = new SiteModel();
			_presite = DBHelper.GetSiteById(4);//这里是指定了precheck的id为4
			var _precolor = _presite.Last_request_result;//如果percheck为错误 就不进行请求了
			if (_precolor != 0)
			{
				for (int i = 0; i < len; i++)
				{
					//对所有站点进行请求
					var item = _sitelist[i];
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
						SiteModel upSite = new SiteModel();
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
							SiteModel upSite = new SiteModel();
							upSite = DBHelper.GetSiteById(item.Id);
							try
							{
								upSite.Last_request_result = int.Parse(color);
								upSite.Status_code = status;
								upSite.Request_interval = int.Parse(time);
								DBHelper.UpdateSite(upSite);
							}
							catch { }
						}
					}
				}
			}
		}
	}
}
