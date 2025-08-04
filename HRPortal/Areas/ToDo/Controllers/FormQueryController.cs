using ClosedXML.Excel;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class FormQueryController : MultiDepEmpController
    {
        public ActionResult Index()
        {
            string statusData = "";
            if (CurrentUser.Employee.LeaveDate != null && CurrentUser.Employee.LeaveDate < DateTime.Now)
            {
                statusData = "L";
                ViewBag.isLeaveNotShow = "1";
            }
            string beginDate = DateTime.Now.Year + "/01/01";
            ViewBag.StartTime = beginDate;
            ViewBag.EndTime = Convert.ToDateTime(beginDate).AddYears(1).AddDays(-1).ToString("yyyy/MM/dd");

            //20170510 Start 增加 by Daniel
            //個人假單查詢增加假別篩選
            ViewData["AbsentCodeList"] = GetAbsentCodeList();
            //20170510 End

            ViewData["FormTypelist"] = GetFormTypeList();
            ViewData["DepartmentList"] = GetDepartmentList2(CurrentUser.SignDepartmentID);
            ViewData["EmployeeList"] = GetEmployeetList2(CurrentUser.SignDepartmentID.ToString(), CurrentUser.EmployeeNO, statusData, "ID");
            ViewData["StatuslistData"] = GetStatusDataList();

            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.RoleParams;

            ViewBag.isALL = isAdmin || isHR;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetFormQuerySummary(FormQuerySummaryViewModel viewmodel)
        {
            var result = await Query(viewmodel.EmpID, viewmodel.DeptID, viewmodel.StatusData, DateTime.Parse(viewmodel.BeginDate), DateTime.Parse(viewmodel.EndDate), viewmodel.course_category, viewmodel.AbsentCode);

            int currentPage = viewmodel.page < 1 ? 1 : viewmodel.page;
            return PartialView("_FormQuerySummary", result.ToPagedList(currentPage, currentPageSize));
        }

         /// <summary>
        /// 
        /// </summary>
        /// <param name="employee">員工主檔 ID 字串，多筆會用逗號分隔。</param>
        /// <param name="departmentcode"></param>
        /// <param name="StatusData"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="formtype"></param>
        /// <param name="AbsentCode"></param>
        /// <returns></returns>
        public async Task<List<HRPotralFormSignStatus>> Query(string employee, string departmentcode, string StatusData, DateTime startTime, DateTime endTime, string formtype, string AbsentCode, string LanguageCookie = "zh-TW")
        {
            string[] employeeguid;
            List<Employee> employees = new List<Employee>();
            if (!string.IsNullOrWhiteSpace(employee))   //配合跨公司簽核，依照公司分群後查詢資料。
            {
                employeeguid = employee.TrimEnd(',').Split(',');
                employees = Services.GetService<EmployeeService>().GetAll().Where(x => employeeguid.Contains(x.ID.ToString())).ToList();
            }
            string[] departmentid;
            List<Department> department = new List<Department>();
            if (!string.IsNullOrWhiteSpace(departmentcode))   //配合跨公司簽核，依照公司分群後查詢資料。
            {
                departmentid = departmentcode.TrimEnd(',').Split(',');
                department = Services.GetService<DepartmentService>().GetAll().Where(x => departmentid.Contains(x.ID.ToString())).ToList();
            }

            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = new List<HRPotralFormSignStatus>();

                if (!string.IsNullOrWhiteSpace(employee))   //配合跨公司簽核，依照公司分群後查詢資料。
                {
                    foreach (var c in employees.Select(s => s.Company.CompanyCode).Distinct())
                    {
                        var employeenos = string.Join(",", employees.Where(x => x.Company.CompanyCode == c).Select(s => s.EmployeeNO).ToList());
                        _result.AddRange(_queryHelper.GetCurrentSignListByEmployee(c, employeenos, false, startTime, endTime.AddDays(1)));
                    }
                }
                else if (!string.IsNullOrWhiteSpace(departmentcode))
                {
                    foreach (var c in department.Select(s => s.Company.CompanyCode).Distinct())
                    {
                        var departments = string.Join(",", department.Where(x => x.Company.CompanyCode == c).Select(s => s.DepartmentCode).ToList());
                       _result.AddRange(_queryHelper.GetCurrentSignListByDepartment(c, departments, false, startTime, endTime.AddDays(1)));
                    }
                }

                if (!string.IsNullOrWhiteSpace(formtype))
                {
                    _result = _result.Where(x => x.FormType == (FormType)(Enum.Parse(typeof(FormType), formtype))).ToList();
                }

                _result = _result.OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();

                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                //20170510 Start 修改 by Daniel
                //過濾假別，將假別代碼與篩選條件不同的移除，同時進行Summary資料的處理
                _result.RemoveAll(item => _summaryBuilder.BuildSummaryForFormQuery(item, AbsentCode));
                //20170510 End

                List<string> formNoList = _result.Select(s => s.FormNo).Distinct().ToList();

                foreach (var c in employees.Select(s => s.Company.CompanyCode).Distinct())
                {
                    var employeenos = string.Join(",", employees.Where(x => x.Company.CompanyCode == c).Select(s => s.EmployeeNO).ToList());
                    //var departments = string.Join(",", department.Where(x => x.Company.CompanyCode == c).Select(s => s.DepartmentCode).ToList());

                    if (string.IsNullOrWhiteSpace(formtype) || formtype == "Leave")
                    {
                        #region Leave

                        List<AbsentFormData> absentFormDataList = await HRMApiAdapter.GetAbsentFormData(c, employeenos, "", StatusData, startTime, endTime, LanguageCookie);

                        absentFormDataList = absentFormDataList.Where(x => !formNoList.Contains(x.FormNo)).ToList();
                        //20170510 Start 增加 by Daniel
                        //過濾假別
                        if (AbsentCode != null)
                        {
                            FilterLeaveFormByAbsentCode(ref absentFormDataList, AbsentCode);
                        }
                        //20170510 End
                                                
                        string tempFormNo = "0";
                        int formNoOrder = 0;
                        string[] sArray ;//= _item.FormSummary.Split(new char[2] { '(', ')' });
                        foreach (var item in absentFormDataList)
                        {
                            if (tempFormNo != item.FormNo)
                            {
                                formNoOrder = 0;
                            }
                            else if (tempFormNo == item.FormNo)
                            {
                                formNoOrder++;
                            }
                            tempFormNo = item.FormNo;
                            sArray = item.Summary.Split(new char[2] { '(', ')' });
                            _result.Add(new HRPotralFormSignStatus()
                            {
                                FormType = FormType.Leave,
                                FormCreateDate = item.CreateDate,// item.BeginTime,
                                CompanyCode = item.CompanyCode,
                                CompanyName = item.CompanyName,
                                DepartmentCode = item.DeptCode,
                                DepartmentName = item.DeptName,
                                SenderEmployeeNo = item.EmpID,
                                SenderEmployeeName = item.EmpName,
                                FormSummary = item.Summary,//sArray[1],// item.Summary,
                                FormNo = item.FormNo,
                                FormNoOrder = formNoOrder,
                                BeDate = item.BeginTime,
                                DepartmentEnglishName = item.DeptNameEN,
                                SenderEmployeeEnglishName = item.EmpNameEN,
                                AbsentName = sArray[0]
                            });
                        }

                        #endregion Leave
                    }
                    if (string.IsNullOrWhiteSpace(formtype) || formtype == "OverTime")
                    {
                        #region OverTime

                        List<AbsentFormData> overtimeFormDataList = await HRMApiAdapter.GetOverTimeFormData(c, employeenos, departmentcode, StatusData, startTime, endTime);
                        overtimeFormDataList = overtimeFormDataList.Where(x => !formNoList.Contains(x.FormNo)).ToList();
                        string[] sArray;//= _item.FormSummary.Split(new char[2] { '(', ')' });
                        foreach (var item in overtimeFormDataList)
                        {
                            sArray = item.Summary.Split(new char[2] { '(', ')' });
                            _result.Add(new HRPotralFormSignStatus()
                            {
                                FormType = FormType.OverTime,
                                FormCreateDate = item.CreateDate,// item.BeginTime,
                                CompanyCode = item.CompanyCode,
                                CompanyName = item.CompanyName,
                                DepartmentCode = item.DeptCode,
                                DepartmentName = item.DeptName,
                                SenderEmployeeNo = item.EmpID,
                                SenderEmployeeName = item.EmpName,
                                FormSummary = sArray[1],//item.Summary,
                                FormNo = item.FormNo,
                                FormNoOrder = 0,
                                BeDate = item.BeginTime,
                                DepartmentEnglishName = item.DeptNameEN,
                                SenderEmployeeEnglishName = item.EmpNameEN,
                                AbsentName = sArray[0]
                            });
                        }

                        #endregion OverTime
                    }

                }
                return _result.OrderByDescending(o => o.BeDate).ToList();
            }
        }
       
        //20170510 Start 增加 by Daniel
        //個人假單查詢增加假別篩選
        /// <summary>
        /// 取得所有假別清單
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetAbsentCodeList(string AbsentCode = "")
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<SelectListItem> listItem = new List<SelectListItem>();
            if (Session["Absents"] != null)
            {
                //Dictionary<string, string> dictAbsents = Session["Absents"] as Dictionary<string, string>;
                List<AbsentType> listAbsents = (List<AbsentType>)Session["Absents"];
                
                listItem.Add(new SelectListItem { Text = getLanguageCookie == "en-US" ? "All" : "全部", Value = "" });
                foreach (var item in listAbsents)
                {
                    listItem.Add(new SelectListItem { Text = getLanguageCookie == "en-US" ? item.AbsentEnglishName : item.AbsentName, Value = item.AbsentCode.Trim(), Selected = (item.AbsentCode.Trim() == AbsentCode ? true : false) });
                }

            }
            return listItem;
        }
        //20170510 End

        /// <summary>
        /// 取得員工在離職狀態 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetStatusDataList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = HRPortal.MultiLanguage.Resource.Option_ShowActiveEmployees, Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = HRPortal.MultiLanguage.Resource.Option_ShowInActiveEmployees, Value = "L", Selected = (selecteddata == "L" ? true : false) });
            listItem.Add(new SelectListItem { Text = HRPortal.MultiLanguage.Resource.Option_ShowAllEmployees, Value = "ALL", Selected = (selecteddata == "ALL" ? true : false) });

            //listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }
        
        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<MutiSelectListItem> GetDepartmentList(string selecteddata)
        {
            //使用者角色
            GetRole();
            if (selecteddata == "")
            {
                selecteddata = CurrentUser.DepartmentCode;
            }
            List<MutiSelectListItem> listItem = new List<MutiSelectListItem>();
            List<Department> data = null;
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (isHR || isAdmin)
            {
                data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

                #region 管理員 或 人資
                foreach (var item in data)
                {
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new MutiSelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new MutiSelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                }
                #endregion
            }
            else
            {
                data = CurrentUser.SignDepartments;

                if (data.Count > 0)
                {
                    #region 主管
                    foreach (var item in data)
                    {
                        if (getLanguageCookie == "en-US")
                        {
                            listItem.Add(new MutiSelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                        else
                        {
                            listItem.Add(new MutiSelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 其它
                    Department _signdepartment = Services.GetService<DepartmentService>().GetDepartmentByID(CurrentUser.SignDepartmentID);
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new MutiSelectListItem { Text = _signdepartment.DepartmentEnglishName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new MutiSelectListItem { Text = _signdepartment.DepartmentName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    #endregion
                }
            }

            return listItem;
        }
       
        /// <summary>
        /// 取得簽核類別
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetFormTypeList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (getLanguageCookie == "en-US")
            {
                listItem.Add(new SelectListItem { Text = "All", Value = "", Selected = (selecteddata == "" ? true : false) });
            }
            else
            {
                listItem.Add(new SelectListItem { Text = "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
            }
            //listItem.Add(new SelectListItem { Text = "假單申請", Value = ((int)FormType.Leave).ToString(), Selected = (selecteddata == ((int)FormType.Leave).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "加班申請", Value = ((int)FormType.OverTime).ToString(), Selected = (selecteddata == ((int)FormType.OverTime).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "補刷卡申請", Value = ((int)FormType.PatchCard).ToString(), Selected = (selecteddata == ((int)FormType.PatchCard).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "假單刪除申請", Value = ((int)FormType.LeaveCancel).ToString(), Selected = (selecteddata == ((int)FormType.LeaveCancel).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "加班單刪除申請", Value = ((int)FormType.OverTimeCancel).ToString(), Selected = (selecteddata == ((int)FormType.OverTimeCancel).ToString() ? true : false) });
            OptionListHelper _optionListHelper = new OptionListHelper();
            listItem.AddRange(_optionListHelper.GetOptionList("FormType"));
            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;

            return listItem;
        }

        private void FilterLeaveFormByAbsentCode(ref List<AbsentFormData> listAbsentFormData,string AbsentCode)
        {
            if (!string.IsNullOrWhiteSpace(AbsentCode))
            {
                listAbsentFormData = listAbsentFormData.Where(x => x.AbsentCode == AbsentCode).ToList();
            }
        }

        /// <summary>
        /// 使用者角色
        /// </summary>
        public bool GetRole()
        {
            bool result = false;

            try
            {
                Role roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();

                if (roleData != null)
                {
                    if (!string.IsNullOrEmpty(roleData.RoleParams))
                    {
                        dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
                        isHR = (roleParams.is_hr != null && roleParams.is_hr);
                        isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

         /// <summary>
        /// 匯出資料
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Export(FormQuerySummaryViewModel viewmodel)
        {

            // FormQuerySummaryViewModel model = await GetSummaryDetail(viewmodel);
            var model = await Query(viewmodel.EmpID, viewmodel.DeptID, viewmodel.StatusData, DateTime.Parse(viewmodel.BeginDate), DateTime.Parse(viewmodel.EndDate), viewmodel.course_category, viewmodel.AbsentCode);

            ResourceManager _resourceManager = new ResourceManager("HRPortal.MultiLanguage.Resource", typeof(HRPortal.MultiLanguage.Resource).Assembly);
            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            // Create the workbook
            XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("個人假單查詢");
            //自動換行
            sheet.Style.Alignment.WrapText = true;

            //水平、垂直置中
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Row(1).Height = 50;
            sheet.Range(1, 1, 1, 7).Merge();
            sheet.Cell(1, 1).Value = "個人假單查詢 \n" + viewmodel.BeginDate + "~" + viewmodel.EndDate;

            sheet.Range(1, 1, 1, 1).Style.Font.SetFontSize(20);
            sheet.Range(1, 1, 1, 1).Style.Font.SetBold(true);

            sheet.Columns(1, 7).AdjustToContents();
            sheet.Column(1).Width = 20;
            sheet.Column(2).Width = 20;
            sheet.Column(3).Width = 50;
            sheet.Column(4).Width = 30;
            sheet.Column(5).Width = 20;
            sheet.Column(6).Width = 20;
            sheet.Column(7).Width = 20;
            string[] listColumn = new string[] { "填表日期", "類型", "內容", "部門", "申請人", "簽核者", "簽核狀態" };

            int rowIdx = 2;

            #region 建立欄位表頭。
            for (int i = 0; i < listColumn.Count(); i++)
            {
                sheet.Cell(rowIdx, i + 1).Value = listColumn[i];
            }
            #endregion
            rowIdx++;

            string signtype = string.Empty;
            foreach (var item in model)
            {
                signtype = string.Empty;
                sheet.Cell(rowIdx, 1).Value = item.FormCreateDate.ToShortDateString();
                sheet.Cell(rowIdx, 2).Value = _resourceManager.GetString(item.FormType.ToString()) ?? item.FormType.ToString();
                sheet.Cell(rowIdx, 3).Value = item.FormSummary;
                sheet.Cell(rowIdx, 4).Value = item.DepartmentName;
                sheet.Cell(rowIdx, 5).Value = item.SenderEmployeeName;
                if (!string.IsNullOrEmpty(item.SignFlowID))
                {
                    signtype = item.SignStatus == "W" ? (item.SignType == "S" ? "未送出" : "未簽核") : (item.SignStatus == "R" ? "退回" : (item.SignStatus == "S" ? "己送出" : (item.SignStatus == "B" ? "修改" : "簽核完成")));
                    sheet.Cell(rowIdx, 6).Value = item.SignerEmployeeName;
                    sheet.Cell(rowIdx, 7).Value = signtype;
                }
                else if (string.IsNullOrEmpty(item.SignFlowID) && item.FormNo.Substring(0, 1) != "P")
                {
                    sheet.Cell(rowIdx, 6).Value = "人資代簽核";
                    sheet.Cell(rowIdx, 7).Value = "簽核完成";
                }
                else
                {
                    sheet.Cell(rowIdx, 6).Value = string.Empty;
                    sheet.Cell(rowIdx, 7).Value = string.Empty;
                }
                rowIdx++;

            }

            //框線
            sheet.Range(2, 1, rowIdx - 1, 7).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            sheet.Range(2, 1, rowIdx - 1, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            sheet.Range(2, 1, rowIdx - 1, 7).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            sheet.Range(2, 1, rowIdx - 1, 7).Style.Border.RightBorder = XLBorderStyleValues.Thin;

            //輸出Excel
            MemoryStream excelStream = new MemoryStream();
            workbook.SaveAs(excelStream);
            excelStream.Position = 0;
            string exportFileName = string.Concat("個人假單查詢 _", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
            return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFileName);

        }
    }
}

