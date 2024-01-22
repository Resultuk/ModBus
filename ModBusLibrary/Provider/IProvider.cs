namespace ModBusLibrary.Provider
{
    public interface IProvider
    {
        object GetSynchro { get; }
        void Send(byte[] WriteData);
        int Receive(ref byte[] ReadData, int timeout);
        bool Connect(int timeOut);
        bool Disconnect();
    }
}