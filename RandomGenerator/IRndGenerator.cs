
namespace RandomGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRndGenerator
    {
        /// <summary>
        /// input index then find a random array(16 bytes)
        /// max index call GetMaxIndex method
        /// </summary>
        /// <param name="index">start index</param>
        /// <returns>16 bytes</returns>
        byte[] Get_RandomFromIndex(int index);

        /// <summary>
        /// get a random array(16 bytes) and output start index
        /// </summary>
        /// <param name="index">output start index</param>
        /// <returns>a random array(16 bytes)</returns>
        byte[] Get_Random(out int index);

        /// <summary>
        /// get Max Index
        /// </summary>
        /// <returns></returns>
        int GetMaxIndex();
    }
}
