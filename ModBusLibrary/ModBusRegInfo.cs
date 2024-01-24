using System.Collections;
namespace ModBusLibrary
{
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
        public int Length
        {
            get
            {
                switch(Type)
                {
                    case ModBusType.Double  : return 8;
                    case ModBusType.UInt64  : return 8;
                    case ModBusType.Int64   : return 8;
                    case ModBusType.Float   : return 4;
                    case ModBusType.UInt32  : return 4;
                    case ModBusType.Int32   : return 4;
                    case ModBusType.UInt16  : return 2;
                    case ModBusType.Int16   : return 2;
                    case ModBusType.Byte    : return 1;
                    case ModBusType.Char    : return 1;
                    case ModBusType.Bool    : return 1;
                    case ModBusType.DateTime: return 6;
                    case ModBusType.String  : return 16;
                    default : return 0;
                }
            }
        }
        public new bool Equals(object x, object y)
        {
            if(x == null || y == null) return false;
            return ((ModBusRegInfo)x).Name.Equals(((ModBusRegInfo)y).Name);
        }
        public int GetHashCode(object obj)
        {
            throw new System.NotImplementedException();
        }
        public override string ToString()
        {
            return $"{Name}-{System.Enum.GetName(typeof(ModBusType), Type)}[{Address},{Length}]";
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
}