using System;
using System.Net;
using System.Threading;
using HttpServerLibrary;

namespace CodeTestApp
{
    class Program
    {
        static CountdownEvent countdownEvent;

        static void Main(string[] args)
        {
            countdownEvent = new CountdownEvent(1);

            HttpServer HttpServer = new HttpServer(new IPEndPoint(IPAddress.Loopback, 80), countdownEvent);
            HttpServer.OnClientRequest += HttpServer_OnClientRequest;

            HttpServer.Start();
            countdownEvent.Wait();
        }

        private static string HttpServer_OnClientRequest(RequestType requestType, string parameter, string content)
        {
            return string.Format("RequestType: {0} parameter: {1} content: {2}", requestType.ToString(), parameter, content);
        }
    }
}
