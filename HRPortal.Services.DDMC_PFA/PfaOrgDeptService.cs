using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaOrgDeptService : BaseCrudService<PfaOrgDept>
    {
        public PfaOrgDeptService(HRPortal_Services services) : base(services)
        {
        }

        public List<Guid> GetDeptID(Guid id)
        {
            return GetAll().Where(x => x.PfaOrgID == id).Select(x => x.PfaDeptID).ToList();
        }

        public Result SaveOrgDept(Guid PfaOrgID, string SelDeptID, Guid employeeID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region 把部門代碼清掉
                        if (!string.IsNullOrEmpty(SelDeptID))
                        {
                            string[] SelDeptList = SelDeptID.Split('|');
                            foreach (var item in SelDeptList)
                            {
                                Guid DeptID = Guid.Parse(item);
                                var DeleteList = db.PfaOrgDept.Where(x => x.PfaOrgID != PfaOrgID && x.PfaDeptID == DeptID).ToList();
                                foreach (var item2 in DeleteList)
                                {
                                    db.PfaOrgDept.Remove(item2);
                                }
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        #region Delete Old PfaOrgDept
                        var oldPfaOrgDeptList = db.PfaOrgDept.Where(x => x.PfaOrgID == PfaOrgID).ToList();
                        foreach (var item in oldPfaOrgDeptList)
                        {
                            db.PfaOrgDept.Remove(item);
                        }
                        db.SaveChanges();
                        #endregion

                        #region Add New PfaOrgDept
                        if (!string.IsNullOrEmpty(SelDeptID))
                        {
                            string[] SelDeptList = SelDeptID.Split('|');
                            foreach (var item in SelDeptList)
                            {
                                Guid DeptID = Guid.Parse(item);

                                PfaOrgDept obj = new PfaOrgDept();
                                obj.ID = Guid.NewGuid();
                                obj.PfaOrgID = PfaOrgID;
                                obj.PfaDeptID = DeptID;
                                obj.CreatedBy = employeeID;
                                obj.CreatedTime = DateTime.Now;

                                db.PfaOrgDept.Add(obj);
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        result.success = true;
                        result.message = "適用部門儲存成功";
                        result.log = string.Format("適用部門儲存成功,PfaOrgID:{0}", PfaOrgID);

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "適用部門儲存失敗";
                        result.log = string.Format("適用部門儲存失敗,PfaOrgID:{0},Message:{1},{2}", PfaOrgID, ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}