
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Models;
using ServerMonitor.SiteDb;
using ServerMonitor.ViewModels;
using ServerMonitor.ViewModels.BLL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;

namespace TestServerMonitor.TestDAO
{
    [TestClass]
    public class TestChartUtillmpl
    {
        public ChartPalette DefaultPalette { get { return ChartPalettes.DefaultLight; } }

        /// [TestInitialize]
        public void Initialize()
        {
        }
        /// <summary>
        /// 测试SelectSitesAsync方法
        /// 用例说明：测试选中的前五个站点是否为选中状态
        /// </summary>
        [TestMethod]
        public void TestSelectSitesAsync_siteIsSelected()
        {
            var exp1 = new List<SiteModel>();
            var exp2 = new ObservableCollection<SelectSite>();
            List<SiteModel> sites = new List<SiteModel>();

            sites.Add( new SiteModel() { Id = 6, Site_name = "bool", Is_server = true });
            foreach (var item in sites)
            {
                if (exp2.Count < 5)
                {
                    exp1.Add(item);
                    exp2.Add(new SelectSite()
                    {
                        Site = item,
                        IsSelected = true,
                        ImagePath = "../images/ic_server.png",
                        SiteType = "SERVER"
                    });
                }
                else
                {
                    exp2.Add(new SelectSite()
                    {
                        Site = item,
                        IsSelected = false,
                        ImagePath = "../images/ic_website.png",
                        SiteType = "SERVER"
                    });
                }
                IChartUtil t = new ChartUtilImpl();
                var res = t.SelectSitesAsync(sites).Result;
                var actual1 = res.Item2;
                var actual2 = res.Item1;
                var equal1 = actual1.All(i => exp1.Contains(i, new SiteModelComparer()));
                var equal2 = actual2.All(i => exp2.Contains(i, new SelectSiteComparer()));
                if (!equal1)
                {
                    Assert.Fail("equal1 is not equal;");
                }
                else if (!equal2)
                {
                    Assert.Fail("equal2 is not equal;");
                }
                else if (!(actual1.Count == exp1.Count))
                {
                    Assert.Fail("actual1's count is not equal to exp1's count;");
                }
                else if (!(actual2.Count == exp2.Count))
                {
                    Assert.Fail("actual2's count is not equal to exp2's count;");
                }
            }
         }
        /// <summary>
        /// 测试ChartLengendAsync方法
        /// 用例说明：测试第一个线性图的方法
        /// </summary>
        /// 一定记住测试既是期待值与实际值相比较，设计一个期待值，经过源程序代码逻辑，得到预期值,然后比较
        [TestMethod]
        public void TestChartLengendAsync_test()
        {
            var exp = new ObservableCollection<ChartLengend>();//设一个期待值
            var Sites = new List<SiteModel>();
            int i = 0;
            foreach (var item in Sites)
            {
                exp.Add(new ChartLengend() { Title = "#" + item.Id + " " + item.Site_name, Fill = DefaultPalette.FillEntries.Brushes[i] });
                i++;
            }
            IChartUtil t = new ChartUtilImpl();
            var act = t.ChartLengendAsync(Sites).Result;
            
            var equal = act.All(index => exp.Contains(index, new ChartLengendComparer()));
            //如果不相等，直接判定失败
            if (!(equal && act.Count == exp.Count))
            {
                Assert.Fail();
            }
        }
        /// <summary>
        /// 测试CacuChartAsync方法
        /// 用例说明：输入站点列表，输出对应图例
        /// </summary>
        [TestMethod]
        public void TestCacuChartAsync_testChartAndBarChartData()
        {
            var arg1 = new List<SiteModel>() {
                new SiteModel(){Site_name="SiteTest",Id=1,Is_server=true }
            };
            var arg2 = new List<LogModel>()
            {
                new LogModel()
                {Is_error=true,Status_code="1002",TimeCost=220,Site_id=1,Create_Time=DateTime.Now},
                new LogModel()
                { Is_error=false,Status_code="1001",TimeCost=2220,Site_id=1,Create_Time=DateTime.Now},
                new LogModel()
                { Is_error=true,Status_code="1000",TimeCost=220,Site_id=1,Create_Time=DateTime.Now},
                new LogModel()
                { Is_error=false,Status_code="1001",TimeCost=2720,Site_id=1,Create_Time=DateTime.Now},
                new LogModel()
                {Is_error=true,Status_code="1000",TimeCost=2720,Site_id=1,Create_Time=DateTime.Now},
                new LogModel()
                {Is_error=false,Status_code="1002",TimeCost=220,Site_id=1,Create_Time=DateTime.Now },
                new LogModel(){Is_error = true,Status_code = "1000",TimeCost = 4220,Site_id = 1,Create_Time = DateTime.Now },
                new LogModel(){Is_error = false,Status_code = "1001",TimeCost = 2020,Site_id = 1,Create_Time = DateTime.Now }
            };
            var exp1 = new ObservableCollection<Chart1>();
            var exp2 = new ObservableCollection<BarChartData>();
            int success = 0, error = 0, overtime = 0;
            foreach (var item in arg2)
            {
                string result = "";
                if (!item.Is_error)
                {
                    success++; result = "Success";
                    exp1.Add(new Chart1() { RequestTime = item.Create_Time, Result = result, ResponseTime = item.TimeCost });
                }
                else if (item.Status_code == "1002") //状态码为1002时表示请求超时
                {
                    overtime++; result = "OverTime";
                    exp1.Add(new Chart1() { RequestTime = item.Create_Time, Result = result, ResponseTime = 5000 });
                }
                else
                {
                    error++; result = "Error";
                    exp1.Add(new Chart1() { RequestTime = item.Create_Time, Result = result, ResponseTime = null });
                }
            }
            exp2.Add(new BarChartData() { SiteId = "1", SiteName = "#1 SiteTest", Error = error, Success = success, Overtime = overtime });

            IChartUtil t = new ChartUtilImpl();
            var res = t.CacuChartAsync(arg1, arg2).Result;
            var actual1 = res.Item1;
            var actual2 = res.Item2;

            /// Contains方法使用for循环表达如下
            for (int i = 0; i < actual1[0].Count; i++)
            {
                if (!(actual1[0][i].RequestTime == exp1[i].RequestTime))
                {
                    Assert.Fail("RequestTime is not equal;");
                }
                else if (!(actual1[0][i].ResponseTime == exp1[i].ResponseTime))
                {
                    Assert.Fail("ResponseTime is not equal;");
                }
                else if (!(actual1[0][i].Result == exp1[i].Result))
                {
                    Assert.Fail("Result is not equal;");
                }
            }

            //var equal1 = actual1[0].All(i => exp1.Contains(i, new Chart1Comparer()));
            var equal2 = actual2.All(i => exp2.Contains(i, new BarChartDataComparer()));

            if (!equal2)
            {
                 Assert.Fail("equal2 is not equal;");
            }
            else if (!(actual2.Count == exp2.Count))
            {
                Assert.Fail("actual2's count is not equal to exp2's count;");
            }
           
        }
        //重写相等比较器，比较两个SelectSite对象是否相等
        public class SelectSiteComparer : IEqualityComparer<SelectSite>
        {
            public bool Equals(SelectSite x, SelectSite y)
            {
                return x.IsSelected == y.IsSelected
                    && x.ImagePath == y.ImagePath
                    && x.SiteType == y.SiteType;
            }

            public int GetHashCode(SelectSite obj)
            {
                return base.GetHashCode();
            }
        }
        //重写相等比较器，比较两个SiteModel对象是否相等
        public class SiteModelComparer : IEqualityComparer<SiteModel>
        {
            public bool Equals(SiteModel x, SiteModel y)
            {
                return x.Id == y.Id
                    && x.Site_name == y.Site_name
                    && x.Is_server == y.Is_server;
            }

            public int GetHashCode(SiteModel obj)
            {
                return base.GetHashCode();
            }
        }
        ///重写比较器，比较两个ChartComparer对象是否相等
        public class Chart1Comparer : IEqualityComparer<Chart1>
        {
            public bool Equals(Chart1 x, Chart1 y)
            {
                return DateTime.Compare(x.RequestTime, y.RequestTime) == 0
               && x.ResponseTime == y.ResponseTime
               && x.Result == y.Result;
            }
            public int GetHashCode(Chart1 obj)
            {
                return base.GetHashCode();
            }
        }
        ///重写比较器，比较两个BarChartDataComparer是否相等
        public class BarChartDataComparer : IEqualityComparer<BarChartData>
        {
            public bool Equals(BarChartData x, BarChartData y)
            {
                return x.SiteId == y.SiteId && x.SiteName == y.SiteName && x.Success == y.Success &&  x.Error == y.Error && x.Overtime==y.Overtime &&x.Address==y.Address;
            }
            public int GetHashCode(BarChartData obj)
            {
                return base.GetHashCode();
            }
        }
        //重写比较器，比较两个ChartLengend对象是否相等
        public class ChartLengendComparer : IEqualityComparer<ChartLengend>
        {
            public bool Equals(ChartLengend x, ChartLengend y)
            {
                return x.Title == y.Title && x.Fill == y.Fill;
            }
            public int GetHashCode(ChartLengend obj)
            {
                return base.GetHashCode();
            }
        }
    }
}


