namespace Convey.QoS.Violation.Cache
{
    public interface IQoSCacheFormatter
    {
        byte[] SerializeNumber(long number);
        long DeserializeNumber(byte[] byteArray);
    }
}