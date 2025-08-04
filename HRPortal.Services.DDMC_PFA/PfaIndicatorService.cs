using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaIndicatorService : BaseCrudService<PfaIndicator>
    {
        public PfaIndicatorService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaIndicator> GetPfaIndicatorData(Guid companyID, string PfaIndicatorCode, string PfaIndicatorName)
        {
            var result = GetAll();
            if (companyID != Guid.Empty)
            {
                result = result.Where(x => x.CompanyID == companyID);
            }
            if (!string.IsNullOrEmpty(PfaIndicatorCode))
            {
                result = result.Where(x => x.PfaIndicatorCode.Contains(PfaIndicatorCode));
            }
            if (!string.IsNullOrEmpty(PfaIndicatorName))
            {
                result = result.Where(x => x.PfaIndicatorName.Contains(PfaIndicatorName));
            }
            return result.OrderBy(x => x.CompanyID).ThenBy(x => x.Ordering).ThenBy(x => x.PfaIndicatorCode).ToList();
        }

        public bool IsRepeat(string Code)
        {
            return Where(x => x.PfaIndicatorCode == Code).Any();
        }

        public PfaIndicator GetPfaIndicator(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }

        public Result CreatePfaIndicator(PfaIndicator data, List<PfaIndicatorDetail> detail)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        db.PfaIndicator.Add(data);

                        foreach (var item in detail)
                        {
                            db.PfaIndicatorDetail.Add(item);
                            db.SaveChanges();
                        }

                        result.success = true;
                        result.message = "新增成功";
                        result.log = string.Format("新增成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "新增失敗";
                        result.log = string.Format("新增失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }

        public Result EditPfaIndicator(PfaIndicator data, List<PfaIndicatorDetail> detail)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var obj = db.PfaIndicator.Where(x => x.ID == data.ID).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.PfaIndicatorName = data.PfaIndicatorName;
                            obj.Description = data.Description;
                            obj.Scale = data.Scale;
                            obj.Ordering = data.Ordering;
                            obj.ModifiedBy = data.ModifiedBy;
                            obj.ModifiedTime = data.ModifiedTime;

                            db.SaveChanges();
                        }

                        #region Delete Old PfaIndicatorDetail
                        var oldIndicatorDetail = db.PfaIndicatorDetail.Where(x => x.PfaIndicatorID == data.ID);
                        db.PfaIndicatorDetail.RemoveRange(oldIndicatorDetail);
                        db.SaveChanges();
                        #endregion

                        #region Add New PfaIndicatorDetail
                        foreach (var item in detail)
                        {
                            db.PfaIndicatorDetail.Add(item);
                            db.SaveChanges();
                        }
                        #endregion

                        result.success = true;
                        result.message = "修改成功";
                        result.log = string.Format("修改成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "修改失敗";
                        result.log = string.Format("修改失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }

        public Result DeletePfaIndicator(Guid ID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region PfaIndicatorDetail
                        var IndicatorDetail = db.PfaIndicatorDetail.Where(x => x.PfaIndicatorID == ID);
                        db.PfaIndicatorDetail.RemoveRange(IndicatorDetail);
                        db.SaveChanges();
                        #endregion

                        #region PfaIndicator
                        var PfaIndicator = db.PfaIndicator.Where(x => x.ID == ID);
                        db.PfaIndicator.RemoveRange(PfaIndicator);
                        db.SaveChanges();
                        #endregion

                        result.success = true;
                        result.message = "刪除成功";
                        result.log = string.Format("刪除成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "刪除失敗";
                        result.log = string.Format("刪除失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}