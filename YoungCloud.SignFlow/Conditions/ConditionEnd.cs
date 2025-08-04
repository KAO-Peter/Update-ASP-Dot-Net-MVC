using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Model;

namespace YoungCloud.SignFlow.Conditions
{
    /// <summary>
    /// Desc: 此類別為簽核條件的終點
    /// </summary>
    public class ConditionEnd : ConditionHandler
    {
        public override IList<SignFlowDesignModel> CheckConditionItem(IList<SignFlowDesignModel> SignList, int CheckRowIndex, IFormData FormData)
        {
            return this.RunNoMatchAction(SignList, CheckRowIndex, FormData);
        }
    }
}
