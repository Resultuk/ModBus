namespace ModBusLibrary
{
    public struct ModBusRegValue
    {
        public byte[] Now { get; private set; }
        public byte[] Last { get; private set; }
        public byte[] Wait { get; private set; }
        public ModBusRegValue(byte[] now, byte[] last, byte[] wait)
        {
            Now = now;
            Last = last;
            Wait = wait;
        }
    }
}