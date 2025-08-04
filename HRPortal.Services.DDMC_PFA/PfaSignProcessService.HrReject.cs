

using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System.Collections.Generic;
using System.Transactions;
using System;
using System.Linq;
using System.Data.Entity;
using PFA_Common = HRPortal.Services.DDMC_PFA.Common;
using System.Linq.Dynamic;
using HRPortal.Services.DDMC_PFA.Common;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaSignProcessService
    {

        public Result HrReject(Guid pfaCycleId, Guid[] PfaOrgIds, Guid signEmpID)
        {
            Result result = new Result();
            try
            {
                result.success = true;
                using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
                {
                    using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                    {

                        List<PfaMailMessage> pfaMailMessages = new List<PfaMailMessage>();

                        foreach (Guid PfaOrgId in PfaOrgIds) 
                        {
                            #region PfaCycle
                            /*
                             * 由 e 改回  a
                             */
                            var pfaCycle = db.PfaCycle.Where(r=> r.ID == pfaCycleId
                                && r.Status == PfaCycle_Status.PfaFinish
                            ).FirstOrDefault();
                            if (pfaCycle != null) 
                            {
                                pfaCycle.Status = PfaCycle_Status.InApprovalProcess;
                            }
                            #endregion

                            #region PfaCycleEmp
                            /*
                             * 由 e 改回  a
                             */
                            var pfaCycleEmps = db.PfaCycleEmp.Where(r => r.PfaCycleID == pfaCycleId
                                && r.PfaOrgID == PfaOrgId
                                && r.Status == PfaCycleEmp_Status.Approved)
                                .Include(r => r.PfaSignProcess);
                            foreach (var pfaCycleEmp in pfaCycleEmps)
                            {
                                pfaCycleEmp.Status = PfaCycleEmp_Status.InApprovalProcess;
                            }
                            #endregion

                            #region PfaSignProcess
                            /*
                             * isThirdEvaluation = 1 
                             * 由 e 改成  r
                             */
                            foreach (var pfaCycleEmp in pfaCycleEmps)
                            {
                                int maxSignStep = pfaCycleEmp.PfaSignProcess.Max(r => r.SignStep);
                                var pfaSignProcess = pfaCycleEmp.PfaSignProcess
                                    .FirstOrDefault(r => r.SignStep == maxSignStep);

                                if (pfaSignProcess.Status == PfaSignProcess_Status.Submitted)
                                {
                                    pfaSignProcess.Status = PfaSignProcess_Status.ReturnedForModification;

                                    #region 發送Mail
                                    pfaMailMessages.Add(SentMail(db, pfaCycleEmp, pfaSignProcess));
                                    #endregion

                                    #region 簽核紀錄
                                    AddPfaSignRecord(db, signEmpID, pfaSignProcess);
                                    #endregion
                                }
                            }
                                #endregion


                            db.SaveChanges();
                        }

                        #region 稽催信同一天要合併
                        db.PfaMailMessage.AddRange(pfaMailMessages.GroupBy(r => new
                        {
                            r.Rcpt,
                            r.Subject,
                            r.Body,
                        })
                            .Select(r => r.First()));
                        #endregion

                        db.SaveChanges();

                        if (result.success)
                        {
                            tx.Complete();
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                result.success = false;
                result.log = ExceptionHelper.GetMsg(ex);
                result.message = "退回失敗";
                return result;
            }
        }


        private static void AddPfaSignRecord(NewHRPortalEntitiesDDMC_PFA db, Guid signEmpID, PfaSignProcess pfaSignProcess)
        {
            var pfaSignRecord = new PfaSignRecord
            {
                ID = Guid.NewGuid(),
                PfaCycleEmpID = pfaSignProcess.PfaCycleEmpID,
                SignStep = pfaSignProcess.SignStep,
                SignLevelID = pfaSignProcess.SignLevelID,
                IsSelfEvaluation = false,
                IsFirstEvaluation = false,
                IsSecondEvaluation = false,
                IsThirdEvaluation = false,
                IsHrEvaluation = true,
                IsUpload = pfaSignProcess.IsUpload,
                PfaEmpTypeID = pfaSignProcess.PfaEmpTypeID,
                IsAgent = pfaSignProcess.IsAgent,
                IsRatio = pfaSignProcess.IsRatio,
                OrgSignEmpID = pfaSignProcess.OrgSignEmpID,
                PreSignEmpID = pfaSignProcess.PreSignEmpID,
                Status = PfaSignProcess_Status.Returned, // b:已退回
                Assessment = "HR 退回",
                ConfirmTime = null,
                SignEmpID = signEmpID,
                SignTime = DateTime.Now,
                CreatedBy = signEmpID,
                CreatedTime = DateTime.Now,
            };
            db.PfaSignRecord.Add(pfaSignRecord);
        }

        private static PfaMailMessage SentMail(NewHRPortalEntitiesDDMC_PFA db, PfaCycleEmp pfaCycleEmp, PfaSignProcess pfaSignProcess)
        {
            var mailAppSetting = PFA_Common.AppSettingsMail.Get();
            string _sendMailTestRcpt = mailAppSetting.TestRcpt;
            string _sendMailSubject = mailAppSetting.Subject;
            string _sendMailIsCancel = mailAppSetting.IsCancel;
            string _portalWebUrl = mailAppSetting.PortalWebUrl;

            PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();

            var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == pfaSignProcess.SignEmpID);
            if (preSignEmp != null)
            {
                var mailMessage = new PfaMailMessage
                {
                    ID = Guid.NewGuid(),
                    PfaCycleID = pfaCycleEmp.PfaCycleID,
                    EmployeeID = preSignEmp.ID,
                    SourceType = PfaMailMessage_SourceType.GeneralSubmission,
                    FromAccountID = _mailAccount.ID,
                    IsHtml = true,
                    HadSend = false,
                    IsCancel = _sendMailIsCancel == "Y",
                    CreateTime = DateTime.Now,
                };

                if (string.IsNullOrEmpty(_sendMailTestRcpt))
                {
                    if (string.IsNullOrEmpty(preSignEmp.Email))
                    {
                        string mailMsg = string.Format("簽核主管 {0} {1} 未設定通知email,請通知hr設定。", preSignEmp.EmployeeNO, preSignEmp.EmployeeName);
                        throw new Exception(mailMsg);
                    }
                    mailMessage.Rcpt = preSignEmp.Email;
                }
                else
                {
                    mailMessage.Rcpt = _sendMailTestRcpt;
                }
                var pfaDeptName = "";
                var pfaDept = db.PfaDept.FirstOrDefault(x => x.ID == pfaCycleEmp.PfaDeptID);
                if (pfaDept != null)
                    pfaDeptName = pfaDept.PfaDeptName;
                mailMessage.Subject = _sendMailSubject;
                mailMessage.Body = string.Format(mailAppSetting.PfaSendBackMessage, string.Empty, _portalWebUrl);

                return mailMessage;
            }
            else
            {
                return null;
            }
        }
    }
}