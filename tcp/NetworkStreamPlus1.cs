using System;
using System.Net.Sockets;
using System.Text;
using System.Threading; 

namespace tcp
{
    /// <summary>  
    /// 异步写状态对象  
    /// </summary>  
    internal class AsyncWriteStateObject
    {
        public ManualResetEvent eventDone;
        public NetworkStream stream;
        public Exception exception;
    }

    /// <summary>  
    /// 实现TcpClient的异步发送  
    /// </summary>  
    public partial class NetworkStreamPlus
    {
        /// <summary>  
        /// 异步发送  
        /// </summary>  
        /// <param name="buffer">字节数组</param>  
        /// <param name="offset">起始偏移量</param>  
        /// <param name="size">字节数</param>  
        public void Write(Byte[] buffer, Int32 offset, Int32 size)
        {
            // 用户定义对象  
            AsyncWriteStateObject State = new AsyncWriteStateObject
            {   // 将事件状态设置为非终止状态，导致线程阻止  
                eventDone = new ManualResetEvent(false),
                stream = Stream,
                exception = null,
            };

    

            // 写入加长度信息头的数据  
            Stream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(AsyncWriteCallback), State);

            // 等待操作完成信号  
            if (State.eventDone.WaitOne(Stream.WriteTimeout, false))
            {   // 接收到信号  
                if (State.exception != null) throw State.exception;
            }
            else
            {   // 超时异常  
                throw new TimeoutException();
            }
        }

        /// <summary>  
        /// 异步发送  
        /// </summary>  
        /// <param name="data">字节数组</param>  
        public void Write(Byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>  
        /// 异步发送  
        /// </summary>  
        /// <param name="command">字符串</param>  
        /// <param name="codePage">代码页</param>  
        /// <remarks>  
        /// 代码页：  
        ///     936：简体中文GB2312  
        ///     54936：简体中文GB18030  
        ///     950：繁体中文BIG5  
        ///     1252：西欧字符CP1252  
        ///     65001：UTF-8编码  
        /// </remarks>  
        public void Write(String command, Int32 codePage = 65001)
        {
            Write(Encoding.GetEncoding(codePage).GetBytes(command));
        }

        /// <summary>  
        /// 异步写入回调函数  
        /// </summary>  
        /// <param name="ar">异步操作结果</param>  
        private static void AsyncWriteCallback(IAsyncResult ar)
        {
            AsyncWriteStateObject State = ar.AsyncState as AsyncWriteStateObject;
            try
            {   // 异步写入结束  
                State.stream.EndWrite(ar);
            }

            catch (Exception e)
            {   // 异步连接异常  
                State.exception = e;
            }

            finally
            {   // 将事件状态设置为终止状态，线程继续  
                State.eventDone.Set();
            }
        }
    }  
}
