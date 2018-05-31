using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace ServerMonitor.Views
{
    /// <summary>
    /// 用于aboutus窗口的显示  
    /// 2018/5/30   --wzp
    /// </summary>
    public sealed partial class AboutUs : UserControl
    {

        private Popup m_Popup;
        public static AboutUs Instance
        {
            get
            {
                return Nested.instance;
            }
        }
        //实例化窗口
        private AboutUs()
        {
            this.InitializeComponent();
            m_Popup = new Popup();
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            m_Popup.Child = this;
            this.Loaded += MessagePopupWindow_Loaded;
            this.Unloaded += MessagePopupWindow_Unloaded;
        }
        private void MessagePopupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += MessagePopupWindow_SizeChanged; ;
        }

        private void MessagePopupWindow_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.Width = e.Size.Width;
            this.Height = e.Size.Height;
        }

        private void MessagePopupWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= MessagePopupWindow_SizeChanged; ;
        }

        /// <summary>
        /// 控制窗口出现
        /// </summary>
        public void ShowWIndow()
        {
            m_Popup.IsOpen = true;
        }
        /// <summary>
        /// 控制窗口消失
        /// </summary>
        public void DismissWindow()
        {
            m_Popup.IsOpen = false;
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            DismissWindow();
            LeftClick?.Invoke(this, e);
        }
        public event EventHandler<RoutedEventArgs> LeftClick;
        public event EventHandler<RoutedEventArgs> SymbolClick;

        private async void HyperlinkButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var address = this.email.Content;
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            var mailto = new Uri($"mailto:{address}?subject={"Your subject"}&body={""}");
            await Launcher.LaunchUriAsync(mailto);
        }
        /// <summary>
        /// 让窗口消失的方法  -wzp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SymbolIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DismissWindow();
            SymbolClick?.Invoke(this, e);
        }

        /// <summary>
        /// 用于控制线程安全的内部类
        /// </summary>
        private class Nested
        {
            static Nested()
            {

            }
            internal static readonly AboutUs instance = new AboutUs();
        }
    }
}
