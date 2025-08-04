using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Results
{
    public enum LoginStatus
    {
        Success,
        Failed,
        Expired
    }

    public class LoginResult
    {
        public LoginStatus Status { get; set; }
        public string Message { get; set; }
    }
}
