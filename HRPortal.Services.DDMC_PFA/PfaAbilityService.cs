using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaAbilityService : BaseCrudService<PfaAbility>
    {
        public PfaAbilityService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaAbility> GetPfaAbilityData(Guid companyID, Guid EmpTypeID, string AbilityCode, string AbilityName)
        {
            var result = GetAll();
            if (companyID != Guid.Empty)
            {
                result = result.Where(x => x.CompanyID == companyID);
            }
            if (EmpTypeID != Guid.Empty)
            {
                result = result.Where(x => x.PfaEmpTypeID == EmpTypeID);
            }
            if (!string.IsNullOrEmpty(AbilityCode))
            {
                result = result.Where(x => x.PfaAbilityCode.Contains(AbilityCode));
            }
            if (!string.IsNullOrEmpty(AbilityName))
            {
                result = result.Where(x => x.PfaAbilityName.Contains(AbilityName));
            }
            return result.OrderBy(x => x.CompanyID).ThenBy(x => x.Ordering).ThenBy(x => x.PfaAbilityCode).ToList();
        }

        public bool IsExist(Guid id)
        {
            return Where(x => x.PfaEmpTypeID == id).Any();
        }

        public bool IsRepeat(string Code)
        {
            return Where(x => x.PfaAbilityCode == Code).Any();
        }

        public PfaAbility GetPfaAbility(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }

        public Result CreatePfaAbility(PfaAbility data, List<PfaAbilityDetail> detail)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        db.PfaAbility.Add(data);

                        foreach (var item in detail)
                        {
                            db.PfaAbilityDetail.Add(item);
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

        public Result EditPfaAbility(PfaAbility data, List<PfaAbilityDetail> detail)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var obj = db.PfaAbility.Where(x => x.ID == data.ID).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.PfaAbilityName = data.PfaAbilityName;
                            obj.Description = data.Description;
                            obj.TotalScore = data.TotalScore;
                            obj.Ordering = data.Ordering;
                            obj.ModifiedBy = data.ModifiedBy;
                            obj.ModifiedTime = data.ModifiedTime;

                            db.SaveChanges();
                        }

                        #region Delete Old PfaAbilityDetail
                        var oldPfaAbilityDetail = db.PfaAbilityDetail.Where(x => x.PfaAbilityID == data.ID);
                        db.PfaAbilityDetail.RemoveRange(oldPfaAbilityDetail);
                        db.SaveChanges();
                        #endregion

                        #region Add New PfaAbilityDetail
                        foreach (var item in detail)
                        {
                            db.PfaAbilityDetail.Add(item);
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

        public Result DeletePfaAbility(Guid ID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region PfaAbilityDetail
                        var PfaAbilityDetail = db.PfaAbilityDetail.Where(x => x.PfaAbilityID == ID);
                        db.PfaAbilityDetail.RemoveRange(PfaAbilityDetail);
                        db.SaveChanges();
                        #endregion

                        #region PfaAbility
                        var PfaAbility = db.PfaAbility.Where(x => x.ID == ID);
                        db.PfaAbility.RemoveRange(PfaAbility);
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