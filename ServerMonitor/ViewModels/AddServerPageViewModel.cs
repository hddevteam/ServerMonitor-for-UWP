using ServerMonitor.Controls;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace ServerMonitor.ViewModels
{
    class AddServerPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private ListView contactList;
        public AddServerPageViewModel()
        {
            
        }
        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //GetListSite();
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

        #region 绑定数据
        private ObservableCollection<Contact> contacts = new ObservableCollection<Contact>();
        public ObservableCollection<Contact> Contacts { get => contacts; set => contacts = value; }
        #endregion

        #region 响应事件

        #endregion

        #region 辅助函数
        // OnNavigatedTo后调用
        public void OnLoaded(ListView contactList)
        {
            this.contactList = contactList;
            GetListContact();  //不可测
        }

        /// <summary>
        /// 获取和刷新联系人
        /// </summary>
        private void GetListContact()  //不可测
        {
            List<Contact> list = DBHelper.GetAllContact();
            Contacts.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Contacts.Add(list[i]);
            }
        }
        #endregion
    }
}
