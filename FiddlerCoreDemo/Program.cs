using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiddler;

namespace FiddlerCoreDemo
{

    /// <summary>
    /// 下载fiddlercore包
    /// https://www.nuget.org/packages/FiddlerCore/
    /// 
    /// 简单使用参考这个博客
    /// https://www.cnblogs.com/jasongrass/p/12044321.html
    ///
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            FiddlerApplication.Log.LogString($"启动程序...");

            InstallCertificate();

            AttachListening();

            StartupFiddlerCore();

            Console.WriteLine("按任意键结束本地代理监听..." + Environment.NewLine);
            Console.Read();


            UninstallCertificate();

            UninstallFiddler();


        }

        /// <summary>启动fiddlercore本地设置的代理端口监听</summary>
        private static void StartupFiddlerCore()
        {

            int port = 9898;

            //Console.WriteLine("fiddler启动了...listening: port: 127.0.0.1:" + port);


            FiddlerApplication.Startup(port, FiddlerCoreStartupFlags.Default | FiddlerCoreStartupFlags.RegisterAsSystemProxy);

            FiddlerApplication.Log.LogString($"Created endpoint listening on port {CONFIG.ListenPort}");
        }


        /// <summary>拦截http请求信息事件</summary>
        private static void AttachListening()
        {

            FiddlerApplication.OnNotification += (o, nea) => Console.WriteLine($"** NotifyUser: {nea.NotifyString}");

            FiddlerApplication.Log.OnLogString += (o, lea) => Console.WriteLine($"** LogString: {lea.LogString}");


            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;
        }



        /// <summary>拦截请求返回Request信息</summary>
        private static void FiddlerApplication_BeforeRequest(Session oSession)
        {
            //Debug.WriteLine(oSession.fullUrl);

            // In order to enable response tampering, buffering mode MUST
            // be enabled; this allows FiddlerCore to permit modification of
            // the response in the BeforeResponse handler rather than streaming
            // the response to the client as the response comes in.
            oSession.bBufferResponse = false;

            // Set this property if you want FiddlerCore to automatically authenticate by
            // answering Digest/Negotiate/NTLM/Kerberos challenges itself
            //session["X-AutoAuth"] = "(default)";


            FiddlerApplication.Log.LogString($" {oSession.fullUrl }");
        }

        /// <summary>拦截请求返回Response信息</summary>
        private static void FiddlerApplication_BeforeResponse(Session oSession)
        {

        }


        /// <summary>安装证书</summary>
        public static bool InstallCertificate()
        {
            FiddlerApplication.Log.LogString($"安装证书,为了监听https请求");
            if (!CertMaker.rootCertExists())
            {
                if (!CertMaker.createRootCert())
                    return false;

                if (!CertMaker.trustRootCert())
                    return false;
            }

            return true;
        }

        /// <summary>关闭fiddlercore设置的代理</summary>
        private static void UninstallFiddler()
        {
            if (FiddlerApplication.IsStarted())
            {
                FiddlerApplication.Shutdown();
            }
        }

        /// <summary>卸载证书</summary>
        public static bool UninstallCertificate()
        {
            if (CertMaker.rootCertExists())
            {
                if (!CertMaker.removeFiddlerGeneratedCerts(true))
                    return false;
            }
            return true;
        }
    }
}

