using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRPortal.Mvc.Models
{
    public class LogMsg : IComparable, IEquatable<LogMsg>
    {
        public string timeStamp;
        public string errorMessage;

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals(LogMsg other)
        {
            throw new NotImplementedException();
        }
    }
}