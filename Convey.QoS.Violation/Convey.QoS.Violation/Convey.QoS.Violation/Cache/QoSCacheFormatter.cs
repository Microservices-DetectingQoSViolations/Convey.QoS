using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Convey.QoS.Violation.Cache
{
    public class QoSCacheFormatter : IQoSCacheFormatter
    {
        private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        public byte[] SerializeNumber(long number)
        {
            using var mStream = new MemoryStream();
            BinaryFormatter.Serialize(mStream, number);

            return mStream.ToArray();
        }

        public long DeserializeNumber(byte[] byteArray)
        {
            using var mStream = new MemoryStream();
            mStream.Write(byteArray, 0, byteArray.Length);
            mStream.Position = 0;

            return (long)BinaryFormatter.Deserialize(mStream);
        }
    }
}
