//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class SystemSettingService : BaseCrudService<SystemSetting>
    {
        public SystemSettingService(HRPortal_Services services)
            : base(services)
        {
        }

        public string GetSettingValue(string key)
        {
            //20170512 Daniel，增加沒設定參數需回傳null的邏輯
            SystemSetting ss = FirstOrDefault(x => x.SettingKey == key);
            //return FirstOrDefault(x=>x.SettingKey == key).SettingValue;
            return (ss == null) ? null : ss.SettingValue;
        }

        public int UpdateSetting(string key, string value)
        {
            SystemSetting _setting = FirstOrDefault(x => x.SettingKey == key);
            if (_setting != null && _setting.SettingValue != value)
            {
                _setting.SettingValue = value;
                return Update(_setting);
            }
            return 0;
        }
    }
}
