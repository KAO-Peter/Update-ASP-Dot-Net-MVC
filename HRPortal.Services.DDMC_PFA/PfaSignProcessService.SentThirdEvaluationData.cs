

using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System.Collections.Generic;
using System.Transactions;
using System;
using System.Linq;
using System.Data.Entity;
using HRPortal.Services.DDMC_PFA.Common;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaSignProcessService
    {

        /// <summary>
        /// 績效考核核決-送出
        /// </summary>
        /// <param name="model"></param>
        /// <param name="signEmpID"></param>
        /// <returns></returns>
        public Result SentThirdEvaluationData(List<PfaCycleRation> model, Guid signEmpID)
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

                        var pfaCycleID = model.FirstOrDefault().PfaCycleID;
                        foreach (var data in model)
                        {
                            List<PfaSignProcess> pfaSignProcessList = (from pce in db.PfaCycleEmp
                                          join psp in db.PfaSignProcess on pce.ID equals psp.PfaCycleEmpID into pspGroup
                                          from psp in pspGroup.DefaultIfEmpty()
                                          where pce.PfaCycleID == data.PfaCycleID
                                          && pce.PfaOrgID == data.PfaOrgID
                                          && (psp == null || (psp.Status == PfaSignProcess_Status.Reviewed
                                                              && psp.IsRatio
                                                              && psp.IsThirdEvaluation == true)
                                                          || (psp.Status == PfaSignProcess_Status.PendingThirdReview
                                                              && psp.IsRatio
                                                              && psp.IsThirdEvaluation == true))
                                          select psp
                                          )
                                          .Include(r=>r.PfaCycleEmp)
                                          .ToList();
                              
                            foreach (var pfaSignProcess in pfaSignProcessList)
                            {
                                pfaSignProcess.Status = PfaSignProcess_Status.Submitted; // e: 已送出
                                pfaSignProcess.ConfirmTime = pfaSignProcess.ConfirmTime.HasValue ? pfaSignProcess.ConfirmTime.Value : DateTime.Now;
                                pfaSignProcess.SignEmpID = signEmpID;
                                pfaSignProcess.SignTime = DateTime.Now;

                                pfaSignProcess.PfaCycleEmp.Status = PfaCycleEmp_Status.Approved; // e: 已審核
                                pfaSignProcess.PfaCycleEmp.ModifiedBy = signEmpID;
                                pfaSignProcess.PfaCycleEmp.ModifiedTime = DateTime.Now;

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
                                    // 最後核對試算數 = 組織已核評人數(ThirdFinal) * Multiplier
                                    pfaCycleRationDetail.FinalRation =
                                        pfaCycleRationDetail.ThirdFinal * pfaCycleRationDetail.Multiplier;
                                }
                            }
                            #endregion
                        }
                        db.SaveChanges();

                        #region 檢核考評完成
                        // 未完成狀態
                        string[] notFinstatus = { PfaCycleEmp_Status.NotSubmittedForApproval
                                , PfaCycleEmp_Status.InApprovalProcess }; // m: 未送簽 a:簽核中 
                        var tempPfaCycleEmp = db.PfaCycleEmp
                            .FirstOrDefault(x => x.PfaCycleID == pfaCycleID
                            && notFinstatus.Contains(x.Status));

                        if (tempPfaCycleEmp == null)
                        {
                            var pfaCycle = db.PfaCycle.FirstOrDefault(x => x.ID == pfaCycleID);
                            pfaCycle.Status = PfaCycle_Status.PfaFinish;
                            pfaCycle.ModifiedBy = signEmpID;
                            pfaCycle.ModifiedTime = DateTime.Now;
                            db.SaveChanges();
                        }
                        #endregion

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