using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.CommonUtility
{
    public interface IRandWorker
    {
        /// <summary>
        /// Get random bytes
        /// </summary>
        /// <param name="size">size of bytes</param>
        /// <returns>Random bytes</returns>
        byte[] GetBytes(int size);

        /// <summary>
        /// Get Random bytes
        /// </summary>
        /// <param name="bytes">out byte array</param>
        void GetBytes(byte[] bytes);
    }
}
