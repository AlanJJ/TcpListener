using System;
using System.Net.Sockets; 

namespace tcp
{
    /// <summary>  
    /// 构造函数和析构函数  
    /// </summary>  
    public partial class NetworkStreamPlus : IDisposable
    {
        /// <summary>  
        /// 网络数据流，只读字段  
        /// </summary>  
        public readonly NetworkStream Stream;

        /// <summary>  
        /// 构造函数  
        /// </summary>  
        public NetworkStreamPlus(NetworkStream netStream)
        {   // 只读字段只能在构造函数中初始化  
            Stream = netStream;
        }

        /// <summary>  
        /// 释放所有托管资源和非托管资源  
        /// </summary>  
        public void Dispose()
        {
       

            // 请求系统不要调用指定对象的终结器  
            GC.SuppressFinalize(this);
        }

        /// <summary>  
        /// 析构函数  
        /// </summary>  
        ~NetworkStreamPlus()
        {
            Dispose();
        }
    }  
}
