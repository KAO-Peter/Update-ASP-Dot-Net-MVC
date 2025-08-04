using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Collections.Specialized;
using YoungCloud.Configurations;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.Conditions;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace HRPortal.SignFlow.Conditions
{

    public class LeaveCdCheck : ConditionCheck
	{
		public LeaveCdCheck()
		{
		}

        public LeaveCdCheck(string DesignID)
        {
            this.DesignID = DesignID;
        }
        
        public LeaveCdCheck(string DesignID, string ChkLevel)
		{
            this.DesignID = DesignID;
			this.CheckLevel = ChkLevel;
		}

        public LeaveCdCheck(string DesignID, string ChkLevel, string AbsentCode)
        {
            this.DesignID = DesignID;
            this.CheckLevel = ChkLevel;
            this.AbsentCode = AbsentCode;
        }

        protected override List<ConditionHandler> GetConditions()
        {
            List<ConditionHandler> _result = new List<ConditionHandler>();

            SignFlowConditionsRepository _repository = new SignFlowConditionsRepository();
            List<SignFlowConditions> _conditions = _repository.GetConditions(DesignID, CheckLevel, AbsentCode).ToList();

            foreach(SignFlowConditions _condition in _conditions)
            {
                switch (_condition.ConditionType)
                {
                    case "NDays":
                        _result.Add(new OverNDays(_condition.Parameters));
                        break;
                    case "Abroad":
                        _result.Add(new IsAbored());
                        break;
                    case "OutOfAbsent":
                        _result.Add(new NotEnoughAbsentAmount());
                        break;
                    default:
                        break;
                }
            }

            return _result;
        }
	}
}
