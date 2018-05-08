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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace ServerMonitor.ViewModels
{
    class AddServerPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private ListView contactList;
        private Grid rightFrame1;
        Dictionary<int,bool> vs = new Dictionary<int, bool>();
        Dictionary<int, bool> tempVs = new Dictionary<int, bool>();
        public AddServerPageViewModel()
        {
            
        }
        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //GetListSite();
            //DispatcherTimeSetup();
            ChangeDisplay(0);
            GetListContact();
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

        private ObservableCollection<Contact> selectedContacts = new ObservableCollection<Contact>();
        public ObservableCollection<Contact> SelectedContacts { get => selectedContacts; set => selectedContacts = value; }
        #region UI控件的显示
        private Boolean livePort;
        public Boolean LivePort
        {
            get => livePort;
            set
            {
                livePort = value;
                RaisePropertyChanged(() => LivePort);
            }
        }
        private Boolean diePort;
        public Boolean DiePort
        {
            get => diePort;
            set
            {
                diePort = value;
                RaisePropertyChanged(() => DiePort);
            }
        }

        private Boolean needUser;
        public Boolean NeedUser
        {
            get => needUser;
            set
            {
                needUser = value;
                RaisePropertyChanged(() => NeedUser);
            }
        }

        private Boolean needRecord;
        public Boolean NeedRecord
        {
            get => needRecord;
            set
            {
                needRecord = value;
                RaisePropertyChanged(() => NeedRecord);
            }
        }
        #endregion

        #endregion

        #region 响应事件
        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeDisplay((sender as ComboBox).SelectedIndex);
        }
        public void BindContact_Click(object sender, RoutedEventArgs e)
        {
            //Contacts[i].Telephone;
            rightFrame1.Visibility = Visibility.Visible;
            for (int i = 0; i < contactList.Items.Count; i++)
            {
                var asss = (ListViewItem)contactList.ContainerFromIndex(i);
                ((ListViewItem)contactList.ContainerFromIndex(i)).IsSelected = vs[Contacts[i].Id];
                tempVs[Contacts[i].Id] = vs[Contacts[i].Id];
            }
        }
        
        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            rightFrame1.Visibility = Visibility.Collapsed;
        }

        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            var list = contactList.SelectedItems;
            rightFrame1.Visibility = Visibility.Collapsed;
            foreach (var item in tempVs)
            {
                vs[item.Key] = item.Value;
            }
            SelectedContacts.Clear();
            for (int i = 0; i < Contacts.Count; i++)
            {
                if (vs[Contacts[i].Id])
                {
                    SelectedContacts.Add(Contacts[i]);
                }
            }
        }

        //列表item点击事件
        public void Contactlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Contact)e.ClickedItem;
            tempVs[item.Id] = !tempVs[item.Id];
        }

        //public void Delete_Click(object sender, RoutedEventArgs e)
        //{
        //    var btn = sender as Button;
        //    int did = int.Parse(btn.Tag.ToString());
        //    vs[did] = false;

        //    SelectedContacts.Clear();
        //    for (int i = 0; i < Contacts.Count; i++)
        //    {
        //        if (vs[Contacts[i].Id])
        //        {
        //            SelectedContacts.Add(Contacts[i]);
        //        }
        //    }
        //}
        #endregion

        #region 辅助函数
        // OnNavigatedTo后调用
        public void OnLoaded(ListView contactList,Grid grid)
        {
            rightFrame1 = grid;
            this.contactList = contactList;
            rightFrame1.Visibility = Visibility.Collapsed;
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
                vs.Add(list[i].Id, false);
                tempVs.Add(list[i].Id, false);
                Contacts.Add(list[i]);
            }
        }
        private void ChangeDisplay(int i)
        {
            switch (i)
            {
                case 0:   //ICMP
                    LivePort = true;
                    DiePort = false;
                    NeedUser = false;
                    NeedRecord = false;
                    break;
                case 1:     //SOCKET
                    LivePort = true;
                    DiePort = false;
                    NeedUser = false;
                    NeedRecord = false;
                    break;
                case 2:    //SSH
                    LivePort = false;
                    DiePort = true;
                    NeedUser = true;
                    NeedRecord = false;
                    break;
                case 3:   //FTP
                    LivePort = false;
                    DiePort = true;
                    NeedUser = true;
                    NeedRecord = false;
                    break;
                case 4:    //DNS
                    LivePort = false;
                    DiePort = true;
                    NeedUser = false;
                    NeedRecord = true;
                    break;
                case 5:    //SMTP
                    LivePort = false;
                    DiePort = true;
                    NeedUser = false;
                    NeedRecord = false;
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
