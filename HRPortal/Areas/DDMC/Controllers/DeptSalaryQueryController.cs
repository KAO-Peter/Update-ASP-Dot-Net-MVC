using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.Utility;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

//20180319 Daniel DDMC增加部門主管可以查詢部門人員薪資的功能
namespace HRPortal.Areas.DDMC.Controllers
{
    public class DeptSalaryQueryController : BaseController
    {
        private bool isGetRole = false, isHR = false, isAdmin = false;
        string isHrOrAdmin = "";
        //
        // GET: /DDMC/DeptSalaryQuery/
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1, string SelectedDepartment = "", string SelectedEmployee = "", string SelectedStatusListData = "", string BeginDate = "", string EndDate = "")
        {

            //使用者角色
            GetRole();
           
            int currentPage = page < 1 ? 1 : page;
            SalaryQueryForHRViewModel viewModel = new SalaryQueryForHRViewModel();
           
            //取得可查詢薪資的所有人員資料
            List<Employee> salaryEmployeeList = await GetSalaryEmpAll(isHrOrAdmin);

            //20190221 Daniel 改為HR可以選擇在職離職人員，其他人還是只能查在職的
            if (!isHR && !isAdmin)
            {
                SelectedStatusListData = ""; //不是HR也不是Admin，其他人只能查在職的
            }

            //20190222 Daniel 過濾在職離職，移到前面處裡
            SelectedStatusListData = SelectedStatusListData ?? "";
            salaryEmployeeList = FilterSalaryEmployeeListByLeaveStatus(salaryEmployeeList, SelectedStatusListData);

            bool flagGetSalaryData = false;

            //有傳入EmployeeNo時
            Employee selectedEmp = new Employee();
            if (!string.IsNullOrWhiteSpace(SelectedEmployee))
            {
                selectedEmp = salaryEmployeeList.Where(x => x.EmployeeNO == SelectedEmployee).FirstOrDefault();
                //檢查傳入的EmployeeNo是否在可查詢人員範圍內
                if (selectedEmp != null)
                {
                    viewModel.SelectedEmployeeName = selectedEmp.EmployeeName;
                    viewModel.SelectedEmployeeEnglishName = selectedEmp.EmployeeEnglishName;
                    flagGetSalaryData = true;
                }
            }

            viewModel.flagShowSalaryData = flagGetSalaryData;

            //取得部門列表清單
            viewModel.DepartmentListData = GetSalaryDepartmentList(salaryEmployeeList, SelectedDepartment, SelectedStatusListData);
            string QueryDeptID = "";
            if (string.IsNullOrWhiteSpace(SelectedDepartment))
            {
                QueryDeptID = (viewModel.DepartmentListData.FirstOrDefault() != null) ? viewModel.DepartmentListData.First().Value : "";
            }
            else
            {
                QueryDeptID = SelectedDepartment;
            }

            //取得人員列表清單
            //20180615 Daniel 改為只看在職的
            //20190221 Daniel 改為HR可以選擇在職離職人員，其他人還是只能查在職的
            viewModel.EmployeeListData = GetSalaryEmployeeList(salaryEmployeeList, QueryDeptID, SelectedEmployee, SelectedStatusListData);
            //}

            if (BeginDate == "" && EndDate == "")
            {
                DateTime ED = DateTime.Now;
                DateTime BD = ED.AddDays(-90);
                BeginDate = BD.ToString("yyyy/MM");
                EndDate = ED.ToString("yyyy/MM");
            }

            ViewBag.StartTime = BeginDate;
            ViewBag.EndTime = EndDate;

            //取得薪資資料
            if (flagGetSalaryData)
            {
                viewModel.SalaryFormListData = await GetSalaryData(SelectedEmployee, BeginDate, EndDate, SelectedDepartment, flagGetSalaryData);
            }

            viewModel.SelectedDepartment = SelectedDepartment;
            viewModel.SelectedEmployee = SelectedEmployee;

            viewModel.StatuslistDataData = GetStatusDataList(SelectedStatusListData);

            ViewBag.LanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(string PWdata, string btnQuery, string btnClear, string ExportsalaryYMData, string ExportFormNo, string ExportEmployeeNo, string BeginDate, string EndDate, string searchkey, string SelectedDepartment, string SelectedEmployee, string SelectedStatusListData="", int page = 1)
        {
           
            int currentPage = page < 1 ? 1 : page;

            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;

            SalaryQueryForHRViewModel viewModel = new SalaryQueryForHRViewModel();
            /*
            if (!string.IsNullOrWhiteSpace(PWdata) && checkPassword(PWdata))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            */
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(searchkey) && !string.IsNullOrWhiteSpace(EndDate) && string.IsNullOrWhiteSpace(BeginDate))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                TempData["message"] = "請輸入查詢起始日期";
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }

            else if (!string.IsNullOrWhiteSpace(searchkey))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = BeginDate,
                    EndDate = EndDate,
                    SelectedDepartment = SelectedDepartment,
                    SelectedEmployee = SelectedEmployee,
                    SelectedStatusListData = SelectedStatusListData
                });
            }

            //底下是點下載才會進入的地方
            //取得可查詢薪資的所有人員資料，檢查權限是否可查詢該員工薪資

            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<Employee> salaryEmployeeList = await GetSalaryEmpAll(isHrOrAdmin);
            if (!string.IsNullOrWhiteSpace(ExportFormNo) && !string.IsNullOrWhiteSpace(ExportsalaryYMData) && !string.IsNullOrWhiteSpace(ExportEmployeeNo))
            {
                Employee selectedEmp = salaryEmployeeList.Where(x => x.EmployeeNO == SelectedEmployee).FirstOrDefault();
                if (selectedEmp != null) //有權限才能產出PDF
                {

                    Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                    /* 主管查詢，PDF不需加密碼保護
                    var empData = await HRMApiAdapter.GetEmployeeData(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.EmployeeNO);
                    var IDNumber = empData.IDNumber.Substring(empData.IDNumber.Length - 6);
                    var Birthday = empData.Birthday.Value.ToString("MMdd");
                    var userPW = IDNumber + Birthday;
                    */
                    byte[] data = await ExportPDF(ExportFormNo, ExportEmployeeNo, languageCookie);
                    /*
                    using (MemoryStream input = new MemoryStream(data))
                    {
                    
                        using (MemoryStream output = new MemoryStream())
                        {
                            //加密
                            PdfReader reader = new PdfReader(input);
                            PdfEncryptor.Encrypt(reader, output, true, userPW, "owner", PdfWriter.AllowPrinting);

                            data = output.ToArray();
                       
                            WriteLog("Export-FormNO:" + ExportFormNo);
                            Response.Cache.SetCacheability(HttpCacheability.NoCache);
                            return File(data, "application/pdf", ExportsalaryYMData.Replace(@"/", "") + "個人薪資明細檔.pdf");

                        }
                    }
                    */
                    //紀錄Log
                    WriteLog("Export-FormNO:" + ExportFormNo + "，Export-EmployeeNo:" + ExportEmployeeNo + "，Export-EmployeeName:" + selectedEmp.EmployeeName);

                    string fileName = ExportEmployeeNo + "-" + (languageCookie == "en-US" ? selectedEmp.EmployeeEnglishName : selectedEmp.EmployeeName) + "-" + ExportsalaryYMData.Replace(@"/", "") + "-" + HRPortal.MultiLanguage.Resource.SalaryReceipt_Title.Replace(" ","") + ".pdf";
                    return File(data, "application/pdf", fileName);
                    //return File(data, "application/pdf", ExportEmployeeNo + "-" + selectedEmp.EmployeeName + "-" + ExportsalaryYMData.Replace(@"/", "") + "-個人薪資明細檔.pdf");
                }
                else
                {
                    WriteLog("Export-FormNO:" + ExportFormNo + "|Export-EmployeeNo:" + ExportEmployeeNo + "|無權限");
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.Default))
                        {
                            sw.Write("抱歉，目前還沒開放查詢此員工薪資的權限");
                            sw.Flush();
                            sw.BaseStream.Seek(0, SeekOrigin.Begin);
                            //sw.Close();
                            return File(ms.GetBuffer(), "text/plain", "查詢錯誤.txt");
                        }
                    }
                }
            }

            /*
            viewModel.SelectedDepartment = SelectedDepartment;
            viewModel.SelectedEmployee = SelectedEmployee;

            if (BeginDate == "" && EndDate == "")
            {
                DateTime ED = DateTime.Now;
                DateTime BD = ED.AddDays(-90);
                BeginDate = BD.ToString("yyyy/MM");
                EndDate = ED.ToString("yyyy/MM");
            }

            ViewBag.StartTime = BeginDate;
            ViewBag.EndTime = EndDate;

            //取得部門列表清單
            viewModel.DepartmentListData = GetSalaryDepartmentList(salaryEmployeeList, SelectedDepartment, SelectedStatusListData);
            string QueryDeptID = "";
            if (string.IsNullOrWhiteSpace(SelectedDepartment))
            {
                QueryDeptID = (viewModel.DepartmentListData.FirstOrDefault() != null) ? viewModel.DepartmentListData.First().Value : "";
            }
            else
            {
                QueryDeptID = SelectedDepartment;
            }

            //取得人員列表清單
            viewModel.EmployeeListData = GetSalaryEmployeeList(salaryEmployeeList, QueryDeptID, SelectedEmployee);
            //}
            */

            ViewBag.LanguageCookie = languageCookie;

            return View(viewModel);
            
        }

        private async Task<List<SalaryQueryViewModel>> GetSalaryData(string EmployeeNo, string BeginDate = "", string EndDate = "", string SelectedDepartment = "", bool flagGetSalaryData = false)
        {

            string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
            ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
            if (Session["_OutTime"] != null && DateTime.Parse(Session["_OutTime"].ToString()) > DateTime.Now)
            {
                ViewBag.Status = "true";
                Session["_OutTime"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
            }
            else
            {
                ViewBag.Status = "false";
            }

            List<SalaryQueryViewModel> SalaryFormList = new List<SalaryQueryViewModel>();
            if (flagGetSalaryData)
            {
                var ds = await HRMApiAdapter.GetEmployeeSalaryFormNoAllStatus(CurrentUser.CompanyCode, EmployeeNo, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));

                foreach (var item in ds)
                {
                    SalaryFormList.Add(new SalaryQueryViewModel
                    {
                        FormNo = item.FormNo,
                        SalaryYM = item.SalaryYM
                    });

                }
            }
            ViewBag.ExportEmployeeNo = EmployeeNo;

            //ViewBag.StartTime = BeginDate;
            //ViewBag.EndTime = EndDate;
            return SalaryFormList;
        }

        [HttpPost]
        public async Task<byte[]> ExportPDF(string formNo, string EmployeeNo = "", string languageCookie = "zh-TW")
        {

            //20180314 Daniel 除月薪已外的薪資批號都不顯示考勤，最後兩碼是"-2"的才是月薪
            bool flagShowAbsentInfo = false;
            if (formNo.Substring(formNo.Length - 2) == "-2")
            {
                flagShowAbsentInfo = true;
            }

            string companyName = languageCookie == "en-US" ? CurrentUser.CompanyEnglishName : CurrentUser.CompanyName;
            companyName = companyName.Replace("&", "&amp;"); 

            string htmlText;
            string htmlToConvertSalaryInfo = RenderViewAsString("_SalaryInfo", await GetSalaryInfo(formNo, EmployeeNo, languageCookie));

            string htmlToConvertSalaryItems = RenderViewAsString("_SalaryItems", await GetSalaryItems(formNo, EmployeeNo, languageCookie));

            string htmlToConvertAbsentItems = (flagShowAbsentInfo) ? RenderViewAsString("_AbsentItems", await GetAbsentItems(formNo, EmployeeNo)) : "";

            string htmlToConvertMemo = (flagShowAbsentInfo) ? RenderViewAsString("_MemoAll", null) : RenderViewAsString("_MemoSalaryOnly", null);

            //避免當htmlText無任何html tag標籤的純文字時，轉PDF時會掛掉，所以一律加上<p>標籤
            htmlText = "<div style='text-align:center; font-size:18px; font-family:DFKai-sb; margin-bottom:20px; height:80px;'><div>" + companyName + "</div><div>" + HRPortal.MultiLanguage.Resource.SalaryReceipt_Title + "</div></div><div style='width:100%; margin-left:auto; margin-right:auto; font-family:DFKai-sb; font-size:12px;'>" + htmlToConvertSalaryInfo + htmlToConvertSalaryItems + htmlToConvertAbsentItems + htmlToConvertMemo + "</div>";

            //換clas
            htmlText = htmlText.Replace("<table", "<table cellpadding=\"0\" cellspacing=\"0\"  border=\"1\" style=\"border: 1 solid #000000;width:100%;max-width:100%;margin-bottom:20px;\"").Replace("<th>", "<th style=\"padding: 8px;height:35\">&nbsp;").Replace("<th style=\"text-align:center\"", "<th style=\"text-align:center;padding: 8px;height:35\" ").Replace("<td style=\"text-align:right\">", "<td style=\"text-align:right;padding: 8px;height:35\">").Replace("<td>", "<td style=\"text-align:right\">&nbsp;").Replace("</td>", "&nbsp;</td>");

            using (MemoryStream outputStream = new MemoryStream())
            {
                //要把PDF寫到哪個串流
                byte[] data = Encoding.UTF8.GetBytes(htmlText);//字串轉成byte[]
                MemoryStream msInput = new MemoryStream(data);
                Document doc = new Document();//要寫PDF的文件，建構子沒填的話預設直式A4
                PdfWriter writer = PdfWriter.GetInstance(doc, outputStream);

                //指定文件預設開檔時的縮放為100%
                PdfDestination pdfDest = new PdfDestination(PdfDestination.XYZ, 0, doc.PageSize.Height, 1f);
                //開啟Document文件
                doc.Open();
                //使用XMLWorkerHelper把Html parse到PDF檔裡
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msInput, null, Encoding.UTF8, new UnicodeFontFactory());
                //將pdfDest設定的資料寫到PDF檔
                PdfAction action = PdfAction.GotoLocalPage(1, pdfDest, writer);
                writer.SetOpenAction(action);
                // writer.Close();
                doc.Close();
                msInput.Close();
                outputStream.Close();
                byte[] pdfFile = outputStream.ToArray();
                //回傳PDF檔案
                return pdfFile;
            }

        }

        /// <summary>
        /// 讀HTML
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string RenderViewAsString(string viewName, object model)
        {
            // create a string writer to receive the HTML code
            StringWriter stringWriter = new StringWriter();

            // get the view to render
            ViewEngineResult viewResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
            // create a context to render a view based on a model
            ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    new ViewDataDictionary(model),
                    new TempDataDictionary(),
                    stringWriter
                    );

            // render the view to a HTML code
            viewResult.View.Render(viewContext, stringWriter);

            // return the HTML code
            return stringWriter.ToString();
        }


        private async Task<EmployeeSalaryInfo> GetSalaryInfo(string formNo, string employeeno = "", string LanguageCookie = "zh-TW")
        {

            EmployeeSalaryInfo _result = await HRMApiAdapter.GetEmployeeSalaryInfo(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            _result.LanguageCookie = LanguageCookie;
            return _result;
        }

        private async Task<EmployeeSalaryItems> GetSalaryItems(string formNo, string employeeno = "", string LanguageCookie = "zh-TW")
        {
            EmployeeSalaryItems _result = await HRMApiAdapter.GetEmployeeSalaryDetail(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            _result.LanguageCookie = LanguageCookie;
            return _result;
        }

        private async Task<EmployeeAbsentItemsDDMC> GetAbsentItems(string formNo, string employeeno = "")
        {

            EmployeeAbsentItemsDDMC _result = await HRMApiAdapter.GetEmployeeAbsentDetailDDMC(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        /// <summary>
        /// 取得使用者是否具有HR或Admin的角色(isHR、isAdmin)
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
                        isHrOrAdmin = (isHR) || (isAdmin) ? "1" : "";
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            ViewData["isHR"] = isHR;
            ViewData["isAdmin"] = isAdmin;
            return result;
        }

        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata)
        {
            if (selecteddata == "")
            {
                //selecteddata = CurrentUser.DepartmentCode;
                selecteddata = CurrentUser.DepartmentID.ToString();
            }
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = null;
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (isHR || isAdmin)
            {
                /*
                if (CurrentUser.CompanyCode == "1010") //總公司的HR或Admin可以看到所有部門
                {
                    data = Services.GetService<DepartmentService>().GetAllLists().OrderBy(x => x.DepartmentCode).ToList();
                }
                else
                {
                    data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
                }
                */

                data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
               
                #region 管理員 或 人資
                foreach (var item in data)
                {
                    if (getLanguageCookie == "en-US")
                    {
                        //listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.ID.ToString(), Selected = (selecteddata == item.ID.ToString() ? true : false) });
                    }
                    else
                    {
                        //listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.ID.ToString(), Selected = (selecteddata == item.ID.ToString() ? true : false) });

                    }
                }
                #endregion
            }

            return listItem;
        }

        /// <summary>
        /// 取得可查詢薪資部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetSalaryDepartmentList(List<Employee> SalaryEmpListAll, string selectedDeptID, string selectedStatus = "")
        {
            //使用者角色
            //GetRole();
            if (string.IsNullOrWhiteSpace(selectedDeptID))
            {
                //selecteddata = CurrentUser.DepartmentCode;
                selectedDeptID = CurrentUser.DepartmentID.ToString();
            }

            /*
            if (string.IsNullOrWhiteSpace(selectedStatus))
            {
                //預設是只看在職的
                selectedStatus = "";
            }
            */

            //過濾在職離職，前面也有處理，這邊先不移除
            selectedStatus = selectedStatus ?? "";
            List<Employee> empList = FilterSalaryEmployeeListByLeaveStatus(SalaryEmpListAll, selectedStatus);

            List<SelectListItem> listItem = new List<SelectListItem>();
            //取出所有部門物件
            List<Department> data = empList.Select(x => x.Department).Distinct().ToList();

            //按照部門代碼排序
            data = data.OrderBy(y => y.DepartmentCode).ToList();

            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            
            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = getLanguageCookie == "en-US" ? item.DepartmentEnglishName : item.DepartmentName, Value = item.ID.ToString(), Selected = (item.ID.ToString() == selectedDeptID ? true : false) });

            }

            return listItem;
        }

        //過濾在職離職狀態
        List<Employee> FilterSalaryEmployeeListByLeaveStatus(List<Employee> SalaryEmpListAll, string selectedStatus = "")
        {

            List<Employee> empList = new List<Employee>();

            DateTime now = DateTime.Now.Date;

            switch (selectedStatus)
            {
                case "": //只看在職人員
                    empList = SalaryEmpListAll.Where(x => (!x.LeaveDate.HasValue) || (x.LeaveDate.HasValue && x.LeaveDate.Value >= now)).ToList();
                    break;
                case "L": //只看離職人員
                    empList = SalaryEmpListAll.Where(x => x.LeaveDate.HasValue && x.LeaveDate.Value < now).ToList();
                    break;
                default: //全部(在職+離職)
                    empList = SalaryEmpListAll;
                    break;
            }

            return empList;

        }

        /// <summary>
        /// 給下拉式選單讀取員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetEmployee(string DepartmentId, string StatusData)
        {
            List<SelectListItem> result = GetSalaryEmployeeList((List<Employee>)TempData["salaryEmployeeListAll"], DepartmentId, "", StatusData);
            TempData.Keep("salaryEmployeeListAll");
            return Json(result, JsonRequestBehavior.AllowGet);
        }


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

            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }

        private async Task<List<Employee>> GetSalaryEmpAll(string isHrOrAdmin)
        {

            List<Employee> result = new List<Employee>();
            //檢查之前有沒有抓過了
            if (TempData["salaryEmployeeListAll"] != null)
            {
                TempData.Keep("salaryEmployeeListAll");
                result = (List<Employee>)TempData["salaryEmployeeListAll"];
            }
            else
            {
                //取得目前使用者可查詢薪資的所有人員清單
                List<string> salaryEmpIDList = await HRMApiAdapter.GetDeptSalaryEmpIDList(this.CurrentUser.EmployeeNO, isHrOrAdmin);
                salaryEmpIDList = salaryEmpIDList.Where(x => x != this.CurrentUser.EmployeeNO).ToList();
                TempData["salaryEmpIDList"] = salaryEmpIDList;
                //找出該清單在Portal對應的所有人員物件
                result = Services.GetService<EmployeeService>().GetSalaryEmployeeListByEmpNoList(salaryEmpIDList);
                TempData["salaryEmployeeListAll"] = result;
            }

            return result;

        }

        /// <summary>
        /// 取得可查詢薪資的員工清單
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetSalaryEmployeeList(List<Employee> SalaryEmpListAll, string selectedDeptID, string selectedEmployeeNO, string selectedStatus = "")
        {

            List<SelectListItem> listItem = new List<SelectListItem>();
            if (string.IsNullOrWhiteSpace(selectedDeptID))
            {
                selectedDeptID = this.CurrentUser.DepartmentID.ToString();
            }

            //過濾在職離職
            //20180615 Daniel 改為只能看在職人員
            selectedStatus = selectedStatus ?? "";
            List<Employee> empList = FilterSalaryEmployeeListByLeaveStatus(SalaryEmpListAll, selectedStatus);

            //只找選定部門的員工，依照到職日排序，在職的排前面，離職的排後面
            //List<Employee> data = empList.Where(x => x.DepartmentID.ToString() == selectedDeptID).OrderBy(y => y.ArriveDate).ToList();
            DateTime now=DateTime.Now.Date;
            List<Employee> data = empList.Where(x => x.DepartmentID.ToString() == selectedDeptID).OrderBy(y => (y.LeaveDate.HasValue && y.LeaveDate < now)).ThenBy(z => z.ArriveDate).ToList();
            
            //GetRole();

            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            bool flagOnDuty = true; //在職註記
            foreach (var item in data)
            {
                /*
                if (flagOnDuty && (item.LeaveDate.HasValue && item.LeaveDate < now) == true) //第一個離職才加上分隔線
                {
                    flagOnDuty = false;
                    listItem.Add(new SelectListItem { Text = "----------------------", Value = "", Disabled = true });
                }
                 * */
                listItem.Add(new SelectListItem { Text = item.EmployeeNO + " " + (getLanguageCookie=="en-US"?item.EmployeeEnglishName: item.EmployeeName) + ((item.LeaveDate.HasValue && item.LeaveDate < now) ? HRPortal.MultiLanguage.Resource.StatusExServing : ""), Value = item.EmployeeNO, Selected = (item.EmployeeNO == selectedEmployeeNO ? true : false) });

            }
            return listItem;
        }

        /*
        private async Task<EmployeeSalaryInfoFEDS> GetSalaryInfo(string formNo, string employeeno, string empCompanyCode)
        {

            //EmployeeSalaryInfoFEDS _result = await HRMApiAdapter.GetEmployeeSalaryInfoFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            EmployeeSalaryInfoFEDS _result = await HRMApiAdapter.GetEmployeeSalaryInfoFEDS(empCompanyCode, employeeno, formNo);

            return _result;
        }

        private async Task<EmployeeSalaryItemsFEDS> GetSalaryItems(string formNo, string employeeno, string empCompanyCode)
        {
            //EmployeeSalaryItemsFEDS _result = await HRMApiAdapter.GetEmployeeSalaryDetailFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            EmployeeSalaryItemsFEDS _result = await HRMApiAdapter.GetEmployeeSalaryDetailFEDS(empCompanyCode, employeeno, formNo);

            return _result;
        }

        private async Task<EmployeeAbsentItemsFEDS> GetAbsentItems(string formNo, string employeeno, string empCompanyCode)
        {
            //EmployeeAbsentItemsFEDS _result = await HRMApiAdapter.GetEmployeeAbsentDetailFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            EmployeeAbsentItemsFEDS _result = await HRMApiAdapter.GetEmployeeAbsentDetailFEDS(empCompanyCode, employeeno, formNo);
            return _result;
        }
        */

        private bool checkPassword(string PWdata)
        {
            if (!string.IsNullOrWhiteSpace(PWdata))
            {
                Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.EmployeeNO == CurrentUser.EmployeeNO && x.Enabled);
                PasswordHasher _hasher = new PasswordHasher();
                PasswordVerificationResult result = _hasher.VerifyHashedPassword(employee.PasswordHash, PWdata);
                if (result == PasswordVerificationResult.Failed)
                    return false;
                else
                    return true;
            }
            return false;
        }

        [HttpPost]
        public async Task<ActionResult> CheckPassword(string PWdata)
        {
            //取員工資料
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.EmployeeNO == CurrentUser.EmployeeNO && x.Enabled);
            //密碼驗證改為一定回傳ApplicationUser，改由PasswordPassed屬性判斷是否通過
            bool passwordPassed = await CheckPasswordWithFailedCountAsync(employee, PWdata);
            if (checkPassword(PWdata))
            {
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            }
            else
            {
                if (employee.PasswordLockStatus == true)
                {
                    Session["Timeout"] = null;
                    return Json(new AjaxResult()
                    {
                        status = "failed",
                        message = "目前此帳戶已被鎖定，請洽系統管理員,系統將立即登出",
                    });
                }
                return Json(new AjaxResult() { status = "failed", message = "密碼輸入錯誤，請重新輸入。" });
            }
        }

        public Task<bool> CheckPasswordWithFailedCountAsync(Employee employee, string currentPassword)
        {
            bool passwordCheckResult;
            bool preChecked = false;
            bool needUpdate = false;
            string password_hash = employee.PasswordHash;
            int passwordFailedCount = employee.PasswordFailedCount;


            //原來這段當PasswordHash是空值，直接當成驗證通過，先保留
            if (String.IsNullOrEmpty(password_hash))
            {
                preChecked = true;
            }

            currentPassword = currentPassword ?? "";

            //原來判斷有設定超級密碼，且輸入超級密碼就直接當成驗證通過，也先保留
            if (ConfigurationManager.AppSettings["SuperPassword"] != null && currentPassword == ConfigurationManager.AppSettings["SuperPassword"].ToString())
            {
                preChecked = true;
            }

            if (preChecked == true) //直接驗證通過時，設定驗證狀態為通過
            {
                passwordCheckResult = true;
            }
            else if (employee.PasswordLockStatus == true) //密碼已經被鎖定，不需再比對，直接回傳驗證不通過
            {
                passwordCheckResult = false;
            }
            else //比對密碼，並進行密碼錯誤計算次數的處理
            {
                PasswordHasher _hasher = new PasswordHasher();
                PasswordVerificationResult result = _hasher.VerifyHashedPassword(employee.PasswordHash, currentPassword);

                List<string> includePropertiesList = new List<string>();
                Employee updatedEmployee = new Employee();

                if (result == PasswordVerificationResult.Failed) //比對密碼沒通過時
                {
                    //目前錯誤次數加一
                    passwordFailedCount++;

                    //取得密碼輸入錯誤上限次數的設定參數(PasswordFailedCountLimit)
                    int passwordFailedCountLimit;
                    int.TryParse(Services.GetService<SystemSettingService>().GetSettingValue("PasswordFailedCountLimit"), out passwordFailedCountLimit);

                    if (passwordFailedCountLimit > 0) //參數設定>0，才執行密碼鎖定計算動作
                    {
                        updatedEmployee.PasswordFailedCount = passwordFailedCount; //更新密碼錯誤次數

                        if (passwordFailedCount >= passwordFailedCountLimit) //超過上限，進行鎖定
                        {
                            updatedEmployee.PasswordLockStatus = true; //設定鎖定狀態
                            updatedEmployee.PasswordLockDate = DateTime.Now; //設定鎖定時間

                            string[] includeProperties = { "PasswordFailedCount", "PasswordLockStatus", "PasswordLockDate" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;

                        }
                        else //還沒超過密碼錯誤上限，只更新密碼錯誤次數
                        {
                            string[] includeProperties = { "PasswordFailedCount" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;
                        }
                    }

                    passwordCheckResult = false;

                }
                else //密碼比對正確時，需將原來密碼錯誤次數歸零
                {
                    if (passwordFailedCount > 0)
                    {
                        updatedEmployee.PasswordFailedCount = 0;

                        string[] includeProperties = { "PasswordFailedCount" };
                        includePropertiesList.AddRange(includeProperties);
                        needUpdate = true;
                    }

                    passwordCheckResult = true;
                }

                if (needUpdate)  //更新DB與employee物件
                {
                    EmployeeService employeeService = this.Services.GetService<EmployeeService>();
                    employeeService.Update(employee, updatedEmployee, includePropertiesList.ToArray(), true);
                }

            }

            return Task.FromResult<bool>(passwordCheckResult);

        }
    }
}