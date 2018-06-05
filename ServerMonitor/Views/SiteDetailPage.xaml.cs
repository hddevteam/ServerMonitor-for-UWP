using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ServerMonitor.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SiteDetailPage : Page
    {

        public SiteDetailPage()
        {
            InitializeComponent();
            // 确认界面是否采用缓存
            NavigationCacheMode = NavigationCacheMode.Disabled;            
        }                
    }
}
