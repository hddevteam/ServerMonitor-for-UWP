using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServerMonitor.TestRequest
{
    [TestClass]
    public class TestSSHRequest
    {
        //测试信息
        private string TestSshIP = "172.31.0.244";
        private string Username = "root";
        private string Password = "Lucky.2011";
        private SSHRequest sshRequest;

        [TestMethod]
        public void CompleteAndCorrectInfo_ShouldReturnNullExceptionAndTimeCostLessOverTime()
        {
            var result = sshRequest.SSHConnectAsync().Result;
            var timeCost = result.Item2;
            var overtime = sshRequest.OverTime;
            Assert.Fail

        }

        [TestMethod]
        public void MakeRequest_Should()
        {
            sshRequest = new SSHRequest(TestSshIP, Username, Password);
            var actual = sshRequest.MakeRequest().Result;

            Assert.IsTrue(actual);
        }
    }
}
