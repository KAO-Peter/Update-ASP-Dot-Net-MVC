using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Common;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaCycleService : BaseCrudService<PfaCycle>
    {
        PfaSignFlowService pfaSignFlowService;

        public PfaCycleService(HRPortal_Services services) : base(services)
        {
            pfaSignFlowService = new PfaSignFlowService(services);
        }

        public List<PfaCycle> GetPfaCycleData(string formNo, Guid companyID)
        {
            var data = GetAll().Where(x=> x.CompanyID == companyID);

            if (!string.IsNullOrEmpty(formNo))
                data = data.Where(x => x.PfaFormNo.Contains(formNo));
            return data.OrderByDescending(x => x.PfaFormNo).ThenBy(x => x.PfaYear).ToList();
        }

        public bool IsExist(string FormNo, Guid companyID)
        {
            return Where(x => x.PfaFormNo == FormNo && x.CompanyID == companyID).Any();
        }

        public bool IsExist(Guid Id)
        {
            return Where(x => x.ID == Id).Any();
        }

        /// <summary>
        /// 取得績效考核簽核批號
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public PfaCycle GetPfaCycle(Guid Id)
        {
            return Where(x => x.ID == Id).FirstOrDefault();
        }

        /// <summary>
        /// 新增績效考核簽核批號
        /// </summary>
        /// <param name="pfaCycle"></param>
        /// <param name="pfaCycleEmp"></param>
        /// <returns></returns>
        public Result CreatePfaCycle(PfaCycle pfaCycle, List<PfaCycleEmp> pfaCycleEmp)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                try
                {
                    db.PfaCycle.Add(pfaCycle);
                    db.PfaCycleEmp.AddRange(pfaCycleEmp);
                    db.SaveChanges();

                    result.success = true;
                    result.message = "新增成功";
                    result.log = string.Format("新增成功");
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.message = "新增失敗";
                    result.log = string.Format("新增失敗,Message:{0},{1}", ExceptionHelper.GetMsg(ex), ex.StackTrace);
                }
            }

            return result;
        }

        /// <summary>
        /// 編輯績效考核簽核批號
        /// </summary>
        /// <param name="pfaCycle"></param>
        /// <param name="pfaCycleEmps"></param>
        /// <returns></returns>
        public Result EditPfaCycle(PfaCycle pfaCycle, List<PfaCycleEmp> pfaCycleEmps)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var deletePfaCycleEmpList = new List<PfaCycleEmp>();
                        var oldPfaCycleEmpList = db.PfaCycleEmp.Where(x => x.PfaCycleID == pfaCycle.ID).ToList();
                        if (oldPfaCycleEmpList.Any())
                        {
                            foreach (var oldPfaCycleEmp in oldPfaCycleEmpList)
                            {
                                var newPfaCycleEmp = pfaCycleEmps.FirstOrDefault(x => x.EmployeeID == oldPfaCycleEmp.EmployeeID);
                                if (newPfaCycleEmp != null)
                                {
                                    #region 更新
                                    oldPfaCycleEmp.PfaDeptID = newPfaCycleEmp.PfaDeptID;
                                    oldPfaCycleEmp.PfaOrgID = newPfaCycleEmp.PfaOrgID;
                                    oldPfaCycleEmp.HireID = newPfaCycleEmp.HireID;
                                    oldPfaCycleEmp.JobTitleID = newPfaCycleEmp.JobTitleID;
                                    oldPfaCycleEmp.JobFunctionID = newPfaCycleEmp.JobFunctionID;
                                    oldPfaCycleEmp.GradeID = newPfaCycleEmp.GradeID;
                                    oldPfaCycleEmp.PositionID = newPfaCycleEmp.PositionID;
                                    oldPfaCycleEmp.Education = newPfaCycleEmp.Education;
                                    oldPfaCycleEmp.SchoolName = newPfaCycleEmp.SchoolName;
                                    oldPfaCycleEmp.DeptDescription = newPfaCycleEmp.DeptDescription;
                                    oldPfaCycleEmp.Degree = newPfaCycleEmp.Degree;
                                    oldPfaCycleEmp.PersonalLeave = newPfaCycleEmp.PersonalLeave;
                                    oldPfaCycleEmp.SickLeave = newPfaCycleEmp.SickLeave;
                                    oldPfaCycleEmp.LateLE = newPfaCycleEmp.LateLE;
                                    oldPfaCycleEmp.AWL = newPfaCycleEmp.AWL;
                                    oldPfaCycleEmp.Salary01 = newPfaCycleEmp.Salary01;
                                    oldPfaCycleEmp.Salary02 = newPfaCycleEmp.Salary02;
                                    oldPfaCycleEmp.Salary03 = newPfaCycleEmp.Salary03;
                                    oldPfaCycleEmp.Salary04 = newPfaCycleEmp.Salary04;
                                    oldPfaCycleEmp.Salary05 = newPfaCycleEmp.Salary05;
                                    oldPfaCycleEmp.Salary06 = newPfaCycleEmp.Salary06;
                                    oldPfaCycleEmp.FullSalary = newPfaCycleEmp.FullSalary;
                                    oldPfaCycleEmp.SignTypeID = newPfaCycleEmp.SignTypeID;
                                    oldPfaCycleEmp.PfaEmpTypeID = newPfaCycleEmp.PfaEmpTypeID;
                                    oldPfaCycleEmp.IsAgent = newPfaCycleEmp.IsAgent;
                                    oldPfaCycleEmp.IsRatio = newPfaCycleEmp.IsRatio;
                                    oldPfaCycleEmp.Status = newPfaCycleEmp.Status;
                                    oldPfaCycleEmp.ModifiedBy = newPfaCycleEmp.ModifiedBy;
                                    oldPfaCycleEmp.ModifiedTime = newPfaCycleEmp.ModifiedTime;
                                    #endregion
                                    pfaCycleEmps.Remove(newPfaCycleEmp);
                                }
                                else
                                    deletePfaCycleEmpList.Add(oldPfaCycleEmp);
                            }
                        }
                        if (deletePfaCycleEmpList.Any())
                            db.PfaCycleEmp.RemoveRange(deletePfaCycleEmpList);
                        if (pfaCycleEmps.Any())
                            db.PfaCycleEmp.AddRange(pfaCycleEmps);

                        #region PfaCycle
                        var oldPfaCycle = db.PfaCycle.Where(x => x.ID == pfaCycle.ID).FirstOrDefault();
                        oldPfaCycle.Status = pfaCycle.Status;
                        oldPfaCycle.PfaYear = pfaCycle.PfaYear;
                        oldPfaCycle.Desription = pfaCycle.Desription;
                        oldPfaCycle.ModifiedBy = pfaCycle.ModifiedBy;
                        oldPfaCycle.ModifiedTime = pfaCycle.ModifiedTime;

                        db.SaveChanges();
                        #endregion

                        result.success = true;
                        result.message = "編輯成功";
                        result.log = string.Format("編輯成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "編輯失敗";
                        result.log = string.Format("編輯失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 刪除績效考核簽核批號
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Result DelPfaCycle(Guid Id)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var oldPfaCycle = db.PfaCycle.FirstOrDefault(x => x.ID == Id);
                        var oldPfaCycleEmpList = db.PfaCycleEmp.Where(x => x.PfaCycleID == Id).ToList();
                        if (oldPfaCycleEmpList.Any())
                            db.PfaCycleEmp.RemoveRange(oldPfaCycleEmpList);
                        var oldPfaCycleRationList = db.PfaCycleRation.Where(x => x.PfaCycleID == Id).ToList();
                        if (oldPfaCycleRationList.Any())
                        {
                            var oldPfaCycleRationDetailList = oldPfaCycleRationList.SelectMany(x=> x.PfaCycleRationDetail).ToList();
                            if (oldPfaCycleRationDetailList.Any())
                                db.PfaCycleRationDetail.RemoveRange(oldPfaCycleRationDetailList);
                            db.PfaCycleRation.RemoveRange(oldPfaCycleRationList);
                        }
                        db.PfaCycle.Remove(oldPfaCycle);
                        db.SaveChanges();

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

        /// <summary>
        /// 編輯績效考核員工資料
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="UID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Result EditPfaCycleEmp(Guid Id, Guid UID, List<PfaCycleEmp> data)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        foreach (var item in data)
                        {
                            var pfaCycleEmp = db.PfaCycleEmp.Where(x => x.PfaCycleID == Id && x.EmployeeID == item.EmployeeID).FirstOrDefault();
                            if (pfaCycleEmp != null)
                            {
                                pfaCycleEmp.IsRatio = item.IsRatio;
                                pfaCycleEmp.IsAgent = item.IsAgent;
                                pfaCycleEmp.SignTypeID = item.SignTypeID;
                                pfaCycleEmp.ModifiedBy = UID;
                                pfaCycleEmp.ModifiedTime = DateTime.Now;
                                db.SaveChanges();
                            }
                        }
                        result.success = true;
                        result.message = "績效考核員工資料調整成功";
                        result.log = string.Format("績效考核員工資料調整成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "績效考核員工資料調整失敗";
                        result.log = string.Format("績效考核員工資料調整失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}