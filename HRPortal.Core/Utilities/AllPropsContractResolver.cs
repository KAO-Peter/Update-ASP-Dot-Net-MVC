using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Core.Utilities
{
    public class AllPropsContractResolver : DefaultContractResolver
    {
        public AllPropsContractResolver()
        {
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if(member.CustomAttributes.Where(attr=> attr.AttributeType == typeof(JsonIgnoreAttribute)).Count() > 0)
                property.Ignored = false;

            return property;
        }
    }
}
