using System;
using System.Linq;
using System.Reflection;

namespace Crypto.POCO
{
    public static class Extentions
    {
        /// <summary>
        /// lock用的物件
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// (擴充方法)用來檢查POCO物件內字串屬性的資料長度是否符合Attribute設定的長度
        /// ex: obj.CHeckLength();
        /// 長度不符會拋出ArgumentOutOfRangeException
        /// </summary>
        /// <typeparam name="T">要檢查的物件型別type</typeparam>
        /// <param name="obj"></param>
        /// <param name="throwExcption">是:拋出錯誤, 否:異常從errMsg噴出</param>
        /// <param name="errMsg">存放錯誤字串(無錯誤或throwExcption為false:null)</param>
        public static void CheckLength<T>(this T obj,bool throwExcption,out string errMsg)
        {
            errMsg = null;
            string err = null;
            lock (lockObj)
            {
                PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo property in properties.AsEnumerable())
                {
                    LengthCkeckAttribute attr = Attribute.GetCustomAttribute(property, typeof(LengthCkeckAttribute)) as LengthCkeckAttribute;
                    if (attr != null)
                    {
                        //reflect property value(need cast)
                        object propertyValue = property.GetValue(obj, null);
                        //check property value's length
                        if (propertyValue is string && !string.IsNullOrEmpty((string)propertyValue))
                        {
                            int valueLength = ((string)propertyValue).Length;
                            if (attr.FixLength != valueLength)
                            {
                                err = String.Format("{0}屬性與設定資料長度不符 => 輸入(length:{1})不等於屬性的設定(length:{2})", property.Name, valueLength, attr.FixLength);
                                if (throwExcption)
                                {
                                    throw new ArgumentOutOfRangeException("[CheckLength] Error", err);
                                }
                                else
                                {
                                    errMsg = err;
                                }
                                
                            }

                        }
                        else if(propertyValue is byte[] && propertyValue != null)
                        {
                            int valueLength = ((byte[])propertyValue).Length;
                            if (attr.FixLength != valueLength)
                            {
                                err = String.Format("{0}屬性與設定資料長度不符 => 輸入(length:{1})不等於屬性的設定(length:{2})", property.Name, valueLength, attr.FixLength);
                                if (throwExcption)
                                {
                                    throw new ArgumentOutOfRangeException("[CheckLength] Error", err);
                                }
                                else
                                {
                                    errMsg = err;
                                }
                            }
                        }
                        else
                        {
                            err = String.Format("{0}屬性:資料型態不為自訂的檢核型別或資料為空!!!", property.Name);
                            if (throwExcption)
                            {
                                throw new ArgumentOutOfRangeException("[CheckLength] Error", err);
                            }
                            else
                            {
                                errMsg = err;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 用來設定POCO的字串長度
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class LengthCkeckAttribute : Attribute
    {
        /// <summary>
        /// 自訂的固定長度
        /// </summary>
        public int FixLength { get; set; }
    }
}
