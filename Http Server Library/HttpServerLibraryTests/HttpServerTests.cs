using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using HttpServerLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServerLibrary.Tests
{
    [TestClass()]
    public class HttpServerTests
    {
        HttpServer HttpServer;

        [TestMethod()]
        public void HttpServerTest()
        {
            HttpServer = new HttpServer(new IPEndPoint(IPAddress.Loopback, 80));

            HttpServer.Start();

            //while (true) { }
        }
    }
}