using ModBusLibrary;
using ModBusLibrary.Provider;

namespace ModBusTest
{
    internal class Program
    {
        static MyDev rv = new(816, 332);
        static void Main(string[] args)
        {
            rv.Provider = new ComProvider(){ PortName = "COM2", BaudRate = 19200};
            rv.AddInputRegsInfo([new ModBusRegInfo("T1", 0xD6 * 2, ModBusType.Float), new ModBusRegInfo("T2", 0xD8 * 2, ModBusType.Float)]);
            rv.InputValueChanged += rv_InputValueChanged;

            while(true)
            {
                rv.ReadInputRegs(0xD6, 4);
                System.Threading.Thread.Sleep(1000);
            }
        }
        public class MyDev(uint inputSize, uint holdSize) : ModBusDev(inputSize, holdSize)
        {
            public float T1 => BitConverter.ToSingle(GetInputValue("T1").Now.Flip(), 0);
        }
        static void rv_InputValueChanged(object sender, ModBusRegInfo[] e)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - {rv.T1}");
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