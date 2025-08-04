using HRPortal.ApiAdapter;
using HRPortal.DBEntities;
using HRPortal.Services;
using HRPortal.SignFlow.Helper;
using HRPortal.SignFlow.SignLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.SignFlow.Model
{
    public class FormChangeSign
    {
        public HRPortal_Services Services = new HRPortal_Services();
        private SignFlowEntities _sdb;

        public SignFlowEntities SDB
        {
            get
            {
                if (_sdb == null)
                {
                    _sdb = new SignFlowEntities();
                }
                return _sdb;
            }
        }

        public FormChangeSign()
        {
        }

        public FormChangeSign(string oSignerID)
        {
            if (string.IsNullOrWhiteSpace(oSignerID)) return;

            List<string> _FormNumber = SDB.SignFlowRec.Where(x => x.SignerID == oSignerID
                                                                  && x.SignType == "P"
                                                                  && x.SignStatus == "W"
                                                                  && x.IsUsed == "Y")
                                                         .GroupBy(g => new { g.FormNumber })
                                                         .Select(x => x.Key.FormNumber)
                                                         .ToList();

            //取出需要簽核的資料
            List<SignFlowRec> _signFlowList = SDB.SignFlowRec.Where(x => x.SignType == "P"
                                                                      && x.SignStatus == "W"
                                                                      && x.IsUsed == "Y"
                                                                      && _FormNumber.Contains(x.FormNumber)
                                                                    ).ToList();

            SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();

            foreach (SignFlowRec _signFlowData in _signFlowList)
            {
                try
                {
                    switch (_signFlowData.FormType)
                    {
                        case "Leave":
                            //由單號取請假單資料
                            LeaveForm _form_ = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == _signFlowData.FormNumber);
                            LeaveFormData _formData = new LeaveFormData(_form_);
                            LeaveSignList _signList = new LeaveSignList();
                            IList<SignFlowRecModel> _signFlow_new = _signList.GetDefaultSignList(_formData, true);//取新的簽核流程
                            IList<SignFlowRecModel> _signFlow_old = _queryHelper.GetSignFlowByFormNumber(_form_.FormNo);//取舊的單子簽核流程
                            var SignOrder_new = _signFlow_old.Where(x => x.SignerID == _formData.EmployeeNo).OrderByDescending(x => x.SignOrder).FirstOrDefault();
                            IList<SignFlowRecModel> _signFlow_olds = _queryHelper.GetSignFlowByFormNumber(_form_.FormNo).Where(x => Double.Parse(x.SignOrder) >= Double.Parse(SignOrder_new.SignOrder)).ToList();//取舊的單子簽核流程
                            ChangeSign(_signFlow_new, _signFlow_olds);
                            break;

                        case "LeaveCancel":
                            //由單號取請假單資料
                            LeaveCancel _formC = Services.GetService<LeaveCancelService>().FirstOrDefault(x => x.FormNo == _signFlowData.FormNumber);
                            LeaveForm _formCC = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == _formC.LeaveForm.FormNo);
                            LeaveFormData _formDataC = new LeaveFormData(_formCC);
                            LeaveSignList _signListC = new LeaveSignList();
                            IList<SignFlowRecModel> _signFlowC_new = _signListC.GetDefaultSignList(_formDataC, true);//取新的簽核流程
                            IList<SignFlowRecModel> _signFlowC_old = _queryHelper.GetSignFlowByFormNumber(_formC.FormNo);//取舊的單子簽核流程
                            var SignOrderC_new = _signFlowC_old.Where(x => x.SignerID == _formDataC.EmployeeNo).OrderByDescending(x => x.SignOrder).FirstOrDefault();
                            IList<SignFlowRecModel> _signFlowC_olds = _queryHelper.GetSignFlowByFormNumber(_formC.FormNo).Where(x => Double.Parse(x.SignOrder) >= Double.Parse(SignOrderC_new.SignOrder)).ToList();//取舊的單子簽核流程
                            ChangeSign(_signFlowC_new, _signFlowC_olds);
                            break;

                        case "OverTime":
                            //由單號取加班單資料
                            OverTimeForm _formO = Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == _signFlowData.FormNumber);
                            OverTimeFormData OformData = new OverTimeFormData(_formO);
                            OverTimeSignList OsignList = new OverTimeSignList();
                            IList<SignFlowRecModel> OsignFlow_new = OsignList.GetDefaultSignList(OformData, true);//取新的簽核流程
                            IList<SignFlowRecModel> OsignFlow_old = _queryHelper.GetSignFlowByFormNumber(_formO.FormNo);//取舊的單子簽核流程
                            var SignOrderO_new = OsignFlow_old.Where(x => x.SignerID == OformData.EmployeeNo).OrderByDescending(x => x.SignOrder).FirstOrDefault();
                            IList<SignFlowRecModel> _signFlowO_olds = _queryHelper.GetSignFlowByFormNumber(_formO.FormNo).Where(x => Double.Parse(x.SignOrder) >= Double.Parse(SignOrderO_new.SignOrder)).ToList();//取舊的單子簽核流程
                            ChangeSign(OsignFlow_new, _signFlowO_olds);
                            break;

                        case "OverTimeCancel":
                            //由單號取加班單資料
                            OverTimeCancel _formOC = Services.GetService<OverTimeCancelService>().FirstOrDefault(x => x.FormNo == _signFlowData.FormNumber);
                            OverTimeForm _formOCC = Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == _formOC.OverTimeForm.FormNo);
                            OverTimeFormData OCformData = new OverTimeFormData(_formOCC);
                            OverTimeSignList OCsignList = new OverTimeSignList();
                            IList<SignFlowRecModel> OCsignFlow_new = OCsignList.GetDefaultSignList(OCformData, true);//取新的簽核流程
                            IList<SignFlowRecModel> OCsignFlow_old = _queryHelper.GetSignFlowByFormNumber(_formOC.FormNo);//取舊的單子簽核流程
                            var SignOrderOC_new = OCsignFlow_old.Where(x => x.SignerID == OCformData.EmployeeNo).OrderByDescending(x => x.SignOrder).FirstOrDefault();
                            IList<SignFlowRecModel> _signFlowOC_olds = _queryHelper.GetSignFlowByFormNumber(_formOC.FormNo).Where(x => Double.Parse(x.SignOrder) >= Double.Parse(SignOrderOC_new.SignOrder)).ToList();//取舊的單子簽核流程
                            ChangeSign(OCsignFlow_new, _signFlowOC_olds);
                            break;
                    }
                }
                catch 
                {                   
                }
            }
        }

        private void ChangeSign(IList<SignFlowRecModel> _signFlowNew, IList<SignFlowRecModel> _signFlowOld)
        {
            if (_signFlowNew.Count() == _signFlowOld.Count())
            {
                bool isMail = true;
                for (int a = 0; a <= _signFlowNew.Count - 1; a++)
                {
                    if (_signFlowOld[a].SignStatus == "W")
                    {
                        var SignerID_New = string.IsNullOrWhiteSpace(_signFlowNew[a].OrgSignerID) ? _signFlowNew[a].SignerID : _signFlowNew[a].OrgSignerID;
                        var SignerID_Old = string.IsNullOrWhiteSpace(_signFlowOld[a].OrgSignerID) ? _signFlowOld[a].SignerID : _signFlowOld[a].OrgSignerID;

                        if (SignerID_New != SignerID_Old)
                        {
                            GetSignFlow(_signFlowOld[a].ID, SignerID_New);//變更簽核人

                            if (isMail)
                            {
                                SignFlowMail(_signFlowOld[a], SignerID_New);
                                isMail = false;
                            }
                        }
                        else
                        {
                            isMail = false;
                        }
                    }
                }
            }
        }

        /// <summary> 變更簽核人員 </summary>
        private void GetSignFlow(string _signFlowDataID, string agentNo)
        {
            //變更簽核人員
            if (_signFlowDataID != null)
            {
                var updSignFlow = _sdb.SignFlowRec.Find(_signFlowDataID);
                updSignFlow.SignerID = agentNo;
                updSignFlow.OrgSignerID = agentNo;
                _sdb.SaveChanges();
            }
        }

        public async Task SignFlowMail(SignFlowRecModel _flow, string newSignerID)
        {
            SignMailHelper _mailHelper = new SignMailHelper(await HRMApiAdapter.GetAllAbsent(_flow.SenderCompanyID));

            _flow.OrgSignerID = newSignerID;
            _flow.SignerID = newSignerID;
            _mailHelper.SendMailOnFlowAccepted(_flow);
        }
    }
}