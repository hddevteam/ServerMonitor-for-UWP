using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ServerMonitor.ViewModels;
using System.Text.RegularExpressions;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace ServerMonitor.Views
{
    public sealed partial class AddServerPage : UserControl
    {
        private static Popup m_Popup;
        public AddServerPage()
        {
            this.InitializeComponent();

            m_Popup = new Popup();
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            m_Popup.Child = this;

            this.Loaded += MessagePopupWindow_Loaded;
            this.Unloaded += MessagePopupWindow_Unloaded;
        }
        public AddServerPage(string id) : this()
        {
            AddServerPageViewModel model = this.ViewModel as AddServerPageViewModel;
            model.ID = id;         
        }
        private void MessagePopupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //this.tbContent.Text = m_TextBlockContent;
            Window.Current.SizeChanged += MessagePopupWindow_SizeChanged; ;
        }

        private void MessagePopupWindow_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.Width = e.Size.Width;
            this.Height = e.Size.Height;
        }

        private void MessagePopupWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= MessagePopupWindow_SizeChanged;
        }


        public static void ShowWindow()
        {
            m_Popup.IsOpen = true;
        }

        public static void DismissWindow()
        {
            m_Popup.IsOpen = false;
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            DismissWindow();
            LeftClick?.Invoke(this, e);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            DismissWindow();
            RightClick?.Invoke(this, e);
        }
        public event EventHandler<RoutedEventArgs> LeftClick;
        public event EventHandler<RoutedEventArgs> RightClick;

        private void Domain_LostFocus(object sender, RoutedEventArgs e)
        {
            string domain = this.domain.Text.ToString();
            //非空判断 正则验证
            if ("".Equals(domain))
            {
                this.error.Visibility = Visibility.Visible;
                this.btnLeft.IsEnabled = false;
            }
            else
            {
                try
                {
                    Boolean _ipcheck = Regex.IsMatch(domain, "^(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|[1-9])\\."
                                            + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)\\."
                                            + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)\\."
                                            + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)$");//是ip
                    Regex rg = new Regex("^[\u4e00-\u9fa5]+$");//是中文
                    Boolean _domaincheck = rg.IsMatch(domain);
                    if (!_ipcheck && _domaincheck)
                    {
                        this.error.Visibility = Visibility.Visible;
                        this.btnLeft.IsEnabled = false;
                        this.domain.Text = "";
                    }
                    else
                    {
                        this.error.Visibility = Visibility.Collapsed;
                        this.btnLeft.IsEnabled = true;
                    }
                }
                catch { }
            }
            
        }
    }
}
