namespace ModBusLibrary.Provider
{
public interface IProvider
{
    public object GetSynchro();
    public void Send(byte[] WriteData);
    public int Receive(ref byte[] ReadData, int timeout);
    public bool Connect(int timeOut);
    public void Disconnect();
}
}