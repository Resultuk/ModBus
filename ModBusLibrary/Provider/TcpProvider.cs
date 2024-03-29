using System;
using System.Net.Sockets;
namespace ModBusLibrary.Provider
{
public class TcpProvider : IProvider
    {
        private TcpClient tcpClient = new TcpClient();
        private readonly object synchro = new object();
        public object GetSynchro => synchro; 
        public string Url = string.Empty;
        public int Port;
        public bool Connect(int timeOut)
        {
            try
            {
                // tcpClient.Connect(Url, Port);
                // DateTime dt = DateTime.Now;
                tcpClient = new TcpClient();
                var c = tcpClient.BeginConnect(Url, Port, null, null);
                DateTime dt = DateTime.Now;
                var success = c.AsyncWaitHandle.WaitOne(timeOut);
                //Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
                if (success)
                {
                    tcpClient.EndConnect(c);
                    return true;
                }
                TimeSpan timeSpan = DateTime.Now - dt;
            }
            catch (Exception)
            {

            }
            return false;
        }
        public bool Disconnect()
        {
            tcpClient.Close();
            return true;
        }
        public int Receive(ref byte[] ReadData, int timeout)
        {
            try
            {
                tcpClient.ReceiveTimeout = timeout;
                return tcpClient.Client.Receive(ReadData, SocketFlags.None);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public void Send(byte[] WriteData)
        {
            try
            {
                tcpClient.Client.Send(WriteData);
            }
            catch (Exception)
            {
            }
        }
    }
}