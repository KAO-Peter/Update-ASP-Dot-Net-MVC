using HRPortal.DBEntities;
using HRPortal.SignFlow.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using YoungCloud.SignFlow.Conditions;
using YoungCloud.SignFlow.Model;

namespace HRPortal.SignFlow.Conditions
{
    /// <summary>
    /// 請假超過N天需要上層主管簽核。
    /// Author: JerryLin
    /// Date: 2015/09/12
    /// </summary>
    public class OverNDays : ConditionHandler
    {
        private int _days;

        public OverNDays(string days)
        {
            _days = int.Parse(days);
        }

        public override IList<SignFlowDesignModel> CheckConditionItem(IList<SignFlowDesignModel> signList, int checkRowIndex, IFormData formData)
        {
            var _formData = (LeaveFormData)formData;
            string RwSeq = string.Empty;
            bool Match = false;

            //20171128 Daniel 調整日期判斷，直接以請假總時數/8來計算
            //20180815 Daniel 因Portal時數有Bug，單位是日的請假總時數會是日數，所以改為判斷假別單位
            const decimal WorkhoursPerDay = 8; //每日工作時數，這邊最理想是按照每日排班時數來計算，目前先暫定為8小時
            decimal totalAmount = _formData.LeaveAmount;
            string unit = _formData.AbsentUnit.ToLower();
            int calculateDays = 0;
            if (unit != "d")
            {
                calculateDays = (int)Math.Ceiling(totalAmount / WorkhoursPerDay);
            }
            else
            {
                calculateDays = (int)totalAmount; //單位是日，直接用請假時數計算
            }
            //var calculateDays = _fromData.EndTime.Date.Subtract(_formData.StartTime.Date).Days + 1; //計算請假天數
 
            //判斷請假天數和系統天數是否相符
            //20171121 Daniel 原本是>=，修改為>
            //if (calculateDays >= _days)
            if (calculateDays > _days)
            {
                Match = true;
            }
            //20171121 Daniel 因上面改為>，此處已不需判斷破日的部分，如1.5天這類的
            /*
            //如果請假天數和系統天數相同的話 排班工作時數減去請假時數應為0 by Bee 20161214
            if (calculateDays == _days && _fromData.WorkHours-_fromData.LeaveAmount > 0)
            {
                Match = false;
            }
            */
            if (Match)
            {
                return signList;
            }
            return this.RunNoMatchAction(signList, checkRowIndex, formData);
        }
    }

    public class IsAbored : ConditionHandler
    {
        public override IList<SignFlowDesignModel> CheckConditionItem(IList<SignFlowDesignModel> signList, int checkRowIndex, IFormData formData)
        {
            var _fromData = (LeaveFormData)formData;

            bool Match = false;
            if (_fromData.IsAbroad)
            {
                Match = true;
            }
            if (Match)
            {
                return signList;
            }
            return this.RunNoMatchAction(signList, checkRowIndex, formData);
        }
    }

    public class NotEnoughAbsentAmount : ConditionHandler
    {
        public override IList<SignFlowDesignModel> CheckConditionItem(IList<SignFlowDesignModel> signList, int checkRowIndex, IFormData formData)
        {
            var _fromData = (LeaveFormData)formData;

            bool Match = false;
            if (_fromData.AfterAmount < 0)
            {
                Match = true;
            }
            if (Match)
            {
                return signList;
            }
            return this.RunNoMatchAction(signList, checkRowIndex, formData);
        }
    }
}
