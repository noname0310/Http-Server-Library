using HttpServerLibrary;
using System.Net;
using System.Threading;

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
            if (requestType == RequestType.GET)
            {
                return "GET!";
            }
            else
            {
                return "OK";
            }
        }
    }
}
