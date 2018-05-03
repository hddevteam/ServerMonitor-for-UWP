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
        private SSHRequest sshRequest = new SSHRequest("172.31.0.244", "root","Lucky.2011");
        [TestMethod]
        public void MyTestMethod()
        {
            Assert.AreEqual(true,sshRequest.MakeRequest().Result);
        }
    }
}
