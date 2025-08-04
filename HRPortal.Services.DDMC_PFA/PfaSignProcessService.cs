using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Common;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Transactions;
using PFA_Common = HRPortal.Services.DDMC_PFA.Common;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaSignProcessService : BaseCrudService<PfaSignProcess>
    {
        public PfaSignProcessService(HRPortal_Services services) : base(services)
        {
        }

        /// <summary>
        /// 績效考核自評
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycleEmp"></param>
        /// <param name="pfaEmpIndicator"></param>
        /// <param name="pfaEmpTraining"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Result UpdateSelfEvaluationData(PfaSignProcess pfaSignProcess
            , PfaCycleEmp pfaCycleEmp
            , List<PfaEmpIndicator> pfaEmpIndicator // 指標
            , List<PfaEmpTraining> pfaEmpTraining   // 課程
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
                        string oldStatus = oldPfaSignProcess.Status;
                        oldPfaSignProcess.Status = pfaSignProcess.Status;
                        oldPfaSignProcess.ConfirmTime = DateTime.Now;

                        pfaSignProcess.Status = oldStatus;

                        #region 績效考核員工資料
                        var oldPfaCycleEmp = db.PfaCycleEmp.FirstOrDefault(x => x.ID == pfaCycleEmp.ID);
                        oldPfaCycleEmp.SelfIndicator = pfaCycleEmp.SelfIndicator;
                        oldPfaCycleEmp.PfaSelfScore = pfaCycleEmp.PfaSelfScore;
                        oldPfaCycleEmp.SelfAppraisal = pfaCycleEmp.SelfAppraisal;
                        oldPfaCycleEmp.ModifiedBy = pfaCycleEmp.ModifiedBy;
                        oldPfaCycleEmp.ModifiedTime = pfaCycleEmp.ModifiedTime;
                        #endregion

                        #region 績效考核員工工作績效
                        var oldPfaEmpIndicatorList = db.PfaEmpIndicator.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID).ToList();
                        foreach (var oldPfaEmpIndicator in oldPfaEmpIndicatorList)
                        {
                            var newPfaEmpIndicator = pfaEmpIndicator.FirstOrDefault(x => x.ID == oldPfaEmpIndicator.ID);
                            if (newPfaEmpIndicator != null)
                            {
                                if (newPfaEmpIndicator.SelfIndicator != oldPfaEmpIndicator.SelfIndicator)
                                {
                                    oldPfaEmpIndicator.SelfIndicator = newPfaEmpIndicator.SelfIndicator;
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
                        #endregion

                        #region 績效考核員工訓練紀錄
                        var oldPfaEmpTrainingList = db.PfaEmpTraining.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID).ToList();
                        foreach (var oldPfaEmpTraining in oldPfaEmpTrainingList)
                        {
                            var newPfaEmpTraining = pfaEmpTraining.FirstOrDefault(x => x.ID == oldPfaEmpTraining.ID);
                            if (newPfaEmpTraining != null)
                            {
                                if (newPfaEmpTraining.CoursesCode != oldPfaEmpTraining.CoursesCode || newPfaEmpTraining.CoursesName != oldPfaEmpTraining.CoursesName ||
                                    newPfaEmpTraining.TrainingHours != oldPfaEmpTraining.TrainingHours)
                                {
                                    oldPfaEmpTraining.CoursesCode = newPfaEmpTraining.CoursesCode;
                                    oldPfaEmpTraining.CoursesName = newPfaEmpTraining.CoursesName;
                                    oldPfaEmpTraining.TrainingHours = newPfaEmpTraining.TrainingHours;
                                    oldPfaEmpTraining.ModifiedBy = newPfaEmpTraining.ModifiedBy;
                                    oldPfaEmpTraining.ModifiedTime = newPfaEmpTraining.ModifiedTime;
                                }
                                pfaEmpTraining.Remove(newPfaEmpTraining);
                            }
                            else
                                db.PfaEmpTraining.Remove(oldPfaEmpTraining);
                        }
                        if (pfaEmpTraining.Any())
                            db.PfaEmpTraining.AddRange(pfaEmpTraining);
                        #endregion

                        #region 更新退回狀態
                        if (cmd == "btnSent")
                        {
                            var backSignProcessList = db.PfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID 
                            && x.Status == PfaSignProcess_Status.Returned).ToList();

                            if (backSignProcessList.Any())
                            {
                                foreach (var backSignProcess in backSignProcessList)
                                {
                                    backSignProcess.Status = PfaSignProcess_Status.NotReceived;
                                    backSignProcess.SignEmpID = null;
                                    backSignProcess.ConfirmTime = null;
                                    backSignProcess.Assessment = null;
                                    backSignProcess.SignTime = null;
                                }
                            }
                        }
                        #endregion

                        db.SaveChanges();

                        if (cmd == "btnSent")
                        {
                            oldPfaSignProcess.SignEmpID = pfaSignProcess.SignEmpID;
                            oldPfaSignProcess.SignTime = DateTime.Now;

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

                            #region 簽核流程-尋找下一關
                            var nextSignProcess =
                                PFA_Common.SignProcess.GetNextProcess(db.PfaSignProcess, pfaCycleEmp.ID, pfaSignProcess.ID);

                            if (nextSignProcess != null)
                                nextSignProcess.Status = PfaSignProcess_Status.PendingReview;
                            db.SaveChanges();
                            #endregion

                            #region 發送Mail
                            if (nextSignProcess != null)
                            {
                                var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                                string _sendMailTestRcpt = mailAppSetting.TestRcpt;
                                string _sendMailSubject = mailAppSetting.Subject;
                                string _sendMailMessage = mailAppSetting.Message;
                                string _sendMailIsCancel = mailAppSetting.IsCancel;
                                string _portalWebUrl = mailAppSetting.PortalWebUrl;

                                PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();

                                var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == nextSignProcess.PreSignEmpID);
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
                                            mailMsg = string.Format("簽核主管 {0} {1} 未設定通知email,請通知hr設定。", preSignEmp.EmployeeNO, preSignEmp.EmployeeName);
                                            throw new Exception(mailMsg);
                                        }
                                        mailMessage.Rcpt = preSignEmp.Email;
                                    }
                                    else
                                    {
                                        mailMessage.Rcpt = _sendMailTestRcpt;
                                    }

                                    var subject = "";
                                    if (nextSignProcess.IsFirstEvaluation && !nextSignProcess.IsSecondEvaluation)
                                        subject = "初核";
                                    else
                                        subject = "複核";
                                    mailMessage.Subject = string.Format(_sendMailSubject, subject);
                                    mailMessage.Body = string.Format(_sendMailMessage, subject, string.Format("考核員工:{0}", pfaCycleEmp.Employees.EmployeeName), _portalWebUrl);
                                    db.PfaMailMessage.Add(mailMessage);
                                    db.SaveChanges();
                                }
                            }
                            #endregion

                            result.success = true;
                            result.message = string.Format("成功送到下一關{0}", nextSignProcess.Employees.EmployeeName);
                            result.log = string.Format("成功送到下一關{0}", nextSignProcess.Employees.EmployeeName);
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
                            string[] finStatus = { PfaSignProcess_Status.Reviewed, PfaSignProcess_Status.Submitted };

                            var pfaCycleRation = db.PfaCycleRation
                                .FirstOrDefault(x => x.PfaCycleID == pfaCycleEmp.PfaCycleID 
                                    && x.PfaOrgID == pfaCycleEmp.PfaOrgID);

                            if (pfaCycleRation != null)
                                pfaCycleRation.SelfFinal = db.PfaSignProcess
                                    .Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleEmp.PfaCycleID 
                                    && x.PfaCycleEmp.PfaOrgID == pfaCycleEmp.PfaOrgID 
                                    && x.IsSelfEvaluation == true 
                                    && finStatus.Contains(x.Status) 
                                    && x.IsRatio
                                    ).Select(x => x.PfaCycleEmpID).Distinct().Count();
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

        /// <summary>
        /// 績效考核初核
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycleEmp"></param>
        /// <param name="pfaEmpIndicator"></param>
        /// <param name="pfaEmpAbility"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Result UpdateFirstEvaluationData(PfaSignProcess pfaSignProcess
            , PfaCycleEmp pfaCycleEmp
            , List<PfaEmpIndicator> pfaEmpIndicator // 指標
            , List<PfaEmpAbility> pfaEmpAbility     // 勝任能力考核
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

                        if (cmd == "btnSave")
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
                        oldPfaCycleEmp.FirstPerformance_ID = pfaCycleEmp.FirstPerformance_ID;
                        oldPfaCycleEmp.FirstAppraisal = pfaCycleEmp.FirstAppraisal;
                        oldPfaCycleEmp.PastPerformance = pfaCycleEmp.PastPerformance;
                        oldPfaCycleEmp.NowPerformance = pfaCycleEmp.NowPerformance;
                        oldPfaCycleEmp.Development = pfaCycleEmp.Development;
                        oldPfaCycleEmp.LastAppraisal = pfaCycleEmp.LastAppraisal;
                        oldPfaCycleEmp.ModifiedBy = pfaCycleEmp.ModifiedBy;
                        oldPfaCycleEmp.ModifiedTime = pfaCycleEmp.ModifiedTime;
                        #endregion

                        #region 績效考核員工工作績效
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
                        #endregion

                        #region 績效考核員工勝任能力
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
                        #endregion

                        db.SaveChanges();

                        if (cmd == "btnReject")
                        {
                            var lastProcess = db.PfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID && x.SignStep < pfaSignProcess.SignStep)
                                                               .OrderByDescending(x=> x.SignStep).FirstOrDefault();
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
                                PfaCycleEmpID = oldPfaSignProcess.PfaCycleEmpID,
                                SignStep = oldPfaSignProcess.SignStep,
                                SignLevelID = oldPfaSignProcess.SignLevelID,
                                IsSelfEvaluation = oldPfaSignProcess.IsSelfEvaluation,
                                IsFirstEvaluation = oldPfaSignProcess.IsFirstEvaluation,
                                IsSecondEvaluation = oldPfaSignProcess.IsSecondEvaluation,
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
                            if (lastProcess != null)
                            {
                                var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                                string _sendMailTestRcpt = mailAppSetting.TestRcpt;
                                string _sendMailSubject = mailAppSetting.Subject;
                                string _sendMailMessage = mailAppSetting.Message;
                                string _sendMailIsCancel = mailAppSetting.IsCancel;
                                string _portalWebUrl = mailAppSetting.PortalWebUrl;

                                PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();

                                var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == lastProcess.PreSignEmpID);
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
                                            mailMsg = string.Format("簽核主管 {0} {1} 未設定通知email,請通知hr設定。", preSignEmp.EmployeeNO, preSignEmp.EmployeeName);
                                            throw new Exception(mailMsg);
                                        }
                                        mailMessage.Rcpt = preSignEmp.Email;
                                    }
                                    else
                                    {
                                        mailMessage.Rcpt = _sendMailTestRcpt;
                                    }

                                    mailMessage.Subject = _sendMailSubject;
                                    mailMessage.Body = string.Format(mailAppSetting.PfaSendBackMessage, "", _portalWebUrl);
                                    db.PfaMailMessage.Add(mailMessage);
                                    db.SaveChanges();
                                }
                            }
                            #endregion

                            result.success = true;
                            result.message = "退回成功";
                            result.log = "退回成功";
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
                                    , PfaSignProcess_Status.Submitted }; 

                            var pfaCycleRation = 
                                db.PfaCycleRation.FirstOrDefault(x => 
                                x.PfaCycleID == pfaCycleEmp.PfaCycleID 
                                && x.PfaOrgID == pfaCycleEmp.PfaOrgID);

                            if (pfaCycleRation != null)
                            {
                                // 組織已自評人數
                                var selfCnt = 
                                    db.PfaSignProcess.Where(x => 
                                    x.PfaCycleEmp.PfaCycleID == pfaCycleEmp.PfaCycleID 
                                    && x.PfaCycleEmp.PfaOrgID == pfaCycleEmp.PfaOrgID 
                                    && x.IsSelfEvaluation == true 
                                    && finStatus.Contains(x.Status) 
                                    && x.IsRatio)
                                    .Select(x => x.PfaCycleEmpID).Distinct().Count();

                                if (pfaCycleRation.SelfFinal != selfCnt)
                                {
                                    pfaCycleRation.SelfFinal = selfCnt;
                                    pfaCycleRation.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                    pfaCycleRation.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                }

                                // 組織已初評人數
                                var firstEvaluationProcessList = 
                                    db.PfaSignProcess.Where(x => 
                                    x.PfaCycleEmp.PfaCycleID == pfaCycleEmp.PfaCycleID 
                                    && x.PfaCycleEmp.PfaOrgID == pfaCycleEmp.PfaOrgID 
                                    && x.IsFirstEvaluation == true 
                                    && finStatus.Contains(x.Status) 
                                    && x.IsRatio)
                                    .ToList();

                                var firstEvaluationEmpIDs = 
                                    firstEvaluationProcessList
                                    .Select(x => x.PfaCycleEmpID).Distinct().ToList();

                                if (pfaCycleRation.FirstFinal != firstEvaluationEmpIDs.Count())
                                {
                                    pfaCycleRation.FirstFinal = firstEvaluationEmpIDs.Count();
                                    pfaCycleRation.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                    pfaCycleRation.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                }


                                foreach (var pfaCycleRationDetail 
                                    in pfaCycleRation.PfaCycleRationDetail)
                                {
                                    var firstCnt = db.PfaCycleEmp.Where(x => 
                                        firstEvaluationEmpIDs.Contains(x.ID) 
                                        && x.FirstPerformance_ID == 
                                            pfaCycleRationDetail.PfaPerformanceID
                                        ).Count();

                                    if (pfaCycleRationDetail.FirstFinal != firstCnt)
                                    {
                                        pfaCycleRationDetail.FirstFinal = firstCnt;
                                        pfaCycleRationDetail.ModifiedBy = pfaCycleEmp.ModifiedBy;
                                        pfaCycleRationDetail.ModifiedTime = pfaCycleEmp.ModifiedTime;
                                    }
                                }
                                db.SaveChanges();
                            }
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

        /// <summary>
        /// 績效考核初核-送出
        /// </summary>
        /// <param name="model"></param>
        /// <param name="signEmpID"></param>
        /// <returns></returns>
        public Result SentFirstEvaluationData(List<PfaCycleEmp> model, Guid signEmpID)
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
                            throw new Exception("無送出部門資料");

                        #region Mail
                        var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                        PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();
                        #endregion

                        List<PfaMailMessage> pfaMailMessages = new List<PfaMailMessage>();
                        foreach (var data in model)
                        {
                            var pfaCycleEmpIds = db.PfaCycleEmp.Where(x => 
                                x.PfaCycleID == data.PfaCycleID && // 考核簽核批號ID
                                x.PfaDeptID == data.PfaDeptID)     // 考核部門ID
                                .Select(x => x.ID)
                                .Distinct()
                                .ToList();

                            var pfaSignProcessList = db.PfaSignProcess.Where(x => 
                            pfaCycleEmpIds.Contains(x.PfaCycleEmpID) 
                            && x.Status == PfaSignProcess_Status.Reviewed
                            && x.IsRatio // 需配比人數
                            && x.IsFirstEvaluation == true // 是否初核
                            && x.IsSecondEvaluation == false).ToList(); // 是否複核

                            foreach (var pfaSignProcess in pfaSignProcessList)
                            {
                                pfaSignProcess.Status = PfaSignProcess_Status.Submitted; // e: 已送出

                                // 確認時間
                                pfaSignProcess.ConfirmTime = pfaSignProcess.ConfirmTime.HasValue ? pfaSignProcess.ConfirmTime.Value: DateTime.Now;

                                // 實際簽核人ID
                                pfaSignProcess.SignEmpID = signEmpID;
                                pfaSignProcess.SignTime = DateTime.Now;

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
                                    IsUpload = pfaSignProcess.IsUpload,
                                    PfaEmpTypeID = pfaSignProcess.PfaEmpTypeID,
                                    IsAgent = pfaSignProcess.IsAgent,
                                    IsRatio = pfaSignProcess.IsRatio,
                                    OrgSignEmpID = pfaSignProcess.OrgSignEmpID,
                                    PreSignEmpID = pfaSignProcess.PreSignEmpID,
                                    Status = pfaSignProcess.Status,
                                    SignEmpID = pfaSignProcess.SignEmpID,
                                    ConfirmTime = pfaSignProcess.ConfirmTime,
                                    SignTime = pfaSignProcess.SignTime,
                                    CreatedBy = signEmpID,
                                    CreatedTime = DateTime.Now,
                                };
                                db.PfaSignRecord.Add(pfaSignRecord);
                                #endregion

                                #region 簽核流程-尋找下一關
                                var nextSignProcess =
                                    PFA_Common.SignProcess.GetNextProcess(db.PfaSignProcess
                                    , pfaSignProcess.PfaCycleEmpID
                                    , pfaSignProcess.ID);

                                if (nextSignProcess != null)
                                {
                                    nextSignProcess.Status = PfaSignProcess_Status.PendingReview;

                                    #region 發送Mail
                                    var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == nextSignProcess.PreSignEmpID);
                                    if (preSignEmp != null)
                                    {
                                        var mailMessage = new PfaMailMessage
                                        {
                                            ID = Guid.NewGuid(),
                                            PfaCycleID = nextSignProcess.PfaCycleEmp.PfaCycleID,
                                            EmployeeID = preSignEmp.ID,
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
                                        var subject = "複核";
                                        mailMessage.Subject = string.Format(mailAppSetting.Subject, subject);
                                        mailMessage.Body = string.Format(mailAppSetting.Message, subject, "", mailAppSetting.PortalWebUrl);
                                        pfaMailMessages.Add(mailMessage);
                                    }
                                    #endregion
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

                        result.success = true;
                        result.message = "送出成功";
                        result.log = "送出成功";

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

        /// <summary>
        /// 績效考核複核
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycleEmp"></param>
        /// <param name="pfaEmpIndicator"></param>
        /// <param name="pfaEmpAbility"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Result UpdateSecondEvaluationData(PfaSignProcess pfaSignProcess, PfaCycleEmp pfaCycleEmp, List<PfaEmpIndicator> pfaEmpIndicator, List<PfaEmpAbility> pfaEmpAbility, string cmd)
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
                        oldPfaCycleEmp.FirstPerformance_ID = pfaCycleEmp.FirstPerformance_ID;
                        oldPfaCycleEmp.PfaLastScore = pfaCycleEmp.PfaLastScore;
                        oldPfaCycleEmp.LastPerformance_ID = pfaCycleEmp.LastPerformance_ID;

                        oldPfaCycleEmp.PfaFinalScore = pfaCycleEmp.PfaLastScore;
                        oldPfaCycleEmp.FinalPerformance_ID = pfaCycleEmp.LastPerformance_ID;

                        oldPfaCycleEmp.FirstAppraisal = pfaCycleEmp.FirstAppraisal;
                        oldPfaCycleEmp.PastPerformance = pfaCycleEmp.PastPerformance;
                        oldPfaCycleEmp.NowPerformance = pfaCycleEmp.NowPerformance;
                        oldPfaCycleEmp.Development = pfaCycleEmp.Development;
                        oldPfaCycleEmp.LastAppraisal = pfaCycleEmp.LastAppraisal;
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

                        #region 是否還有下一關
                        bool hasNextSignProcess = false;
                        var nextSignProcess =
                                    PFA_Common.SignProcess.GetNextProcess(db.PfaSignProcess
                                    , pfaSignProcess.PfaCycleEmpID
                                    , pfaSignProcess.ID);

                        hasNextSignProcess = nextSignProcess != null;
                        #endregion

                        if (cmd == "btnReject")
                        {
                            var lastProcess = db.PfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID && x.SignStep < pfaSignProcess.SignStep)
                                                               .OrderByDescending(x => x.SignStep).FirstOrDefault();
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
                                PfaCycleEmpID = oldPfaSignProcess.PfaCycleEmpID,
                                SignStep = oldPfaSignProcess.SignStep,
                                SignLevelID = oldPfaSignProcess.SignLevelID,
                                IsSelfEvaluation = oldPfaSignProcess.IsSelfEvaluation,
                                IsFirstEvaluation = oldPfaSignProcess.IsFirstEvaluation,
                                IsSecondEvaluation = oldPfaSignProcess.IsSecondEvaluation,
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
                            if (lastProcess != null)
                            {
                                var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                                string _sendMailTestRcpt = mailAppSetting.TestRcpt;
                                string _sendMailSubject = mailAppSetting.Subject;
                                string _sendMailMessage = mailAppSetting.Message;
                                string _sendMailIsCancel = mailAppSetting.IsCancel;
                                string _portalWebUrl = mailAppSetting.PortalWebUrl;

                                PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();

                                var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == lastProcess.PreSignEmpID);
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

                                    mailMessage.Subject = _sendMailSubject;
                                    mailMessage.Body = string.Format(mailAppSetting.PfaSendBackMessage, "", _portalWebUrl);
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

                            if(hasNextSignProcess == false)
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
                                db.SaveChanges();
                            }
                        }
                        #endregion


                        #region 檢核考評完成
                        bool pfaCycleEmpHasNextSignProcess = false;
                        string[] notFinstatus = { PfaCycleEmp_Status.NotSubmittedForApproval
        , PfaCycleEmp_Status.InApprovalProcess }; // m: 未送簽 a:簽核中 e: 已審核 y:已鎖定

                        var tempPfaCycleEmp = db.PfaCycleEmp.FirstOrDefault(x =>
                            x.PfaCycleID == pfaCycleEmp.PfaCycleID
                            && notFinstatus.Contains(x.Status));

                        pfaCycleEmpHasNextSignProcess = tempPfaCycleEmp != null;
                        if (pfaCycleEmpHasNextSignProcess == false)
                        {
                            var pfaCycle = db.PfaCycle.FirstOrDefault(x => x.ID == pfaCycleEmp.PfaCycleID);
                            pfaCycle.Status = PfaCycle_Status.PfaFinish;
                            pfaCycle.ModifiedBy = pfaCycleEmp.ModifiedBy.Value;
                            pfaCycle.ModifiedTime = pfaCycleEmp.ModifiedTime.Value;
                            db.SaveChanges();
                        }
                        #endregion

                        #region 複核之後，如果還有核決，設定核決相關資料 & 發送信件
                        if(cmd == "btnSent" && hasNextSignProcess)
                        {
                            #region Mail
                            var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                            PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();
                            #endregion

                            #region 簽核流程-尋找下一關
                            nextSignProcess =
                                    PFA_Common.SignProcess.GetNextProcess(db.PfaSignProcess
                                    , pfaSignProcess.PfaCycleEmpID
                                    , pfaSignProcess.ID);

                            if (nextSignProcess != null)
                            {
                                nextSignProcess.Status = PfaSignProcess_Status.PendingReview;

                                #region 發送Mail
                                var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == nextSignProcess.PreSignEmpID);
                                if (preSignEmp != null)
                                {
                                    var mailMessage = new PfaMailMessage
                                    {
                                        ID = Guid.NewGuid(),
                                        PfaCycleID = nextSignProcess.PfaCycleEmp.PfaCycleID,
                                        EmployeeID = preSignEmp.ID,
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
                                    var subject = "核決";
                                    mailMessage.Subject = string.Format(mailAppSetting.Subject, subject);
                                    mailMessage.Body = string.Format(mailAppSetting.Message, subject, "", mailAppSetting.PortalWebUrl);
                                    db.PfaMailMessage.Add(mailMessage);
                                }
                                #endregion
                            }
                            #endregion

                            /*
                             * 送達 核決 之後，設定成待核決，待核決成績沿用複核的成績
                             */
                            DateTime dNow = DateTime.Now;

                            var pfaCycleEmpData = db.PfaCycleEmp.Where(r =>
                                   r.ID == pfaSignProcess.PfaCycleEmpID
                            ).FirstOrDefault();

                            var pfaSignProcesss = db.PfaSignProcess.Where(r => r.PfaCycleEmpID == pfaSignProcess.PfaCycleEmpID
                                && r.IsThirdEvaluation == true
                                && r.IsRatio == false).ToList();


                            pfaCycleEmpData.PfaFinalScore = pfaCycleEmpData.PfaLastScore;
                            pfaCycleEmpData.FinalPerformance_ID = pfaCycleEmpData.LastPerformance_ID;


                            foreach (var itemSP in pfaSignProcesss)
                            {
                                itemSP.Status = PfaSignProcess_Status.PendingThirdReview;
                                itemSP.ConfirmTime = dNow;
                            }

                        }

                        db.SaveChanges();
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


        /// <summary>
        /// 績效考核複核-退回
        /// </summary>
        /// <param name="model"></param>
        /// <param name="assessment"></param>
        /// <param name="signEmpID"></param>
        /// <returns></returns>
        public Result BackSecondEvaluationData(List<PfaCycleEmp> model, string assessment, Guid signEmpID)
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

                        var pfaCycleID = model.FirstOrDefault().PfaCycleID;
                        var pfaDeptIDs = model.Select(x => x.PfaDeptID).ToList();

                        List<PfaMailMessage> pfaMailMessages = new List<PfaMailMessage>();

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
                                && x.IsSecondEvaluation == true).ToList();

                            foreach (var pfaSignProcess in pfaSignProcessList)
                            {
                                var previousProcess = SignProcess.GetPreviousProcess(db.PfaSignProcess
                                    , pfaSignProcess.PfaCycleEmpID
                                    , pfaSignProcess.SignStep);

                                if (previousProcess != null)
                                {
                                    previousProcess.Status = 
                                        PfaSignProcess_Status.ReturnedForModification; // r:退回修改
                                    previousProcess.ConfirmTime = null;
                                    previousProcess.SignTime = null;
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
                                if (previousProcess != null)
                                {
                                    var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                                    string _sendMailTestRcpt = mailAppSetting.TestRcpt;
                                    string _sendMailSubject = mailAppSetting.Subject;
                                    string _sendMailIsCancel = mailAppSetting.IsCancel;
                                    string _portalWebUrl = mailAppSetting.PortalWebUrl;

                                    PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();

                                    var preSignEmp = db.Employees.FirstOrDefault(x => x.ID == previousProcess.SignEmpID);
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

        /// <summary>
        /// 績效考核指定代簽
        /// </summary>
        /// <param name="model"></param>
        /// <param name="signEmpID"></param>
        /// <returns></returns>
        public Result Transfer(List<PfaSignProcess> model, Guid signEmpID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        foreach (var data in model)
                        {
                            var pfaSignProcess = db.PfaSignProcess
                                .FirstOrDefault(x => (x.Status == PfaSignProcess_Status.PendingReview 
                                                        || x.Status == PfaSignProcess_Status.PendingThirdReview
                                                        || x.Status == PfaSignProcess_Status.NotReceived
                                                        ) 
                                    && x.ID == data.ID 
                                    && (x.IsSelfEvaluation == true 
                                        || (x.IsFirstEvaluation == true && x.IsSecondEvaluation == false) 
                                        || x.IsSecondEvaluation == true
                                        || x.IsThirdEvaluation == true
                                        ));

                            if (pfaSignProcess != null)
                                pfaSignProcess.PreSignEmpID = data.PreSignEmpID;
                        }
                        db.SaveChanges();

                        result.success = true;
                        result.message = "送出成功";
                        result.log = "送出成功";

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "送出失敗";
                        result.log = string.Format("送出失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 績效考核自評待處理
        /// </summary>
        /// <param name="EmpID"></param>
        /// <returns></returns>
        public List<PfaSignProcess> GetIsSelfData(Guid EmpID)
        {
            //  m: 未收件 c:待評核 a:已評核 e: 已送出 r:退回修改
            string[] status = { PfaSignProcess_Status.PendingReview, 
                PfaSignProcess_Status.Reviewed,
                PfaSignProcess_Status.ReturnedForModification
            };

            List<PfaSignProcess> queryData = Services.GetService<PfaSignProcessService>().GetAll()
                .Where(x => x.PreSignEmpID == EmpID 
                && status.Contains(x.Status) 
                && x.IsSelfEvaluation == true
                ).ToList();

            return queryData;
        }
        /// <summary>
        /// 績效考核初核待簽核
        /// </summary>
        /// <param name="EmpID"></param>
        /// <returns></returns>
        public List<PfaSignProcess> GetIsFirstData(Guid EmpID)
        {
            string[] status = { PfaSignProcess_Status.PendingReview,
                PfaSignProcess_Status.Reviewed,
                PfaSignProcess_Status.ReturnedForModification };

            var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                .Where(x => x.PreSignEmpID == EmpID 
                && status.Contains(x.Status) 
                && x.IsFirstEvaluation == true 
                && x.IsSecondEvaluation == false
                ).ToList();

            return queryData;
        }
        /// <summary>
        /// 績效考核複核待簽核
        /// </summary>
        /// <param name="EmpID"></param>
        /// <returns></returns>
        public List<PfaSignProcess> GetIsSecondData(Guid EmpID)
        {
            string[] status = { PfaSignProcess_Status.PendingReview,
                PfaSignProcess_Status.Reviewed,
                PfaSignProcess_Status.ReturnedForModification };

            var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                .Where(x => x.PreSignEmpID == EmpID 
                && status.Contains(x.Status) 
                && x.IsSecondEvaluation == true
                ).ToList();

            return queryData;
        }
        /// <summary>
        /// 績效考核核決待簽核
        /// </summary>
        /// <param name="EmpID"></param>
        /// <returns></returns>
        public List<PfaSignProcess> GetIsThirdData(Guid EmpID)
        {
            string[] status = {
                PfaSignProcess_Status.PendingReview,
                PfaSignProcess_Status.Reviewed,
                PfaSignProcess_Status.ReturnedForModification, 
                PfaSignProcess_Status.PendingThirdReview
            };

            var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                .Where(x => x.PreSignEmpID == EmpID
                && status.Contains(x.Status)
                && x.IsThirdEvaluation == true
                ).ToList();

            return queryData;
        }

    }
}