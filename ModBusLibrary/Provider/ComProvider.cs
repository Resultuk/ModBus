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
                if (port.IsOpen) port.Close();
                port.PortName = value;
            }
        }
    }
    private SerialPort port = new();
    private object Synchro = new();
    public bool Connect(int timeOut)
    {
        try
        {
            if (!port.IsOpen) port.Open();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public void Disconnect()
    {
        lock (Synchro)
        {
            try
            {
                if (port.IsOpen) port.Close();
            }
            catch (Exception)
            {
            }
        }
    }
    public object GetSynchro()
    {
        return Synchro;
    }
    public int Receive(ref byte[] ReadData, int timeout)
    {
        lock (Synchro)
        {
            try
            {
                DateTime T = DateTime.Now;
                while (port.BytesToRead < ReadData.Length && (DateTime.Now - T).TotalMilliseconds < timeout) Thread.Sleep(5);
                port.ReadTimeout = timeout;
                return port.Read(ReadData, 0, ReadData.Length);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
    public void Send(byte[] WriteData)
    {
        lock (Synchro)
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