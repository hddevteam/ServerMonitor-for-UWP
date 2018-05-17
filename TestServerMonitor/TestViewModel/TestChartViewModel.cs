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

    }
}
