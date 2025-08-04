using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaRatioJobTitleService : BaseCrudService<PfaRatioJobTitle>
    {
        public PfaRatioJobTitleService(HRPortal_Services services) : base(services)
        {
        }

        public Result SaveJobTitle(string SelJboTitleID, Guid EmployeeID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region Delete Old PfaRatioJobTitle
                        var oldPfaRatioJobTitle = db.PfaRatioJobTitle;
                        db.PfaRatioJobTitle.RemoveRange(oldPfaRatioJobTitle);
                        db.SaveChanges();
                        #endregion

                        #region Add New PfaRatioJobTitle
                        if (!string.IsNullOrEmpty(SelJboTitleID))
                        {
                            string[] SelDeptList = SelJboTitleID.Split('|');
                            foreach (var item in SelDeptList)
                            {
                                Guid TitleID = Guid.Parse(item);

                                PfaRatioJobTitle obj = new PfaRatioJobTitle();
                                obj.ID = Guid.NewGuid();
                                obj.JobTitleID = TitleID;
                                obj.CreatedBy = EmployeeID;
                                obj.CreatedTime = DateTime.Now;

                                db.PfaRatioJobTitle.Add(obj);
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        result.success = true;
                        result.message = "適用職稱儲存成功";
                        result.log = string.Format("適用職稱儲存成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "適用職稱儲存失敗";
                        result.log = string.Format("適用職稱儲存失敗,Message:{1},{2}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}