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
    }
}
