using ModBusLibrary.Provider;
namespace ModBusLibrary;
public static class Modbus
{
    private static Report Inquiry(IProvider provider, byte[] writeData, ref byte[] readData, int CountWaitBytes, int timeout = 550)
    {
        Report swapResult = new();
        lock (provider.GetSynchro())
        {
            swapResult = new Report() { Request = writeData };
            try
            {
                if (provider.Connect(timeout))
                {
                    swapResult.RequestTime = DateTime.Now;
                    provider.Send(writeData);
                    System.Threading.Thread.Sleep(15);
                    int countReadBytes = provider.Receive(ref readData, timeout);
                    swapResult.ResponseTime = DateTime.Now;
                    swapResult.Response = readData;
                    swapResult.Result = ResultRequest.OK;
                    if (!CRC16.Check(ref readData, countReadBytes))
                    {
                        swapResult.Result = ResultRequest.CRCError;
                    }
                    else
                    {
                        if (countReadBytes != CountWaitBytes)
                        {
                            swapResult.Result = ResultRequest.WrongRequest;
                        }
                        if (writeData[0] != readData[0])
                        {
                            swapResult.Result = ResultRequest.WrongResponce;
                        }
                    }
                }
                else
                {
                    swapResult.Result = ResultRequest.TransportError;
                }
            }
            catch (Exception ex)
            {
                swapResult.ErrorMassage = ex.Message;
                swapResult.Result = ResultRequest.Error;
            }
            provider.Disconnect();
        }
        return swapResult;
    }
    public static Report ReadRegs(IProvider? provider, uint netAddress = 1, byte function = 4, int startReg = 0, int countReg = 1, int timeout = 550)
    {
        if (provider == null) return new Report(){ Result = ResultRequest.TransportError, ErrorMassage = "Провайдер не определен!"};
        if (countReg < 1 || countReg > 128 || startReg * 2 + countReg * 2 > ushort.MaxValue) return new Report { Result = ResultRequest.WrongRequest };
        byte[] writeData = [(byte)netAddress, function, (byte)(startReg >> 8), (byte)startReg, (byte)(countReg >> 8), (byte)countReg, 0, 0];
        byte[] ReadData = new byte[countReg * 2 + 5];
        CRC16.Add(ref writeData);
        return Inquiry(provider, writeData, ref ReadData, ReadData.Length, timeout);
    }
    public static Report WriteRegs(IProvider provider, byte[] buffer, uint netAddress = 1, byte function = 16, int startReg = 0, int countReg = 1, int timeout = 550)
    {
        if (countReg < 1 || countReg > 128 || startReg * 2 + countReg * 2 > ushort.MaxValue) return new Report { Result = ResultRequest.WrongRequest };
        byte[] writeData = new byte[9 + countReg * 2];
        byte[] ReadData = new byte[8];
        writeData[0] = (byte)netAddress;
        writeData[1] = (byte)function;
        writeData[2] = (byte)(startReg >> 8);
        writeData[3] = (byte)(startReg);
        writeData[4] = (byte)(countReg >> 8);
        writeData[5] = (byte)(countReg);
        writeData[6] = (byte)(countReg * 2);
        buffer.CopyTo(writeData, 7);
        CRC16.Add(ref writeData);
        return Inquiry(provider, writeData, ref ReadData, ReadData.Length, timeout);
    }
    public static Report Command(IProvider provider, uint netAddress, byte[] buffer, int timeout = 550, int function = 0x11)
    {
        byte[] writeBuff = new byte[6] { 0, 0, 0, 0, 0, 0 };
        if (buffer != null)
        {
            int k = 0;
            foreach (var item in buffer)
            {
                writeBuff[k] = buffer[k];
                k++;
            }
        }
        return WriteRegs(provider, writeBuff, netAddress, (byte)function, 0, 3, timeout);
    }
    public static async Task<Report> WriteRegsAsync(IProvider provider, byte[] buffer, uint netAddress = 1, byte function = 16, int startReg = 0, int countReg = 1, int timeout = 550)
    {
        if (countReg < 1 || countReg > 128 || startReg * 2 + countReg * 2 > ushort.MaxValue) return new Report { Result = ResultRequest.WrongRequest };
        byte[] writeData = new byte[9 + countReg * 2];
        byte[] ReadData = new byte[8];
        writeData[0] = (byte)netAddress;
        writeData[1] = (byte)function;
        writeData[2] = (byte)(startReg >> 8);
        writeData[3] = (byte)(startReg);
        writeData[4] = (byte)(countReg >> 8);
        writeData[5] = (byte)(countReg);
        writeData[6] = (byte)(countReg * 2);
        buffer.CopyTo(writeData, 7);
        CRC16.Add(ref writeData);
        return await Task.Run(() => Inquiry(provider, writeData, ref ReadData, ReadData.Length, timeout));
    }
    public static async Task<Report> ReadRegsAsync(IProvider provider, uint netAddress = 1, byte function = 4, int startReg = 0, int countReg = 1, int timeout = 550)
    {
        if (countReg < 1 || countReg > 128 || startReg * 2 + countReg * 2 > ushort.MaxValue) return new Report { Result = ResultRequest.WrongRequest };
        byte[] writeData = [(byte)netAddress, function, (byte)(startReg >> 8), (byte)startReg, (byte)(countReg >> 8), (byte)countReg, 0, 0];
        byte[] ReadData = new byte[countReg * 2 + 5];
        CRC16.Add(ref writeData);
        return await Task.Run(() => Inquiry(provider, writeData, ref ReadData, ReadData.Length, timeout));
    }
    private static Dictionary<ushort, ushort> SequenceToIntervalList(List<ushort> list)
    {
        if (list == null || list.Count == 0) return [];
        list.Sort();
        ushort Prev = 0;
        ushort LastInsert = 0;
        Dictionary<ushort, ushort> Result = [];
        foreach (ushort item in list)
        {
            if (Result.Count == 0) 
            { 
                Result.Add(item, 1); 
                LastInsert = Prev = item; 
                continue; 
            }
            if (Prev == item)
            { 
                continue; 
            }
            if (Prev + 1 == item) 
            { 
                if (Result[LastInsert] < 127) 
                { 
                    Result[LastInsert]++; 
                    Prev = item; 
                } 
                else 
                { 
                    Result.Add(item, 1); 
                    LastInsert = Prev = item; 
                } 
                continue;
            }
            else 
            { 
                Result.Add(item, 1); 
                LastInsert = Prev = item; 
                continue; 
            }
        }
        return Result;
    }
}
public enum ModBusFunc
{
    ReadInput = 4,
    ReadHold = 3,
    WriteHold = 16
}