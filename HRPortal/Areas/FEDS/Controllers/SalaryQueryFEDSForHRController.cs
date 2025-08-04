using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using System.Threading.Tasks;
using HRPortal.ApiAdapter;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using HRPortal.Utility;
using HRPortal.Mvc.Models;
using PagedList;
using HRPortal.Mvc.Results;
using Microsoft.AspNet.Identity;
using HRPortal.DBEntities;
using System.Configuration;
using HRPortal.Services;

namespace HRPortal.Areas.FEDS.Controllers
{
    public class SalaryQueryFEDSForHRController : BaseController
    {
        private bool isGetRole = false, isHR = false, isAdmin = false;
        //
        // GET: /FEDS/SalaryQueryFEDSForHR/
        public async Task<ActionResult> Index(int page = 1, string SelectedDepartment = "", string SelectedEmployee = "", string SelectedStatusListData = "", string BeginDate = "", string EndDate = "")
        {
            int currentPage = page < 1 ? 1 : page;
            //List<SalaryQueryForHRViewModel> viewModel = new List<SalaryQueryForHRViewModel>();
            SalaryQueryForHRViewModel viewModel = new SalaryQueryForHRViewModel();

            //viewModel.SalaryFormListData = new List<SalaryQueryViewModel>();

            #region 使用者角色
            if (!isGetRole)
            {
                isGetRole = GetRole();
            }
            #endregion

            //進入此方法時，要依據是否傳入EmployeeNo判斷底下需不需要顯示薪資批號
            bool flagGetSalaryData = false;
            //有傳入EmployeeNo時
            if (!string.IsNullOrWhiteSpace(SelectedEmployee))
            {
                if (isHR || isAdmin) //目前限制只有HR或是Admin可以看到資料
                { flagGetSalaryData = true; }
            }

            viewModel.flagShowSalaryData = flagGetSalaryData;

            if (isHR || isAdmin)
            {
                if (string.IsNullOrWhiteSpace(SelectedDepartment))
                {
                    //SelectedDepartment = CurrentUser.DepartmentCode;
                    SelectedDepartment = CurrentUser.DepartmentID.ToString();

                }
                
                if (string.IsNullOrWhiteSpace(SelectedEmployee))
                {
                    SelectedEmployee = CurrentUser.EmployeeNO;
                }
                
                viewModel.DepartmentListData = GetDepartmentList(SelectedDepartment);
                viewModel.EmployeeListData = GetEmployeeetList(SelectedDepartment, SelectedStatusListData ,SelectedEmployee);
            }

            if (BeginDate == "" && EndDate == "")
            {
                DateTime ED = DateTime.Now;
                DateTime BD = ED.AddDays(-90);
                BeginDate = BD.ToString("yyyy/MM");
                EndDate = ED.ToString("yyyy/MM");
            }

            viewModel.SalaryFormListData = await GetSalaryData(SelectedEmployee, BeginDate, EndDate, SelectedDepartment, flagGetSalaryData);
  

            viewModel.StatuslistDataData = GetStatusDataList(SelectedStatusListData);

            //selected data
            viewModel.SelectedDepartment = SelectedDepartment;
            viewModel.SelectedEmployee = SelectedEmployee;
            viewModel.SelectedStatuslistData = SelectedStatusListData;
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(string PWdata, string btnQuery, string btnClear, string ExportsalaryYMData, string ExportFormNo, string BeginDate, string EndDate, string searchkey, string SelectedDepartment, string SelectedEmployee, string SelectedStatusListData, int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;

            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;
            //List<SalaryQueryForHRViewModel> viewModel = new List<SalaryQueryForHRViewModel>();
            SalaryQueryForHRViewModel viewModel = new SalaryQueryForHRViewModel();
            //viewModel.SalaryFormListData = new List<SalaryQueryViewModel>();

            if (!string.IsNullOrWhiteSpace(PWdata) && checkPassword(PWdata))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(btnClear))
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
            
            else if (!string.IsNullOrWhiteSpace(ExportFormNo) && !string.IsNullOrWhiteSpace(ExportsalaryYMData))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                //var userPW = await HRMApiAdapter.GetEmployeeIDNumber(CurrentUser.CompanyCode, CurrentUser.EmployeeNO);
                //var empData = await HRMApiAdapter.GetEmployeeData(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.EmployeeNO);
                //因部門代碼是前台簽核部門，可能後台沒有這單位-->查詢員工資料時，只看公司別與員工編號
                //因總公司可以查所有分公司資料，所以如果是總公司HR或Admin，要先取得被查詢的員工所屬部門的公司別
               
                string empCompanyCode=CurrentUser.CompanyCode;

                if (CurrentUser.CompanyCode == "1010") //總公司HR或Admin，取得被查詢員工的公司別
                {
                    Department empDepartment = Services.GetService<DepartmentService>().GetDepartmentByID(Guid.Parse(SelectedDepartment));
                    if (empDepartment != null)
                    {
                        empCompanyCode = empDepartment.Company.CompanyCode;
                    }
                  
                }
               
                //EmployeeData empData = await HRMApiAdapter.GetEmployeeData(CurrentUser.CompanyCode, "", SelectedEmployee);
                EmployeeData empData = await HRMApiAdapter.GetEmployeeData(empCompanyCode, "", SelectedEmployee);
                
                var IDNumber = empData.IDNumber.Substring(empData.IDNumber.Length - 6);
                var Birthday = empData.Birthday.Value.ToString("MMdd");
                var userPW = IDNumber + Birthday;
                string EmpName = empData.EmpName;
                byte[] data = await ExportPDF(ExportFormNo, SelectedEmployee, empCompanyCode);
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
                        
                        //移除檔名內斜線字元(Windows檔名原本就不能有/)
                        string fileName =ExportsalaryYMData.Replace("/","") + "個人薪資明細檔(" + SelectedEmployee + EmpName + ").pdf";
                      
                        return File(data, "application/pdf", fileName);

                    }
                }
            }

            //底下這整段其實不會執行到，包括return那行
            /*
            if (BeginDate == null && EndDate == null)
            {
                DateTime ED = DateTime.Now;
                DateTime BD = ED.AddDays(-90);
                BeginDate = BD.ToString("yyyy/MM");
                EndDate = ED.ToString("yyyy/MM");
            }

            viewModel.SalaryFormListData = await GetSalaryData(SelectedEmployee, BeginDate, EndDate, SelectedDepartment, false);
            
            //return View(viewModel.ToPagedList(currentPage, currentPageSize));
            
            */ 
            return View(viewModel);
            
        }

        private async Task<List<SalaryQueryViewModel>> GetSalaryData(string EmployeeNo, string BeginDate = "", string EndDate = "", string SelectedDepartment="", bool flagGetSalaryData=false)
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
            
            //List<SalaryQueryForHRViewModel> viewModel = new List<SalaryQueryForHRViewModel>();

            List<SalaryQueryViewModel> SalaryFormList = new List<SalaryQueryViewModel>();
            //為了遠百修改 4月份計薪區間

            //20170503 Start 修改 by Daniel，之後要改成參數設定
            //DateTime tempDate = new DateTime(2017, 4, 6,9,0,0);
            DateTime tempDate = new DateTime(2017, 5, 4, 9, 0, 0);
            //20170503 End

            DateTime Today = DateTime.Now;
            string tempEndDate = EndDate;
            if (DateTime.Compare(tempDate, Today) > 0)
            {
                //20170503 Start
                //EndDate = new DateTime(2017, 2, 28, 9, 0, 0).ToString("yyyy/MM");
                EndDate = new DateTime(2017, 3, 31, 9, 0, 0).ToString("yyyy/MM");
                //20170503 End
            }
            //END　P.S ViewBag.EndTime = tempEndDate; 123行記得修正回 ViewBag.EndTime = EndDate

            //var ds = await HRMApiAdapter.GetEmployeeSalaryFormNo(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));
            if (flagGetSalaryData)
            {

                //因總公司可以查所有分公司資料，所以如果是總公司HR或Admin，要先取得被查詢的員工所屬部門的公司別
                string empCompanyCode = CurrentUser.CompanyCode;

                if (CurrentUser.CompanyCode == "1010") //總公司HR或Admin，取得被查詢員工的公司別
                {
                    Department empDepartment = Services.GetService<DepartmentService>().GetDepartmentByID(Guid.Parse(SelectedDepartment));
                    if (empDepartment != null)
                    {
                        empCompanyCode = empDepartment.Company.CompanyCode;
                    }

                }
                //var ds = await HRMApiAdapter.GetEmployeeSalaryFormNo(CurrentUser.CompanyCode, EmployeeNo, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));
                var ds = await HRMApiAdapter.GetEmployeeSalaryFormNo(empCompanyCode, EmployeeNo, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));

                foreach (var item in ds)
                {
                    SalaryFormList.Add(new SalaryQueryViewModel
                    {
                        FormNo = item.FormNo,
                        SalaryYM = item.SalaryYM
                    });

                }
            }
            ViewBag.StartTime = BeginDate;
            ViewBag.EndTime = tempEndDate;
            return SalaryFormList;
        }

        [HttpPost]
        public async Task<byte[]> ExportPDF(string formNo,string EmployeeNo, string empCompanyCode)
        {
            string htmlText;
            string htmlToConvertSalaryInfo = RenderViewAsString("_SalaryInfo", await GetSalaryInfo(formNo, EmployeeNo, empCompanyCode));

            string htmlToConvertSalaryItems = RenderViewAsString("_SalaryItems", await GetSalaryItems(formNo, EmployeeNo, empCompanyCode));

            string htmlToConvertAbsentItems = RenderViewAsString("_AbsentItems", await GetAbsentItems(formNo, EmployeeNo, empCompanyCode));


            //避免當htmlText無任何html tag標籤的純文字時，轉PDF時會掛掉，所以一律加上<p>標籤
            htmlText = "<div style='text-align:center; font-size:18px; font-family:DFKai-sb; margin-bottom:20px; height:80px;'><div>遠東百貨股份有限公司" + ((CurrentUser.CompanyCode == "1010") ? "總公司" : CurrentUser.CompanyName) + "</div><div>個人薪資明細表</div></div><div style='width:100%; margin-left:auto; margin-right:auto; font-family:DFKai-sb; font-size:12px;'>" + htmlToConvertSalaryInfo + htmlToConvertSalaryItems + htmlToConvertAbsentItems + "</div>";

            //換class"
            //htmlText = htmlText.Replace("<table", "<table cellpadding=\"0\" cellspacing=\"0\"  border=\"1\" style=\"border: 1 solid #000000;width:100%;max-width:100%;margin-bottom:20px;\"").Replace("<th>", "<th style=\"padding: 8px;height:35\">&nbsp;").Replace("<th style=\"text-align:center\"", "<th style=\"text-align:center;padding: 8px;height:35\" ").Replace("<td style=\"text-align:right\">", "<td style=\"text-align:right;padding: 8px;height:35\">").Replace("<td>", "<td style=\"text-align:right\">&nbsp;").Replace("</td>", "&nbsp;</td>");

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
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata)
        {
            //使用者角色
            GetRole();
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
                if (CurrentUser.CompanyCode == "1010") //總公司的HR或Admin可以看到所有部門
                {
                    data = Services.GetService<DepartmentService>().GetAllLists().OrderBy(x => x.DepartmentCode).ToList();
                }
                else
                {
                    data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
                }
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
            /*
            else
            {
                data = CurrentUser.SignDepartments;

                if (data.Count > 0)
                {
                    #region 主管
                    bool flag = true;//用來判斷簽核主管是否為外同部門人員
                    foreach (var item in data)
                    {
                        if (item.DepartmentCode == CurrentUser.SignDepartmentCode)
                            flag = false;
                        if (getLanguageCookie == "en-US")
                        {
                            listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                        else
                        {
                            listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                    }

                    if (flag == true)
                    {
                        data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && x.DepartmentCode == CurrentUser.SignDepartmentCode).OrderBy(x => x.DepartmentCode).ToList();
                        foreach (var item in data)
                        {
                            if (getLanguageCookie == "en-US")
                            {
                                listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                            }
                            else
                            {
                                listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                            }
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
                        listItem.Add(new SelectListItem { Text = _signdepartment.DepartmentEnglishName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new SelectListItem { Text = _signdepartment.DepartmentName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    #endregion
                }
            }
            */

            return listItem;
        }


        /// <summary>
        /// 給下拉式選單讀取員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetEmployee(string DepartmentId, string StatusData)
        {
            List<SelectListItem> result = GetEmployeeetList(DepartmentId, StatusData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        /*
        private List<SelectListItem> GetEmployeetList(string departmentdata, string selecteddata = "")
        {
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.Name;
            //取得部門
            if (departmentdata == "")
            {
                departmentdata = CurrentUser.DepartmentCode;
            }
            Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentdata && x.Enabled).FirstOrDefault();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();

            //取得員工列表
            data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();

            //listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });
            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    {
                        listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    }
                }
                else
                {
                    if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    {
                        listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    }
                }
            }
            return listItem;
        }
        */

        /// <summary>
        /// 取得員工在離職狀態 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetStatusDataList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "顯示在職人員", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "顯示離職人員", Value = "L", Selected = (selecteddata == "L" ? true : false) });
            listItem.Add(new SelectListItem { Text = "全部", Value = "ALL", Selected = (selecteddata == "ALL" ? true : false) });

            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }

        /// <summary>
        /// 給下拉式選單讀取在離職員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetStatusData(string DepartmentId, string StatusData)
        {
            List<SelectListItem> result = GetEmployeeetList(DepartmentId, StatusData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 取得在離職員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeeetList(string departmentdata, string StatusData, string selecteddata = "")
        {
            //取得部門，目前這功能不需要這段
            //Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentdata && x.Enabled).FirstOrDefault();
            
            
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();

            GetRole();
            if (isHR || isAdmin)
            {
                //data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
                data = Services.GetService<EmployeeService>().GetAllListsBySignDepartment((string.IsNullOrWhiteSpace(departmentdata) ? Guid.Empty : Guid.Parse(departmentdata))).OrderBy(x => x.EmployeeNO).ToList();       
            }
            /*
            else if (_department.SignManager.EmployeeNO != CurrentUser.EmployeeNO)
            {
                bool flag = false;
                foreach (var item in CurrentUser.SignDepartments)
                {
                    if (item.DepartmentCode == departmentdata)
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).Where(x => x.EmployeeNO == CurrentUser.EmployeeNO).OrderBy(x => x.EmployeeNO).ToList();
                }
                else
                {
                    data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
                }
            }
            */
            //listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });
            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    if (StatusData == "")
                    {
                        if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                        {
                            listItem.Add(new SelectListItem { Text = item.EmployeeNO + " " + item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                        }
                    }
                    else
                        if (StatusData == "L")
                        {
                            if (item.LeaveDate < DateTime.Now)
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeNO + " " + item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                        }
                        else
                            if (StatusData == "ALL")
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeNO + " " + item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                }
                else
                {
                    if (StatusData == "")
                    {
                        if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                        {
                            listItem.Add(new SelectListItem { Text = item.EmployeeNO + " " + item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                        }
                    }
                    else
                        if (StatusData == "L")
                        {
                            if (item.LeaveDate < DateTime.Now)
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeNO + " " + item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                        }
                        else
                            if (StatusData == "ALL")
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeNO + " " + item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                }
            }
            return listItem;
        }


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