using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlFileParse
{
    /// <summary>
    /// XML物件
    /// </summary>
    public class XmlObject
    {
        /// <summary>
        /// XML節點名稱
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// XML節點內容
        /// </summary>
        public string NodeValue { get; set; }

        /// <summary>
        /// XML的子節點
        /// </summary>
        public IDictionary<string, IList<XmlObject>> ChildNode { get; set; }

        /// <summary>
        /// XML節點的屬性
        /// </summary>
        public IDictionary<string, string> NodeAttribute { get; set; }
    }
}
