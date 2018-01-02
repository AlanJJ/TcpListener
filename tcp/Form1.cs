using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
namespace tcp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private TcpListenerPlus Server;
        private void button1_Click(object sender, EventArgs e)
        {
            if (Server == null)
            {
                try
                {
                    Server = new TcpListenerPlus(IPAddress.Parse(textBox_IP.Text), Convert.ToInt32(textBox_Port.Text));
                    Server.OnThreadTaskRequest += new TcpListenerPlus.ThreadTaskRequest(GetAnswer);
                    ((Button)sender).Text = "关闭服务器";
                }
                catch (Exception)
                {
                    if (Server != null)
                    {
                        Server.Stop();
                        Server = null;
                    }
                    MessageBox.Show(this, "启动服务器失败！", "信息");
                }
            }
            else
            {
                Server.Stop();
                Server = null;
                ((Button)sender).Text = "启动服务器";
            }
        }

        private void GetAnswer(object sender, EventArgs e)
        {
            TcpClient tcpClient = (TcpClient)sender;
            using (NetworkStreamPlus Stream = new NetworkStreamPlus(tcpClient.GetStream()))
            {   // 调整接收缓冲区大小  
                Stream.ReceiveBufferSize = tcpClient.ReceiveBufferSize;
                        
                while (true)
                {
                    try
                    {
                        // 获取查询内容  
                        String Question;
                        Stream.Read(out Question);
                        SetText("客户端:" + Question + "\r\n");

                        // 返回查询结果  
                        var Answer = Question;
                        Stream.Write(Answer);
                        SetText("服务端:" + Answer + "\r\n");
                      
                    }
                    catch (Exception ex)
                    {
                        Type type = ex.GetType();
                        if (type == typeof(TimeoutException))
                        {   // 超时异常，不中断连接  
                            SetText("数据超时失败！\r\n\r\n");
                        }
                        else
                        {   // 仍旧抛出异常，中断连接  
                           // SetText("中断连接异常原因：" + type.Name + "\r\n");
                            SetText("连接中断" + "\r\n");
                            throw ex;
                        }
                    }
                }
            }
        }
        // 对 Windows 窗体控件进行线程安全调用  
        private void SetText(String text)
        {
            if (textBox_Notes.InvokeRequired)
            {
                textBox_Notes.BeginInvoke(new Action<String>((msg) =>
                {
                    textBox_Notes.AppendText(msg);
                }), text);
            }
            else
            {
                textBox_Notes.AppendText(text);
            }
        }
        // 对 Windows 窗体控件进行线程安全调用  
        //private String GetSecretKey()
        //{
        //    if (textBox_SecretKey.InvokeRequired)
        //    {
        //        return (String)textBox_SecretKey.Invoke(new Func<String>(() => { return textBox_SecretKey.Text; }));
        //    }
        //    else
        //    {
        //        return textBox_SecretKey.Text;
        //    }
        //}


    }
}
