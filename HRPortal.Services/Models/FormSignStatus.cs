//using HRPortal.Models.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.Models
{
    //避免交互參照，將HRPortal.Mvc.Models內的FormStatus複製到此處
    public enum FormSignStatus
    {
        Draft,
        Signing,
        Approved,
        Send,
    }
}
