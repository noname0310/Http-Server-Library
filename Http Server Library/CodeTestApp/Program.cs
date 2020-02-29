using HttpServerLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CodeTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer HttpServer = new HttpServer(new IPEndPoint(IPAddress.Loopback, 80));

            HttpServer.Start();

            while(true)
            {

            }
        }
    }
}
