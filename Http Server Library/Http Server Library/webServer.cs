using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

// ArinCherryBlossom - WebServer via C# Test Project
/* References
 * http://www.codeproject.com/Articles/137979/Simple-HTTP-Server-in-C
 * https://github.com/jeske/SimpleHttpServer/blob/master/SimpleHttpServer.cs
 * http://www.codeproject.com/Articles/1505/Create-your-own-Web-Server-using-C
 * http://nowonbun.tistory.com/257
 * http://nowonbun.tistory.com/178
 */


namespace webServerTest
{
    /// <summary>
    /// TcpListener를 사용한 웹 서버 (일단 테스트용)
    /// </summary>
    public class WebServer
    {
        private IPEndPoint ipep;
        private TcpListener listener;
        private TcpClient client;
        
        // 헤더 분석
        private string reqMethod;
        private string reqURI;
        private string httpVer;
        private Dictionary<string, string> headers;

        public WebServer(int port)
        {
            ipep = new IPEndPoint(IPAddress.Any, port);
            // IP 및 포트를 할당하여 EndPoint를 만듦
            listener = new TcpListener(ipep);
            listener.Start();
            Console.WriteLine("[TcpSock] TcpListener Opened");
            // Socket으로 치면 바인딩 후 리슨 시작
            //Listen();
            Thread thread = new Thread(Listen);
            thread.Start();
        }

        public void Listen() {
            // 클라이언트로부터 accept를 받는 부분
            // block이 걸리므로 별도의 스레드 처리를 하는게 낫다
            // ...그 block이 걸려야 하는데
            while(true) {
                // 먼저 들어오는 연결을 받는다
                Console.WriteLine("[TcpSock] Accept Waiting");
                client = listener.AcceptTcpClient();

                // 연결된 후 서버와 통신하는 동작을 별도의 스레드로 처리한다
                Thread connector = new Thread(new ParameterizedThreadStart(HttpWork));
                connector.Start(client);
            }
        }

        private void HttpWork(Object client)
        {
            // 클라이언트로부터 메시지를 받기 위한 스트림 생성
            NetworkStream ns = ((TcpClient)client).GetStream();

            // 클라이언트에서 헤더 메시지 받기
            HttpRecvHeader(ns);
            
            // 클라이언트로 서버 메시지 보내기
            HttpSendMessage(ns);

            // 웹페이지 내용 전송하기
            WebViewer(ns);

            // 파일 스트림 닫기
            ns.Close();
            
        }

        private void HttpRecvHeader(NetworkStream ns)
        {
            /* ******************
             * HTTP 헤더 예시 from www.joinc.co.kr
             * 1 GET /cgi-bin/http_trace.pl HTTP/1.1\r\n
             * 2 ACCEPT_ENCODING: gzip,deflate,sdch\r\n
             * 3 CONNECTION: keep-alive\r\n
             * 4 ACCEPT: text/html,application/xhtml+xml,application/xml;q=0.9,* /*;q=0.8\r\n
             * 5 ACCEPT_CHARSET: windows-949,utf-8;q=0.7,*;q=0.3\r\n
             * 6 USER_AGENT: Mozilla/5.0 (X11; Linux i686) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.24\r\n 
             * 7 ACCEPT_LANGUAGE: ko-KR,ko;q=0.8,en-US;q=0.6,en;q=0.4\rn
             * 8 HOST: www.joinc.co.kr\r\n
             * 9 \r\n
             * 
             * (중요, 4번 라인의 q=0.9, 이후의 *와 /*는 공백없이 붙어있어야 함)
             * 
             * HTTP 헤더의 구성 (최상위라인 헤더 3개)
             * 1. 요청 메소드: GET, PUT, POST, PUSH, OPTION 중 하나의 요청방식
             * 2. 요청 URI: 요청하는 자원의 위치 명시
             * 3. HTTP 프로토콜 버전: 웹 브라우저가 사용하는 프로토콜 버전
             * 
             * HTTP 헤더는 캐리지리턴(\r\n)을 사용함을 주의
             * 첫 줄은 그냥 읽고, 다음 줄 부터는 콜론과 캐리지리턴을 기준으로 split하면 될것
             */

            // 클라이언트로부터 헤더를 입력 받는 부분
            // 여기서는 첫 줄만 입력 받음
            string read = "";
            while(true) {
                int readbyte = ns.ReadByte();
                if (readbyte == '\r') { continue; }
                if (readbyte == '\n') { break; }
                if (readbyte == -1) { Thread.Sleep(1); continue; } // 스트림이 브라우저로 부터 읽지 못한 경우이므로 패스
                read += Convert.ToChar(readbyte);
            }

            // 헤더 첫 줄 분석
            string[] headerFirst = read.Split(' ');
            reqMethod = headerFirst[0];
            reqURI = headerFirst[1];
            httpVer = headerFirst[2];
            Console.WriteLine("{0} / {1} / {2}", reqMethod, reqURI, httpVer);
            
            string key = "";
            string value = "";
            bool iskey = true;
            byte[] headerLeft = new byte[4096];
            ns.Read(headerLeft, 0, 4096);

            // 나머지 헤더 분석
            headers = new Dictionary<string, string>();
            for (int i = 0; i < 4096; i++ )
            {
                int readbyte = headerLeft[i];
                if (readbyte == ':') { iskey = false; continue; }
                if (readbyte == '\n') {
                    iskey = true;
                    headers.Add(key, value);
                    key = "";
                    value = "";
                    continue;
                }
                if (readbyte == '\r') continue;
                if (readbyte == -1) { Thread.Sleep(1); continue; }
                if (iskey == true)
                {
                    key += Convert.ToChar(readbyte);
                }
                else
                {
                    value += Convert.ToChar(readbyte);
                }
            }

            for (int i = 0; i < headers.Count; i++)
            {
                Console.WriteLine("{0}: {1}", headers.ElementAt(i).Key, headers.ElementAt(i).Value);
            }
            Console.WriteLine("[TcpSock] Header Anaysis Complete\n");
        }

        private void HttpSendMessage(NetworkStream ns)
        {
            // 파일 내용을 보내기 전에 HTTP 프로토콜에 대한 리턴 메시지 전송
            if (reqMethod == "GET")
            {
                Console.WriteLine("GET Message Send");
                string httpMsg = "HTTP/1.1 101 Web Socket Protocol Handshake\r\n"
                     + "Upgrade: websocket\r\n"
                     + "Connection: Upgrade\r\n"
                    //    + "Cache-Control: private\r\n"
                    //    + "Data: "+data+"\r\n"
                     + "Server: ArinCherryBlossom\r\n"
                     + "Content-Type: text/html; charset=utf-8\r\n"
                     + "WebSocket-Origin: http://localhost:8080\r\n"
                     + "WebSocket-Location: ws://localhost:8081\r\n";

                byte[] send = Encoding.UTF8.GetBytes(httpMsg);
                ns.Write(send, 0, send.Length);
                Console.WriteLine(httpMsg);
                Console.WriteLine("[TcpSock] GET Message Send Complete");
            }
            else if (reqMethod == "POST")
            {

            }
            else
            {
                Console.WriteLine("GET, POST 이외의 방식 미지원");
            }
        }

        private void WebViewer(NetworkStream ns)
        {
            // 지원 가능한 파일에 대해 웹페이지 파일을 열고 클라이언트로 전송
            string path = "";
            string data = "";

            // index 여부 확인
            path = GetPath(reqURI);
            if (path == "NO_INDEX")
            {
                data = "<html><body>Cannot find index</body></html>";
            }
            else if (path == "")
            {
                data = "";
            }
            else
            {
                FileStream fs = File.Open(path, FileMode.Open);
                Console.WriteLine("Index: {0}", new FileInfo(path).FullName);
                StreamReader fsr = new StreamReader(fs);
                data = fsr.ReadToEnd();
            }
            
            // 메시지 추가 전송
            string aMsg = "Content-Length: " + data.Length + "\r\n"
            + "\r\n"
            + data;

            byte[] send = Encoding.UTF8.GetBytes(aMsg);
            ns.Write(send, 0, send.Length);
            Console.WriteLine(aMsg);
            Console.WriteLine("[TcpSock] GET Message Send Complete\n\n\n");
        }

        private string GetPath(string uri)
        {
            string path = "";
            if (uri == "/")
            {
                // index인 경우 default.dat 열기
                FileStream fs = File.Open("./data/default.dat", FileMode.Open);
                StreamReader fsr = new StreamReader(fs);
                while (fsr.Peek() != -1)
                {
                    string data = fsr.ReadLine();
                    FileInfo file = new FileInfo("./root/" + data);
                    if (file.Exists == true)
                    {
                        path = "./root/" + data;
                        break;
                    }
                }
                if (path == "") path = "NO_INDEX";
            }
            else
            {
                bool exist = new FileInfo("./root" + uri).Exists;
                if(exist == true) path = "./root" + uri;
                else path = "";
            }

            // 파일 존재여부 확인

            return path;
        }
    }
}
