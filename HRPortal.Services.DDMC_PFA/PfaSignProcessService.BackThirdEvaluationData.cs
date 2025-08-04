

using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System.Collections.Generic;
using System.Transactions;
using System;
using System.Linq;
using System.Data.Entity;
using PFA_Common = HRPortal.Services.DDMC_PFA.Common;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaSignProcessService
    {
        /// <summary>
        /// 績效考核核決-批次退回
        /// </summary>
        /// <param name="model"></param>
        /// <param name="assessment"></param>
        /// <param name="signEmpID"></param>
        /// <returns></returns>
        public Result BackThirdEvaluationData(List<PfaCycleEmp> model, string assessment, Guid signEmpID)
        {
            var mailMsg = "";
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        if (!model.Any())
                            throw new Exception("無退回部門資料");

                        List<PfaMailMessage> pfaMailMessages = new List<PfaMailMessage>();

                        var pfaCycleID = model.FirstOrDefault().PfaCycleID;
                        var pfaDeptIDs = model.Select(x => x.PfaDeptID).ToList();
                        foreach (var data in model)
                        {
                            var pfaCycleEmpIds = db.PfaCycleEmp
                                .Where(x => x.PfaCycleID == data.PfaCycleID
                                && x.PfaDeptID == data.PfaDeptID)
                                .Select(x => x.ID)
                                .Distinct()
                                .ToList();


                            var pfaSignProcessList = db.PfaSignProcess
                                .Where(x => pfaCycleEmpIds.Contains(x.PfaCycleEmpID)
                                && (x.Status == PfaSignProcess_Status.Reviewed
                                    || x.Status == PfaSignProcess_Status.PendingReview
                                    || x.Status == PfaSignProcess_Status.PendingThirdReview
                                    || x.Status == PfaSignProcess_Status.ReturnedForModification
                                    )
                                && x.IsRatio
                                && x.IsThirdEvaluation == true).ToList();

                            foreach (var pfaSignProcess in pfaSignProcessList)
                            {
                                var lastProcess = db.PfaSignProcess
                                    .Where(x => x.PfaCycleEmpID == pfaSignProcess.PfaCycleEmpID
                                    && x.SignStep < pfaSignProcess.SignStep)
                                    .OrderByDescending(x => x.SignStep)
                                    .FirstOrDefault();

                                if (lastProcess != null)
                                {
                                    lastProcess.Status =
                                        PfaSignProcess_Status.ReturnedForModification; // r:退回修改
                                    lastProcess.ConfirmTime = null;
                                    lastProcess.SignTime = null;
                                }

                                #region 簽核紀錄
                                var pfaSignRecord = new PfaSignRecord
                                {
                                    ID = Guid.NewGuid(),
                                    PfaCycleEmpID = pfaSignProcess.PfaCycleEmpID,
                                    SignStep = pfaSignProcess.SignStep,
                                    SignLevelID = pfaSignProcess.SignLevelID,
                                    IsSelfEvaluation = pfaSignProcess.IsSelfEvaluation,
                                    IsFirstEvaluation = pfaSignProcess.IsFirstEvaluation,
                                    IsSecondEvaluation = pfaSignProcess.IsSecondEvaluation,
                                    IsThirdEvaluation = pfaSignProcess.IsThirdEvaluation,
                                    IsUpload = pfaSignProcess.IsUpload,
                                    PfaEmpTypeID = pfaSignProcess.PfaEmpTypeID,
                                    IsAgent = pfaSignProcess.IsAgent,
                                    IsRatio = pfaSignProcess.IsRatio,
                                    OrgSignEmpID = pfaSignProcess.OrgSignEmpID,
                                    PreSignEmpID = pfaSignProcess.PreSignEmpID,
                                    Status = PfaSignProcess_Status.Returned, // b:已退回
                                    Assessment = assessment,
                                    ConfirmTime = null,
                                    SignEmpID = pfaSignProcess.SignEmpID,
                                    SignTime = DateTime.Now,
                                    CreatedBy = signEmpID,
                                    CreatedTime = DateTime.Now,
                                };
                                db.PfaSignRecord.Add(pfaSignRecord);
                                #endregion

                                #region 清除狀態
                                var clearSignProcessList = db.PfaSignProcess
                                    .Where(x => x.PfaCycleEmpID == pfaSignProcess.PfaCycleEmpID
                                    && x.SignStep >= pfaSignProcess.SignStep).ToList();

                                if (clearSignProcessList.Any())
                                {
                                    foreach (var clearSignProcess in clearSignProcessList)
                                    {
                                        clearSignProcess.Status =
                                            PfaSignProcess_Status.NotReceived;  //m: 未收件

                                        clearSignProcess.SignEmpID = null;
                                        clearSignProcess.ConfirmTime = null;
                                        clearSignProcess.Assessment = null;
                                        clearSignProcess.SignTime = null;
                                    }
                                }
                                #endregion

                                #region 發送Mail
                                if (lastProcess != null)
                                {
                                    var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                                    string _sendMailTestRcpt = mailAppSetting.TestRcpt;
                                    string _sendMailSubject = mailAppSetting.Subject;
                                    string _sendMailIsCancel = mailAppSetting.IsCancel;
                                    string _portalWebUrl = mailAppSetting.PortalWebUrl;

                                    PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();

                                    var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == lastProcess.SignEmpID);
                                    if (preSignEmp != null)
                                    {
                                        var mailMessage = new PfaMailMessage
                                        {
                                            ID = Guid.NewGuid(),
                                            PfaCycleID = data.PfaCycleID,
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
                                                mailMsg = string.Format("簽核主管 {0} {1} 未設定通知email,請通知hr設定。", preSignEmp.EmployeeNO, preSignEmp.EmployeeName);
                                                throw new Exception(mailMsg);
                                            }
                                            mailMessage.Rcpt = preSignEmp.Email;
                                        }
                                        else
                                        {
                                            mailMessage.Rcpt = _sendMailTestRcpt;
                                        }
                                        var pfaDeptName = "";
                                        var pfaDept = db.PfaDept.FirstOrDefault(x => x.ID == data.PfaDeptID);
                                        if (pfaDept != null)
                                            pfaDeptName = pfaDept.PfaDeptName;
                                        mailMessage.Subject = _sendMailSubject;
                                        mailMessage.Body = string.Format(mailAppSetting.PfaSendBackMessage, string.Empty, _portalWebUrl);
                                        pfaMailMessages.Add(mailMessage);
                                        db.SaveChanges();
                                    }
                                }
                                #endregion
                            }
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

                        #region 績效考核人數配比設定
                        string[] finStatus = { PfaSignProcess_Status.Reviewed
                                , PfaSignProcess_Status.Submitted }; // a:已評核 e: 已送出
                        var pfaOrgIDs = db.PfaOrgDept
                            .Where(x => pfaDeptIDs.Contains(x.PfaDeptID))
                            .Select(x => x.PfaOrgID)
                            .Distinct()
                            .ToList();

                        foreach (var pfaOrgID in pfaOrgIDs)
                        {
                            var pfaCycleRation = db.PfaCycleRation
                                .FirstOrDefault(x => x.PfaCycleID == pfaCycleID
                                && x.PfaOrgID == pfaOrgID);

                            if (pfaCycleRation != null)
                            {
                                // 組織已自評人數
                                pfaCycleRation.SelfFinal = db.PfaSignProcess
                                    .Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleID
                                    && x.PfaCycleEmp.PfaOrgID == pfaOrgID
                                    && x.IsSelfEvaluation == true
                                    && finStatus.Contains(x.Status)
                                    && x.IsRatio)
                                    .Select(x => x.PfaCycleEmpID)
                                    .Distinct()
                                    .Count();

                                // 組織已初評人數
                                var firstEvaluationProcessList = db.PfaSignProcess
                                    .Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleID
                                    && x.PfaCycleEmp.PfaOrgID == pfaOrgID
                                    && x.IsFirstEvaluation == true
                                    && finStatus.Contains(x.Status)
                                    && x.IsRatio)
                                    .ToList();

                                var firstEvaluationEmpIDs =
                                    firstEvaluationProcessList.Select(x => x.PfaCycleEmpID)
                                    .Distinct()
                                    .ToList();
                                pfaCycleRation.FirstFinal = firstEvaluationEmpIDs.Count();


                                foreach (var pfaCycleRationDetail in pfaCycleRation.PfaCycleRationDetail)
                                {
                                    pfaCycleRationDetail.FirstFinal =
                                        db.PfaCycleEmp.Where(x => firstEvaluationEmpIDs.Contains(x.ID)
                                            && x.FirstPerformance_ID ==
                                                pfaCycleRationDetail.PfaPerformanceID).Count();
                                }

                                // 組織已複評人數
                                var secondEvaluationProcessList =
                                    db.PfaSignProcess.Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleID
                                    && x.PfaCycleEmp.PfaOrgID == pfaOrgID
                                    && x.IsSecondEvaluation == true
                                    && finStatus.Contains(x.Status)
                                    && x.IsRatio)
                                    .ToList();

                                var secondEvaluationEmpIDs =
                                    secondEvaluationProcessList
                                    .Select(x => x.PfaCycleEmpID)
                                    .Distinct()
                                    .ToList();
                                pfaCycleRation.SecondFinal = secondEvaluationEmpIDs.Count();

                                foreach (var pfaCycleRationDetail in pfaCycleRation.PfaCycleRationDetail)
                                {
                                    pfaCycleRationDetail.SecondFinal =
                                        db.PfaCycleEmp.Where(x => secondEvaluationEmpIDs.Contains(x.ID)
                                            && x.LastPerformance_ID == pfaCycleRationDetail.PfaPerformanceID)
                                        .Count();
                                }



                                // 組織已核評人數
                                var thirdEvaluationProcessList =
                                    db.PfaSignProcess.Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleID
                                    && x.PfaCycleEmp.PfaOrgID == pfaOrgID
                                    && x.IsThirdEvaluation == true
                                    && finStatus.Contains(x.Status)
                                    && x.IsRatio)
                                    .ToList();

                                var thirdEvaluationEmpIDs =
                                    thirdEvaluationProcessList
                                    .Select(x => x.PfaCycleEmpID)
                                    .Distinct()
                                    .ToList();
                                pfaCycleRation.ThirdFinal = thirdEvaluationEmpIDs.Count();

                                foreach (var pfaCycleRationDetail in pfaCycleRation.PfaCycleRationDetail)
                                {
                                    pfaCycleRationDetail.ThirdFinal =
                                        db.PfaCycleEmp.Where(x => thirdEvaluationEmpIDs.Contains(x.ID)
                                            && x.LastPerformance_ID == pfaCycleRationDetail.PfaPerformanceID)
                                        .Count();
                                }

                            }
                        }


                        db.SaveChanges();
                        #endregion

                        result.success = true;
                        result.message = "退回成功";
                        result.log = "退回成功";

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "送出失敗";
                        if (!string.IsNullOrEmpty(mailMsg))
                            result.message = result.message + ":" + mailMsg;
                        result.log = string.Format("送出失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}