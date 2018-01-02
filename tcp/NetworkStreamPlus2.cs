using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;   

namespace tcp
{
    /// <summary>  
    /// 异步读状态对象  
    /// </summary>  
    internal class AsyncReadStateObject
    {
        public ManualResetEvent eventDone;
        public NetworkStream stream;
        public Exception exception;
        public Int32 numberOfBytesRead;
    }


    /// <summary>
    /// 实现TcpClient的异步接收
    /// </summary>
    public partial class NetworkStreamPlus
    {
        /// <summary>
        /// 接收缓冲区大小
        /// </summary>
        public Int32 ReceiveBufferSize = 128;

        /// <summary>
        /// 异步接收
        /// </summary>
        /// <param name="data">接收到的字节数组</param>
        public void Read(out Byte[] data)
        {
            // 用户定义对象
            AsyncReadStateObject State = new AsyncReadStateObject
            {   // 将事件状态设置为非终止状态，导致线程阻止
                eventDone = new ManualResetEvent(false),
                stream = Stream,
                exception = null,
                numberOfBytesRead = -1
            };

            Byte[] Buffer = new Byte[ReceiveBufferSize];
            using (MemoryStream memStream = new MemoryStream(ReceiveBufferSize))
            {
                Int32 TotalBytes = 0;       // 总共需要接收的字节数
                Int32 ReceivedBytes = 0;    // 当前已接收的字节数
                while (true)
                {
                    // 将事件状态设置为非终止状态，导致线程阻止
                    State.eventDone.Reset();

                    // 异步读取网络数据流
                    Stream.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(AsyncReadCallback), State);

                    // 等待操作完成信号
                    if (State.eventDone.WaitOne(Stream.ReadTimeout, false))
                    {   // 接收到信号
                        if (State.exception != null) throw State.exception;

                        if (State.numberOfBytesRead == 0)
                        {   // 连接已经断开
                            throw new SocketException();
                        }
                        else if (State.numberOfBytesRead > 0)
                        {
                          
                            memStream.Write(Buffer, 0, State.numberOfBytesRead); break;

                        }
                    }
                    else
                    {   // 超时异常
                        throw new TimeoutException();
                    }
                }

                // 将流内容写入字节数组
              
                    data = (memStream.Length > 0) ? memStream.ToArray() : null;
              
            }
        }

        /// <summary>
        /// 异步接收
        /// </summary>
        /// <param name="answer">接收到的字符串</param>
        /// <param name="codePage">代码页</param>
        /// <remarks>
        /// 代码页：
        ///     936：简体中文GB2312
        ///     54936：简体中文GB18030
        ///     950：繁体中文BIG5
        ///     1252：西欧字符CP1252
        ///     65001：UTF-8编码
        /// </remarks>
        public void Read(out String answer, Int32 codePage = 65001)
        {
            Byte[] data;
            Read(out data);
       
            answer = (data != null) ? System.Text.Encoding.Default.GetString(data) : "";
         
        }

        /// <summary>
        /// 异步读取回调函数
        /// </summary>
        /// <param name="ar">异步操作结果</param>
        private static void AsyncReadCallback(IAsyncResult ar)
        {
            AsyncReadStateObject State = ar.AsyncState as AsyncReadStateObject;
            try
            {   // 异步写入结束
                State.numberOfBytesRead = State.stream.EndRead(ar);
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
