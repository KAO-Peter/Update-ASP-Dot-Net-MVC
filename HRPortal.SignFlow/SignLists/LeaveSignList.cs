using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using YoungCloud.SignFlow.Conditions;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.Model;
using YoungCloud.Configurations;
using Autofac;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.SignLists;
using HRPortal.SignFlow.Conditions;
using HRPortal.DBEntities;
using System.Linq;
using HRPortal.SignFlow.Model;

namespace HRPortal.SignFlow.SignLists
{
    public partial class LeaveSignList : HRPortalSignList<SignFlowRec>
	{
		public LeaveSignList() : base(FormType.Leave)
		{
            SignFlowRecRepository = new SignFlowRecRepository();
            OnGetSignerId = getSignerId;
		}

        protected virtual string[] getSignerId(SignFlowDesignModel design, SignflowCreateBase etyInitData)
        {
            string _signDeptType = (design.DeptType == null ? null : design.DeptType.ToString());
            string _dept = null;
            if (!string.IsNullOrEmpty(_signDeptType) && _signDeptType == "11")
            {
                LeaveFormData _formData = (LeaveFormData)GetFormData(etyInitData); //20201230 Daniel �y�{�]�w���N�z�H�~�h��а���
                return new string[] { "P", _formData.AgentNo };
            }
            else
            {
                return base.getSignerId(design, etyInitData);
            }
        }

        public override IFormData GetFormData(SignflowCreateBase enty)
        {
            //20201230 Daniel �p�G���]�w_tempFormData�A�N���h��DB����LeaveForm�A���@���٨S�s�JDB������]�i�H�w�����oñ�֬y�{
            if (this._tempFormData != null)
            {
                return this._tempFormData;
            }
            else
            {
                using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
                {
                    LeaveForm _leaveForm = _db.LeaveForms.FirstOrDefault(x => x.FormNo == enty.FormNumber);
                    return new LeaveFormData(_leaveForm);
                }
            }
        }
	}
	// SiteRelateDateSignList
}
