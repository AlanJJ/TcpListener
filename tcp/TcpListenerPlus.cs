using System;
using System.Net;
using System.Net.Sockets;
using System.Threading; 
namespace tcp
{
    public partial class TcpListenerPlus : TcpListener
    {
        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="localEP">本地终结点</param>  
        public TcpListenerPlus(IPEndPoint localEP)
            : base(localEP)
        {   // 启动独立的侦听线程  
            Thread ListenThread = new Thread(new ThreadStart(ListenThreadAction));
            ListenThread.Start();
        }

        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="localaddr">本地IP地址</param>  
        /// <param name="port">侦听端口</param>  
        public TcpListenerPlus(IPAddress localaddr, Int32 port)
            : base(localaddr, port)
        {   // 启动独立的侦听线程  
            Thread ListenThread = new Thread(new ThreadStart(ListenThreadAction));
            ListenThread.Start();
        }

        /// <summary>  
        /// 析构函数  
        /// </summary>  
        ~TcpListenerPlus()
        {
            Stop();
        }
    }  
}
