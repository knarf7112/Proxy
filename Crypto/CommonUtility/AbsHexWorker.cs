
namespace Crypto.CommonUtility
{
    public abstract class AbsHexWorker : IHexWorker
    {
        /// <summary>
        /// 2 hex can make a byte
        /// </summary>
        public static readonly int HexPerByte = 2;

        public abstract string Byte2Hex(byte b);

        public abstract byte Hex2Byte(string hexStr);
    }
}
