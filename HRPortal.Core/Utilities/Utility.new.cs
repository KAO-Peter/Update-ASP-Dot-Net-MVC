using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace HRPortal.Core.Utilities;

/// <summary>
/// 提供各種通用工具方法的靜態類別
/// </summary>
public static class Utility
{
    private static readonly DateTime BaseDate = new(1900, 1, 1);
    
    // 編譯的正則表達式以提高性能
    private static readonly Regex EmailRegex = new(
        @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    /// <summary>
    /// 生成基於時間的GUID
    /// </summary>
    /// <returns>包含時間戳的GUID</returns>
    public static Guid GenerateGuid()
    {
        byte[] guidArray = Guid.NewGuid().ToByteArray();
        DateTime now = DateTime.Now;
        TimeSpan days = now - BaseDate;
        TimeSpan msecs = now.TimeOfDay;

        byte[] daysArray = BitConverter.GetBytes(days.Days);
        byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);
        }

        Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

        return new Guid(guidArray);
    }

    /// <summary>
    /// 將對象轉換為動態對象
    /// </summary>
    /// <param name="obj">要轉換的對象</param>
    /// <returns>動態對象</returns>
    public static dynamic ConvertToDynamic(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var result = new ExpandoObject() as IDictionary<string, object>;
        var properties = TypeDescriptor.GetProperties(obj.GetType());
        
        foreach (PropertyDescriptor property in properties)
        {
            var value = property.GetValue(obj);
            result.Add(property.Name, value);
        }

        return result as ExpandoObject ?? new ExpandoObject();
    }

    /// <summary>
    /// 將兩個對象的屬性合併為一個動態對象
    /// </summary>
    /// <param name="obj1">第一個對象</param>
    /// <param name="obj2">第二個對象</param>
    /// <returns>合併後的動態對象</returns>
    public static dynamic ConvertToDynamic(object obj1, object obj2)
    {
        ArgumentNullException.ThrowIfNull(obj1);
        ArgumentNullException.ThrowIfNull(obj2);

        var result = (ConvertToDynamic(obj1) as IDictionary<string, object>)!;
        var properties = TypeDescriptor.GetProperties(obj2.GetType());

        foreach (PropertyDescriptor property in properties)
        {
            var value = property.GetValue(obj2);
            if (result.ContainsKey(property.Name))
            {
                result[property.Name] = value;
            }
            else
            {
                result.Add(property.Name, value);
            }
        }

        return result as ExpandoObject ?? new ExpandoObject();
    }

    /// <summary>
    /// 驗證電子郵件地址格式
    /// </summary>
    /// <param name="email">要驗證的電子郵件地址</param>
    /// <returns>如果格式有效則返回true</returns>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            return EmailRegex.IsMatch(email);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
