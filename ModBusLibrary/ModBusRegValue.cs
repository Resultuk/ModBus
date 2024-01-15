namespace ModBusLibrary;
    public struct ModBusRegValue(byte[] now, byte[] last, byte[] wait)
    {
        public byte[] Now { get; private set; } = now;
        public byte[] Last { get; private set; } = last;
        public byte[] Wait { get; private set; } = wait;
    }