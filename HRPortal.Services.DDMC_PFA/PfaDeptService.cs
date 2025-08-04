using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaDeptService : BaseCrudService<PfaDept>
    {
        public PfaDeptService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaDept> GetPfaDeptData(Guid companyID, Guid DepartmentID, Guid deptClassID)
        {
            var result = GetAll();
            if (companyID != Guid.Empty)
            {
                result = result.Where(x => x.CompanyID == companyID);
            }
            if (DepartmentID != Guid.Empty)
            {
                result = result.Where(x => x.ID == DepartmentID);
            }
            if (deptClassID != Guid.Empty)
            {
                result = result.Where(x => x.DeptClassID == deptClassID);
            }
            return result.OrderBy(x => x.CompanyID).ThenBy(x => x.PfaDeptCode).ToList();
        }

        public PfaDept GetPfaDept(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }

        public bool IsExist(string code)
        {
            return Where(x => x.PfaDeptCode == code).Any();
        }

        public Result DelPfaDept(Guid PfaDeptID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region Delete PfaDeptEmp
                        var PfaDeptEmp = db.PfaDeptEmp.Where(x => x.PfaDeptID == PfaDeptID);
                        foreach (var item in PfaDeptEmp)
                        {
                            db.PfaDeptEmp.Remove(item);
                        }
                        db.SaveChanges();
                        #endregion

                        #region Delete PfaDept
                        var PfaDept = db.PfaDept.Where(x => x.ID == PfaDeptID);
                        foreach (var item in PfaDept)
                        {
                            db.PfaDept.Remove(item);
                        }
                        db.SaveChanges();
                        #endregion

                        result.success = true;
                        result.message = "刪除成功";
                        result.log = string.Format("刪除成功,ID:{0}", PfaDeptID);

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "刪除失敗";
                        result.log = string.Format("刪除失敗,ID:{0},Message:{1},{2}", PfaDeptID, ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}