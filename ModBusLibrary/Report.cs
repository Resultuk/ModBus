namespace ModBusLibrary;
public class Report
{
    public DateTime RequestTime { get; set; }
    public DateTime ResponseTime { get; set; }
    public byte[] Request { get; set; } = Array.Empty<byte>();
    public byte[] Response { get; set; } = Array.Empty<byte>();
    public ResultRequest Result { get; set; }
    public int NumberOfRequery { get; set; }
    public string ErrorMassage { get; set; } = String.Empty;
    public override string ToString()
    {
        return $"{Result} : Отправлено {Request?.Length} байт в {RequestTime:mm:ss.ffff} \n Принято {Response?.Length} байт в {ResponseTime:mm:ss.fff}";
    }
}
public enum ResultRequest
{
    OK,
    NoAnswer,
    WrongResponce,
    WrongRequest,
    Error,
    CRCError,
    TransportError,
    NoChanges,
    BroadCast,
    WrongData
}