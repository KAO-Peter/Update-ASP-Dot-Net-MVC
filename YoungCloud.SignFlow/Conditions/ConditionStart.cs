using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Model;

namespace YoungCloud.SignFlow.Conditions
{
    public class ConditionStart : ConditionHandler
    {
        protected void VerifyNothing(IList<SignFlowDesignModel> signList, IFormData formData)
        {
            base.VerifyNothing(signList, formData);
        }

        public override IList<SignFlowDesignModel> CheckConditionItem(IList<SignFlowDesignModel> signList, int checkRowIndex, IFormData formData)
        {
            this.VerifyNothing(signList, formData);
            return this.RunNoMatchAction(signList, checkRowIndex, formData);
        }
    }
}
