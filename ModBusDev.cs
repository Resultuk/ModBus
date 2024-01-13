using System.Reflection.Metadata;

namespace Protocols;
public abstract class ModBusDev(uint inputSize, uint holdSize)
{
    private readonly Dictionary<string, ModBusRegInfo> holdsInfo = [];
    private readonly Dictionary<string, ModBusRegInfo> inputsInfo = [];
    private readonly byte[] holdsIN = new byte[holdSize];
    private readonly byte[] holdsOUT = new byte[holdSize];
    private readonly byte[] inputIN = new byte[inputSize];
    private readonly byte[] inputCASH = new byte[inputSize];
    public byte NetAddress { get; set; }
    public uint WaitingTime { get; set; }
    public IProvider? Provider {get; set; }
    public uint LengthInputs => (uint)inputIN.Length;
    public uint LengthHolds => (uint)holdsIN.Length;
    public bool AnyDataForWriting => !Enumerable.SequenceEqual(holdsIN, holdsOUT);
    public void UndoChanges()
    {
        holdsIN.CopyTo(holdsOUT, 0);
    }
    public int AddHoldRegsInfo(ModBusRegInfo[] modBusRegInfos)
    {
        int Result = 0;
        foreach( ModBusRegInfo item in modBusRegInfos)
        {
            if(holdsInfo.TryAdd(item.Name, item)) 
                Result++;
        }
        return Result;
    }
    public int AddInputRegsInfo(ModBusRegInfo[] modBusRegInfos)
    {
        int Result = 0;
        foreach( ModBusRegInfo item in modBusRegInfos)
        {
            if(inputsInfo.TryAdd(item.Name, item)) 
                Result++;
        }
        return Result;
    }
    public Report ReadInputRegs(ushort startReg, ushort countRegs)
    {
        int startbyte = startReg * 2;

        int countbyte = countRegs * 2;

        if (countRegs == 0 || countRegs > 127 || startbyte + countbyte > LengthInputs) return new Report { Result = ResultRequest.WrongRequest };

        Report report;
        int k = 0;
        do
        {
            report = Modbus.ReadRegs(Provider, 1, (byte)ModBusFunc.ReadInput, startReg, countRegs, (int)WaitingTime);
        }
        while (report.Result != ResultRequest.OK && k++ < 3);
        report.NumberOfRequery = k;

        if (report.Result == ResultRequest.OK)
        {
            ToInputRegs(report.Response.Skip(3).Take(countbyte).ToArray(), startbyte, countbyte);
        }
        return report;
    }
    private void ToInputRegs(byte[] buffer, int offset, int count)
    {
        if (buffer == null || buffer.Length == 0 || offset < 0 || count < 1) throw new ArgumentException();
        Array.Copy(inputIN, offset, inputCASH, offset, count);
        Array.Copy(buffer, 0, inputIN, offset, count);
        //List<ushort> bytesList = GetChangingBytes(offset, count);
        //if (bytesList.Count > 0)
        //MemoryChanged?.Invoke(this, new EventMemoryArgs() { Name = _name, Bytes = bytesList }); 
    }
}