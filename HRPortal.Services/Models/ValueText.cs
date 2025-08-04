//using HRPortal.Models.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.Models
{
    public class ValueText
    {
        public Guid id { get; set; }
        public string v { get; set; }
        public string t { get; set; }
        public string c { get; set; }
        public string originalCompanyName { get; set; }
    }
}
