using System.IO.Ports;
namespace ModBusLibrary.Provider
{
public class ComProvider : IProvider
{
    public int BaudRate
    {
        get => port.BaudRate;
        set
        {
            if (port.BaudRate != value)
            {
                port.BaudRate = value;
            }
        }
    }
    public string PortName
    {
        get => port.PortName;
        set
        {
            if (!port.PortName.Equals(value))
            {
                lock(synchro)
                {
                    if (port.IsOpen) 
                        port.Close();
                    port.PortName = value;
                }
            }
        }
    }
    private readonly SerialPort port = new();
    private readonly object synchro = new();
    public bool Connect(int timeOut)
    {
        try
        {
            lock(synchro)
                if (!port.IsOpen) 
                    port.Open();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public bool Disconnect()
    {
        try
        {
            lock(synchro)
                if (port.IsOpen) 
                    port.Close();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public object GetSynchro()
    {
        return synchro;
    }
    public int Receive(ref byte[] ReadData, int timeout)
    {
        lock (synchro)
        {
            try
            {
                DateTime T = DateTime.Now;
                while (port.BytesToRead < ReadData.Length && (DateTime.Now - T).TotalMilliseconds < timeout) Thread.Sleep(5);
                port.ReadTimeout = timeout;
                return port.Read(ReadData, 0, ReadData.Length);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
    public void Send(byte[] WriteData)
    {
        lock (synchro)
        {
            try
            {
                port.WriteTimeout = 1000;
                port.Write(WriteData, 0, WriteData.Length);
            }
            catch
            {
            }
        }
    }
 }
}