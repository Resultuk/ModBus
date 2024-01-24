using System;
using System.Collections.Generic;
using System.Linq;
using ModBusLibrary.Provider;

namespace ModBusLibrary
{
    public abstract class ModBusDev
    {
        private readonly Dictionary<string, ModBusRegInfo> holdsInfo = new Dictionary<string, ModBusRegInfo>();
        private readonly Dictionary<string, ModBusRegInfo> inputsInfo = new Dictionary<string, ModBusRegInfo>();
        private readonly byte[] holdIN = new byte[0];
        private readonly byte[] holdOUT = new byte[0];
        private readonly byte[] holdCASH = new byte[0];
        private readonly byte[] inputIN = new byte[0];
        private readonly byte[] inputCASH = new byte[0];
        public ModBusDev(uint inputSize, uint holdSize)
        {
            holdIN = new byte[holdSize];
            holdOUT = new byte[holdSize];
            holdCASH= new byte[holdSize];
            inputIN = new byte[inputSize];
            inputCASH = new byte[inputSize];

            for(int i = 0 ; i < holdSize; i++)
            {
                holdIN[i] = 0xFF;
                holdOUT[i] = 0xFF;
                holdCASH[i] = 0xFF;
            }
            for(int i = 0 ; i < inputSize; i++)
            {
                inputIN[i] = 0xFF;
                inputCASH[i] = 0xFF;
            }
        }
        public byte NetAddress { get; set; }
        public uint WaitingTime { get; set; }
        public IProvider Provider {get; set; }
        public uint LengthInputs => (uint)inputIN.Length;
        public uint LengthHolds => (uint)holdIN.Length;
        public bool AnyDataForWriting => !Enumerable.SequenceEqual(holdIN, holdOUT);
        public void UndoChanges()
        {
            holdIN.CopyTo(holdOUT, 0);
        }
        public int AddHoldRegsInfo(ModBusRegInfo[] modBusRegInfos)
        {
            int Result = 0;
            foreach( ModBusRegInfo item in modBusRegInfos)
            {
                try
                {
                    holdsInfo.Add(item.Name, item); 
                    Result++;
                }
                catch
                {

                }
            }
            return Result;
        }
        public int AddInputRegsInfo(ModBusRegInfo[] modBusRegInfos)
        {
            int Result = 0;
            foreach( ModBusRegInfo item in modBusRegInfos)
            {
                try
                {
                    inputsInfo.Add(item.Name, item); 
                    Result++;
                }
                catch
                {

                }
            }
            return Result;
        }
        public Report ReadInputRegs(ushort startReg, ushort countRegs)
        {
            int startbyte = startReg * 2;

            int countbyte = countRegs * 2;

            if (countRegs == 0 || countRegs > 127 || startbyte + countbyte > LengthInputs) return new Report { Result = ResultRequest.WrongRequest };

            Report report = Modbus.ReadRegs(Provider, NetAddress, (byte)ModBusFunc.ReadInput, startReg, countRegs, (int)WaitingTime);

            if (report.Result == ResultRequest.OK)
            {
                ToInputRegs(report.Response.Skip(3).Take(countbyte).ToArray(), startbyte, countbyte);
            }
            return report;
        }
        public Report ReadHoldRegs(ushort startReg, ushort countRegs)
        {
            int startbyte = startReg * 2;

            int countbyte = countRegs * 2;

            if (countRegs == 0 || countRegs > 127 || startbyte + countbyte > LengthInputs) return new Report { Result = ResultRequest.WrongRequest };

            Report report = Modbus.ReadRegs(Provider, NetAddress, (byte)ModBusFunc.ReadHold, startReg, countRegs, (int)WaitingTime);

            if (report.Result == ResultRequest.OK)
            {
                ToHoldRegs(report.Response.Skip(3).Take(countbyte).ToArray(), startbyte, countbyte);
            }
            return report;
        }
        private void ToInputRegs(byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length == 0 || offset < 0 || count < 1) throw new ArgumentException();
            Array.Copy(inputIN, offset, inputCASH, offset, count);
            Array.Copy(buffer, 0, inputIN, offset, count);
            List<uint> bytesList = GetChangingInputBytes(offset, count);
            if(bytesList.Count > 0)
            {
                ModBusRegInfo[] modBusRegs = GetInputsFromByteList(bytesList);
                if(modBusRegs.Length > 0)
                    InputValueChanged?.Invoke(this, modBusRegs);
            }
        }
        private void ToHoldRegs(byte[] buffer, int offset, int count)
        {
            if (buffer == null || buffer.Length == 0 || offset < 0 || count < 1) throw new ArgumentException();
            Array.Copy(holdIN, offset, holdCASH, offset, count);
            Array.Copy(buffer, 0, holdIN, offset, count);
            List<uint> bytesList = GetChangingHoldBytes(offset, count);
            if(bytesList.Count > 0)
            {
                ModBusRegInfo[] modBusRegs = GetHoldsFromByteList(bytesList);
                if(modBusRegs.Length > 0)
                    HoldValueChanged?.Invoke(this, modBusRegs);
            }
        }
        private ModBusRegInfo[] GetInputsFromByteList(List<uint> bytesList)
        {
            List<ModBusRegInfo> Result = new List<ModBusRegInfo>();
            foreach(uint byteNum in bytesList)
            {
               foreach (var item in inputsInfo.Values)
                {
                    if (item.Address <= byteNum && byteNum < item.Address + item.Length)
                        Result.Add(item);
                }
            }
            return Result.Distinct().ToArray();
        }
        private ModBusRegInfo[] GetHoldsFromByteList(List<uint> bytesList)
        {
            List<ModBusRegInfo> Result = new List<ModBusRegInfo>();
            foreach(uint byteNum in bytesList)
            {
               foreach (var item in holdsInfo.Values)
                {
                    if (item.Address <= byteNum && byteNum < item.Address + item.Length)
                        Result.Add(item);
                }
            }
            return Result.Distinct().ToArray();
        }
        private List<uint> GetChangingInputBytes(int count = 0, int offset = 0)
        {
            if(count == 0) count = (int)LengthInputs;
            List<uint> result = new List<uint>();
            for (int i = offset; i < offset + count; i++)
                    if (inputIN[i] != inputCASH[i]) result.Add((ushort)i);
            return result;
        }
        private List<uint> GetChangingHoldBytes(int count = 0, int offset = 0)
        {
            if(count == 0) count = (int)LengthInputs;
            List<uint> result = new List<uint>();
            for (int i = offset; i < offset + count; i++)
                    if (holdIN[i] != holdCASH[i]) result.Add((ushort)i);
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
            if(inputsInfo.TryGetValue(regInfoName, out ModBusRegInfo regInfo))
            {
                return new ModBusRegValue(  inputIN.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                        inputCASH.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                        Array.Empty<byte>()
                                    );
            }
            return new ModBusRegValue(new byte[0], new byte[0], new byte[0]);
        }
        public ModBusRegValue GetHoldValue(string regInfoName)
        {
            if(holdsInfo.TryGetValue(regInfoName, out ModBusRegInfo regInfo))
            {
                return new ModBusRegValue(  
                                            holdIN.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                            holdCASH.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray(), 
                                            holdOUT.Skip((int)regInfo.Address).Take(regInfo.Length).ToArray()
                                         );
            }
            return new ModBusRegValue(new byte[0], new byte[0], new byte[0]);
        }
        #region Events
        public event EventHandler<ModBusRegInfo[]> InputValueChanged = delegate { };
        public event EventHandler<ModBusRegInfo[]> HoldValueChanged = delegate { };
        #endregion
    }
}