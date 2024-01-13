namespace Protocols;
public class ModBusRegInfo(string name, uint address, ModBusType type, bool littleEndian = false)
{
    public string Name { get; private set; } = name;
    public uint Address { get; private set; } = address;
    public ModBusType Type { get; private set; } = type;
    public bool LittleEndian {get; private set; } = littleEndian;
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