using System.Linq.Expressions;
using Protocols;

namespace ModBusTest
{
    internal class Program
    {
        static MyDev rv = new(816, 332);
        static void Main(string[] args)
        {
            rv.Provider = new ComProvider(){ PortName = "COM2", BaudRate = 115200};
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
            public float T1 => 
        }
        static void rv_InputValueChanged(object sender, ModBusRegInfo[] e)
        {

        }
    }
}