

using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System.Collections.Generic;
using System.Transactions;
using System;
using System.Linq;
using PFA_Common = HRPortal.Services.DDMC_PFA.Common;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaSignProcessService
    {
        // UpdateThirdEvaluationData

        /// <summary>
        /// 績效考核複核
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycleEmp"></param>
        /// <param name="pfaEmpIndicator">工作績效</param>
        /// <param name="pfaEmpAbility">勝任能力</param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Result UpdateThirdEvaluationData(PfaSignProcess pfaSignProcess
            , PfaCycleEmp pfaCycleEmp
            , List<PfaEmpIndicator> pfaEmpIndicator
            , List<PfaEmpAbility> pfaEmpAbility
            , string cmd)
        {
            var mailMsg = "";
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var oldPfaSignProcess = db.PfaSignProcess.FirstOrDefault(x => x.ID == pfaSignProcess.ID);
                        oldPfaSignProcess.Status = pfaSignProcess.Status;
                        oldPfaSignProcess.Assessment = pfaSignProcess.Assessment;

                        if (cmd == "btnSave" || cmd == "btnSent")
                        {
                            oldPfaSignProcess.ConfirmTime = DateTime.Now;

                            #region 更新退回狀態
                            var backSignProcessList = db.PfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID && x.Status == PfaSignProcess_Status.Returned).ToList();
                            if (backSignProcessList.Any())
                            {
                                foreach (var backSignProcess in backSignProcessList)
                                {
                                    backSignProcess.Status = PfaSignProcess_Status.NotReceived;  //m: 未收件
                                    backSignProcess.SignEmpID = null;
                                    backSignProcess.ConfirmTime = null;
                                    backSignProcess.Assessment = null;
                                    backSignProcess.SignTime = null;
                                }
                            }
                            #endregion
                        }

                        #region 績效考核員工資料
                        var oldPfaCycleEmp = db.PfaCycleEmp.FirstOrDefault(x => x.ID == pfaCycleEmp.ID);
                        oldPfaCycleEmp.ManagerIndicator = pfaCycleEmp.ManagerIndicator;
                        oldPfaCycleEmp.ManagerAbility = pfaCycleEmp.ManagerAbility;

                        oldPfaCycleEmp.PfaFirstScore = pfaCycleEmp.PfaFirstScore;
                        oldPfaCycleEmp.PfaLastScore = pfaCycleEmp.PfaLastScore;
                        oldPfaCycleEmp.PfaFinalScore = pfaCycleEmp.PfaFinalScore;

                        oldPfaCycleEmp.FirstPerformance_ID = pfaCycleEmp.FirstPerformance_ID;
                        oldPfaCycleEmp.LastPerformance_ID = pfaCycleEmp.LastPerformance_ID;
                        oldPfaCycleEmp.FinalPerformance_ID = pfaCycleEmp.FinalPerformance_ID;

                        oldPfaCycleEmp.FirstAppraisal = pfaCycleEmp.FirstAppraisal;
                        oldPfaCycleEmp.LastAppraisal = pfaCycleEmp.LastAppraisal;
                        oldPfaCycleEmp.FinalAppraisal = pfaCycleEmp.FinalAppraisal;

                        oldPfaCycleEmp.PastPerformance = pfaCycleEmp.PastPerformance;
                        oldPfaCycleEmp.NowPerformance = pfaCycleEmp.NowPerformance;
                        oldPfaCycleEmp.Development = pfaCycleEmp.Development;
                        oldPfaCycleEmp.ModifiedBy = pfaCycleEmp.ModifiedBy;
                        oldPfaCycleEmp.ModifiedTime = pfaCycleEmp.ModifiedTime;
                        #endregion

                        #region 績效考核員工工作績效
                        if (pfaEmpIndicator.Count > 0)
                        {
                            var oldPfaEmpIndicatorList = db.PfaEmpIndicator.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID).ToList();
                            foreach (var oldPfaEmpIndicator in oldPfaEmpIndicatorList)
                            {
                                var newPfaEmpIndicator = pfaEmpIndicator.FirstOrDefault(x => x.ID == oldPfaEmpIndicator.ID);
                                if (newPfaEmpIndicator != null)
                                {
                                    if (newPfaEmpIndicator.ManagerIndicator != oldPfaEmpIndicator.ManagerIndicator)
                                    {
                                        oldPfaEmpIndicator.ManagerIndicator = newPfaEmpIndicator.ManagerIndicator;
                                        oldPfaEmpIndicator.ModifiedBy = newPfaEmpIndicator.ModifiedBy;
                                        oldPfaEmpIndicator.ModifiedTime = newPfaEmpIndicator.ModifiedTime;
                                    }
                                    pfaEmpIndicator.Remove(newPfaEmpIndicator);
                                }
                                else
                                    db.PfaEmpIndicator.Remove(oldPfaEmpIndicator);
                            }
                            if (pfaEmpIndicator.Any())
                                db.PfaEmpIndicator.AddRange(pfaEmpIndicator);
                        }
                        #endregion

                        #region 績效考核員工勝任能力
                        if (pfaEmpAbility.Count > 0)
                        {
                            var oldPfaEmpAbilityList = db.PfaEmpAbility.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID).ToList();
                            foreach (var oldPfaEmpAbility in oldPfaEmpAbilityList)
                            {
                                var newPfaEmpAbility = pfaEmpAbility.FirstOrDefault(x => x.ID == oldPfaEmpAbility.ID);
                                if (newPfaEmpAbility != null)
                                {
                                    if (newPfaEmpAbility.ManagerAbility != oldPfaEmpAbility.ManagerAbility)
                                    {
                                        oldPfaEmpAbility.ManagerAbility = newPfaEmpAbility.ManagerAbility;
                                        oldPfaEmpAbility.ModifiedBy = newPfaEmpAbility.ModifiedBy;
                                        oldPfaEmpAbility.ModifiedTime = newPfaEmpAbility.ModifiedTime;
                                    }
                                    foreach (var oldPfaEmpAbilityDetail in oldPfaEmpAbility.PfaEmpAbilityDetail)
                                    {
                                        var newPfaEmpAbilityDetail = newPfaEmpAbility.PfaEmpAbilityDetail.FirstOrDefault(x => x.ID == oldPfaEmpAbilityDetail.ID);
                                        if (newPfaEmpAbilityDetail != null)
                                        {
                                            if (newPfaEmpAbilityDetail.AbilityScore == oldPfaEmpAbilityDetail.AbilityScore)
                                            {
                                                newPfaEmpAbility.PfaEmpAbilityDetail.Remove(newPfaEmpAbilityDetail);
                                                continue;
                                            }
                                            oldPfaEmpAbilityDetail.AbilityScore = newPfaEmpAbilityDetail.AbilityScore;
                                            oldPfaEmpAbilityDetail.ModifiedBy = newPfaEmpAbilityDetail.ModifiedBy;
                                            oldPfaEmpAbilityDetail.ModifiedTime = newPfaEmpAbilityDetail.ModifiedTime;
                                        }
                                        else
                                            db.PfaEmpAbilityDetail.Remove(oldPfaEmpAbilityDetail);
                                    }
                                    pfaEmpAbility.Remove(newPfaEmpAbility);
                                }
                                else
                                {
                                    if (oldPfaEmpAbility.PfaEmpAbilityDetail.Any())
                                        db.PfaEmpAbilityDetail.RemoveRange(oldPfaEmpAbility.PfaEmpAbilityDetail.ToList());
                                    db.PfaEmpAbility.Remove(oldPfaEmpAbility);
                                }
                            }
                            if (pfaEmpAbility.Any())
                                db.PfaEmpAbility.AddRange(pfaEmpAbility);
                        }
                        #endregion

                        db.SaveChanges();

                        if (cmd == "btnReject")
                        {
                            PfaSignProcess perviousProcess = PFA_Common.SignProcess.GetPreviousProcess(db.PfaSignProcess
                                , pfaCycleEmp.ID
                                , pfaSignProcess.SignStep);


                            if (perviousProcess != null)
                            {
                                perviousProcess.Status =
                                    PfaSignProcess_Status.ReturnedForModification; // r:退回修改
                                perviousProcess.ConfirmTime = null;
                                perviousProcess.SignTime = null;
                            }

                            oldPfaSignProcess.Status = oldPfaSignProcess.Status;
                            oldPfaSignProcess.ConfirmTime= oldPfaSignProcess.ConfirmTime;
                            oldPfaSignProcess.SignTime= oldPfaSignProcess.SignTime;


                            #region 簽核紀錄
                            var pfaSignRecord = new PfaSignRecord
                            {
                                ID = Guid.NewGuid(),
                                PfaCycleEmpID = oldPfaSignProcess.PfaCycleEmpID,
                                SignStep = oldPfaSignProcess.SignStep,
                                SignLevelID = oldPfaSignProcess.SignLevelID,
                                IsSelfEvaluation = oldPfaSignProcess.IsSelfEvaluation,
                                IsFirstEvaluation = oldPfaSignProcess.IsFirstEvaluation,
                                IsSecondEvaluation = oldPfaSignProcess.IsSecondEvaluation,
                                IsThirdEvaluation = oldPfaSignProcess.IsThirdEvaluation,
                                IsUpload = oldPfaSignProcess.IsUpload,
                                PfaEmpTypeID = oldPfaSignProcess.PfaEmpTypeID,
                                IsAgent = oldPfaSignProcess.IsAgent,
                                IsRatio = oldPfaSignProcess.IsRatio,
                                OrgSignEmpID = oldPfaSignProcess.OrgSignEmpID,
                                PreSignEmpID = oldPfaSignProcess.PreSignEmpID,
                                Status = PfaSignProcess_Status.Returned,
                                SignEmpID = oldPfaSignProcess.SignEmpID,
                                ConfirmTime = null,
                                SignTime = DateTime.Now,
                                CreatedBy = pfaCycleEmp.ModifiedBy.Value,
                                CreatedTime = pfaCycleEmp.ModifiedTime.Value,
                                Assessment = oldPfaSignProcess.Assessment,
                            };
                            db.PfaSignRecord.Add(pfaSignRecord);
                            #endregion

                            #region 清除狀態
                            var clearSignProcessList = db.PfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID && x.SignStep >= pfaSignProcess.SignStep).ToList();
                            if (clearSignProcessList.Any())
                            {
                                foreach (var clearSignProcess in clearSignProcessList)
                                {
                                    clearSignProcess.Status = PfaSignProcess_Status.NotReceived;  //m: 未收件
                                    clearSignProcess.SignEmpID = null;
                                    clearSignProcess.ConfirmTime = null;
                                    clearSignProcess.Assessment = null;
                                    clearSignProcess.SignTime = null;
                                }
                            }
                            #endregion

                            #region 發送Mail
                            if (perviousProcess != null)
                            {
                                var mailAppSetting = PFA_Common.AppSettingsMail.Get();

                                PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();

                                var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == perviousProcess.PreSignEmpID);
                                if (preSignEmp != null)
                                {
                                    var mailMessage = new PfaMailMessage
                                    {
                                        ID = Guid.NewGuid(),
                                        PfaCycleID = pfaCycleEmp.PfaCycleID,
                                        EmployeeID = pfaCycleEmp.EmployeeID,
                                        SourceType = PfaMailMessage_SourceType.GeneralSubmission,
                                        FromAccountID = _mailAccount.ID,
                                        IsHtml = true,
                                        HadSend = false,
                                        IsCancel = mailAppSetting.IsCancel == "Y",
                                        CreateTime = DateTime.Now,
                                    };

                                    if (string.IsNullOrEmpty(mailAppSetting.TestRcpt))
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
                                        mailMessage.Rcpt = mailAppSetting.TestRcpt;
                                    }

                                    mailMessage.Subject = mailAppSetting.Subject;
                                    mailMessage.Body = string.Format(mailAppSetting.PfaSendBackMessage, "", mailAppSetting.PortalWebUrl);
                                    db.PfaMailMessage.Add(mailMessage);
                                    db.SaveChanges();
                                }
                            }
                            #endregion

                            result.success = true;
                            result.message = "退回成功";
                            result.log = "退回成功";
                        }
                        if (cmd == "btnSent")
                        {
                            oldPfaSignProcess.SignEmpID = pfaSignProcess.SignEmpID;
                            oldPfaSignProcess.SignTime = DateTime.Now;

                            oldPfaCycleEmp.Status = PfaCycleEmp_Status.Approved; // e: 已審核

                            #region 簽核紀錄
                            var pfaSignRecord = new PfaSignRecord
                            {
                                ID = Guid.NewGuid(),
                                PfaCycleEmpID = oldPfaSignProcess.PfaCycleEmpID,
                                SignStep = oldPfaSignProcess.SignStep,
                                SignLevelID = oldPfaSignProcess.SignLevelID,
                                IsSelfEvaluation = oldPfaSignProcess.IsSelfEvaluation,
                                IsFirstEvaluation = oldPfaSignProcess.IsFirstEvaluation,
                                IsSecondEvaluation = oldPfaSignProcess.IsSecondEvaluation,
                                IsThirdEvaluation = oldPfaSignProcess.IsThirdEvaluation,
                                IsUpload = oldPfaSignProcess.IsUpload,
                                PfaEmpTypeID = oldPfaSignProcess.PfaEmpTypeID,
                                IsAgent = oldPfaSignProcess.IsAgent,
                                IsRatio = oldPfaSignProcess.IsRatio,
                                OrgSignEmpID = oldPfaSignProcess.OrgSignEmpID,
                                PreSignEmpID = oldPfaSignProcess.PreSignEmpID,
                                Status = oldPfaSignProcess.Status,
                                SignEmpID = oldPfaSignProcess.SignEmpID,
                                ConfirmTime = oldPfaSignProcess.ConfirmTime,
                                SignTime = oldPfaSignProcess.SignTime,
                                CreatedBy = pfaCycleEmp.ModifiedBy.Value,
                                CreatedTime = pfaCycleEmp.ModifiedTime.Value,
                            };
                            db.PfaSignRecord.Add(pfaSignRecord);
                            #endregion

                            db.SaveChanges();

                            result.success = true;
                            result.message = "簽核完成";
                            result.log = "簽核完成";
                        }
                        else
                        {
                            result.success = true;
                            result.message = "儲存成功";
                            result.log = "儲存成功";
                        }

                        #region 績效考核人數配比設定
                        if (oldPfaCycleEmp.IsRatio)
                        {
                            string[] finStatus = { PfaSignProcess_Status.Reviewed
                                    , PfaSignProcess_Status.PendingThirdReview
                                    , PfaSignProcess_Status.Submitted }; // a:已評核 e: 已送出 b:已退回 r:退回修改
                            var pfaCycleRation = db.PfaCycleRation.FirstOrDefault(x => x.PfaCycleID == pfaCycleEmp.PfaCycleID && x.PfaOrgID == pfaCycleEmp.PfaOrgID);
                            if (pfaCycleRation != null)
                            {
                                // 組織已自評人數
                                var selfCnt = db.PfaSignProcess.Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleEmp.PfaCycleID && x.PfaCycleEmp.PfaOrgID == pfaCycleEmp.PfaOrgID &&
                                                                           x.IsSelfEvaluation == true && finStatus.Contains(x.Status) && x.IsRatio).Select(x => x.PfaCycleEmpID).Distinct().Count();
                                if (pfaCycleRation.SelfFinal != selfCnt)
                                {
                                    pfaCycleRation.SelfFinal = selfCnt;
                                    pfaCycleRation.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                    pfaCycleRation.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                }
                                // 組織已初評人數
                                var firstEvaluationProcessList = db.PfaSignProcess.Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleEmp.PfaCycleID && x.PfaCycleEmp.PfaOrgID == pfaCycleEmp.PfaOrgID &&
                                                                                              x.IsFirstEvaluation == true && finStatus.Contains(x.Status) && x.IsRatio).ToList();
                                var firstEvaluationEmpIDs = firstEvaluationProcessList.Select(x => x.PfaCycleEmpID).Distinct().ToList();
                                if (pfaCycleRation.FirstFinal != firstEvaluationEmpIDs.Count())
                                {
                                    pfaCycleRation.FirstFinal = firstEvaluationEmpIDs.Count();
                                    pfaCycleRation.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                    pfaCycleRation.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                }
                                foreach (var pfaCycleRationDetail in pfaCycleRation.PfaCycleRationDetail)
                                {
                                    var firstCnt = db.PfaCycleEmp.Where(x => firstEvaluationEmpIDs.Contains(x.ID) && x.FirstPerformance_ID == pfaCycleRationDetail.PfaPerformanceID).Count();
                                    if (pfaCycleRationDetail.FirstFinal != firstCnt)
                                    {
                                        pfaCycleRationDetail.FirstFinal = firstCnt;
                                        pfaCycleRationDetail.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                        pfaCycleRationDetail.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                    }
                                }

                                // 組織已複評人數
                                var secondEvaluationProcessList =
                                    db.PfaSignProcess.Where(x =>
                                    x.PfaCycleEmp.PfaCycleID == pfaCycleEmp.PfaCycleID
                                    && x.PfaCycleEmp.PfaOrgID == pfaCycleEmp.PfaOrgID
                                    && x.IsSecondEvaluation == true
                                    && finStatus.Contains(x.Status)
                                    && x.IsRatio).ToList();

                                var secondEvaluationEmpIDs =
                                    secondEvaluationProcessList
                                    .Select(x => x.PfaCycleEmpID).Distinct().ToList();

                                if (pfaCycleRation.SecondFinal != secondEvaluationEmpIDs.Count())
                                {
                                    pfaCycleRation.SecondFinal = secondEvaluationEmpIDs.Count();
                                    pfaCycleRation.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                    pfaCycleRation.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                }
                                foreach (var pfaCycleRationDetail in pfaCycleRation.PfaCycleRationDetail)
                                {
                                    var lastCnt = db.PfaCycleEmp
                                        .Where(x => secondEvaluationEmpIDs.Contains(x.ID)
                                        && x.LastPerformance_ID == pfaCycleRationDetail.PfaPerformanceID).Count();

                                    if (pfaCycleRationDetail.SecondFinal != lastCnt)
                                    {
                                        pfaCycleRationDetail.SecondFinal = lastCnt;
                                        pfaCycleRationDetail.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                        pfaCycleRationDetail.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                    }
                                }

                                // 組織已核評人數
                                var thirdEvaluationProcessList =
                                    db.PfaSignProcess.Where(x =>
                                    x.PfaCycleEmp.PfaCycleID == pfaCycleEmp.PfaCycleID
                                    && x.PfaCycleEmp.PfaOrgID == pfaCycleEmp.PfaOrgID
                                    && x.IsThirdEvaluation == true
                                    && finStatus.Contains(x.Status)
                                    && x.IsRatio).ToList();

                                var thirdEvaluationEmpIDs =
                                    thirdEvaluationProcessList
                                    .Select(x => x.PfaCycleEmpID).Distinct().ToList();

                                if (pfaCycleRation.ThirdFinal != thirdEvaluationEmpIDs.Count())
                                {
                                    pfaCycleRation.ThirdFinal = thirdEvaluationEmpIDs.Count();
                                    pfaCycleRation.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                    pfaCycleRation.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                }
                                foreach (var pfaCycleRationDetail in pfaCycleRation.PfaCycleRationDetail)
                                {
                                    var thirdCnt = db.PfaCycleEmp
                                        .Where(x => thirdEvaluationEmpIDs.Contains(x.ID)
                                        && x.FinalPerformance_ID == pfaCycleRationDetail.PfaPerformanceID).Count();

                                    if (pfaCycleRationDetail.ThirdFinal != thirdCnt)
                                    {
                                        pfaCycleRationDetail.ThirdFinal = thirdCnt;
                                        pfaCycleRationDetail.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                        pfaCycleRationDetail.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                    }
                                }
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        #region 檢核考評完成
                        // 未完成狀態
                        string[] notFinstatus = { PfaCycleEmp_Status.NotSubmittedForApproval
                                , PfaCycleEmp_Status.InApprovalProcess }; // m: 未送簽 a:簽核中 e: 已審核 y:已鎖定

                        var tempPfaCycleEmp = db.PfaCycleEmp.FirstOrDefault(x =>
                            x.PfaCycleID == pfaCycleEmp.PfaCycleID
                            && notFinstatus.Contains(x.Status));

                        if (tempPfaCycleEmp == null)
                        {
                            var pfaCycle = db.PfaCycle.FirstOrDefault(x => x.ID == pfaCycleEmp.PfaCycleID);
                            pfaCycle.Status = PfaCycle_Status.PfaFinish;
                            pfaCycle.ModifiedBy = pfaCycleEmp.ModifiedBy.Value;
                            pfaCycle.ModifiedTime = pfaCycleEmp.ModifiedTime.Value;
                            db.SaveChanges();
                        }
                        #endregion

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "儲存失敗";
                        if (!string.IsNullOrEmpty(mailMsg))
                            result.message = result.message + ":" + mailMsg;
                        result.log = string.Format("儲存失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}