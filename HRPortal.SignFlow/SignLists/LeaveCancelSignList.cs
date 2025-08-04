using HRPortal.DBEntities;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.Model;

namespace HRPortal.SignFlow.SignLists
{
    public partial class LeaveCancelSignList : HRPortalSignList<SignFlowRec>
    {
        protected HRPortal_Services Services;

        public LeaveCancelSignList()
            : base(FormType.Leave)
        {
            SignFlowRecRepository = new SignFlowRecRepository();
            Services = new HRPortal_Services();
        }

        public IList<SignFlowRecModel> CopyFlow(string newFormNo, string oringinFormNo, string senderId, bool addSender = true)
        {
            //20180511 Frank 取得最新簽核流程
            LeaveForm _formCC = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == oringinFormNo);
            LeaveFormData _formDataC = new LeaveFormData(_formCC);
            LeaveSignList _signListC = new LeaveSignList();
            IList<SignFlowRecModel> _signFlowC_new = _signListC.GetDefaultSignList(_formDataC, true).ToList();

            List<SignFlowRecModel> _result = new List<SignFlowRecModel>();

            foreach (SignFlowRecModel _flow in _signFlowC_new)
            {
                SignFlowRecModel _newFlow = new SignFlowRecModel()
                {
                    FormNumber = newFormNo,
                    FormType = FormType.LeaveCancel.ToString(),
                    FormLevelID = _flow.FormLevelID,
                    SignStatus = "W",
                    SignType = _flow.SignType == "S" ? "S" : "P",
                    SignerID = _flow.SignerID,
                    IsUsed = "Y",
                    GroupID = 0,
                    CUser = _flow.CUser,
                    CDate = DateTime.Now,
                    MUser = _flow.CUser,
                    MDate = DateTime.Now,
                    DataState = "Add",
                    SenderID = senderId,
                    SenderCompanyID = _flow.SenderCompanyID,
                    SignCompanyID = _flow.SignCompanyID,
                    OrgSignerID = _flow.OrgSignerID
                };
                _result.Add(_newFlow);
            }

            if (!addSender)
            {
                _result.RemoveAll(x => x.SignType == "S");
            }

            for (int i = 1; i <= _result.Count; i++)
            {
                _result[i - 1].SignOrder = i.ToString();
            }

            return _result;
        }

        public override IFormData GetFormData(SignflowCreateBase enty)
        {
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                LeaveCancel _leaveCancel = _db.LeaveCancels.FirstOrDefault(x => x.FormNo == enty.FormNumber);
                return new LeaveFormData(_leaveCancel.LeaveForm);
            }
        }
    }
}