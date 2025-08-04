using HRPortal.DBEntities.DDMC_PFA;
using System;

namespace HRPortal.Services.DDMC_PFA
{
    public class SystemSettingService : BaseCrudService<PfaSystemSetting>
    {
        public SystemSettingService(HRPortal_Services services)
            : base(services)
        {
        }

        public bool GetPfaProgresEmployesQueryLock()
        {
            PfaSystemSetting ss = FirstOrDefault(x => x.SettingKey == "PfaProgresEmployesQueryLock");

            if (ss == null || ss.SettingValue == null)
            {
                return false;
            }
            else
            {
                return ss.SettingValue == "1";
            }
        }

        public string GetSettingValue(string key)
        {
            PfaSystemSetting ss = FirstOrDefault(x => x.SettingKey == key);
            return (ss == null) ? null : ss.SettingValue;
        }

        public int UpdateSetting(string key, string value)
        {
            PfaSystemSetting _setting = FirstOrDefault(x => x.SettingKey == key);
            if (_setting != null && _setting.SettingValue != value)
            {
                _setting.SettingValue = value;
                return Update(_setting);
            }
            return 0;
        }
    }
}
