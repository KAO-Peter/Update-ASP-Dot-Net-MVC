using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaDeptEmpService : BaseCrudService<PfaDeptEmp>
    {
        public PfaDeptEmpService(HRPortal_Services services) : base(services)
        {
        }

        public Result SavePfaDeptEmp(Guid txtPfaDeptID, string selEmps)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region Update PfaDeptEmp
                        if (!string.IsNullOrEmpty(selEmps))
                        {
                            string[] selEmpList = selEmps.Split('|');
                            foreach (var item in selEmpList)
                            {
                                Guid employeeID = Guid.Parse(item);
                                var PfaDeptEmp = db.PfaDeptEmp.Where(x => x.EmployeeID == employeeID).FirstOrDefault();
                                if (PfaDeptEmp != null)
                                {
                                    if (PfaDeptEmp.PfaDeptID != txtPfaDeptID)
                                    {
                                        PfaDeptEmp.PfaDeptID = txtPfaDeptID;
                                        db.SaveChanges();
                                    }
                                }
                                else
                                {
                                    db.PfaDeptEmp.Add(new PfaDeptEmp
                                    {
                                        ID = Guid.NewGuid(),
                                        EmployeeID = employeeID,
                                        PfaDeptID = txtPfaDeptID,
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }
                        #endregion

                        result.success = true;
                        result.message = "選擇人員成功";
                        result.log = string.Format("選擇人員成功,PfaDeptID:{0}", txtPfaDeptID);

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "選擇人員失敗";
                        result.log = string.Format("選擇人員失敗,PfaDeptID:{0},Data:{1},Message:{2},{3}", txtPfaDeptID, selEmps, ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}