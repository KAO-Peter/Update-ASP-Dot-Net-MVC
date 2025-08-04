using HRPortal.ApiAdapter.DDMC_PFA;
using HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData;
using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRPortal.HRMImport.DDMC_PFA
{
    public class ImportTask : IDisposable
    {
        public Action<string> OnMessage;

        public Guid DeptClassID = Guid.Empty;
        public Guid AdminID = Guid.Empty;

        private NewHRPortalEntitiesDDMC_PFA _db;

        public NewHRPortalEntitiesDDMC_PFA DB
        {
            get
            {
                if (_db == null)
                {
                    _db = new NewHRPortalEntitiesDDMC_PFA();
                }
                return _db;
            }
        }

        public ImportTask()
        {
        }

        public ImportTask(NewHRPortalEntitiesDDMC_PFA db)
        {
            this._db = db;
        }

        public void WriteMessage(string message)
        {
            if (OnMessage != null)
            {
                OnMessage(message);
            }
        }

        public async Task Run()
        {
            AdminID = DB.Employees.Where(x => x.EmployeeNO == "admin").Select(x => x.ID).FirstOrDefault();
            // 新增時, 部門階級取得代碼中最小的階級
            DeptClassID = DB.PfaOption.Where(x => x.PfaOptionGroup.OptionGroupCode == "DeptClass").OrderByDescending(x => x.Ordering).Select(x => x.ID).FirstOrDefault();

            List<PfaDept> _departmentList = DB.PfaDept.ToList();
            List<PfaEmployee> _employeeList = DB.Employees.Where(x => x.EmployeeType == "2").ToList();
            List<PfaDeptEmp> _deptEmpList = DB.PfaDeptEmp.ToList();

            List<CompanyData> _hrmCompany = await GetCompanyData();
            // 取得有效分公司
            _hrmCompany = _hrmCompany.Where(x => x.Enabled).ToList();
            List<DepartmentData> _hrmDepartment;
            List<PfaEmployee> _protalEmpList;

            #region PfaDept,PfaDeptEmp
            foreach (CompanyData _companyData in _hrmCompany)
            {
                PfaCompany _company = DB.Companys.Where(x => x.CompanyCode == _companyData.Code).FirstOrDefault();

                if (_company == null)
                {
                    WriteMessage(string.Format("公司別為 {0} 在 Portal 查無資料", _companyData.Code));
                }
                else
                {
                    #region PfaDept部門資料同步
                    WriteMessage(string.Format("開始同步公司別為 {0} 的 PfaDept 資料", _companyData.Code));

                    _hrmDepartment = await GetDepartmentData(_companyData.Code);

                    foreach (DepartmentData _departmentData in _hrmDepartment)
                    {
                        PfaDept _department = RenewDepartmentData(_departmentList, _departmentData, _company);
                    }

                    DB.SaveChanges();

                    foreach (DepartmentData _departmentData in _hrmDepartment)
                    {
                        PfaDept _department = _departmentList.FirstOrDefault(x => x.CompanyID == _company.ID && x.PfaDeptCode == _departmentData.Code);

                        // 如果部門不存在就不處理
                        if (_department == null) continue;

                        if (!string.IsNullOrEmpty(_departmentData.ParentDeptCode))
                        {
                            PfaDept _deptList = _departmentList.FirstOrDefault(x => x.CompanyID == _company.ID && x.PfaDeptCode == _departmentData.ParentDeptCode);

                            if (_deptList != null)
                            {
                                Guid _parentId = _departmentList.FirstOrDefault(x => x.CompanyID == _company.ID && x.PfaDeptCode == _departmentData.ParentDeptCode).ID;

                                if (!_department.ParentDepartmentID.HasValue || _department.ParentDepartmentID.Value != _parentId)
                                {
                                    WriteMessage(string.Format("設定部門為 {0} 的上層部門", _departmentData.Code));
                                    _department.ParentDepartmentID = _parentId;
                                }

                                if (!_department.SignParentID.HasValue)
                                {
                                    _department.SignParentID = _parentId;
                                }
                            }
                            else if (_deptList == null)
                            {
                                WriteMessage(string.Format("設定部門為 {0} 的上層部門", _departmentData.Code));

                                _department.ParentDepartmentID = null;

                                if (!_department.SignParentID.HasValue)
                                {
                                    _department.SignParentID = null;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(_departmentData.ManagerEmpID))
                        {
                            PfaEmployee _DeptManager = _employeeList.FirstOrDefault(x => x.CompanyID == _company.ID && x.EmployeeNO == _departmentData.ManagerEmpID);

                            if (_DeptManager != null)
                            {
                                Guid _managerId = _employeeList.FirstOrDefault(x => x.CompanyID == _company.ID && x.EmployeeNO == _departmentData.ManagerEmpID).ID;

                                if (!_department.ManagerID.HasValue || _department.ManagerID.Value != _managerId)
                                {
                                    WriteMessage(string.Format("設定部門 {0} 的HRM部門主管為 {1}", _departmentData.Code, _departmentData.ManagerEmpID));
                                    _department.ManagerID = _managerId;
                                }

                                if (!_department.SignManagerID.HasValue)
                                {
                                    WriteMessage(string.Format("設定部門 {0} 的簽核主管為 {1}", _departmentData.Code, _departmentData.ManagerEmpID));
                                    _department.SignManagerID = _managerId;
                                }
                            }
                        }
                        else
                        {
                            if (_department.ManagerID.HasValue)
                            {
                                WriteMessage(string.Format("移除部門 {0} 的HRM部門主管", _departmentData.Code));
                                _department.ManagerID = null;
                            }
                        }
                    }

                    DB.SaveChanges();

                    WriteMessage(string.Format("結束同步公司別為 {0} 的 PfaDept 資料", _companyData.Code));
                    #endregion

                    #region PfaDeptEmp員工所屬單位異動
                    WriteMessage(string.Format("開始同步公司別為 {0} 的 PfaDeptEmp 資料", _companyData.Code));

                    //更新位離職或離職一年內的資料
                    DateTime _leaveDate = DateTime.Now.AddYears(-1).Date;

                    _protalEmpList = _employeeList.Where(x => x.CompanyID == _company.ID && (!x.LeaveDate.HasValue || (x.LeaveDate.HasValue && x.LeaveDate >= _leaveDate))).ToList();

                    foreach (PfaEmployee _employee in _protalEmpList)
                        RenewEmployeeData(_deptEmpList, _employee);
                    DB.SaveChanges();

                    WriteMessage(string.Format("結束同步公司別為 {0} 的 PfaDeptEmp 資料", _companyData.Code));
                    #endregion
                }
            }
            #endregion

            #region PfaPerformance 分數評等區間設定
            List<PfaPerformanceData> _hrmPfaPerformance = await GetPfaPerformance();
            RenewPfaPerformanceData(_hrmPfaPerformance);
            #endregion

            #region Hire 雇用別
            List<CodeData> _hrmHire = await GetHireData();
            RenewPfaOptionData(_hrmHire, "Hire");
            #endregion

            #region JobTitle 職稱
            List<CodeData> _hrmJobTitle = await GetJobTitleData();
            RenewPfaOptionData(_hrmJobTitle, "JobTitle");
            #endregion

            #region JobFunction 職務
            List<CodeData> _hrmJobFunction = await GetJobFunctionData();
            RenewPfaOptionData(_hrmJobFunction, "JobFunction");
            #endregion

            #region Position 職級
            List<CodeData> _hrmPosition = await GetPositionData();
            RenewPfaOptionData(_hrmPosition, "Position");
            #endregion

            #region Grade 職等
            List<CodeData> _hrmGrade = await GetGradeData();
            RenewPfaOptionData(_hrmGrade, "Grade");
            #endregion
        }

        private PfaDept RenewDepartmentData(List<PfaDept> departmentList, DepartmentData departmentData, PfaCompany company)
        {
            PfaDept _department = departmentList.FirstOrDefault(x => x.CompanyID == company.ID && x.PfaDeptCode == departmentData.Code);

            if (_department == null)
            {
                // 如果已經失效 就不新增
                if (departmentData.EndDate < DateTime.Today) return null;

                _department = new PfaDept()
                {
                    ID = Guid.NewGuid(),
                    PfaDeptCode = departmentData.Code,
                    PfaDeptName = departmentData.Name,
                    CompanyID = company.ID,
                    DeptClassID = DeptClassID,
                    BeginDate = DateTime.Now,
                    EndDate = departmentData.EndDate,
                    OnlyForSign = false,
                    Department_ID = departmentData.ID,
                    CreatedBy = AdminID,
                    CreatedTime = DateTime.Now
                };

                departmentList.Add(_department);
                DB.PfaDept.Add(_department);

                WriteMessage(string.Format("PfaDept {0} 已新增", departmentData.Code));
            }
            else
            {
                //判斷不是Portal建立的部門OnlyForSign=0，或是雖然是Portal建立的部門，但是後台還沒失效或是當日才失效的，才更新
                if ((!_department.OnlyForSign) || (_department.OnlyForSign && (departmentData.EndDate >= DateTime.Today)))
                {
                    if (_department.PfaDeptName != departmentData.Name || _department.EndDate != departmentData.EndDate)
                    {
                        _department.PfaDeptName = departmentData.Name;
                        _department.EndDate = departmentData.EndDate;
                        _department.ModifiedBy = AdminID;
                        _department.ModifiedTime = DateTime.Now;

                        WriteMessage(string.Format("PfaDept {0} 已更新.", departmentData.Code));
                    }
                }
            }
            return _department;
        }

        private PfaDeptEmp RenewEmployeeData(List<PfaDeptEmp> employeeList, PfaEmployee employeeData)
        {
            PfaDeptEmp _employee = employeeList.FirstOrDefault(x => x.EmployeeID == employeeData.ID);

            if (_employee == null)
            {
                // 以公司別ID + 部門代碼判斷 兩邊來源的Guid不會是一致的
                PfaDept _dept = DB.PfaDept.Where(x => x.CompanyID == employeeData.CompanyID && x.PfaDeptCode == employeeData.Department.DepartmentCode).FirstOrDefault();

                if (_dept != null)
                {
                    _employee = new PfaDeptEmp();
                    _employee.ID = Guid.NewGuid();
                    _employee.EmployeeID = employeeData.ID;
                    _employee.PfaDeptID = _dept.ID;

                    employeeList.Add(_employee);
                    DB.PfaDeptEmp.Add(_employee);

                    WriteMessage(string.Format("PfaDeptEmp {0} 已新增", employeeData.EmployeeNO));
                }
            }
            else
            {
                // 如果是虛擬部門就不更新
                if (!_employee.PfaDept.OnlyForSign)
                {
                    // 以公司別ID + 部門代碼判斷 兩邊來源的Guid不會是一致的
                    PfaDept _dept = DB.PfaDept.Where(x => x.CompanyID == employeeData.CompanyID && x.PfaDeptCode == employeeData.Department.DepartmentCode).FirstOrDefault();

                    if (_dept != null)
                    {
                        if (_employee.PfaDeptID != _dept.ID)
                        {
                            _employee.PfaDeptID = _dept.ID;

                            WriteMessage(string.Format("PfaDeptEmp {0} 已更新", employeeData.EmployeeNO));
                        }
                    }
                    else
                    {
                        WriteMessage(string.Format("PfaDeptEmp {0} 查無 PfaDept 資料", employeeData.EmployeeNO));
                    }
                }
            }

            return _employee;
        }

        /// <summary>
        /// 分數評等區間設定
        /// </summary>
        /// <param name="items"></param>
        private void RenewPfaPerformanceData(List<PfaPerformanceData> items)
        {
            WriteMessage("開始同步分數評等區間資料");

            var maxOrdering = 0;
            var companysList = DB.Companys.ToList();
            foreach (var companys in companysList)
            {
                maxOrdering = 0;
                var itemList = items.Where(x => x.CompanyCode == companys.CompanyCode).ToList();
                if (itemList.Any())
                {
                    var tempOrdering = DB.PfaPerformance.Where(x => x.CompanyID == companys.ID).OrderByDescending(x => x.Ordering).FirstOrDefault();
                    if (tempOrdering == null)
                        maxOrdering = 1;
                    else
                        maxOrdering = maxOrdering + 1;

                    foreach (var item in itemList)
                    {
                        var pfaPerformance = DB.PfaPerformance.FirstOrDefault(x => x.CompanyID == companys.ID && x.Code == item.Code);
                        if (pfaPerformance == null)
                        {
                            pfaPerformance = new PfaPerformance
                            {
                                ID = Guid.NewGuid(),
                                Code = item.Code,
                                Name = item.Name,
                                IsUsed = item.IsUsed,
                                CompanyID = companys.ID,
                                Ordering = maxOrdering,
                                Performance = item.Performance,
                                band = item.band,
                                Rates = item.Rates,
                                Multiplier = item.Multiplier,
                                ScoresStart = item.ScoresStart,
                                ScoresEnd = item.ScoresEnd,
                                CreatedBy = AdminID,
                                CreatedTime = DateTime.Now,
                                ModifiedBy = AdminID,
                                ModifiedTime = DateTime.Now,
                            };
                            DB.PfaPerformance.Add(pfaPerformance);

                            WriteMessage(string.Format("{0} {1} 已新增", pfaPerformance.Code, pfaPerformance.Name));
                        }
                        else
                        {
                            if (pfaPerformance.Name != item.Name || pfaPerformance.Performance != item.Performance ||
                                pfaPerformance.band != item.band || pfaPerformance.Multiplier != item.Multiplier ||
                                pfaPerformance.ScoresStart != item.ScoresStart || pfaPerformance.ScoresEnd != item.ScoresEnd ||
                                pfaPerformance.Rates != item.Rates || pfaPerformance.IsUsed != item.IsUsed)
                            {
                                pfaPerformance.Name = item.Name;
                                pfaPerformance.Performance = item.Performance;
                                pfaPerformance.band = item.band;
                                pfaPerformance.Multiplier = item.Multiplier;
                                pfaPerformance.ScoresStart = item.ScoresStart;
                                pfaPerformance.ScoresEnd = item.ScoresEnd;
                                pfaPerformance.Rates = item.Rates;
                                pfaPerformance.IsUsed = item.IsUsed;
                                WriteMessage(string.Format("{0} {1} 已更新", pfaPerformance.Code, pfaPerformance.Name));
                            }
                        }
                    }
                    DB.SaveChanges();
                }
            }
            WriteMessage("結束同步分數評等區間資料資料");
        }


        /// <summary>
		/// 更新表單代碼選項
		/// </summary>
		/// <param name="items">清單</param>
		/// <param name="code">表單代碼群組</param>
		private void RenewPfaOptionData(List<CodeData> items, string code)
        {
            WriteMessage(string.Format("開始同步 {0} 資料", code));

            var _optionGroup = DB.PfaOptionGroup.FirstOrDefault(x => x.OptionGroupCode == code);

            if (_optionGroup != null)
            {
                var _optionGroupID = _optionGroup.ID;

                foreach (var item in items)
                {
                    var _option = DB.PfaOption.Where(x => x.PfaOptionGroupID == _optionGroupID && x.OptionCode == item.Code).FirstOrDefault();

                    if (_option == null)
                    {
                        _option = new PfaOption();
                        _option.ID = Guid.NewGuid();
                        _option.PfaOptionGroupID = _optionGroupID;
                        _option.OptionCode = item.Code;
                        _option.OptionName = item.Name.Trim();
                        _option.Ordering = 1;
                        _option.CreatedBy = AdminID;
                        _option.CreatedTime = DateTime.Now;

                        DB.PfaOption.Add(_option);

                        WriteMessage(string.Format("{0} {1} 已新增", code, item.Code));
                    }
                    else
                    {
                        if (_option.OptionName != item.Name.Trim())
                        {
                            _option.OptionName = item.Name.Trim();

                            WriteMessage(string.Format("{0} {1} 已更新", code, item.Code));
                        }
                    }
                }
                DB.SaveChanges();
            }
            else
            {
                WriteMessage(string.Format("更新 {0} 資料失敗，PfaOptionGroup 未設定 {0}。", code));
            }

            WriteMessage(string.Format("結束同步 {0} 資料", code));
        }

        private async Task<List<CompanyData>> GetCompanyData()
        {
            return await HRMApiAdapter.GetCompany();
        }

        private async Task<List<DepartmentData>> GetDepartmentData(string companyCode)
        {
            return await HRMApiAdapter.GetAllDepartment(companyCode);
        }

        private async Task<List<PfaPerformanceData>> GetPfaPerformance()
        {
            return await HRMApiAdapter.GetPfaPerformance();
        }

        private async Task<List<CodeData>> GetHireData()
        {
            return await HRMApiAdapter.GetHire();
        }

        private async Task<List<CodeData>> GetJobTitleData()
        {
            return await HRMApiAdapter.GetJobTitle();
        }

        private async Task<List<CodeData>> GetJobFunctionData()
        {
            return await HRMApiAdapter.GetJobFunction();
        }

        private async Task<List<CodeData>> GetPositionData()
        {
            return await HRMApiAdapter.GetPosition();
        }

        private async Task<List<CodeData>> GetGradeData()
        {
            return await HRMApiAdapter.GetGrade();
        }

        public void Dispose()
        {
        }
    }
}
