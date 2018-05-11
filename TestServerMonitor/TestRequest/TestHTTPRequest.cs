using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Services.RequestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServerMonitor.TestRequest
{
    [TestClass]
    public class TestHTTPRequest
    {
        HTTPRequest request;

        [TestInitialize()] // 测试类生成预处理
        public void Initialize()
        {
            request = HTTPRequest.Instance;
        }

        [Owner("Bin")]
        [TestMethod]
        [Priority(1)]
        [Timeout(5000)]
        public void TestHTTPRequest_NormallyRequest() {
            request.Uri = "https://www.baidu.com";
            bool result = request.MakeRequest().Result;
            //Assert.IsTrue(result);
        }
    }
}
