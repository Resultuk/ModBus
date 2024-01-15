using ModBusLibrary.Provider;
namespace ModBusLibrary;
public abstract class ModBusDev
{
    private readonly Dictionary<string, ModBusRegInfo> holdsInfo = [];
    private readonly Dictionary<string, ModBusRegInfo> inputsInfo = [];
    private readonly byte[] holdsIN = [];
    private readonly byte[] holdsOUT = [];
    private readonly byte[] inputIN = [];
    private readonly byte[] inputCASH = [];
    public ModBusDev(uint inputSize, uint holdSize)
    {
        holdsIN = new byte[holdSize];
        holdsOUT = new byte[holdSize];
        inputIN = new byte[inputSize];
        inputCASH = new byte[inputSize];
        for(int i = 0 ; i < holdSize; i++)
        {
            holdsIN[i] = 0xFF;
            holdsOUT[i] = 0xFF;
        }
        for(int i = 0 ; i < inputSize; i++)
        {
            inputIN[i] = 0xFF;
            inputCASH[i] = 0xFF;
        }
    }
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
        List<uint> bytesList = GetChangingBytes(offset, count);
        if(bytesList.Count > 0)
        {
            ModBusRegInfo[] modBusRegs = GetInputsFromByteList(bytesList);
            if(modBusRegs.Length > 0)
                InputValueChanged?.Invoke(this, modBusRegs);
        }
    }
    private ModBusRegInfo[] GetInputsFromByteList(List<uint> bytesList)
    {
        List<ModBusRegInfo> Result = [];
        foreach(uint byteNum in bytesList)
        {
           foreach (var item in inputsInfo.Values)
            {
                if (item.Address <= byteNum && byteNum < item.Address + item.Length)
                    Result.Add(item);
            }
        }
        return Result.ToArray();
    }
    private List<uint> GetChangingBytes(int count = 0, int offset = 0)
    {
        if(count == 0) count = (int)LengthInputs;
        List<uint> result = [];
        for (int i = offset; i < offset + count; i++)
                if (inputIN[i] != inputCASH[i]) result.Add((ushort)i);
        return result;
    }
    public ModBusRegValue GetInputValue(ModBusRegInfo regInfo)
    {
        return new ModBusRegValue(  inputIN.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                    inputCASH.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                    Array.Empty<byte>()
                                );
    }
    public ModBusRegValue GetInputValue(string regInfoName)
    {
        if(inputsInfo.TryGetValue(regInfoName, out ModBusRegInfo? regInfo))
        {
            return new ModBusRegValue(  inputIN.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                    inputCASH.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                    Array.Empty<byte>()
                                );
        }
        return new ModBusRegValue([], [], []);
    }
    #region Events
    public event EventHandler<ModBusRegInfo[]> InputValueChanged = delegate { };
    public event EventHandler<ModBusRegInfo[]> HoldValueChanged = delegate { };
    #endregion
}