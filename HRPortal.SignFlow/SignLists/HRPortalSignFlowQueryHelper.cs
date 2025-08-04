using HRPortal.SignFlow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.SignFlow.SignLists
{
    public class HRPortalSignFlowQueryHelper : IDisposable
    {
        private SignFlowRecRepository m_signFlowRecRepository;

        public SignFlowRecRepository SignFlowRecRepository
        {
            get
            {
                if (m_signFlowRecRepository == null) m_signFlowRecRepository = new SignFlowRecRepository();
                return m_signFlowRecRepository;
            }
        }

        private SignFlowRecQueryHelper _signFlowRecQueryHelper;

        public SignFlowRecQueryHelper SignFlowRecQueryHelper
        {
            get
            {
                if (_signFlowRecQueryHelper == null)
                {
                    _signFlowRecQueryHelper = new SignFlowRecQueryHelper((SignFlowRecRepository)SignFlowRecRepository);
                }
                return _signFlowRecQueryHelper;
            }
        }

        public List<HRPotralFormSignStatus> GetToSignList(string companyCode, string departmentCode, string signerNo)
        {
            List<SignFlowRecModel> _signFlowList = SignFlowRecQueryHelper.GetSignFlowByCurrentSignerId(signerNo).ToList();
            _signFlowList.AddRange(SignFlowRecQueryHelper.GetSignFlowByCurrentDepartmentId(departmentCode));

            List<HRPotralFormSignStatus> _toSignList = new List<HRPotralFormSignStatus>();
            foreach (SignFlowRecModel _signFlow in _signFlowList)
            {
                HRPotralFormSignStatus _signStatus = new HRPotralFormSignStatus(_signFlow);
                if (_signFlow.SignCompanyID == companyCode || _signStatus.CompanyCode == companyCode)
                {
                    _toSignList.Add(new HRPotralFormSignStatus(_signFlow));
                }
            }

            return _toSignList;
        }

        //20151215 增加待處理 by Bee
        public List<HRPotralFormSignStatus> GetPendingList(string companyCode, string signerNo)
        {
            List<SignFlowRecModel> _signFlowList = SignFlowRecQueryHelper.GetPendingSignFlowByCurrentSignerId(signerNo).ToList();

            List<HRPotralFormSignStatus> _toPendingList = new List<HRPotralFormSignStatus>();
            foreach (SignFlowRecModel _signFlow in _signFlowList)
            {
                HRPotralFormSignStatus _signStatus = new HRPotralFormSignStatus(_signFlow);
                if (_signStatus.CompanyCode == companyCode)
                {
                    _toPendingList.Add(new HRPotralFormSignStatus(_signFlow));
                }
            }

            return _toPendingList;
        }

        public List<HRPotralFormSignStatus> GetCurrentSignListByEmployee(string companyCode, string employeeNo, bool onlyPending, DateTime startFormDate, DateTime? endFormDate = null)
        {
            var _empNo = employeeNo.TrimEnd(',').Split(',');
            if (endFormDate == null) endFormDate = startFormDate.AddDays(1);
            List<string> _formNoList = new List<string>();
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode && _empNo.Contains(x.Employee.EmployeeNO)
                     && x.StartTime >= startFormDate && (endFormDate == null || x.StartTime <= endFormDate)
                     && !x.IsDeleted)
                    .Select(x => x.FormNo).ToList());
                _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode && _empNo.Contains(x.Employee.EmployeeNO)
                     && x.StartTime >= startFormDate && (endFormDate == null || x.StartTime <= endFormDate)
                     && !x.IsDeleted)
                     .Select(x => x.FormNo).ToList());
                //2018/12/3 Neo 修改查詢以補刷卡時間查詢，而非填寫日期的區間
                _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode && _empNo.Contains(x.Employee.EmployeeNO)
                     && x.PatchCardTime >= startFormDate && (endFormDate == null || x.PatchCardTime <= endFormDate)
                     && !x.IsDeleted)
                     .Select(x => x.FormNo).ToList());
                //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間
                _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode && _empNo.Contains(x.LeaveForm.Employee.EmployeeNO) 
                     && x.LeaveForm.StartTime >= startFormDate && (endFormDate == null || x.LeaveForm.EndTime <= endFormDate)
                     && !x.IsDeleted)
                     .Select(x => x.FormNo).ToList());
                //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode &&  _empNo.Contains(x.OverTimeForm.Employee.EmployeeNO) 
                     && x.OverTimeForm.StartTime >= startFormDate && (endFormDate == null || x.OverTimeForm.EndTime <= endFormDate)
                     && !x.IsDeleted)
                     .Select(x => x.FormNo).ToList());
            }

            List<SignFlowRecModel> _signFlowList = SignFlowRecQueryHelper.GetCurrentSignFlowByFormNumber(_formNoList).ToList();
            if (!onlyPending)
            {
                List<string> _hadDataList = _signFlowList.Select(x => x.FormNumber).ToList();
                _formNoList.RemoveAll(x => _hadDataList.Contains(x));
                _signFlowList.AddRange(SignFlowRecQueryHelper.GetLastSignFlowByFormNumber(_formNoList));
            }

            List<HRPotralFormSignStatus> _signList = new List<HRPotralFormSignStatus>();
            foreach (SignFlowRecModel _signFlow in _signFlowList)
            {
                _signFlow.SignerID = _signFlow.ActSignerID != null ? _signFlow.ActSignerID : _signFlow.SignerID;//修改簽核者為實際簽核者 Irving 20170706 //20220110 小榜修正尚未簽核時，以原本的簽核者為主
                _signList.Add(new HRPotralFormSignStatus(_signFlow));
            }

            return _signList.OrderBy(x => x.UrgentOrder).ThenByDescending(x => x.FormCreateDate).ToList();
        }

        public List<HRPotralFormSignStatus> GetCurrentSignListByDepartmentt(string companyCode, string departmentCode, string StatusData, bool onlyPending, DateTime startFormDate, DateTime? endFormDate = null)
        {
            List<string> _formNoList = new List<string>();
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                if (departmentCode == "All")
                {
                    if (StatusData == "L")
                    {
                        _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                               && x.StartTime >= startFormDate
                                                               && x.Employee.LeaveDate != null
                                                               && (endFormDate == null || x.StartTime <= endFormDate)
                                                               && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                               && x.StartTime >= startFormDate
                                                               && (x.Employee.LeaveDate > DateTime.Now || x.Employee.LeaveDate == null)
                                                               && (endFormDate == null || x.StartTime <= endFormDate)
                                                               && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                                    && x.StartTime >= startFormDate
                                                                    && (endFormDate == null || x.StartTime <= endFormDate)
                                                                    && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                       && x.StartTime >= startFormDate
                                                                       && x.Employee.LeaveDate != null
                                                                       && (endFormDate == null || x.StartTime <= endFormDate)
                                                                       && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                       && x.StartTime >= startFormDate
                                                                       && (x.Employee.LeaveDate > DateTime.Now || x.Employee.LeaveDate == null)
                                                                       && (endFormDate == null || x.StartTime <= endFormDate)
                                                                       && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                       && x.StartTime >= startFormDate
                                                                       && (endFormDate == null || x.StartTime <= endFormDate)
                                                                       && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        //2018/12/3 Neo 修改查詢以補刷卡時間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                           && x.PatchCardTime >= startFormDate
                                                                           && x.Employee.LeaveDate != null
                                                                           && (endFormDate == null || x.PatchCardTime <= endFormDate)
                                                                           && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        //2018/12/3 Neo 修改查詢以補刷卡時間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                           && x.PatchCardTime >= startFormDate
                                                                           && (x.Employee.LeaveDate > DateTime.Now || x.Employee.LeaveDate == null)
                                                                           && (endFormDate == null || x.PatchCardTime <= endFormDate)
                                                                           && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        //2018/12/3 Neo 修改查詢以補刷卡時間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                        && x.PatchCardTime >= startFormDate
                                                                        && (endFormDate == null || x.PatchCardTime <= endFormDate)
                                                                        && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間        
                        _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                         && x.LeaveForm.StartTime >= startFormDate
                                                                         && x.LeaveForm.Employee.LeaveDate != null
                                                                         && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                         && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                         && x.LeaveForm.StartTime >= startFormDate
                                                                         && (x.LeaveForm.Employee.LeaveDate > DateTime.Now || x.LeaveForm.Employee.LeaveDate == null)
                                                                         && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                         && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                      && x.LeaveForm.StartTime >= startFormDate
                                                                      && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                      && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                           && x.OverTimeForm.StartTime >= startFormDate
                                                                           && x.OverTimeForm.Employee.LeaveDate != null
                                                                           && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                           && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                           && x.OverTimeForm.StartTime >= startFormDate
                                                                           && (x.OverTimeForm.Employee.LeaveDate > DateTime.Now || x.OverTimeForm.Employee.LeaveDate == null)
                                                                           && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                           && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                         && x.OverTimeForm.StartTime >= startFormDate
                                                                         && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                         && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                }
                else
                {
                    if (StatusData == "L")
                    {
                        _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                                       && x.Department.DepartmentCode == departmentCode
                                                                       && x.StartTime >= startFormDate
                                                                       && x.Employee.LeaveDate != null
                                                                       && (endFormDate == null || x.StartTime <= endFormDate)
                                                                       && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                                          && x.Department.DepartmentCode == departmentCode
                                                                          && x.StartTime >= startFormDate
                                                                          && (x.Employee.LeaveDate > DateTime.Now || x.Employee.LeaveDate == null)
                                                                          && (endFormDate == null || x.StartTime <= endFormDate)
                                                                          && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                                    && x.Department.DepartmentCode == departmentCode
                                                                    && x.StartTime >= startFormDate
                                                                    && (endFormDate == null || x.StartTime <= endFormDate)
                                                                    && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                      && x.Department.DepartmentCode == departmentCode
                                                                      && x.StartTime >= startFormDate
                                                                      && x.Employee.LeaveDate != null
                                                                      && (endFormDate == null || x.StartTime <= endFormDate)
                                                                      && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                      && x.Department.DepartmentCode == departmentCode
                                                                      && x.StartTime >= startFormDate
                                                                      && (x.Employee.LeaveDate > DateTime.Now || x.Employee.LeaveDate == null)
                                                                      && (endFormDate == null || x.StartTime <= endFormDate)
                                                                      && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                       && x.Department.DepartmentCode == departmentCode
                                                                       && x.StartTime >= startFormDate
                                                                       && (endFormDate == null || x.StartTime <= endFormDate)
                                                                       && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                           && x.Department.DepartmentCode == departmentCode
                                                                           && x.CreatedTime >= startFormDate
                                                                           && x.Employee.LeaveDate != null
                                                                           && (endFormDate == null || x.CreatedTime <= endFormDate)
                                                                           && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                           && x.Department.DepartmentCode == departmentCode
                                                                           && x.CreatedTime >= startFormDate
                                                                           && (x.Employee.LeaveDate > DateTime.Now || x.Employee.LeaveDate == null)
                                                                           && (endFormDate == null || x.CreatedTime <= endFormDate)
                                                                           && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                        && x.Department.DepartmentCode == departmentCode
                                                                        && x.CreatedTime >= startFormDate
                                                                        && (endFormDate == null || x.CreatedTime <= endFormDate)
                                                                        && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        //2018/12/3 Neo 修改查詢以開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                     && x.LeaveForm.Department.DepartmentCode == departmentCode
                                                                     && x.LeaveForm.StartTime >= startFormDate
                                                                     && x.LeaveForm.Employee.LeaveDate != null
                                                                     && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                     && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                     && x.LeaveForm.Department.DepartmentCode == departmentCode
                                                                     && x.LeaveForm.StartTime >= startFormDate
                                                                     && (x.LeaveForm.Employee.LeaveDate > DateTime.Now || x.LeaveForm.Employee.LeaveDate == null)
                                                                     && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                     && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                      && x.LeaveForm.Department.DepartmentCode == departmentCode
                                                                      && x.LeaveForm.StartTime >= startFormDate
                                                                      && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                      && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "L")
                    {
                        //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                            && x.OverTimeForm.Department.DepartmentCode == departmentCode
                                                                            && x.OverTimeForm.StartTime >= startFormDate
                                                                            && x.OverTimeForm.Employee.LeaveDate != null
                                                                            && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                            && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "")
                    {
                        //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                            && x.OverTimeForm.Department.DepartmentCode == departmentCode
                                                                            && x.OverTimeForm.StartTime >= startFormDate
                                                                            && (x.OverTimeForm.Employee.LeaveDate > DateTime.Now || x.OverTimeForm.Employee.LeaveDate == null)
                                                                            && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                            && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                    if (StatusData == "ALL")
                    {
                        //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                        _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                         && x.OverTimeForm.Department.DepartmentCode == departmentCode
                                                                         && x.OverTimeForm.StartTime >= startFormDate
                                                                         && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                         && !x.IsDeleted).Select(x => x.FormNo).ToList());
                    }
                }
            }

            List<SignFlowRecModel> _signFlowList = SignFlowRecQueryHelper.GetCurrentSignFlowByFormNumber(_formNoList).ToList();
            if (!onlyPending)
            {
                List<string> _hadDataList = _signFlowList.Select(x => x.FormNumber).ToList();
                _formNoList.RemoveAll(x => _hadDataList.Contains(x));
                _signFlowList.AddRange(SignFlowRecQueryHelper.GetLastSignFlowByFormNumber(_formNoList));
            }

            List<HRPotralFormSignStatus> _signList = new List<HRPotralFormSignStatus>();
            foreach (SignFlowRecModel _signFlow in _signFlowList)
            {
                _signList.Add(new HRPotralFormSignStatus(_signFlow));
            }

            return _signList.OrderBy(x => x.UrgentOrder).ThenByDescending(x => x.FormCreateDate).ToList();
        }

        public List<HRPotralFormSignStatus> GetCurrentSignListByDepartment(string companyCode, string departmentCode, bool onlyPending, DateTime startFormDate, DateTime? endFormDate = null)
        {
            List<string> _formNoList = new List<string>();
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                if (departmentCode == "All")
                {
                    _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                                && x.StartTime >= startFormDate
                                                                && (endFormDate == null || x.StartTime <= endFormDate)
                                                                && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                   && x.StartTime >= startFormDate
                                                                   && (endFormDate == null || x.StartTime <= endFormDate)
                                                                   && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    //2018/12/3 Neo 修改查詢以補刷卡時間查詢，而非填寫日期的區間
                    _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                    && x.PatchCardTime >= startFormDate
                                                                    && (endFormDate == null || x.PatchCardTime <= endFormDate)
                                                                    && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間
                    _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                  && x.LeaveForm.StartTime >= startFormDate
                                                                  && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                  && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                    _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                     && x.OverTimeForm.StartTime >= startFormDate
                                                                     && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                     && !x.IsDeleted).Select(x => x.FormNo).ToList());
                }
                else
                {
                    _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                                && x.Department.DepartmentCode == departmentCode
                                                                && x.StartTime >= startFormDate
                                                                && (endFormDate == null || x.StartTime <= endFormDate)
                                                                && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    _formNoList.AddRange(_db.OverTimeForms.Where(x => x.Company.CompanyCode == companyCode
                                                                   && x.Department.DepartmentCode == departmentCode
                                                                   && x.StartTime >= startFormDate
                                                                   && (endFormDate == null || x.StartTime <= endFormDate)
                                                                   && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    //2018/12/3 Neo 修改查詢以補刷卡時間查詢，而非填寫日期的區間
                    _formNoList.AddRange(_db.PatchCardForms.Where(x => x.Company.CompanyCode == companyCode
                                                                    && x.Department.DepartmentCode == departmentCode
                                                                    && x.PatchCardTime >= startFormDate
                                                                    && (endFormDate == null || x.PatchCardTime <= endFormDate)
                                                                    && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    //2018/12/3 Neo 修改查詢以假單開始結束日期區間查詢，而非填寫日期的區間
                    _formNoList.AddRange(_db.LeaveCancels.Where(x => x.LeaveForm.Company.CompanyCode == companyCode
                                                                  && x.LeaveForm.Department.DepartmentCode == departmentCode
                                                                  && x.LeaveForm.StartTime >= startFormDate
                                                                  && (endFormDate == null || x.LeaveForm.StartTime <= endFormDate)
                                                                  && !x.IsDeleted).Select(x => x.FormNo).ToList());

                    //2018/12/3 Neo 修改查詢以加班單開始結束日期區間查詢，而非填寫日期的區間
                    _formNoList.AddRange(_db.OverTimeCancels.Where(x => x.OverTimeForm.Company.CompanyCode == companyCode
                                                                     && x.OverTimeForm.Department.DepartmentCode == departmentCode
                                                                     && x.OverTimeForm.StartTime >= startFormDate
                                                                     && (endFormDate == null || x.OverTimeForm.StartTime <= endFormDate)
                                                                     && !x.IsDeleted).Select(x => x.FormNo).ToList());
                }
            }

            List<SignFlowRecModel> _signFlowList = SignFlowRecQueryHelper.GetCurrentSignFlowByFormNumber(_formNoList).ToList();
            if (!onlyPending)
            {
                List<string> _hadDataList = _signFlowList.Select(x => x.FormNumber).ToList();
                _formNoList.RemoveAll(x => _hadDataList.Contains(x));
                _signFlowList.AddRange(SignFlowRecQueryHelper.GetLastSignFlowByFormNumber(_formNoList));
            }

            List<HRPotralFormSignStatus> _signList = new List<HRPotralFormSignStatus>();
            foreach (SignFlowRecModel _signFlow in _signFlowList)
            {
                _signList.Add(new HRPotralFormSignStatus(_signFlow));
            }

            return _signList.OrderBy(x => x.UrgentOrder).ThenByDescending(x => x.FormCreateDate).ToList();
        }

        public List<HRPotralSignFlowStatus> GetSignFlowRecord(string formNo, Guid companyId)
        {
            IList<SignFlowRecModel> _signFlowList = SignFlowRecQueryHelper.GetSignFlowByFormNumber(formNo);
            List<HRPotralSignFlowStatus> _signList = new List<HRPotralSignFlowStatus>();

            SignFlowFormLevelRepository _signFlowFormLevelRepository = new SignFlowFormLevelRepository();
            int i = 1;
            foreach (SignFlowRecModel _signFlow in _signFlowList)
            {
                HRPotralSignFlowStatus _signFlowRecord = new HRPotralSignFlowStatus(_signFlow, companyId);
                _signFlowRecord.FormLevelName = _signFlowFormLevelRepository.FindOne(x => x.FormLevelID == _signFlowRecord.FormLevelID).Name;
                _signFlowRecord.OrderNumber = i++;
                _signList.Add(_signFlowRecord);
            }

            return _signList;
        }

        public List<HRPotralLeaveSummary> GetCurrentLeaveSignListByDepartment(string companyCode, string departmentCode, DateTime startFormDate, DateTime endFormDate)
        {
            List<string> _formNoList = new List<string>();
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                _formNoList.AddRange(_db.LeaveForms.Where(x => x.Company.CompanyCode == companyCode
                                                               && (departmentCode == "All" ? true : x.Department.DepartmentCode == departmentCode)
                                                               && ((x.StartTime >= startFormDate && x.StartTime <= endFormDate) || (x.EndTime >= startFormDate && x.EndTime <= endFormDate))
                                                               && !x.IsDeleted)
                                                    .Select(x => x.FormNo).ToList());
            }

            List<SignFlowRecModel> _signFlowList = SignFlowRecQueryHelper.GetCurrentSignFlowByFormNumber(_formNoList).ToList();
            List<string> _hadDataList = _signFlowList.Select(x => x.FormNumber).ToList();
            _formNoList.RemoveAll(x => _hadDataList.Contains(x));
            _signFlowList.AddRange(SignFlowRecQueryHelper.GetLastSignFlowByFormNumber(_formNoList));

            List<HRPotralLeaveSummary> _signList = new List<HRPotralLeaveSummary>();
            foreach (SignFlowRecModel _signFlow in _signFlowList)
            {
                _signList.Add(new HRPotralLeaveSummary(_signFlow));
            }

            return _signList.OrderBy(x => x.UrgentOrder).ThenByDescending(x => x.FormCreateDate).ToList();
        }

        public void Dispose()
        {
            _signFlowRecQueryHelper.Dispose();
        }
    }
}