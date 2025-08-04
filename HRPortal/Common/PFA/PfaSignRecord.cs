using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA.Models;
using System.Collections.Generic;
using System.Linq;

namespace HRPortal.Common.PFA
{
    public class PfaSignRecord
    {
        public static List<PfaSignRecordViewModel> GePfaSignRecord(PfaSignProcess pfaSignProcess, PfaCycleEmpSignViewModel model)
        {
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                List<PfaSignRecordViewModel> history = db.PfaSignRecord
                        .Where(x => x.PfaCycleEmpID == model.PfaCycleEmpID)
                        .ToList()
                        .Select(x => new PfaSignRecordViewModel()
                        {
                            SignStep = x.SignStep,
                            SignLevelID = x.SignLevelID,
                            StatusCode = x.Status,
                            OrgSignEmpID = x.OrgSignEmpID,
                            PreSignEmpID = x.PreSignEmpID,
                            Assessment = x.Assessment,
                            SignTime = x.SignTime.HasValue ? x.SignTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "",
                            IsSelfEvaluation = x.IsSelfEvaluation,
                            IsFirstEvaluation = x.IsFirstEvaluation,
                            IsSecondEvaluation = x.IsSecondEvaluation,
                            IsThirdEvaluation = x.IsThirdEvaluation,
                            IsHrEvaluation = x.IsHrEvaluation,
                        })
                        .OrderBy(x => x.SignTime)
                        .ThenBy(x => x.SignStep)
                        .ToList();


                int idx = 0;

                List<DBEntities.DDMC_PFA.PfaOption> signLevelList = db.PfaOption
                .Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel")
                .ToList();

                var signStatusList = db.PfaOption
                .Where(x => x.PfaOptionGroup.OptionGroupCode == "SignStatus")
                .ToList();

                foreach (PfaSignRecordViewModel item in history)
                {
                    idx += 1;
                    item.Order = idx.ToString();
                    var signEmp = db.Employees
                    .FirstOrDefault(x => x.ID == item.PreSignEmpID);
                    if (signEmp != null)
                    {
                        item.EmployeeNo = signEmp.EmployeeNO;
                        item.EmployeeName = signEmp.EmployeeName;
                    }

                    var signLevel = signLevelList.FirstOrDefault(x => x.ID == item.SignLevelID);
                    if (signLevel != null)
                        item.SignLevelName = signLevel.OptionName;

                    var assessment = new List<string>();
                    if (item.IsSelfEvaluation)
                        assessment.Add("自評");
                    if (item.IsFirstEvaluation)
                        assessment.Add("初核");
                    if (item.IsSecondEvaluation)
                        assessment.Add("複核");
                    if (item.IsThirdEvaluation)
                        assessment.Add("核決");
                    if (item.IsHrEvaluation)
                        assessment.Add("HR");
                    item.AssessmentName = string.Join("/", assessment.ToArray());

                    if (item.PreSignEmpID != item.OrgSignEmpID)
                        item.IsAgent = true;
                    else
                        item.IsAgent = false;

                    var signStatus = signStatusList.FirstOrDefault(x => x.OptionCode == item.StatusCode);
                    if (signStatus != null)
                        item.StatusName = signStatus.OptionName;
                }

                idx = SetNowStatus(model, db, history, idx, signLevelList, signStatusList);

                return history;

            }
   

        }

        public static List<PfaSignRecordViewModel> GePfaSignRecord(PfaCycleEmpSignViewModel model)
        {
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                List<PfaSignRecordViewModel> history = db.PfaSignRecord
                        .Where(x => x.PfaCycleEmpID == model.PfaCycleEmpID)
                        .ToList()
                        .Select(x => new PfaSignRecordViewModel()
                        {
                            SignStep = x.SignStep,
                            SignLevelID = x.SignLevelID,
                            StatusCode = x.Status,
                            OrgSignEmpID = x.OrgSignEmpID,
                            PreSignEmpID = x.PreSignEmpID,
                            Assessment = x.Assessment,
                            SignTime = x.SignTime.HasValue ? x.SignTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "",
                            IsSelfEvaluation = x.IsSelfEvaluation,
                            IsFirstEvaluation = x.IsFirstEvaluation,
                            IsSecondEvaluation = x.IsSecondEvaluation,
                            IsThirdEvaluation = x.IsThirdEvaluation,
                            IsHrEvaluation = x.IsHrEvaluation,
                        })
                        .OrderBy(x => x.SignTime)
                        .ThenBy(x => x.SignStep)
                        .ToList();


                int idx = 0;

                var signLevelList = db.PfaOption
                .Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel")
                .ToList();

                var signStatusList = db.PfaOption
                .Where(x => x.PfaOptionGroup.OptionGroupCode == "SignStatus")
                .ToList();

                foreach (PfaSignRecordViewModel item in history)
                {
                    idx += 1;
                    item.Order = idx.ToString();
                    var signEmp = db.Employees
                    .FirstOrDefault(x => x.ID == item.PreSignEmpID);
                    if (signEmp != null)
                    {
                        item.EmployeeNo = signEmp.EmployeeNO;
                        item.EmployeeName = signEmp.EmployeeName;
                    }

                    var signLevel = signLevelList.FirstOrDefault(x => x.ID == item.SignLevelID);
                    if (signLevel != null)
                        item.SignLevelName = signLevel.OptionName;

                    var assessment = new List<string>();
                    if (item.IsSelfEvaluation)
                        assessment.Add("自評");
                    if (item.IsFirstEvaluation)
                        assessment.Add("初核");
                    if (item.IsSecondEvaluation)
                        assessment.Add("複核");
                    if (item.IsThirdEvaluation)
                        assessment.Add("核決");
                    if (item.IsHrEvaluation)
                        assessment.Add("HR");
                    item.AssessmentName = string.Join("/", assessment.ToArray());

                    if (item.PreSignEmpID != item.OrgSignEmpID)
                        item.IsAgent = true;
                    else
                        item.IsAgent = false;

                    var signStatus = signStatusList.FirstOrDefault(x => x.OptionCode == item.StatusCode);
                    if (signStatus != null)
                        item.StatusName = signStatus.OptionName;
                }

                idx = SetNowStatus(model, db, history, idx, signLevelList, signStatusList);

                return history;

            }


        }

        private static int SetNowStatus(PfaCycleEmpSignViewModel model, NewHRPortalEntitiesDDMC_PFA db, List<PfaSignRecordViewModel> history, int idx, List<DBEntities.DDMC_PFA.PfaOption> signLevelList, List<DBEntities.DDMC_PFA.PfaOption> signStatusList)
        {
            var pfaCycleEmp = db.PfaCycleEmp.First(r => r.ID == model.PfaCycleEmpID);

            switch (pfaCycleEmp.Status)
            {
                case "m":  //未送簽
                case "a":  //考評中
                    List<string> statusList = new List<string>();
                    statusList.Add(PfaSignProcess_Status.NotReceived);
                    statusList.Add(PfaSignProcess_Status.PendingReview);
                    statusList.Add(PfaSignProcess_Status.PendingThirdReview);
                    statusList.Add(PfaSignProcess_Status.Reviewed);
                    statusList.Add(PfaSignProcess_Status.ReturnedForModification);

                    var process = db.PfaSignProcess
                    .Where(x => x.PfaCycleEmpID == model.PfaCycleEmpID && statusList.Contains(x.Status))
                    .ToList()
                    .Select(x => new PfaSignRecordViewModel()
                    {
                        SignStep = x.SignStep,
                        SignLevelID = x.SignLevelID,
                        StatusCode = x.Status,
                        OrgSignEmpID = x.OrgSignEmpID,
                        PreSignEmpID = x.PreSignEmpID,
                        Assessment = x.Assessment,
                        SignTime = x.SignTime.HasValue ? x.SignTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "",
                        IsSelfEvaluation = x.IsSelfEvaluation,
                        IsFirstEvaluation = x.IsFirstEvaluation,
                        IsSecondEvaluation = x.IsSecondEvaluation,
                        IsThirdEvaluation = x.IsThirdEvaluation,
                    }).OrderBy(x => x.SignStep).ToList();

                    foreach (PfaSignRecordViewModel item in process)
                    {
                        idx += 1;
                        item.Order = idx.ToString();
                        var signEmp = db.Employees
                        .Where(x => x.ID == item.PreSignEmpID)
                        .FirstOrDefault();
                        if (signEmp != null)
                        {
                            item.EmployeeNo = signEmp.EmployeeNO;
                            item.EmployeeName = signEmp.EmployeeName;
                        }

                        var signLevel = signLevelList.FirstOrDefault(x => x.ID == item.SignLevelID);
                        if (signLevel != null)
                            item.SignLevelName = signLevel.OptionName;

                        var assessment = new List<string>();
                        if (item.IsSelfEvaluation)
                            assessment.Add("自評");
                        if (item.IsFirstEvaluation)
                            assessment.Add("初核");
                        if (item.IsSecondEvaluation)
                            assessment.Add("複核");
                        if (item.IsThirdEvaluation)
                            assessment.Add("核決");
                        item.AssessmentName = string.Join("/", assessment.ToArray());

                        if (item.PreSignEmpID != item.OrgSignEmpID)
                            item.IsAgent = true;
                        else
                            item.IsAgent = false;

                        var signStatus = signStatusList.FirstOrDefault(x => x.OptionCode == item.StatusCode);
                        if (signStatus != null)
                            item.StatusName = signStatus.OptionName;
                        history.Add(item);
                    }
                    break;
                default:
                    break;
            }

            return idx;
        }
    }
}