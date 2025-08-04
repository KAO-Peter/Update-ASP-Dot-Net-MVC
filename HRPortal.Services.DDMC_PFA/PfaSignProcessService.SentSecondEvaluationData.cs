

using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System.Collections.Generic;
using System.Transactions;
using System;
using System.Linq;
using System.Data.Entity;
using PFA_Common = HRPortal.Services.DDMC_PFA.Common;
using HRPortal.Services.DDMC_PFA.Common;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaSignProcessService
    {

        /// <summary>
        /// 績效考核複核-送出
        /// </summary>
        /// <param name="model"></param>
        /// <param name="signEmpID"></param>
        /// <returns></returns>
        public Result SentSecondEvaluationData(List<PfaCycleRation> model, Guid signEmpID)
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
                            throw new Exception("無送出部門類別資料");


                        #region Mail
                        var mailAppSetting = PFA_Common.AppSettingsMail.Get();
                        PfaMailAccount _mailAccount = db.MailAccount.FirstOrDefault();
                        #endregion

                        var pfaCycleID = model.FirstOrDefault().PfaCycleID;
                        List<PfaMailMessage> pfaMailMessages = new List<PfaMailMessage>();
                        foreach (var data in model)
                        {
                            List<PfaSignProcess> pfaSignProcessList = (from pce in db.PfaCycleEmp
                                          join psp in db.PfaSignProcess on pce.ID equals psp.PfaCycleEmpID into pspGroup
                                          from psp in pspGroup.DefaultIfEmpty()
                                          where pce.PfaCycleID == data.PfaCycleID
                                          && pce.PfaOrgID == data.PfaOrgID
                                          && (psp == null || (psp.Status == PfaSignProcess_Status.Reviewed
                                                              && psp.IsRatio
                                                              && psp.IsSecondEvaluation == true))
                                          select psp
                                          )
                                          .ToList();
                              
                            foreach (var pfaSignProcess in pfaSignProcessList)
                            {
                                pfaSignProcess.Status = PfaSignProcess_Status.Submitted; // e: 已送出
                                pfaSignProcess.ConfirmTime = pfaSignProcess.ConfirmTime.HasValue ? pfaSignProcess.ConfirmTime.Value : DateTime.Now;
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
                            }

                            #region 績效考核人數配比設定
                            var pfaCycleRation = db.PfaCycleRation
                                .FirstOrDefault(x => x.PfaCycleID == pfaCycleID
                                && x.PfaOrgID == data.PfaOrgID);

                            if (pfaCycleRation != null)
                            {
                                pfaCycleRation.FinalRation = data.FinalRation; // 最後核對試算數
                                pfaCycleRation.IsRation = true; // 是否配比正確

                                var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.ToList();

                                foreach (var pfaCycleRationDetail in pfaCycleRationDetailList)
                                {
                                    // 最後核對試算數 = 組織已複評人數(SecondFinal) * Multiplier
                                    pfaCycleRationDetail.FinalRation =
                                        pfaCycleRationDetail.SecondFinal * pfaCycleRationDetail.Multiplier;
                                }
                            }
                            #endregion

                            #region 簽核流程-尋找下一關
                            
                            foreach (var pfaSignProcess in pfaSignProcessList)
                            {
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
                                        var subject = "核決";
                                        mailMessage.Subject = string.Format(mailAppSetting.Subject, subject);
                                        mailMessage.Body = string.Format(mailAppSetting.Message, subject, "", mailAppSetting.PortalWebUrl);
                                        pfaMailMessages.Add(mailMessage);
                                    }
                                    #endregion
                                }
                            }

                            
                            #endregion
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

                        #region 把待評核改成已評核，已評核成績沿用複核的成績
                        /*
                         * 送達 核決 之後，把待評核改成待核決，待核決成績沿用複核的成績
                         */
                        DateTime dNow = DateTime.Now;
                        foreach (var data in model)
                        {
                            var pfaCycleEmps = db.PfaCycleEmp.Where(r =>
                                   r.PfaCycleID == data.PfaCycleID
                                && r.PfaOrgID == data.PfaOrgID
                                && r.PfaSignProcess.Any(x=>x.IsThirdEvaluation == true && x.IsRatio == true)
                            ).ToList();

                            if (pfaCycleEmps.Any() == false)
                            {
                                continue;
                            }

                            var pfaSignProcesss = db.PfaSignProcess.Where(r =>
                                   r.PfaCycleEmp.PfaCycleID == data.PfaCycleID
                                && r.PfaCycleEmp.PfaOrgID == data.PfaOrgID
                                && r.IsThirdEvaluation == true && r.IsRatio == true
                            ).ToList();

                            var pfaCycleRations = db.PfaCycleRation.Where(r =>
                                   r.PfaCycleID == data.PfaCycleID
                                && r.PfaOrgID == data.PfaOrgID
                            ).ToList();

                            var pfaCycleRationDetails = db.PfaCycleRationDetail.Where(r =>
                                   r.PfaCycleRation.PfaCycleID == data.PfaCycleID
                                && r.PfaCycleRation.PfaOrgID == data.PfaOrgID
                            ).ToList();


                            foreach (var itemCE in pfaCycleEmps)
                            {
                                itemCE.PfaFinalScore = itemCE.PfaLastScore;
                                itemCE.FinalPerformance_ID = itemCE.LastPerformance_ID;
                            }

                            foreach (var itemSP in pfaSignProcesss)
                            {
                                itemSP.Status = PfaSignProcess_Status.PendingThirdReview;
                                itemSP.ConfirmTime = dNow;
                            }

                            foreach (var itemCR in pfaCycleRations)
                            {
                                if (itemCR.SecondFinal.HasValue)
                                {
                                    itemCR.ThirdFinal = itemCR.SecondFinal.Value;
                                }
                                else if (itemCR.FirstFinal.HasValue)
                                {
                                    itemCR.SecondFinal = itemCR.FirstFinal.Value;
                                    itemCR.ThirdFinal = itemCR.SecondFinal.Value;
                                }
                            }

                            foreach (var itemCRD in pfaCycleRationDetails)
                            {
                                if (itemCRD.SecondFinal.HasValue)
                                {
                                    itemCRD.ThirdFinal = itemCRD.SecondFinal.Value;
                                }
                                else if (itemCRD.FirstFinal.HasValue)
                                {
                                    itemCRD.SecondFinal = itemCRD.FirstFinal.Value;
                                    itemCRD.ThirdFinal = itemCRD.SecondFinal.Value;
                                }
                            }

                        }

                        #endregion
                        
                        db.SaveChanges();

                        result.success = true;
                        result.message = "簽核完成";
                        result.log = "簽核完成";

                        //完成交易
                        tx.Complete();

                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "送出失敗";
                        if (!string.IsNullOrEmpty(mailMsg))
                            result.message = result.message + ":" + mailMsg;
                        result.log = ExceptionHelper.GetMsg(ex);
                    }
                }
            }
            return result;
        }
    }
}