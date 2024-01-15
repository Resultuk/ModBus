using System;
using ModBusLibrary;
using ModBusLibrary.Provider;

namespace ModBusTest
{
    internal class Program
    {
        static MyDev rv = new(816, 332){ NetAddress = 1, WaitingTime = 500};
        static void Main(string[] args)
        {
            rv.Provider = new ComProvider(){ PortName = "COM2", BaudRate = 19200};
            rv.AddInputRegsInfo(MyDev.GetModBusRegsInfo());
            rv.InputValueChanged += rv_InputValueChanged;

            while(true)
            {
                rv.ReadInputRegs(210, 50);
                System.Threading.Thread.Sleep(1000);
            }
        }
        public class MyDev(uint inputSize, uint holdSize) : ModBusDev(inputSize, holdSize)
        {
            public float T1 => BitConverter.ToSingle(GetInputValue("T1").Now.Flip(), 0);
            public float P1 => BitConverter.ToSingle(GetInputValue("P1").Now.Flip(), 0);
            public float T2 => BitConverter.ToSingle(GetInputValue("T2").Now.Flip(), 0);
            public float P2 => BitConverter.ToSingle(GetInputValue("P2").Now.Flip(), 0);
            public static ModBusRegInfo[] GetModBusRegsInfo()
            {
                return  [
                            new("T1Om", 210, ModBusType.Float), 
                            new("T2Om", 212, ModBusType.Float), 
                            new("T1",   214, ModBusType.Float), 
                            new("T2",   216, ModBusType.Float),
                            new("P1mA", 220, ModBusType.Float), 
                            new("P2mA", 222, ModBusType.Float), 
                            new("P1",   256, ModBusType.Float), 
                            new("P2",   258, ModBusType.Float), 
                        ];
            }
        }
        static void rv_InputValueChanged(object? sender, ModBusRegInfo[] e)
        {
            foreach(ModBusRegInfo item in e)
            {
                if(item.Name.Equals("T1"))
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - T1 = {rv.T1:F2} °C");
                if(item.Name.Equals("T2"))
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - T2 = {rv.T2:F2} °C");
                if(item.Name.Equals("P1"))
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - T1 = {rv.P1:F3} МПа");
                if(item.Name.Equals("P2"))
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - T2 = {rv.P2:F3} МПа");
            }
        }

    }
        public static class MyExtensions
        {
        public static byte[] Flip(this byte[] bytes)
        {
            byte[] Result = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                Result[i + 1] = bytes[i];
                Result[i] = bytes[i + 1];
            }
            return Result;
        }
        }
}