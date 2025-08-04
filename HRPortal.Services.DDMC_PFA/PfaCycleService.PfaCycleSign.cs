using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaCycleService : BaseCrudService<PfaCycle>
    {

        /// <summary>
        /// 啟動送簽
        /// </summary>
        /// <param name="pfaCycleID"></param>
        /// <param name="selfEmpId"></param>
        /// <returns></returns>
        public Result PfaCycleSign(Guid pfaCycleID, Guid selfEmpId)
        {
            var now = DateTime.Now.Date;

            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var signFlowResultList = new List<PfaSignProcessDataViewModel>();
                        var pfaCycle = db.PfaCycle.FirstOrDefault(x => x.ID == pfaCycleID);
                        var pfaCycleEmpList = db.PfaCycleEmp.Where(x => x.PfaCycleID == pfaCycleID).ToList();

                        var employeesList = db.Employees.Where(x => x.CompanyID == pfaCycle.CompanyID).ToList();
                        foreach (var pfaCycleEmp in pfaCycleEmpList)
                        {
                            #region 刪除舊簽核流程
                            var pfaSignProcess = db.PfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmp.ID).ToList();
                            db.PfaSignProcess.RemoveRange(pfaSignProcess);
                            db.SaveChanges();
                            #endregion

                            #region 簽核流程
                            var signFlow = new PfaSignFlowDataViewModel
                            {
                                PfaSignTypeID = pfaCycleEmp.SignTypeID.Value,
                                PfaCycleEmpID = pfaCycleEmp.ID,
                                PfaDeptID = pfaCycleEmp.PfaDeptID,
                                PfaOrgID = pfaCycleEmp.PfaOrgID,
                                EmployeeID = pfaCycleEmp.EmployeeID,
                                IsAgent = pfaCycleEmp.IsAgent,
                                CreatedBy = selfEmpId
                            };
                            PfaSignProcessDataViewModel signFlowResult = pfaSignFlowService.SignFlow(signFlow);

                            if (!string.IsNullOrWhiteSpace(signFlowResult.message))
                                throw new Exception(signFlowResult.message);

                            // SignFlow 整理好的 PfaSignProcess
                            var pfaSignProcesses = signFlowResult.PfaSignProcess;

                            signFlowResult.EmployeeID = pfaCycleEmp.EmployeeID;
                            signFlowResult.EmpEmail = pfaCycleEmp.Employees.Email;
                            if (string.IsNullOrWhiteSpace(signFlowResult.EmpEmail))
                                signFlowResult.EmpEmail = "";

                            foreach (var signProcess in pfaSignProcesses)
                            {
                                var sign = new PfaSignProcess
                                {
                                    ID = signProcess.ID,
                                    PfaCycleEmpID = signProcess.PfaCycleEmpID,
                                    SignStep = signProcess.SignStep,
                                    SignLevelID = signProcess.SignLevelID,
                                    IsUpload = signProcess.IsUpload,
                                    PfaEmpTypeID = pfaCycleEmp.PfaEmpTypeID,
                                    IsAgent = pfaCycleEmp.IsAgent,
                                    IsRatio = pfaCycleEmp.IsRatio,
                                    OrgSignEmpID = signProcess.OrgSignEmpID,
                                    PreSignEmpID = signProcess.PreSignEmpID,
                                    Status = signProcess.Status,
                                    CreatedBy = selfEmpId,
                                    CreatedTime = DateTime.Now,
                                };

                                var employees = employeesList.FirstOrDefault(x => x.ID == sign.PreSignEmpID);
                                if (employees != null)
                                {
                                    signProcess.PreSignEmpEmail = employees.Email;
                                    if (string.IsNullOrWhiteSpace(signProcess.PreSignEmpEmail))
                                        signProcess.PreSignEmpEmail = "";
                                }
                                else
                                    signProcess.PreSignEmpEmail = "";

                                sign.IsSelfEvaluation = signProcess.IsSelfEvaluation;
                                sign.IsFirstEvaluation = signProcess.IsFirstEvaluation;
                                sign.IsSecondEvaluation = signProcess.IsSecondEvaluation;
                                sign.IsThirdEvaluation = signProcess.IsThirdEvaluation;
                                db.PfaSignProcess.Add(sign);
                            }
                            signFlowResultList.Add(signFlowResult);
                            #endregion

                            pfaCycleEmp.Status = "a"; // a: 簽核中
                            pfaCycleEmp.ModifiedBy = selfEmpId;
                            pfaCycleEmp.ModifiedTime = DateTime.Now;
                        }
                        pfaCycle.Status = pfaCycle.Status == "m" ? "a" : pfaCycle.Status;
                        pfaCycle.StartDate = DateTime.Now;
                        pfaCycle.ModifiedBy = selfEmpId;
                        pfaCycle.ModifiedTime = DateTime.Now;
                        db.SaveChanges();

                        #region 產生考核簽核人名單
                        var mailAccount = db.MailAccount.FirstOrDefault();
                        if (mailAccount == null)
                            throw new Exception("查無Maill帳號");

                        List<PfaSignerList> pfaSignerList = new List<PfaSignerList>();

                        #region 受評員工發信起算天數;間隔天數;逾期天數
                        pfaSignerList = new List<PfaSignerList>();
                        int selfEvaluationSendDay = 0;
                        int selfEvaluationIntervalDay = 0;
                        int selfEvaluationOverdueDay = 0;

                        PfaSystemSetting selfEvaluationParameter = db.SystemSetting
                            .FirstOrDefault(x => x.SettingKey == "SelfEvaluationParameter");

                        if (selfEvaluationParameter != null)
                        {
                            var tempSelfEvaluationParameter = selfEvaluationParameter.SettingValue.Split(';');
                            try
                            {
                                selfEvaluationSendDay = int.Parse(tempSelfEvaluationParameter[0]);
                                selfEvaluationIntervalDay = int.Parse(tempSelfEvaluationParameter[1]);
                                selfEvaluationOverdueDay = int.Parse(tempSelfEvaluationParameter[2]);
                            }
                            catch
                            {
                                throw new Exception("一般設定參數錯誤:受評員工發信起算天數;間隔天數;逾期天數");
                            }

                            var selfEvaluationSendStartDay = now;
                            if (selfEvaluationSendDay > 0)
                                selfEvaluationSendStartDay = selfEvaluationSendStartDay.AddDays(selfEvaluationSendDay - 1);

                            var selfEvaluationSendEndDay = now;
                            if (selfEvaluationOverdueDay > 0)
                                selfEvaluationSendEndDay = selfEvaluationSendEndDay.AddDays(selfEvaluationOverdueDay);

                            if (selfEvaluationSendStartDay > selfEvaluationSendEndDay)
                                throw new Exception("一般設定參數錯誤:受評員工發信起算天數;間隔天數;逾期天數，發信起日大於逾期日");


                            #region 寄信
                            selfEvaluationSendMail(pfaCycleID, db, signFlowResultList, mailAccount, selfEvaluationSendEndDay);
                            #endregion

                            #region PfaSignerList
                            DateTime? firstDate = null;
                            DateTime? secondDate = null;

                            GetFirstDateAndSecondDate(selfEvaluationSendStartDay
                                , selfEvaluationSendEndDay
                                , selfEvaluationIntervalDay
                                , out firstDate
                                , out secondDate);

                            pfaSignerList = signFlowResultList.Select(x => new PfaSignerList
                            {
                                ID = Guid.NewGuid(),
                                PfaCycleID = pfaCycleID,
                                EmployeeID = x.EmployeeID,
                                SignType = PfaSigner_SignType.SelfAssessment,
                                FirstDate = firstDate,
                                SecondDate = secondDate,
                                LastDate = selfEvaluationSendEndDay,
                                IsMeat = true,
                            }).ToList();


                            if (pfaSignerList.Any())
                                db.PfaSignerList.AddRange(pfaSignerList);
                            #endregion
                        }
                        #endregion

                        #region 初核主管發信起算天數;間隔天數;逾期天數
                        pfaSignerList = new List<PfaSignerList>();
                        int firstEvaluationSendDay = 0;
                        int firstEvaluationIntervalDay = 0;
                        int firstEvaluationOverdueDay = 0;
                        var firstEvaluationParameter = db.SystemSetting.FirstOrDefault(x => x.SettingKey == "FirstEvaluationParameter");
                        if (firstEvaluationParameter != null)
                        {
                            var tempFirstEvaluationParameter = firstEvaluationParameter.SettingValue.Split(';');
                            try
                            {
                                firstEvaluationSendDay = int.Parse(tempFirstEvaluationParameter[0]);
                                firstEvaluationIntervalDay = int.Parse(tempFirstEvaluationParameter[1]);
                                firstEvaluationOverdueDay = int.Parse(tempFirstEvaluationParameter[2]);
                            }
                            catch
                            {
                                throw new Exception("一般設定參數錯誤:初核主管發信起算天數;間隔天數;逾期天數");
                            }

                            var firstEvaluationSendStartDay = now;
                            if (firstEvaluationSendDay > 0)
                                firstEvaluationSendStartDay = firstEvaluationSendStartDay.AddDays(firstEvaluationSendDay - 1);
                            var firstEvaluationSendEndDay = now;
                            if (firstEvaluationOverdueDay > 0)
                                firstEvaluationSendEndDay = firstEvaluationSendEndDay.AddDays(firstEvaluationOverdueDay);
                            if (firstEvaluationSendStartDay > firstEvaluationSendEndDay)
                                throw new Exception("一般設定參數錯誤:初核主管發信起算天數;間隔天數;逾期天數，發信起日大於逾期日");


                            #region PfaSignerList
                            DateTime? firstDate = null;
                            DateTime? secondDate = null;

                            GetFirstDateAndSecondDate(firstEvaluationSendStartDay
                                , firstEvaluationSendEndDay
                                , firstEvaluationIntervalDay
                                , out firstDate
                                , out secondDate);

                            var firstEvaluationList = signFlowResultList.SelectMany(x => x.PfaSignProcess)
                                .Where(x => x.IsFirstEvaluation
                                        && !x.IsSecondEvaluation)
                                .GroupBy(x => new {
                                    PreSignEmpID = x.PreSignEmpID
                                    ,
                                    PreSignEmpEmail = x.PreSignEmpEmail
                                })
                                .ToList();

                            pfaSignerList = firstEvaluationList.Select(x => new PfaSignerList
                            {
                                ID = Guid.NewGuid(),
                                PfaCycleID = pfaCycleID,
                                EmployeeID = x.Key.PreSignEmpID,
                                SignType = PfaSigner_SignType.FirstReview,
                                FirstDate = firstDate,
                                SecondDate = firstEvaluationSendEndDay.Date,
                                IsMeat = true,
                            }).ToList();

                            if (pfaSignerList.Any())
                                db.PfaSignerList.AddRange(pfaSignerList);
                            #endregion
                        }
                        #endregion

                        #region 複核主管發信起算天數;間隔天數;逾期天數
                        pfaSignerList = new List<PfaSignerList>();
                        int secondEvaluationSendDay = 0;
                        int secondEvaluationIntervalDay = 0;
                        int secondEvaluationOverdueDay = 0;
                        var secondEvaluationParameter = db.SystemSetting.FirstOrDefault(x => x.SettingKey == "SecondEvaluationParameter");
                        if (secondEvaluationParameter != null)
                        {
                            var tempSecondEvaluationParameter = secondEvaluationParameter.SettingValue.Split(';');
                            try
                            {
                                secondEvaluationSendDay = int.Parse(tempSecondEvaluationParameter[0]);
                                secondEvaluationIntervalDay = int.Parse(tempSecondEvaluationParameter[1]);
                                secondEvaluationOverdueDay = int.Parse(tempSecondEvaluationParameter[2]);
                            }
                            catch
                            {
                                throw new Exception("一般設定參數錯誤:複核主管發信起算天數;間隔天數;逾期天數");
                            }

                            var secondEvaluationSendStartDay = now;
                            if (secondEvaluationSendDay > 0)
                                secondEvaluationSendStartDay = secondEvaluationSendStartDay.AddDays(secondEvaluationSendDay - 1);
                            var secondEvaluationSendEndDay = now;
                            if (secondEvaluationOverdueDay > 0)
                                secondEvaluationSendEndDay = secondEvaluationSendEndDay.AddDays(secondEvaluationOverdueDay);
                            if (secondEvaluationSendStartDay > secondEvaluationSendEndDay)
                                throw new Exception("一般設定參數錯誤:複核主管發信起算天數;間隔天數;逾期天數，發信起日大於逾期日");


                            #region PfaSignerList
                            DateTime? firstDate = null;
                            DateTime? secondDate = null;

                            GetFirstDateAndSecondDate(secondEvaluationSendStartDay
                                , secondEvaluationSendEndDay
                                , secondEvaluationIntervalDay
                                , out firstDate
                                , out secondDate);

                            var secondEvaluationList = signFlowResultList
                                .SelectMany(x => x.PfaSignProcess)
                                .Where(x => x.IsSecondEvaluation
                                        && !x.IsThirdEvaluation)
                                .GroupBy(x => new { PreSignEmpID = x.PreSignEmpID, PreSignEmpEmail = x.PreSignEmpEmail })
                                .ToList();

                            pfaSignerList = secondEvaluationList.Select(x => new PfaSignerList
                            {
                                ID = Guid.NewGuid(),
                                PfaCycleID = pfaCycleID,
                                EmployeeID = x.Key.PreSignEmpID,
                                SignType = PfaSigner_SignType.SecondaryReview,
                                FirstDate = firstDate,
                                SecondDate = secondEvaluationSendEndDay.Date,
                                IsMeat = true,
                            }).ToList();

                            if (pfaSignerList.Any())
                                db.PfaSignerList.AddRange(pfaSignerList);
                            #endregion
                        }
                        #endregion


                        #region 核決主管發信起算天數;間隔天數;逾期天數
                        pfaSignerList = new List<PfaSignerList>();
                        int thirdEvaluationSendDay = 0;
                        int thirdEvaluationIntervalDay = 0;
                        int thirdEvaluationOverdueDay = 0;
                        var thirdEvaluationParameter = db.SystemSetting.FirstOrDefault(x => x.SettingKey == "ThirdEvaluationParameter");
                        if (thirdEvaluationParameter != null)
                        {
                            var tempThirdEvaluationParameter = thirdEvaluationParameter.SettingValue.Split(';');
                            try
                            {
                                thirdEvaluationSendDay = int.Parse(tempThirdEvaluationParameter[0]);
                                thirdEvaluationIntervalDay = int.Parse(tempThirdEvaluationParameter[1]);
                                thirdEvaluationOverdueDay = int.Parse(tempThirdEvaluationParameter[2]);
                            }
                            catch
                            {
                                throw new Exception("一般設定參數錯誤:複核主管發信起算天數;間隔天數;逾期天數");
                            }

                            var thirdEvaluationSendStartDay = now;
                            if (thirdEvaluationSendDay > 0)
                                thirdEvaluationSendStartDay = thirdEvaluationSendStartDay.AddDays(thirdEvaluationSendDay - 1);
                            var thirdEvaluationSendEndDay = now;
                            if (thirdEvaluationOverdueDay > 0)
                                thirdEvaluationSendEndDay = thirdEvaluationSendEndDay.AddDays(thirdEvaluationOverdueDay);
                            if (thirdEvaluationSendStartDay > thirdEvaluationSendEndDay)
                                throw new Exception("一般設定參數錯誤:複核主管發信起算天數;間隔天數;逾期天數，發信起日大於逾期日");


                            #region PfaSignerList
                            DateTime? firstDate = null;
                            DateTime? thirdDate = null;

                            GetFirstDateAndSecondDate(thirdEvaluationSendStartDay
                                , thirdEvaluationSendEndDay
                                , thirdEvaluationIntervalDay
                                , out firstDate
                                , out thirdDate);

                            var thirdEvaluationList = signFlowResultList
                                .SelectMany(x => x.PfaSignProcess)
                                .Where(x => x.IsThirdEvaluation)
                                .GroupBy(x => new { PreSignEmpID = x.PreSignEmpID, PreSignEmpEmail = x.PreSignEmpEmail })
                                .ToList();

                            pfaSignerList = thirdEvaluationList.Select(x => new PfaSignerList
                            {
                                ID = Guid.NewGuid(),
                                PfaCycleID = pfaCycleID,
                                EmployeeID = x.Key.PreSignEmpID,
                                SignType = PfaSigner_SignType.FinalApproval,
                                FirstDate = firstDate,
                                SecondDate = thirdEvaluationSendEndDay.Date,
                                IsMeat = true,
                            }).ToList();

                            if (pfaSignerList.Any())
                                db.PfaSignerList.AddRange(pfaSignerList);
                            #endregion
                        }
                        #endregion


                        db.SaveChanges();
                        #endregion

                        result.success = true;
                        result.message = "啟動送簽成功";
                        result.log = string.Format("啟動送簽成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = string.Format("啟動送簽失敗,{0}", ex.Message);
                        result.log = string.Format("啟動送簽失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }

        private static void selfEvaluationSendMail(Guid pfaCycleID, NewHRPortalEntitiesDDMC_PFA db, List<PfaSignProcessDataViewModel> signFlowResultList, PfaMailAccount mailAccount, DateTime selfEvaluationSendEndDay)
        {
            var selfEvaluationMM = selfEvaluationSendEndDay.AddDays(-1).ToString("MM");
            var selfEvaluationDD = selfEvaluationSendEndDay.AddDays(-1).ToString("dd");
            var pfaMailMessageList = signFlowResultList.Select(x => new PfaMailMessage
            {
                ID = Guid.NewGuid(),
                PfaCycleID = pfaCycleID,
                EmployeeID = x.EmployeeID,
                SourceType = "4",
                FromAccountID = mailAccount.ID,
                Rcpt = x.EmpEmail,
                Subject = "績效考核通知",
                Body = string.Format("提醒您，考績表需於{0}月{1}日自評完畢，謝謝。", selfEvaluationMM, selfEvaluationDD),
                IsHtml = false,
                IsCancel = false,
                HadSend = false,
                CreateTime = DateTime.Now,
            });
            if (pfaMailMessageList.Any())
                db.PfaMailMessage.AddRange(pfaMailMessageList);
        }

        private void GetFirstDateAndSecondDate(DateTime selfEvaluationSendStartDay, DateTime selfEvaluationSendEndDay, int selfEvaluationIntervalDay, out DateTime? firstDate, out DateTime? secondDate)
        {
            firstDate = null;
            secondDate = null;

            List<DateTime> tempDateList = GenerateEvaluationDates(selfEvaluationSendStartDay
                , selfEvaluationSendEndDay
                , selfEvaluationIntervalDay);

            for (int i = 0; i < tempDateList.Count; i++)
            {
                if (i == 0)
                    firstDate = tempDateList[i];
                else if (i == 1)
                {
                    secondDate = tempDateList[i];
                    break;
                }
            }
        }

        private List<DateTime> GenerateEvaluationDates(DateTime startDay, DateTime endDay, int intervalDay)
        {
            List<DateTime> tempDateList = new List<DateTime>();

            if (intervalDay > 0)
            {
                for (var date = startDay.Date;
                     date <= endDay.Date;
                     date = date.AddDays(intervalDay).Date)
                {
                    if (date != startDay.Date && date < endDay.Date)
                    {
                        tempDateList.Add(date);
                    }
                }
            }

            return tempDateList;
        }

    }
}