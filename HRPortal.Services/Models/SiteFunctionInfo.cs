using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.Models
{
    //20170328增加 by Daniel，切版判斷由Session物件改為使用Global變數
    public static class SiteFunctionInfo
    {
        private static string siteType = "";
        private static string siteFunctionRange = "";
        private static List<string> allowedList;
        private static List<string> allowedMenuID;
        private static string mobileSitePath = "";
        private static string baseURL = "";

        public static string SiteType
        {
            get
            {
                return siteType;
            }
            set
            {
                siteType = value;
            }
        }

        public static string SiteFunctionRange
        {
            get
            {
                return siteFunctionRange;
            }
            set
            {
                siteFunctionRange = value;
            }
        }

        public static List<string> AllowedList
        {
            get
            {
                return allowedList;
            }
            set
            {
                allowedList = value;
            }
        }

        public static List<string> AllowedMenuID
        {
            get
            {
                return allowedMenuID;
            }
            set
            {
                allowedMenuID = value;
            }
        }

        public static string MobileSitePath
        {
            get
            {
                return mobileSitePath;
            }
            set
            {
                mobileSitePath = value;
            }
        }

        //20181123 Daniel 增加Portal網址為Static資訊
        public static string BaseURL 
        {
            get 
            {
                return baseURL;
            }
            set 
            {
                baseURL = value;
            }
        }
    }
}
