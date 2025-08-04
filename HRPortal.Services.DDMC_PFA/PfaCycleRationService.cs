using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Common;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaCycleRationService : BaseCrudService<PfaCycleRation>
    {
        public PfaCycleRationService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaCycleRation> GetData(Guid companyID, Guid CycleID, Guid OrgID)
        {
            var result = GetAll();
            if (CycleID != Guid.Empty)
            {
                result = result.Where(x => x.PfaCycleID == CycleID);
            }
            if (OrgID != Guid.Empty)
            {
                result = result.Where(x => x.PfaOrgID == OrgID);
            }
            return result.OrderBy(x => x.PfaCycle.PfaFormNo).ToList();
        }

        public PfaCycleRation GetPfaCycleRation(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }

        public Result CreatePfaCycleRation(PfaCycleRation data, List<PfaCycleRationDetail> detail)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        db.PfaCycleRation.Add(data);

                        foreach (var item in detail)
                        {
                            db.PfaCycleRationDetail.Add(item);
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
                        result.log = string.Format("新增失敗,Message:{0},{1}", ExceptionHelper.GetMsg(ex), ex.StackTrace);
                    }
                }
            }
            return result;
        }

        public Result EditPfaCycleRation(PfaCycleRation data, List<PfaCycleRationDetail> detail)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var obj = db.PfaCycleRation.Where(x => x.ID == data.ID).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.ModifiedBy = data.ModifiedBy;
                            obj.ModifiedTime = data.ModifiedTime;

                            db.SaveChanges();
                        }

                        foreach (var item in detail)
                        {
                            var obj2 = db.PfaCycleRationDetail.Where(x => x.ID == item.ID).FirstOrDefault();
                            if (obj2 != null)
                            {
                                obj2.Staffing = item.Staffing;
                                obj2.ModifiedBy = data.ModifiedBy;
                                obj2.ModifiedTime = data.ModifiedTime;
                            }
                            db.SaveChanges();
                        }

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

        public Result DeletePfaCycleRation(Guid ID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region PfaCycleRationDetail
                        var PfaCycleRationDetail = db.PfaCycleRationDetail.Where(x => x.PfaCycleRationID == ID);
                        db.PfaCycleRationDetail.RemoveRange(PfaCycleRationDetail);
                        db.SaveChanges();
                        #endregion

                        #region PfaCycleRation
                        var PfaCycleRation = db.PfaCycleRation.Where(x => x.ID == ID);
                        db.PfaCycleRation.RemoveRange(PfaCycleRation);
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
