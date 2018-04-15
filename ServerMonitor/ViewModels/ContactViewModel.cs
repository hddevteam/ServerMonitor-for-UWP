using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ServerMonitor.Models;
using ServerMonitor.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using System;
using System.Text.RegularExpressions;

namespace ServerMonitor.ViewModels
{
    public class ContactViewModel : Template10.Mvvm.ViewModelBase
    {
        private Contact selectedContact;
        Grid rightFrame2, rightFrame1;
        int isAddContact = 0; //1:添加联系人 2：编辑联系人 0：什么都不做
        public ContactViewModel()
        {

        }
        #region 绑定数据
        private ObservableCollection<Contact> contacts = new ObservableCollection<Contact>();
        public ObservableCollection<Contact> Contacts { get => contacts; set => contacts = value; }

        //编辑联系人，新建联系人绑定联系人
        public Contact RightContact { get => rightContact; set => rightContact = value; }
        private Contact rightContact = new Contact();
        
        private string rightFrame2Title = "New Contact";
        public string RightFrame2Title
        {
            get => rightFrame2Title;
            set
            {
                rightFrame2Title = value;
                RaisePropertyChanged(() => RightFrame2Title);
            }
        }
        #endregion 绑定数据

        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
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

        #region 响应事件
        /// <summary>
        /// 联系人列表右击列表 得到右击的站点id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool ContactList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var x = e.OriginalSource;
            if (x is Rectangle)
            {
                selectedContact = (Contact)(((Rectangle)(e.OriginalSource)).DataContext);
            }
            else if (x is TextBlock)
            {
                selectedContact = (Contact)(((TextBlock)(e.OriginalSource)).DataContext);
            }
            else if(x is Image)
            {
                selectedContact = (Contact)(((Image)(e.OriginalSource)).DataContext);
            }
            return true;
        }
        public void Contactlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            rightFrame1.Visibility = Visibility.Collapsed;
            int x = (sender as ListBox).SelectedIndex;
            if (x >= 0)
            {
                selectedContact = (Contact)((sender as ListBox).Items[x]);
                rightFrame1.Visibility = Visibility.Visible;
                rightFrame2.Visibility = Visibility.Collapsed;
            }
        }
        //新建联系人
        public void AddContact()
        {
            isAddContact = 1;
            RightFrame2Title = "New Contact";
            RightContact.Id = 0;
            RightContact.Telephone = "";
            RightContact.Contact_email = "";
            RightContact.Contact_name = "";
            rightFrame1.Visibility = Visibility.Collapsed;
            rightFrame2.Visibility = Visibility.Visible;
        }
        //编辑联系人
        public void EditFlyoutItem_Click()
        {
            isAddContact = 2;
            RightFrame2Title = "Edit Contact";
            RightContact.Id = selectedContact.Id;
            RightContact.Telephone = selectedContact.Telephone;
            RightContact.Contact_email = selectedContact.Contact_email;
            RightContact.Contact_name = selectedContact.Contact_name;
            rightFrame1.Visibility = Visibility.Collapsed;
            rightFrame2.Visibility = Visibility.Visible;
        }
        //1新建/ 2编辑联系人 确认
        public async void ConfirmContact()
        {
            string str = JudgeInput(RightContact);
            if (!str.Equals(""))
            {
                var messageBox = new Windows.UI.Popups.MessageDialog(str) { Title = "Error" };
                messageBox.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
                await messageBox.ShowAsync();
                return;
            }
            if(isAddContact==1)
            {
                RightContact.Create_time = DateTime.Now;
                RightContact.Update_time = DateTime.Now;
                if (DBHelper.InsertOneContact(RightContact) == 1)
                {
                    rightFrame1.Visibility = Visibility.Collapsed;
                    rightFrame2.Visibility = Visibility.Collapsed;
                }
            }
            else if(isAddContact==2)
            {
                RightContact.Update_time = DateTime.Now;
                if (DBHelper.UpdateContact(RightContact) == 1)
                {
                    rightFrame1.Visibility = Visibility.Collapsed;
                    rightFrame2.Visibility = Visibility.Collapsed;
                }
            }
            GetListContact();
        }
        //新建/编辑联系人 取消
        public void CancelContact()
        {
            rightFrame1.Visibility = Visibility.Collapsed;
            rightFrame2.Visibility = Visibility.Collapsed;
        }
        //删除联系人
        public async void DeleteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            string str = selectedContact.Contact_name + " will be deleted.";
            var messageBox = new Windows.UI.Popups.MessageDialog(str) { Title = "Delete this contact?" };
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("Delete", uicommand =>
            {
                if (DBHelper.DeleteOneContact(selectedContact.Id) == 1)
                {
                    rightFrame1.Visibility = Visibility.Collapsed;
                    rightFrame2.Visibility = Visibility.Collapsed;
                    GetListContact();
                }
            }));
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", uicommand =>
            {
                
            }));
            await messageBox.ShowAsync();
        }
        #endregion 响应事件

        #region 辅助函数
        //获取联系人
        private void GetListContact()
        {
            List<Contact> list = DBHelper.GetAllContact();
            Contacts.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Contacts.Add(list[i]);
            }
        }
        //设置，获取界面右端隐藏元素
        public void SetFrame(Grid grid1,Grid grid2)
        {
            rightFrame1 = grid1;
            rightFrame2 = grid2;
        }
        //深度克隆联系人Contact 
        private Contact CloneContact(Contact contact)
        {
            Contact ct = new Contact()
            {
                Id = contact.Id,
                Contact_name = contact.Contact_name+"1",
                Contact_email = contact.Contact_email,
                Create_time = contact.Create_time,
                Telephone = contact.Telephone,
            };
            return ct;
        }
        //判断新建，编辑时输入是否合法
        private string JudgeInput(Contact contact)
        {
            string str = "";
            //手机号码
            Regex regPho = new Regex(@"^(13[0-9]|14[5|7]|15[0|1|2|3|5|6|7|8|9]|18[0|1|2|3|5|6|7|8|9])\d{8}$");

            //国内电话号码(0511-4405222、021-87888822)
            Regex regPho1 = new Regex(@"\d{3}-\d{8}|\d{4}-\d{7}");

            //Email地址
            Regex regE = new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

            if(!(regPho.IsMatch(contact.Telephone)|| regPho1.IsMatch(contact.Telephone)))
            {
                if (!("".Equals(contact.Telephone) || contact.Telephone == null))
                {
                    str += "The telephone number is error.";
                }
            }
            if(!regE.IsMatch(contact.Contact_email))
            {
                if (!("".Equals(contact.Contact_email) || contact.Contact_email == null))
                    str += "The E-mail is error.";
            }
            return str;
        }
        #endregion 辅助函数
    }
}
