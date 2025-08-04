using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRPortal.Helper
{
    public class FormNoGenerator : IFormNoGenerator
    {
        private HRPortal_Services _services;

        public FormNoGenerator()
        {
            _services = new HRPortal_Services();
        }

        public string GetLeaveFormNo()
        {
            return GetFormNo();
        }

        public string GetOverTimeFormNo()
        {
            return GetFormNo();
        }

        public string GetPatchCardFormNo()
        {
            return GetFormNo();
        }

        public string GetLeaveCancelFormNo()
        {
            return GetFormNo();
        }

        public string GetOverTimeCancelFormNo()
        {
            return GetFormNo();
        }


        private string GetFormNo()
        {
            return "P" + DateTime.Now.ToString("yyyyMMdd") + _services.GetService<SerialControlService>().GetSerialNumber("FormNo").ToString(new string('0', 6));
        }
    }
}