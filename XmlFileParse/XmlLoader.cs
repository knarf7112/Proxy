using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Xml;
using System.Xml.Linq;

namespace XmlFileParse
{
    public class XmlLoader
    {
        //private XDocument doc;

        public XmlObject Parse(string url)
        {
            XDocument xmlRoot = XDocument.Load(url);
            XElement node = xmlRoot.Root;
            XmlObject result = new XmlObject();
            //call recusive to parse Xml file
            Parse(result, node);
            return result;
        }

        private void Parse(XmlObject outputObject,XElement inputXmlNode)
        {
            //若node內有子項目
            if (inputXmlNode.HasElements)
            {
                outputObject.ChildNode = new Dictionary<string, IList<XmlObject>>();
                
                foreach (XElement childNode in inputXmlNode.Elements())
                {
                    IList<XmlObject> list = new List<XmlObject>();
                    

                    XmlObject childObj = new XmlObject()
                    {
                        NodeName = childNode.Name.LocalName,
                        NodeAttribute = ParseAttribute(childNode),
                    };
                    list.Add(childObj);
                    outputObject.ChildNode.Add(childNode.Name.LocalName, list);
                    Parse(childObj, childNode);
                }
                //TODO.............................................


                //抓取node內Tag的名稱超過一個的(即兩個以上同樣的node)
                //if (inputXmlNode.Elements(inputXmlNode.Elements().First().Name.LocalName).Count() > 1)
                //{

                //}
                //else
                //{
                //    //
                //    foreach (XElement childNode in inputXmlNode.Elements())
                //    {

                //    }
                //}
            }
            else
            {
                //沒有子節點的Node
                outputObject.NodeValue = inputXmlNode.Value;//設定node名稱
                outputObject.NodeName = inputXmlNode.Name.LocalName;//設定node值

                if (inputXmlNode.HasAttributes)
                {
                    //設定node的屬性
                    foreach (XAttribute nodeAttribute in inputXmlNode.Attributes())
                    {
                        outputObject.NodeAttribute.Add(nodeAttribute.Name.LocalName, nodeAttribute.Value);
                    }
                }
                
            }
        }

        private IDictionary<string, string> ParseAttribute(XElement childNode)
        {
            if (childNode.HasAttributes)
            {
                IDictionary<string, string> attributes = new Dictionary<string, string>();
                foreach (XAttribute attribute in childNode.Attributes())
                {
                    attributes.Add(attribute.Name.LocalName, attribute.Value);
                }
                return attributes;
            }
            else
            {
                return null;
            }
        }
    }
}
