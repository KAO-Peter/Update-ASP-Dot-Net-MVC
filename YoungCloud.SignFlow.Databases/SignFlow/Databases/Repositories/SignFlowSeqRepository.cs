using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class SignFlowSeqRepository:SignFlowRepositoryBase<SystemSetting>
    {
        static private object _signFlowSeqLock = new object();
        static private object _signFromLevelSeqLock = new object();
        public SignFlowSeqRepository()
            : base()
        {
        }

        public string GetSignFlowSeq(){
            lock (_signFlowSeqLock)
            {
                string _result = "1";
                SystemSetting _sys = base.GetAll().Where(x => x.SettingKey == "SignFlowSeq").First();
                _result = _sys.SettingValue;
                _sys.SettingValue = (int.Parse(_sys.SettingValue) + 1).ToString();
                base.Update(_sys);
                base.SaveChanges();
                return _result;
            }
        }
        public string GetSignFromLevelSeq()
        {
            lock (_signFromLevelSeqLock)
            {
                string _result = "1";
                SystemSetting _sys = base.GetAll().Where(x => x.SettingKey == "SingFromLevelSeq").First();
                _result = _sys.SettingValue;
                _sys.SettingValue = (int.Parse(_sys.SettingValue) + 1).ToString();
                base.Update(_sys);
                base.SaveChanges();
                return _result;
            }
        }
        public string GetSignFlowDesingSeq()
        {
            lock (_signFromLevelSeqLock)
            {
                string _result = "1";
                SystemSetting _sys = base.GetAll().Where(x => x.SettingKey == "SingFromDesignSeq").First();
                _result = _sys.SettingValue;
                _sys.SettingValue = (int.Parse(_sys.SettingValue) + 1).ToString();
                base.Update(_sys);
                base.SaveChanges();
                return _result;
            }
        }
    }
}
