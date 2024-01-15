using System.Collections;
namespace ModBusLibrary;
public class ModBusRegInfo : IEqualityComparer
{
    private uint number = 0;
    public string Name { get; private set; } = string.Empty;
    public uint Address => number * 2;
    public uint Number 
    { 
        get => number; 
        private set
        {
            if(value != number)
            {
                number = value;
            }
        }
    }
    public ModBusRegInfo(string name, uint regNumber, ModBusType regType, bool littleEndian = false)
    {
        Name = name;
        Number = regNumber;
        Type = regType;
        LittleEndian = littleEndian;
    }
    public ModBusType Type { get; private set; } = ModBusType.Byte;
    public bool LittleEndian {get; private set; } = false;
    public bool NeedFlip {get; set;} = false;
    public int Length => Type switch
    {
        ModBusType.Double or ModBusType.UInt64 or ModBusType.Int64 => 8,
        ModBusType.Float or ModBusType.UInt32 or ModBusType.Int32 => 4,
        ModBusType.UInt16 or ModBusType.Int16 => 2,
        ModBusType.Byte or ModBusType.Char or ModBusType.Bool => 1,
        ModBusType.DateTime => 6,
        ModBusType.String => 16,
        _ => 0,
    };
    public new bool Equals(object? x, object? y)
    {
        if(x == null || y == null) return false;
        return ((ModBusRegInfo)x).Name.Equals(((ModBusRegInfo)y).Name);
    }
    public int GetHashCode(object obj)
    {
        throw new NotImplementedException();
    }
}
public enum ModBusType
{
    Bool        = 0,
    Char        = 1,
    Byte        = 2,
    Int16       = 3,
    UInt16      = 4,
    Int32       = 5,
    UInt32      = 6,
    Int64       = 7,
    UInt64      = 8,
    Float       = 9,
    Double      = 10,
    TimeStamp   = 11,
    DateTime    = 12,
    String      = 13,
}