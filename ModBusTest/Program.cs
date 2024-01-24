using System;
using ModBusLibrary;
using ModBusLibrary.Provider;

namespace ModBusTest
{
    internal class Program
    {
        static MyDev rv = new(816, 332){ NetAddress = 1, WaitingTime = 500};
        static Dictionary<string, Calculator> ParamDict = new(
                                                                [
                                                                    new KeyValuePair<string, Calculator>("T1", new Calculator("T1", "°C")),
                                                                    new KeyValuePair<string, Calculator>("T2", new Calculator("T2", "°C")),
                                                                    new KeyValuePair<string, Calculator>("P1", new Calculator("P1", "МПа") { Precision = 3}),
                                                                    new KeyValuePair<string, Calculator>("P2", new Calculator("P2", "МПа") { Precision = 3}),
                                                                    new KeyValuePair<string, Calculator>("T1Om", new Calculator("T1Om", "Ом")),
                                                                    new KeyValuePair<string, Calculator>("T2Om", new Calculator("T2Om", "Ом")),
                                                                    new KeyValuePair<string, Calculator>("P1mA", new Calculator("P1mA", "мА")),
                                                                    new KeyValuePair<string, Calculator>("P2mA", new Calculator("P2mA", "мА")),
                                                                ]
                                                            );
        static void Main(string[] args)
        {
            rv.Provider = new ComProvider(){ PortName = "COM2", BaudRate = 19200};
            rv.AddInputRegsInfo(MyDev.GetModBusRegsInfo());
            rv.AddHoldRegsInfo(MyDev.GetModBusHoldRegInfo());
            rv.InputValueChanged += rv_InputValueChanged;
            rv.HoldValueChanged += rv_HoldValueChanged;

            while(true)
            {
                rv.ReadInputRegs(210, 50);
                rv.ReadHoldRegs(16, 4);
                rv.ReadHoldRegs(86, 2);
                System.Threading.Thread.Sleep(100);
            }
        }
        public class MyDev(uint inputSize, uint holdSize) : ModBusDev(inputSize, holdSize)
        {
            public float T1 => BitConverter.ToSingle(GetInputValue("T1").Now.Flip(), 0);
            public float P1 => BitConverter.ToSingle(GetInputValue("P1").Now.Flip(), 0);
            public float T2 => BitConverter.ToSingle(GetInputValue("T2").Now.Flip(), 0);
            public float P2 => BitConverter.ToSingle(GetInputValue("P2").Now.Flip(), 0);

            public uint N2 => BitConverter.ToUInt32(GetHoldValue("N2").Now.Flip(), 0);
            public ThermometerType TypeT1
            {
                get
                {
                    try
                    {
                        return (ThermometerType)BitConverter.ToUInt32(GetHoldValue("Tip_PT1").Now.Flip(), 0);
                    }
                    catch
                    {
                        return ThermometerType.Unknown;
                    }
                }
            }
            public ThermometerType TypeT2
            {
                get
                {
                    try
                    {
                        return (ThermometerType)BitConverter.ToUInt32(GetHoldValue("Tip_PT2").Now.Flip(), 0);
                    }
                    catch
                    {
                        return ThermometerType.Unknown;
                    }
                }
            }
            public uint Tip_PT2 => BitConverter.ToUInt32(GetHoldValue("Tip_PT2").Now.Flip(), 0);
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
            public static ModBusRegInfo[] GetModBusHoldRegInfo()
            {
                return new List<ModBusRegInfo>() {  
                                                new ModBusRegInfo("N2", 86, ModBusType.UInt32),
                                                new ModBusRegInfo("Tip_PT1", 16, ModBusType.UInt32),
                                                new ModBusRegInfo("Tip_PT2", 18, ModBusType.UInt32),
                                              }.ToArray();
            }
        }
        static void rv_HoldValueChanged(object? sender, ModBusRegInfo[] e)
        {
            foreach(ModBusRegInfo item in e)
            {
                switch(item.Name)
                {
                    case "N2" :         Console.WriteLine($"N2 - {rv.N2}"); break;
                    case "Tip_PT1" :    Console.WriteLine($"Тип датчика 1 - {rv.TypeT1}"); break;
                    case "Tip_PT2" :    Console.WriteLine($"Тип датчика 2 - {rv.TypeT2}");break;
                }
            }
        }
        static void rv_InputValueChanged(object? sender, ModBusRegInfo[] e)
        {
            foreach(ModBusRegInfo item in e)
            {
                switch(item.Name)
                {
                    case "T1" : ParamDict[item.Name].Value = rv.T1; break;
                    case "T2" : ParamDict[item.Name].Value = rv.T2; break;
                    case "P1" : ParamDict[item.Name].Value = rv.P1; break;
                    case "P2" : ParamDict[item.Name].Value = rv.P2; break;
                }
                Console.WriteLine($"{ParamDict[item.Name]}");
            }
        }

    }
    public enum ThermometerType
    {
        Unknown = 0,
        Pt100 = 1,
        Pt500 = 2,
        Pt1000 = 3,
        _100П = 4,
        _500П = 5,
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
    public class Calculator(string name, string units = "")
    {
        private double value = double.NaN;
        private double minValue = double.NaN;
        private double maxValue = double.NaN;
        private double sum = 0;
        private long count = 0;

        public string Name = name;
        public string UnitsOfMeasurement {get; set;} = units;
        public double Value
        {
            get => value;
            set
            {
                this.value = value; LastChanges = DateTime.Now;
                if (count == 0)
                {
                    Max = value;
                    Min = value;
                    sum = value;
                    FirstChanges = DateTime.Now;
                }
                else
                {
                    if (value > Max) Max = value;
                    if (value < Min) Min = value;
                    sum += value;
                }
                count++;
            }
        }
        public DateTime FirstChanges { get; private set; }
        public DateTime ResetTime { get; private set; }
        public DateTime LastChanges { get; private set; }
        public int Precision { get; set; } = 2;
        public double Min { get => minValue; private set => minValue = value; }
        public double Max { get => maxValue; private set => maxValue = value; }
        public double Avg { get => count == 0 ? 0 : sum / count; }
        public void Reset()
        {
            Value = double.NaN;
            Min = double.NaN;
            Max = double.NaN;
            sum = 0;
            count = 0;
            ResetTime = DateTime.Now;
            FirstChanges = DateTime.MinValue;
            LastChanges = DateTime.MinValue;
        }
        public override string ToString()
        {
            return $"{Name}-{Math.Round(Value, Precision)}; Min:{Math.Round(Min, Precision)}; Avg:{Math.Round(Avg, Precision)}; Max:{Math.Round(Max, Precision)} {UnitsOfMeasurement}";
        }
    }
}