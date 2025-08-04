using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HRPortal.Core.Utilities;

/// <summary>
/// JSON 序列化解析器，用於處理所有屬性的序列化，包括被標記為忽略的屬性
/// </summary>
public class AllPropsContractResolver : DefaultContractResolver
{
    public AllPropsContractResolver()
    {
        // 在 .NET 8 中，我們可以使用 init-only 屬性和其他新特性
        NamingStrategy = new CamelCaseNamingStrategy
        {
            ProcessDictionaryKeys = true,
            OverrideSpecifiedNames = true
        };
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        // 使用更現代的 LINQ 語法和 pattern matching
        if (member.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
        {
            property.Ignored = false;
        }

        return property;
    }
}
