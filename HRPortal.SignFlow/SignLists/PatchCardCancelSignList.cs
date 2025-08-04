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
    public partial class PatchCardCancelSignList : HRPortalSignList<SignFlowRec>
    {
        public PatchCardCancelSignList()
            : base(FormType.PatchCard)
        {
            SignFlowRecRepository = new SignFlowRecRepository();
            //OnGetSignerId = getSignerId;
        }

        public IList<SignFlowRecModel> CopyFlow(string newFormNo, string oringinFormNo, string senderId, bool addSender = true)
        {
            List<SignFlowRec> _orignalSignFlow = SignFlowRecRepository.GetSignFlowRec(oringinFormNo, "Y").OrderBy(x => x.GroupId).ThenBy(x => x.SignOrder).ToList();
            List<SignFlowRecModel> _result = new List<SignFlowRecModel>();

            foreach (SignFlowRec _flow in _orignalSignFlow)
            {
                if (_flow.SignStatus == "S" || _flow.SignStatus == "A")
                {
                    SignFlowRecModel _newFlow = new SignFlowRecModel()
                    {
                        FormNumber = newFormNo,
                        FormType = FormType.PatchCardCancel.ToString(),
                        FormLevelID = _flow.FormLevelID,
                        SignStatus = "W",
                        SignType = _flow.SignType == "S" ? "S" : "P",
                        SignerID = _flow.ActSignerID,
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
                    };
                    _result.Add(_newFlow);
                }
                else
                {
                    _result.RemoveAt(_result.Count - 1);
                }
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
                PatchCardCancel _overTimeCancel = _db.PatchCardCancel.FirstOrDefault(x => x.FormNo == enty.FormNumber);
                return new PatchCardFormData(_overTimeCancel.PatchCardForm);
            }
        }
    }
    // SiteRelateDateSignList
}
