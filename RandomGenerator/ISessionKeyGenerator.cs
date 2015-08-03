
namespace RandomGenerator
{
    /// <summary>
    /// 產生指定的RanDom A 值
    /// </summary>
    public interface ISessionKeyGenerator
    {
        /// <summary>
        /// get Random A from start index
        /// </summary>
        /// <param name="startIndex">start index</param>
        /// <returns>16 Byte Array</returns>
        byte[] GetRanA(int startIndex);

        /// <summary>
        /// get session key(16 bytes)
        /// SesKey = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
        /// </summary>
        /// <param name="ranAStartIndex">Random A start index</param>
        /// <param name="ranB">Random B</param>
        /// <returns>Session Key(16 bytes)</returns>
        byte[] GetSessionKey(int ranAStartIndex, byte[] ranB);

        /// <summary>
        /// get Random A total length
        /// </summary>
        /// <returns>length</returns>
        int GetTotalLength();
    }
}
