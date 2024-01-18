namespace ModBusLibrary.Provider
{
public interface IProvider
{
    object GetSynchro { get; }
    public void Send(byte[] WriteData);
    public int Receive(ref byte[] ReadData, int timeout);
    public bool Connect(int timeOut);
    public bool Disconnect();
}
}