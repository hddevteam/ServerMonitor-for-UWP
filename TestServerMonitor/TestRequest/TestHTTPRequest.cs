using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Services.RequestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServerMonitor.TestRequest
{
    /// <summary>
    /// 测试HTTP模块
    /// </summary>
    [TestClass]
    public class TestHTTPRequest
    {
        /// <summary>
        /// 待测试的对象
        /// </summary>
        HTTPRequest request;
        private const string NormallySiteAddress = "https://www.baidu.com";

        /// <summary>
        /// 测试类生成预处理
        /// </summary>
        [TestInitialize()]
        [Owner("Bin")]
        public void Initialize()
        {
            // 实例化待测试的对象
            request = HTTPRequest.Instance;
        }

        /// <summary>
        /// 测试正常请求Http站点
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Priority(1)]
        [Timeout(5000)] // 默认5s内完成
        public void TestHTTPRequest_NormallyRequest() {
            request.Uri = NormallySiteAddress;
            bool result = request.MakeRequest().Result;

            Assert.IsTrue(result, "测试 成功请求case 请求异常未通过!");
            Assert.AreEqual(request.Status, "200", "测试 成功请求case 请求状态码不为 200!");
        }

        /// <summary>
        /// 测试请求一个不合法的Uri
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Timeout(5000)]
        public void TestHTTPRequest_UriInvalid()
        {
            request.Uri = "test";
            bool result = request.MakeRequest().Result;
            Assert.IsFalse(result,"测试 非法Uri请求case 请求异常通过！");
            Assert.IsNotNull(request.RequestInfo, "测试 非法Uri请求case 显示请求异常信息为空!");
        }

        /// <summary>
        /// 测试请求Uri为空
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Timeout(5000)]
        public void TestHTTPRequest_UriIsNull()
        {
            request.Uri = null;
            bool result = request.MakeRequest().Result;
            Assert.IsFalse(result, "测试 空Uri请求case 请求异常通过!");
            Assert.IsNotNull(request.RequestInfo, "测试 空Uri请求case 显示请求异常信息为空!");
            
        }

        /// <summary>
        /// 测试请求超时的处理是否符合逻辑
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Timeout(5000)]
        public void TestHTTPRequest_OverTime()
        {
            request.Uri = "https://www.baidu1.com";
            request.OverTime = 1;
            bool result = request.MakeRequest().Result;
            Assert.IsFalse(result, "测试 超时请求case 请求异常通过!");
            Assert.IsNotNull(request.RequestInfo, "测试 超时请求case 显示请求异常信息为空!");
        }

        /// <summary>
        /// 测试请求一个服务器（未打开80端口的）
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Timeout(5000)]
        public void TestHTTPRequest_UriWithoutWebSite()
        {
            request.Uri = "8.8.8.8";
            bool result = request.MakeRequest().Result;
            Assert.IsFalse(result, "测试 无http站点请求case 请求异常通过!");
            Assert.IsNotNull(request.RequestInfo, "测试 无http站点请求case 显示请求异常信息为空!");
        }

    }
}
