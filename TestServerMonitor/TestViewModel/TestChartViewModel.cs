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

        public ChartPalette DefaultPalette { get { return ChartPalettes.DefaultLight; } }
        public List<SiteModel> Sites { get; set; }
        public List<LogModel> Logs { get; set; }
        public IChartUtil chartUtil { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            viewModel = new ChartPageViewModel();
            chartUtil = new ChartUtilImpl();
            Sites = new List<SiteModel>();
            Logs = new List<LogModel>();
            for (int i = 1; i <= 5; i++)
            {
                Logs.Add(new LogModel() { Site_id = i, Is_error = true });
                Sites.Add(new SiteModel() { Id = i, Site_name = "Site" + i, Is_server = true });
            }
        }

        #region ChartPageViewModel_Test Author:fjl
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
                viewModel.Infos.SiteInfoCompleted.Add(new AddSiteInfo() { IsSelected = true });
            }
            stub.SetLineChartLengendAsync(async (sites) =>
            {
                ObservableCollection<LineChartLengend> s = new ObservableCollection<LineChartLengend>();
                await Task.CompletedTask;
                return s;
            }, Times.Twice);
            stub.StatisticsSiteRequestResultAsync(async (sites, logs) =>
            {
                ObservableCollection<ObservableCollection<LineChartData>> data1 = new ObservableCollection<ObservableCollection<LineChartData>>();
                ObservableCollection<BarChartData> data2 = new ObservableCollection<BarChartData>();
                await Task.CompletedTask;
                return new Tuple<ObservableCollection<ObservableCollection<LineChartData>>, ObservableCollection<BarChartData>>(data1, data2);
            }, Times.Twice);
            viewModel.RequestResultType = "All";
            //less
            Assert.IsTrue(viewModel.AcceptClickAsync().Result);
            //equal
            viewModel.Infos.SiteInfoCompleted.Add(new AddSiteInfo() { IsSelected = true });
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
                viewModel.Infos.SiteInfoCompleted.Add(new AddSiteInfo() { IsSelected = true });
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

            ObservableCollection<LineChartData> item = new ObservableCollection<LineChartData>
            {
                new LineChartData() { Result = "Error" },
                new LineChartData() { Result = "Success" },
                new LineChartData() { Result = "OverTime" },
                new LineChartData() { Result = "Success" },
                new LineChartData() { Result = "Error" }
            };
            viewModel.LineChartCollection.Add(item);

            Assert.IsTrue(viewModel.TypeChanged("All"));
            Assert.AreEqual(5, viewModel.Infos.LineChartCollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("Success"));
            Assert.AreEqual(2, viewModel.Infos.LineChartCollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("Error"));
            Assert.AreEqual(2, viewModel.Infos.LineChartCollectionCopy[0].Count);

            Assert.IsTrue(viewModel.TypeChanged("OverTime"));
            Assert.AreEqual(1, viewModel.Infos.LineChartCollectionCopy[0].Count);
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
        #endregion

        #region ChartUtilImpl_Test Author:lyy
        /// <summary>
        /// 测试AddInfoForSiteAsync方法
        /// 用例说明：测试选中的前五个被监测站点是否为选中状态，完善站点信息
        /// 用例变量说明：方法所需参数：sites 期待值：exp1，exp2 
        /// 方法说明：输入站点列表，输出正在监测的前五个站点，及完善信息的所有站点
        /// </summary>
        [TestMethod]
        public void TestAddInfoForSiteAsync()
        {
            var exp1 = new List<SiteModel>();
            var exp2 = new ObservableCollection<AddSiteInfo>();
            List<SiteModel> sites = new List<SiteModel>();
            sites.Add(new SiteModel() { Id = 5, Site_name = "site5", Is_server = true, Is_Monitor = false });
            sites.Add(new SiteModel() { Id = 6, Site_name = "site6", Is_server = true, Is_Monitor = true });
            //获取前五个被监测的站点
            foreach (var item in sites)
            {
                //若所选站点数量小于5且站点处于监测状态
                if (exp1.Count < 5 && item.Is_Monitor)
                {
                    exp1.Add(item);
                    exp2.Add(new AddSiteInfo()
                    {
                        Site = item,
                        IsSelected = true,
                        ImagePath = item.Is_server ? "../images/ic_server.png" : "../images/ic_website.png",
                        SiteType = item.Is_server ? "SERVER" : "WEBSITE"
                    });
                }
                else
                {
                    exp2.Add(new AddSiteInfo()
                    {
                        Site = item,
                        IsSelected = false,
                        ImagePath = item.Is_server ? "../images/ic_server.png" : "../images/ic_website.png",
                        SiteType = item.Is_server ? "SERVER" : "WEBSITE"
                    });
                }
            }
            var res = chartUtil.AddInfoForSiteAsync(sites).Result;
            var actual1 = res.Item2;
            var actual2 = res.Item1;
            //比较实际值是否在预期值之内
            var equal1 = actual1.All(i => exp1.Contains(i, new SiteModelComparer()));
            var equal2 = actual2.All(i => exp2.Contains(i, new SelectSiteComparer()));

            if (!equal1)//实际值1不在在预期值之内
            {
                Assert.Fail("actual1 is not equal exp1;");
            }
            if (!equal2)//实际值2不在在预期值之内
            {
                Assert.Fail("actual2 is not equal exp2;");
            }
            else if (!(actual1.Count == exp1.Count))//实际值1不等于预期值
            {
                Assert.Fail("actual1's count is not equal to exp1's count;");
            }
            else if (!(actual2.Count == exp2.Count))//实际值2不等于预期值
            {
                Assert.Fail("actual2's count is not equal to exp2's count;");
            }
        }
        /// <summary>
        /// 测试SetLineChartLengendAsync方法
        /// 用例说明：测试线性图图例是否正确生成
        /// 用例变量说明：方法所需参数：Sites 期待值：exp 
        /// </summary>
        /// 一定记住测试既是期待值与实际值相比较，设计一个期待值，经过源程序代码逻辑，得到预期值,然后比较
        [UITestMethod]
        public void TestSetLineChartLengendAsync()
        {
            //设一个期待值
            var exp = new ObservableCollection<LineChartLengend>();
            exp.Add(new LineChartLengend() { Title = "#1 Site", Fill = DefaultPalette.FillEntries.Brushes[0] });
            //所需参数
            var Sites = new List<SiteModel>();
            Sites.Add(new SiteModel() { Id = 1, Site_name = "Site" });

            var act = chartUtil.SetLineChartLengendAsync(Sites).Result;
            //比较实际值是否在预期值之内
            var equal = act.All(index => exp.Contains(index, new ChartLengendComparer()));
            //如果不相等，直接判定失败
            if (!(equal && act.Count == exp.Count))
            {
                Assert.Fail();
            }
        }

        /// <summary>
        /// 测试StatisticsSiteRequestResultAsync方法
        /// 用例说明：测试是否正确计算站点记录结果
        /// 用例变量说明：方法所需参数：arg1，arg2 期待值：exp1，exp2 
        /// 方法说明：输入站点列表，站点记录，输出站点记录的统计结果（包含2个返回值）
        /// </summary>
        [TestMethod]
        public void TestStatisticsSiteRequestResultAsync()
        {
            var exp1 = new ObservableCollection<LineChartData>();
            var exp2 = new ObservableCollection<BarChartData>();
            //设置输入参数
            var arg1 = new List<SiteModel>() {
                new SiteModel(){Site_name = "SiteTest",Id = 1,Is_server = true }
            };
            var arg2 = new List<LogModel>()
            {
                new LogModel()
                { Is_error = true,Status_code = "1002",TimeCost = 220,Site_id = 1,Create_Time = DateTime.Now},//overtime
                new LogModel()
                { Is_error = false,Status_code = "1000",TimeCost = 2220,Site_id = 1,Create_Time = DateTime.Now},//success
                new LogModel()
                { Is_error = true,Status_code = "1001",TimeCost = 220,Site_id = 1,Create_Time = DateTime.Now},//error
                new LogModel()
                { Is_error = false,Status_code = "1000",TimeCost = 2720,Site_id = 1,Create_Time = DateTime.Now},//success
                new LogModel()
                { Is_error = true,Status_code = "1001",TimeCost = 2720,Site_id = 1,Create_Time = DateTime.Now},//error
                new LogModel()
                { Is_error = true,Status_code = "1002",TimeCost = 220,Site_id = 1,Create_Time = DateTime.Now },//overtime
            };
            //创建期待值
            foreach (var item in arg2)
            {
                if (!item.Is_error)//成功
                {
                    exp1.Add(new LineChartData()
                    {
                        Result = "Success",
                        ResponseTime = item.TimeCost,
                        RequestTime = item.Create_Time
                    });
                }
                else if (item.Status_code == "1002")//超时
                {
                    exp1.Add(new LineChartData()
                    {
                        Result = "OverTime",
                        ResponseTime = 5000,
                        RequestTime = item.Create_Time
                    });
                }
                else//失败
                {
                    exp1.Add(new LineChartData()
                    {
                        Result = "Error",
                        ResponseTime = null,
                        RequestTime = item.Create_Time
                    });
                }
            }
            exp2.Add(new BarChartData() { SiteId = "1", SiteName = "#1 SiteTest", Error = 2, Success = 2, Overtime = 2 });

            var res = chartUtil.StatisticsSiteRequestResultAsync(arg1, arg2).Result;
            var actual1 = res.Item1;
            var actual2 = res.Item2;
            //比较实际值是否在预期值之内
            var equal1 = actual1[0].All(i => exp1.Contains(i, new Chart1Comparer()));
            var equal2 = actual2.All(i => exp2.Contains(i, new BarChartDataComparer()));

            if (!equal1)//实际值1不在在预期值之内
            {
                Assert.Fail("actual1 is not equal exp1;");
            }
            if (!equal2)//实际值2不在在预期值之内
            {
                Assert.Fail("actual2 is not equal exp2;");
            }
            else if (!(actual1[0].Count == exp1.Count))//实际值1不等于预期值
            {
                Assert.Fail("actual1's count is not equal to exp1's count;");
            }
            else if (!(actual2.Count == exp2.Count))//实际值2不等于预期值
            {
                Assert.Fail("actual2's count is not equal to exp2's count;");
            }

        }

        #region 重写相等比较器
        //重写相等比较器，比较两个SelectSite对象是否相等
        public class SelectSiteComparer : IEqualityComparer<AddSiteInfo>
        {
            public bool Equals(AddSiteInfo x, AddSiteInfo y)
            {
                return x.Site.Id == y.Site.Id
                    && x.Site.Site_name == y.Site.Site_name
                    && x.Site.Is_server == y.Site.Is_server
                    && x.Site.Is_Monitor == y.Site.Is_Monitor
                    && x.IsSelected == y.IsSelected
                    && x.ImagePath == y.ImagePath
                    && x.SiteType == y.SiteType;
            }

            public int GetHashCode(AddSiteInfo obj)
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
                    && x.Is_server == y.Is_server
                    && x.Is_Monitor == y.Is_Monitor;
            }

            public int GetHashCode(SiteModel obj)
            {
                return base.GetHashCode();
            }
        }
        ///重写比较器，比较两个ChartComparer对象是否相等
        public class Chart1Comparer : IEqualityComparer<LineChartData>
        {
            public bool Equals(LineChartData x, LineChartData y)
            {
                return DateTime.Compare(x.RequestTime, y.RequestTime) == 0
                    && x.ResponseTime == y.ResponseTime
                    && x.Result == y.Result;
            }
            public int GetHashCode(LineChartData obj)
            {
                return base.GetHashCode();
            }
        }
        ///重写比较器，比较两个BarChartDataComparer是否相等
        public class BarChartDataComparer : IEqualityComparer<BarChartData>
        {
            public bool Equals(BarChartData x, BarChartData y)
            {
                return x.SiteId == y.SiteId
                    && x.SiteName == y.SiteName
                    && x.Success == y.Success
                    && x.Error == y.Error
                    && x.Overtime == y.Overtime
                    && x.Address == y.Address;
            }
            public int GetHashCode(BarChartData obj)
            {
                return base.GetHashCode();
            }
        }
        //重写比较器，比较两个ChartLengend对象是否相等
        public class ChartLengendComparer : IEqualityComparer<LineChartLengend>
        {
            public bool Equals(LineChartLengend x, LineChartLengend y)
            {
                return x.Title == y.Title && x.Fill == y.Fill;
            }
            public int GetHashCode(LineChartLengend obj)
            {
                return base.GetHashCode();
            }
        }
        #endregion
        #endregion

    }
}
