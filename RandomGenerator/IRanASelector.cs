
namespace RandomGenerator
{
    /// <summary>
    /// 產生指定的RanDom A 值
    /// </summary>
    public interface IRanASelector
    {
        /// <summary>
        /// get Random A from start index
        /// </summary>
        /// <param name="startIndex">start index</param>
        /// <returns>16 Byte Array</returns>
        byte[] GetRanA(int startIndex);

        /// <summary>
        /// get Random A(hex String) from start index
        /// </summary>
        /// <param name="startIndex">start index</param>
        /// <returns>16 Byte Array</returns>
        string GetRandAByHex(int startIndex);

        /// <summary>
        /// get Random A from start index and length
        /// </summary>
        /// <param name="startIndex">start index</param>
        /// <param name="length">specified length</param>
        /// <returns>specified count Byte Array</returns>
        byte[] GetRanA(int startIndex, int length);

        /// <summary>
        /// get Random A total length
        /// </summary>
        /// <returns>length</returns>
        int GetTotalLength();
    }
}
