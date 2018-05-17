using Microsoft.VisualStudio.TestTools.UnitTesting;
using Etg.SimpleStubs;
using ServerMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.ViewModels;
using ServerMonitor.ViewModels.BLL;
using ServerMonitor.Models;
using System.Collections.ObjectModel;
using Telerik.UI.Xaml.Controls.Chart;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace TestServerMonitor.TestViewModel
{
    [TestClass]
    public class TestChartViewModel
    {
        private ChartPageViewModel viewModel;

        private IChartUtil chartUtil = new ChartUtilImpl();

        public ChartPalette DefaultPalette { get { return ChartPalettes.DefaultLight; } }
        public List<SiteModel> Sites { get; set; }
        public List<LogModel> Logs { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            viewModel = new ChartPageViewModel();
            Sites = new List<SiteModel>();
            Logs = new List<LogModel>();
            for (int i = 1; i <= 5; i++)
            {
                Logs.Add(new LogModel() { Site_id = i, Is_error = true });
                Sites.Add(new SiteModel() { Id = i, Site_name = "Site" + i, Is_server = true });
            }
        }

        /// <summary>
        /// 测试AcceptClickAsync方法
        /// 用例说明：测试选择站点数目小于等于5，返回true
        /// </summary>
        [TestMethod]
        public void TestAcceptClickAsync_NumberOfSiteLE5_ShouldReturnTrue()
        {
            Assert.IsTrue(viewModel.InitAsync().Result);
            var stub = new StubIChartUtil(MockBehavior.Strict);
            viewModel.ChartDao = stub;
            for (int i = 0; i < 4; i++)
            {
                viewModel.Infos.SelectSites.Add(new SelectSite() { IsSelected = true });
            }
            stub.ChartLengendAsync(async (sites) =>
            {
                ObservableCollection<ChartLengend> s = new ObservableCollection<ChartLengend>();
                await Task.CompletedTask;
                return s;
            }, Times.Twice);
            stub.CacuChartAsync(async (sites, logs) =>
            {
                ObservableCollection<ObservableCollection<Chart1>> data1 = new ObservableCollection<ObservableCollection<Chart1>>();
                ObservableCollection<BarChartData> data2 = new ObservableCollection<BarChartData>();
                await Task.CompletedTask;
                return new Tuple<ObservableCollection<ObservableCollection<Chart1>>, ObservableCollection<BarChartData>>(data1, data2);
            }, Times.Twice);
            viewModel.Type = "All";
            //less
            Assert.IsTrue(viewModel.AcceptClickAsync().Result);
            //equal
            viewModel.Infos.SelectSites.Add(new SelectSite() { IsSelected = true });
            Assert.IsTrue(viewModel.AcceptClickAsync().Result);
        }

        /// <summary>
        /// 测试AcceptClickAsync方法
        /// 用例说明：测试选择站点数目大于5，抛出异常
        /// </summary>
        [TestMethod]
        public void TestAcceptClickAsync_NumberOfSiteGT5_ShouldThrowAggregateException()
        {
            Assert.IsTrue(viewModel.InitAsync().Result);
            for (int i = 0; i < 7; i++)
            {
                viewModel.Infos.SelectSites.Add(new SelectSite() { IsSelected = true });
            }
            try
            {
                var res = viewModel.AcceptClickAsync().Result;
                Assert.Fail();
            }
            catch (AggregateException)
            {

            }  
        }

        /// <summary>
        /// 测试TypeChanged方法
        /// 用例说明：测试方法是否执行,返回true
        ///           测试方法计算是否正确，返回期待值
        /// </summary>
        [TestMethod]
        public void TestTypeChanged_CalculationCorrect_ShouldReturnTrue()
        {
            Assert.IsTrue(viewModel.InitAsync().Result);

            ObservableCollection<Chart1> item = new ObservableCollection<Chart1>
            {
                new Chart1() { Result = "Error" },
                new Chart1() { Result = "Success" },
                new Chart1() { Result = "OverTime" },
                new Chart1() { Result = "Success" },
                new Chart1() { Result = "Error" }
            };
            viewModel.Chart1Collection.Add(item);

            Assert.IsTrue(viewModel.TypeChanged("All"));
            Assert.AreEqual(5, viewModel.Infos.Chart1CollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("Success"));
            Assert.AreEqual(2, viewModel.Infos.Chart1CollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("Error"));
            Assert.AreEqual(2, viewModel.Infos.Chart1CollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("OverTime"));
            Assert.AreEqual(1, viewModel.Infos.Chart1CollectionCopy[0].Count);
        }

        /// <summary>
        /// 测试PivotSelectionChanged方法
        /// 用例说明：测试方法对输入值大于2是否做出正确响应，返回ERROR_CODE=4
        ///           其他情况下输入==输出
        /// </summary>
        [TestMethod]
        public void TestPivotSelectionChanged_ExcuteNormal_ShouldReturnInputPara()
        {
            viewModel.PivotIndex = 2;
            Assert.AreEqual(viewModel.PivotIndex, viewModel.PivotSelectionChanged());

            viewModel.PivotIndex = 3;
            Assert.AreEqual(4, viewModel.PivotSelectionChanged());
        }

        /// <summary>
        /// 测试SelectSitesAsync方法
        /// 用例说明：输入站点列表，输出计算好的两个不同类型列表
        /// </summary>
        [TestMethod]
        public void TestSelectSitesAsync()
        {
            var exp1 = new List<SiteModel>();
            Sites.ForEach(i => exp1.Add(i));//复制一份作期待值
            var exp2 = new ObservableCollection<SelectSite>();
            Sites.Add(new SiteModel() { Id = 6, Site_name = "Site" + 6, Is_server = false });
            foreach (var item in Sites)
            {
                if (exp2.Count < 5)
                {
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
                        SiteType = "WEBSITE"
                    });
                }
            }
            var res = chartUtil.SelectSitesAsync(Sites).Result;
            var actual1 = res.Item2;
            var actual2 = res.Item1;
            var equal1 = actual1.All(i => exp1.Contains(i, new SiteModelComparer()));
            var equal2 = actual2.All(i => exp2.Contains(i, new SelectSiteComparer()));
            //若不相等，判定失败
            if (!(equal1 && equal2 && actual1.Count == exp1.Count && actual2.Count == exp2.Count))
            {
                Assert.Fail();
            }
        }

        /// <summary>
        /// 测试ChartLengendAsync方法
        /// 用例说明：输入站点列表，输出对应的图例
        /// </summary>
        [UITestMethod]
        public void TestChartLengendAsync()
        {
            var exp = new ObservableCollection<ChartLengend>();
            int index = 0;
            foreach (var item in Sites)
            {
                exp.Add(new ChartLengend() { Title = "#" + item.Id + " "+item.Site_name, Fill = DefaultPalette.FillEntries.Brushes[index] });
                index++;
            }
            var actual = chartUtil.ChartLengendAsync(Sites).Result;

            var equal = actual.All(i => exp.Contains(i, new ChartLengendComparer()));
            //若不相等，判定失败
            if (!(equal && actual.Count == exp.Count))
            {
                Assert.Fail();
            }
        }

        /// <summary>
        /// 测试CacuChartAsync方法
        /// 用例说明：输入站点列表，输出对应的图例
        /// </summary>
        [TestMethod]
        public void TestCacuChartAsync()
        {
            var arg1 = new List<SiteModel>() { new SiteModel() { Site_name = "SiteTest", Id = 1,Is_server=true } };
            var arg2 = new List<LogModel>() {
                new LogModel(){Is_error = true,Status_code = "1002",Request_time = 220,Site_id = 1,Create_time = DateTime.Now },
                new LogModel(){Is_error = false,Status_code = "1001",Request_time = 2220,Site_id = 1,Create_time = DateTime.Now },
                new LogModel(){Is_error = true,Status_code = "1000",Request_time = 220,Site_id = 1,Create_time = DateTime.Now },
                new LogModel(){Is_error = false,Status_code = "1001",Request_time = 2720,Site_id = 1,Create_time = DateTime.Now },
                new LogModel(){Is_error = false,Status_code = "1001",Request_time = 6220,Site_id = 1,Create_time = DateTime.Now },
                new LogModel(){Is_error = true,Status_code = "1000",Request_time = 2720,Site_id = 1,Create_time = DateTime.Now },
                new LogModel(){Is_error = true,Status_code = "1000",Request_time = 4220,Site_id = 1,Create_time = DateTime.Now },
                new LogModel(){Is_error = false,Status_code = "1001",Request_time = 2020,Site_id = 1,Create_time = DateTime.Now }
            };
            var exp1 = new ObservableCollection<Chart1>();//这里只测试了一个站点，故没有嵌套集合
            var exp2 = new ObservableCollection<BarChartData>();
            int success = 0, overtime = 0, error = 0;
            foreach (var item in arg2)
            {
                string result = "";
                if (!item.Is_error) { result = "Success";success++; }
                else if (item.Status_code == "1002") { result = "OverTime"; overtime++; }
                else { result = "Error";error++; item.Request_time = 0; }
                exp1.Add(new Chart1() { RequestTime = item.Create_time,Result=result, ResponseTime = item.Request_time });
            }
            exp2.Add(new BarChartData() { SiteId = "1", SiteName = "#1 SiteTest", Success = success, Overtime = overtime, Error = error, Type = "SERVER" });

            var res = chartUtil.CacuChartAsync(arg1,arg2).Result;
            var actual1 = res.Item1;
            var actual2 = res.Item2;
            var equal1 = actual1[0].All(i => exp1.Contains(i, new Chart1Comparer()));
            var equal2 = actual2.All(i => exp2.Contains(i, new BarChartDataComparer()));
            //若不相等，判定失败
            if (!(equal1 && equal2 && actual1[0].Count == exp1.Count && actual2.Count == exp2.Count))
            {
                Assert.Fail();
            }
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

    //重写相等比较器，比较两个ChartLengend对象是否相等
    public class ChartLengendComparer : IEqualityComparer<ChartLengend>
    {
        public bool Equals(ChartLengend x, ChartLengend y)
        {
            return x.Title == y.Title
                && x.Fill == y.Fill;
        }

        public int GetHashCode(ChartLengend obj)
        {
            return base.GetHashCode();
        }
    }

    //重写相等比较器，比较两个Chart1对象是否相等
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

    //重写相等比较器，比较两个BarChartData对象是否相等
    public class BarChartDataComparer : IEqualityComparer<BarChartData>
    {
        public bool Equals(BarChartData x, BarChartData y)
        {
            return x.SiteId == y.SiteId
                && x.SiteName == y.SiteName
                && x.Success == y.Success
                && x.Error == y.Error
                && x.Overtime == y.Overtime
                && x.Type == y.Type;
        }

        public int GetHashCode(BarChartData obj)
        {
            return base.GetHashCode();
        }
    }
}
