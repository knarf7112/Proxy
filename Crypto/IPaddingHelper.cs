//
using System.IO;

namespace Crypto
{
    /// <summary>
    /// Padding
    /// </summary>
    public interface IPaddingHelper
    {
        /// <summary>
        /// GET/SET Block size, default 16
        /// </summary>
        int BlockSize { get; set; }

        /// <summary>
        ///  Add padding to source
        /// </summary>
        /// <param name="src">source content</param>
        /// <returns>content with padding</returns>
        byte[] AddPadding(byte[] src);

        void AddPadding(Stream src, Stream dest);

        /// <summary>
        /// Remove padding from content
        /// </summary>
        /// <param name="src">content with padding</param>
        /// <returns>original source</returns>
        byte[] RemovePadding(byte[] src);

        void RemovePadding(Stream src, Stream dest);
    }
}
