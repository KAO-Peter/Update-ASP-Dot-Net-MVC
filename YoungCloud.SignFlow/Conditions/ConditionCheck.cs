using System;
using System.Linq;
using System.Collections.Generic;
using YoungCloud.Configurations;
using YoungCloud.SignFlow.Model;

namespace YoungCloud.SignFlow.Conditions
{
    public abstract class ConditionCheck : ClassBase
    {

        private string _DesignID = string.Empty;
        public string DesignID
        {
            get { return _DesignID; }
            set { _DesignID = value; }
        }
        
        private string _CheckLevel = string.Empty;
		public string CheckLevel {
			get { return _CheckLevel; }
			set { _CheckLevel = value; }
		}

        private string _AbsentCode = string.Empty;
        public string AbsentCode
        {
            get { return _AbsentCode; }
            set { _AbsentCode = value; }
        }

		private bool _MatchCondition = false;
		public bool MatchCondition {
			get { return _MatchCondition; }
			set { _MatchCondition = value; }
		}

        protected abstract List<ConditionHandler> GetConditions();

        public IList<SignFlowDesignModel> CheckCondition(IList<SignFlowDesignModel> signList, int checkRowIndex, IFormData formData)
        {
            ConditionStart _cdStart = new ConditionStart();
            ConditionEnd _cdEnd = new ConditionEnd();

            List<ConditionHandler> _handlers = GetConditions();

            if (_handlers.Count == 0)
            {
                return signList;
            }

            ConditionHandler _currentHandler = _cdStart;

            foreach (ConditionHandler _handler in _handlers)
            {
                _currentHandler.SetNextConditionHandler(_handler);
                _currentHandler = _handler;
            }

            _currentHandler.SetNextConditionHandler(_cdEnd);

            signList = _cdStart.CheckConditionItem(signList, checkRowIndex, formData);
            this.MatchCondition = _cdEnd.MatchCondition;

            if (signList == null || signList.Count == 0) throw new Exception("判斷" + CheckLevel + "是否須簽核時，發生錯誤！");
            return signList;
        }

        public IList<SignFlowDesignModel> CheckCondition(IList<SignFlowDesignModel> signList, IList<SignFlowFormLevelModel> listFormLevel, IFormData formData)
        {
            string _formLevelID = null;
            //一個LevelId可能會有多個FormLevelId
            var _formLevelIds = (from y in listFormLevel where y.LevelID == CheckLevel select y.FormLevelID).ToList();

            IList<SignFlowDesignModel> result = signList;
            int checkRowIndex = 0;
            //DataRow[] dr = null;

            for (int i = 0; i <= _formLevelIds.Count() - 1; i++)
            {
                _formLevelID = _formLevelIds[i].ToString();
                var _temp = signList.Where(x => x.FormLevelID == _formLevelID);
                if (_temp.Count() == 0)
                {
                    continue;
                }

                checkRowIndex = signList.IndexOf(_temp.ToList()[0]);
                signList = this.CheckCondition(signList, checkRowIndex, formData);
                _temp = signList.Where(x => x.FormLevelID == _formLevelID);
                if (_temp.ToList().Count() == 0 || checkRowIndex != signList.IndexOf(_temp.ToList()[0]))
                {
                    result = (from x in result where !x.FormLevelID.Equals(_formLevelID) select x).ToList();
                }
                break;
            }
            return result;
        }
    }
}
