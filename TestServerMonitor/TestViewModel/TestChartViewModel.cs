using Microsoft.VisualStudio.TestTools.UnitTesting;
using Etg.SimpleStubs;
using ServerMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.ViewModels;
using ServerMonitor.Models;
using System.Collections.ObjectModel;
using Telerik.UI.Xaml.Controls.Chart;

namespace TestServerMonitor.TestViewModel
{
    [TestClass]
    public class TestChartViewModel
    {
        private ChartPageViewModel viewModel;
        private List<Site> sites;
        private List<Log> logs;

        public ChartPalette DefaultPalette { get { return ChartPalettes.DefaultLight; } }
        public List<Site> Sites { get; set; }
        public List<Log> Logs { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            viewModel = new ChartPageViewModel();
            Sites = new List<Site>();
            Logs = new List<Log>();
            for (int i = 1; i <= 5; i++)
            {
                Logs.Add(new Log() { Site_id = i, Is_error = true });
                Sites.Add(new Site() { Id = i, Site_name = "Site" + i, Is_server = true });
            }
        }

        /// <summary>
        /// 测试ChartAsync
        /// 用例说明：测试方法正常执行，返回true
        /// </summary>
        [TestMethod]
        public void TestMethodIsPerfomedNormall_ShouldReturnTrue()
        {
            var stub = new StubIChartDao(MockBehavior.Strict);
            viewModel.ChartDao = stub;
            int except1 = 0, except2 = 0, except3 = 0;
            stub.ChartLengendAsync(async (sites) =>
            {
                ObservableCollection<ChartLengend> s = new ObservableCollection<ChartLengend>();
                for (int i = 0; i < sites.Count; i++)
                {
                    s.Add(new ChartLengend() { Title = "Site" + i});
                }
                await Task.CompletedTask;
                except1 = s.Count;
                return s;
            }, Times.Once);
            stub.CacuChartAsync(async (sites, logs) => 
            {
                ObservableCollection<ObservableCollection<Chart1>> data = new ObservableCollection<ObservableCollection<Chart1>>();
                foreach (var item in sites)
                {
                    ObservableCollection<Chart1> chart1Series = new ObservableCollection<Chart1>();
                    for (int i = 0; i < logs.Count; i++)
                    {
                        chart1Series.Add(new Chart1() { Result="result"+i});
                    }
                    data.Add(chart1Series);
                }
                except2 = data.Count;
                int[,] array = new int[sites.Count,3];
                await Task.CompletedTask;
                return new Tuple<ObservableCollection<ObservableCollection<Chart1>>, int[,]>(data, array);
            }, Times.Once);
            stub.CacuBarChart((sites, array) =>
            {
                ObservableCollection<BarChartData> data1 = new ObservableCollection<BarChartData>();
                ObservableCollection<BarChartData> data2 = new ObservableCollection<BarChartData>();
                for (int i = 0; i < sites.Count; i++)
                {
                    data1.Add(new BarChartData() { SiteName = "site" + i });
                    data2.Add(new BarChartData() { SiteName = "site" + i });
                }
                except3 = data1.Count + data2.Count;
                return new Tuple<ObservableCollection<BarChartData>, ObservableCollection<BarChartData>>(data1, data2);
            }, Times.Once);

            Assert.IsTrue(viewModel.ChartAsync(Sites, Logs).Result);
            Assert.AreEqual(5, except1);
            Assert.AreEqual(5, except2);
            Assert.AreEqual(10, except3);
        }

        [TestMethod]
        public void 

        [TestMethod]
        public void Test()
        {
            viewModel.TestClick(); //调用被测试方法

            Assert.AreEqual(10, viewModel.My);//My为在该方法内被改变的某个全局变量
        }

    }
}
