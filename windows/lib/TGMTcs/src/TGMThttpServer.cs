using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
using System.Threading.Tasks;

// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace TGMTcs
{
    public class HttpServer
    {
        protected int port;
        static Func<string, string> m_GEThandler;
        static Func<string, string> m_POSThandler;


        public static HttpListener m_listener;
        public static string m_url = "http://*:80/";
        public static int m_pageViews = 0;
        public static int m_requestCount = 0;

        static bool m_runServer = true;

        public static async Task HandleIncomingConnections()
        {
            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (m_runServer)
            {
                // Will wait here until we hear from a connection
                if(m_listener == null || m_listener.IsListening == false)
                {
                    m_runServer = false;
                    continue;
                }
                HttpListenerContext ctx = await m_listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++m_requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                byte[] data = null;
                if (req.HttpMethod == "POST")
                {
                    data = Encoding.UTF8.GetBytes(m_POSThandler(req.Url.AbsolutePath)); //TODO: undone yet
                }
                else if(req.HttpMethod == "GET")
                {
                    data = Encoding.UTF8.GetBytes(m_GEThandler(req.Url.AbsolutePath));
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    m_pageViews += 1;

                // Write the response info
                
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public HttpServer(int port = 80)
        {
            Console.WriteLine("Server ready with port: " + port.ToString());
            m_listener = new HttpListener();
            m_listener.Prefixes.Add(m_url);

            this.port = port;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        Task m_listenTask;
        public void Listen()
        {
            m_listener.Start();
            try
            {
                m_listenTask = HandleIncomingConnections();
                m_listenTask.GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {

            }           

            m_listener.Close();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public void SetGEThandler(Func<string, string> GEThandler)
        {
            m_GEThandler = GEThandler;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SetPOSThandler(Func<string, string> POSThandler)
        {
            m_POSThandler = POSThandler; //TODO: undone yet
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Stop()
        {
            if (m_listener != null && m_listener.IsListening)
                m_listener.Close();

            m_runServer = false;
        }
    }
}



