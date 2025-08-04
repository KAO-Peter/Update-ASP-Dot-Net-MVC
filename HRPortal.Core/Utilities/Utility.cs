using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace HRPortal.Core.Utilities
{
    public class Utility
    {
        public static Guid GenerateGuid()
        {
            byte[] guidArray = Guid.NewGuid().ToByteArray();

            var baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            TimeSpan msecs = now.TimeOfDay;

            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }

        public static dynamic ConvertToDynamic(object obj)
        {
            IDictionary<string, object> result = new ExpandoObject();

            foreach (PropertyDescriptor pro in TypeDescriptor.GetProperties(obj.GetType()))
            {
                result.Add(pro.Name, pro.GetValue(obj));
            }

            return result as ExpandoObject;
        }

        public static dynamic ConvertToDynamic(object obj1, object obj2)
        {
            IDictionary<string, object> result = ConvertToDynamic(obj1);
            foreach (PropertyDescriptor pro in TypeDescriptor.GetProperties(obj2.GetType()))
            {
                if (result.ContainsKey(pro.Name))
                    result[pro.Name] = pro.GetValue(obj2);
                else
                    result.Add(pro.Name, pro.GetValue(obj2));
            }
            return result as ExpandoObject;
        }

        public static bool IsValidEmail(string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
    }
}
