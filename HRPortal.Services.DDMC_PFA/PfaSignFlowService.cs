using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaSignFlowService : BaseCrudService<PfaSignFlow>
    {
        public PfaSignFlowService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaOption> GetPfaSignFlowData(string pfaSignFlowCode, string pfaSignFlowName)
        {
            var signTypeIDList = GetAll().GroupBy(x => x.SignTypeID).Select(x => x.Key).ToList();
            var signType = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignType" && signTypeIDList.Contains(x.ID));

            if (!string.IsNullOrEmpty(pfaSignFlowCode))
            {
                signType = signType.Where(x => x.OptionCode == pfaSignFlowCode);
            }
            if (!string.IsNullOrEmpty(pfaSignFlowName))
            {
                signType = signType.Where(x => x.OptionName.Contains(pfaSignFlowName));
            }
            return signType.OrderBy(x => x.OptionCode).ToList();
        }

        public bool IsExist(Guid id)
        {
            return Where(x => x.SignTypeID == id).Any();
        }

        public Result CreatePfaSignFlow(List<PfaSignFlow> data)
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
                            db.PfaSignFlow.Add(item);
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

        public List<PfaSignFlow> GetPfaSignFlow(Guid? id)
        {
            return Where(x => x.SignTypeID == id).OrderBy(x => x.SignStep).ToList();
        }

        public Result EditPfaSignFlow(Guid id, List<PfaSignFlow> data)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var oldData = db.PfaSignFlow.Where(x => x.SignTypeID == id);
                        db.PfaSignFlow.RemoveRange(oldData);
                        db.SaveChanges();

                        foreach (var item in data)
                        {
                            db.PfaSignFlow.Add(item);
                            db.SaveChanges();
                        }

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

        public Result DeletePfaSignFlow(Guid SignTypeID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region PfaTargets
                        var PfaTargets = db.PfaTargets.Where(x => x.SignTypeID == SignTypeID);
                        db.PfaTargets.RemoveRange(PfaTargets);
                        db.SaveChanges();
                        #endregion

                        #region PfaSignFlow
                        var PfaSignFlow = db.PfaSignFlow.Where(x => x.SignTypeID == SignTypeID);
                        db.PfaSignFlow.RemoveRange(PfaSignFlow);
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

        /// <summary>
        /// 簽核流程
        /// </summary>
        /// <returns></returns>
        public PfaSignProcessDataViewModel SignFlow(PfaSignFlowDataViewModel signFlow)
        {
            var signProcessList = new PfaSignProcessDataViewModel();
            signProcessList.PfaSignProcess = new List<PfaSignProcessViewModel>();

            try
            {
                var employeeName = "";
                using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
                {
                    var employee = db.Employees.FirstOrDefault(x => x.ID == signFlow.EmployeeID);
                    if (employee != null)
                        employeeName = employee.EmployeeName;

                    var signLevelList = db.PfaOption.Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel")
                        .OrderBy(x => x.Ordering)
                        .ToList();

                    var pfaSignFlowList = db.PfaSignFlow.Where(x => x.SignTypeID == signFlow.PfaSignTypeID)
                        .OrderBy(x => x.SignStep)
                        .ToList();

                    if (!pfaSignFlowList.Any())
                    {
                        signProcessList.message = "未設定適用簽核流程";
                        return signProcessList;
                    }

                    #region 簽核流程
                    string msg = "";
                    Guid? preSignEmpID = null;
                    Guid? orgSignEmpID = null;
                    
                    foreach (var pfaSignFlow in pfaSignFlowList.Select((value, index) => new { value, index }))
                    {
                        msg = "";
                        orgSignEmpID = null;

                        var signProcess = new PfaSignProcessViewModel
                        {
                            ID = Guid.NewGuid(),
                            PfaCycleEmpID = signFlow.PfaCycleEmpID,
                            SignStep = pfaSignFlow.value.SignStep,
                            SignLevelID = pfaSignFlow.value.SignLevelID,
                            IsSelfEvaluation = pfaSignFlow.value.IsSelfEvaluation,
                            IsFirstEvaluation = pfaSignFlow.value.IsFirstEvaluation,
                            IsSecondEvaluation = pfaSignFlow.value.IsSecondEvaluation,
                            IsThirdEvaluation = pfaSignFlow.value.IsThirdEvaluation,
                            IsUpload = pfaSignFlow.value.IsUpload,
                            Status = "m", // m: 未收件
                        };

                        var nowSignLevel = signLevelList.Where(x => x.ID == signProcess.SignLevelID).FirstOrDefault();
                        if (nowSignLevel == null)
                            throw new Exception(string.Format("查無簽核關卡資料"));

                        #region 簽核判斷
                        signProcess.SignLevelCode = nowSignLevel.OptionCode;
                        signProcess.SignLevelName = nowSignLevel.OptionName;
                        switch (nowSignLevel.OptionCode)
                        {
                            case "Applicant": //申請人
                                orgSignEmpID = signFlow.EmployeeID;
                                break;
                            case "DeptManager": //部門主管
                                orgSignEmpID = FindDeptManager(pfaSignFlow.value.DeptClassID, signFlow.PfaDeptID, ref msg);
                                break;
                            case "SpEmp": //特定人員
                                orgSignEmpID = pfaSignFlow.value.EmployeesID;
                                break;
                            case "OrgManager": //組織主管
                                orgSignEmpID = db.PfaOrg.Where(x => x.ID == signFlow.PfaOrgID).Select(x => x.OrgManagerId).FirstOrDefault();
                                break;
                            case "SpDept": //特定部門主管
                                orgSignEmpID = db.PfaDept.Where(x => x.ID == pfaSignFlow.value.DepartmentsID).Select(x => x.SignManagerID).FirstOrDefault();
                                break;
                        }

                        if (!string.IsNullOrEmpty(msg))
                            throw new Exception(string.Format("{0}找不到簽核關卡為{1}的簽核人員，因為{2}", employeeName, signProcess.SignLevelName, msg));

                        if (!orgSignEmpID.HasValue)
                            throw new Exception(string.Format("{0}找不到簽核關卡為{1}的簽核人員", employeeName, signProcess.SignLevelName));
                        #endregion

                        preSignEmpID = orgSignEmpID;
                        signProcess.OrgSignEmpID = orgSignEmpID.Value;
                        signProcess.PreSignEmpID = preSignEmpID.Value;
                        signProcessList.PfaSignProcess.Add(signProcess);
                    }
                    #endregion

                    PfaSignProcessViewModel nowSignProcess = null;
                    PfaSignProcessViewModel nextSignProcess = null;
                    signProcessList.PfaSignProcess = signProcessList.PfaSignProcess.OrderBy(x => x.SignStep).ToList();
                    for (int i = 0; i < signProcessList.PfaSignProcess.Count(); i++)
                    {
                        nowSignProcess = signProcessList.PfaSignProcess[i];
                        nextSignProcess = ((i + 1) < signProcessList.PfaSignProcess.Count()) ? signProcessList.PfaSignProcess[i + 1] : null;
                        if (nowSignProcess.SignLevelCode == "Applicant" && nowSignProcess.IsSelfEvaluation && signFlow.IsAgent)
                        {
                            if (nextSignProcess != null) 
                                nowSignProcess.PreSignEmpID = nextSignProcess.PreSignEmpID;
                            else 
                                throw new Exception("代理自評找不到初核人員代理");
                        }
                        if (i == 0)
                            nowSignProcess.Status = "c"; // c:待評核
                    }
                }
            }
            catch (Exception ex)
            {
                signProcessList.message = ex.Message;
            }
            return signProcessList;
        }

        /// <summary>
        /// 取得部門主管
        /// </summary>
        /// <param name="deptClassID"></param>
        /// <param name="pfaDeptID"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Guid? FindDeptManager(Guid? deptClassID, Guid? pfaDeptID, ref string msg)
        {
            Guid? result = null;

            try
            {
                if (deptClassID == null || pfaDeptID == null)
                    throw new Exception("查無績效考核部門資料");

                var pfaDept = Db.PfaDept.Where(x => x.ID == pfaDeptID).FirstOrDefault();
                if (pfaDept.DeptClassID == deptClassID)
                {
                    if (pfaDept.SignManagerID.HasValue)
                        result = pfaDept.SignManagerID;
                    else
                        throw new Exception(string.Format("{0}{1}未設定簽核主管名單", pfaDept.PfaDeptCode, pfaDept.PfaDeptName));
                }
                else
                {
                    if (pfaDept.SignParentID != null)
                        result = FindDeptManager(deptClassID, pfaDept.SignParentID, ref msg);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                result = null;
            }
            return result;
        }
    }
}