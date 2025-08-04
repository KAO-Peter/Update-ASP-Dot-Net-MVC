using ClosedXML.Excel;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using HRPortal.Services.Models;
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
using System.Globalization;
using Newtonsoft.Json;

namespace HRPortal.Areas.DDMC.Controllers
{
    public class EmpHolidayTimeSpanQuotaController : BaseController
    {
        private bool isHR = false, isAdmin = false;

        // GET: /ToDo/FormQuery/
        public async Task<ActionResult> Index(int page = 1, string DepartmentData = "", string beginDate = "", string endDate = "", string AbsentCode = "")
        {
            SetDefaultData(DepartmentData, beginDate, endDate, AbsentCode);
           
            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            //Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);

            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }

            return View();
        }

        /// <summary>
        /// 查詢資料，回傳PartialView
        /// </summary>
        /// <param name="DepartmentCode">部門代碼，一次只查一個部門</param>
        /// <param name="QueryBeginDate">查詢區間起日</param>
        /// <param name="QueryEndDate">查詢區間迄日</param>
        /// <param name="EmpIDList">員工編號清單</param>
        /// <param name="AbsentCodeList">假別代碼清單</param>
        /// <param name="IncludingPendingData">是否要顯示簽核中資料</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult QueryData(string DepartmentCode, string QueryBeginDate, string QueryEndDate, List<string> EmpIDList, List<string> AbsentCodeList, bool IncludingPendingData)
        {
            DateTime beginDate;
            DateTime endDate;
            string dateFormat = "yyyy/MM/dd";
            CultureInfo culture = CultureInfo.InvariantCulture;
            string CompanyCode = CurrentUser.CompanyCode;

            EmpHolidayTimeSpanQuotaViewModel model = new EmpHolidayTimeSpanQuotaViewModel()
                {
                    QuotaData = new List<EmpHolidayTimeSpanQuotaData>(),
                    QueryCondition = new QueryEmpHolidayTimeSpanQuotaObj()
                };

            //轉換EmpIDList與AbsentCodeList，沒選擇的時候會變成有一個空字串元素，轉為空List
            if (EmpIDList.Count == 1 && string.IsNullOrWhiteSpace(EmpIDList.First()))
            {
                EmpIDList.Clear();
            }

            if (AbsentCodeList.Count == 1 && string.IsNullOrWhiteSpace(AbsentCodeList.First()))
            {
                AbsentCodeList.Clear();
            }


            if (DateTime.TryParseExact(QueryBeginDate, dateFormat, culture, DateTimeStyles.None, out beginDate)
                 && DateTime.TryParseExact(QueryEndDate, dateFormat, culture, DateTimeStyles.None, out endDate))
            {
                //如果未傳入假別代碼，改取得BambooHR上有的假別，使用之前的參數BambooHR_AbsentCodeList
                if (AbsentCodeList.Count == 0)
                {
                    string BambooHRAbsentCodes = Services.GetService<SystemSettingService>().GetSettingValue("BambooHR_AbsentCodeList");
                    AbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();
                }

                //透過API查詢後台時數
                QueryEmpHolidayTimeSpanQuotaObj queryObj = new QueryEmpHolidayTimeSpanQuotaObj()
                {
                    CompanyCode = CompanyCode,
                    DepartmentCode = DepartmentCode,
                    QueryBeginDate = beginDate,
                    QueryEndDate = endDate,
                    EmpIDList = EmpIDList,
                    AbsentCodeList = AbsentCodeList,
                    IncludingPendingData = IncludingPendingData
                };

                List<EmpHolidayTimeSpanQuotaData> data = Task.Run(() => HRMApiAdapter.GetEmpHolidayTimeSpanQuota(queryObj)).Result;

                //組合回傳前端的假單資訊，減少資料量
                foreach (EmpHolidayTimeSpanQuotaData item in data)
                {
                    //減少傳到前端的資料
                    //在查詢區間內已使用的假單資料
                    if (item.InRangeUsedList != null)
                    {
                        item.InRangeUsedString = ConvertAbsentDataToJSONString(item.InRangeUsedList);
                    }

                    //在查詢區間內尚未發生的假單資料
                    if (item.InRangeScheduledList != null)
                    {
                        item.InRangeScheduledString = ConvertAbsentDataToJSONString(item.InRangeScheduledList);
                    }

                }

                //如果要顯示簽核中時數，檢查前台資料
                //先不用依據單筆核假來看核假區間，如果有多筆核假就都顯示一樣的資料，因Portal請假一筆假單本來就可以跨多筆核假
                if (IncludingPendingData)
                {
                    List<TimeSpanPendingData> pendingData = Services.GetService<LeaveFormService>().GetTimeSpanPendingLeaveData(queryObj);

                    //合併前後台資料
                    if (data.Count > 0 && pendingData.Count > 0)
                    {
                        foreach (EmpHolidayTimeSpanQuotaData item in data)
                        {
                            TimeSpanPendingData pending = pendingData.Where(x => x.EmpID == item.EmpID && x.AbsentCode == item.AbsentCode).FirstOrDefault();
                            if (pending != null)
                            {
                                item.HoursPending = (double)pending.AbsentHoursSum;
                                item.PendingString = ConvertPendingAbsentDataToJSONString(pending);
                            }
                        }
                    }

                }

                model.QuotaData = data;
                model.QueryCondition = queryObj;

            }

            return PartialView("_TimeSpanQuotaData", model);
        }

        //轉換假單資料為Json字串(前端使用)，並減少資料量
        private string ConvertAbsentDataToJSONString(List<GeneralAbsentData> data)
        {
            var tempList = data.Select(x => new
            {
                Adj = x.isAdjust,
                FNo = x.FormNo,
                BTime = x.BeginTime.ToString("yyyy/MM/dd HH:mm"),
                ETime = x.EndTime.ToString("yyyy/MM/dd HH:mm"),
                Hours = x.AbsentAmount
            }).ToList();

            return tempList.Count > 0 ? JsonConvert.SerializeObject(tempList) : "";
        }

        //轉換簽核中假單資料為Json字串(前端使用)，並減少資料量
        private string ConvertPendingAbsentDataToJSONString(TimeSpanPendingData data)
        {
            string result = "";
            if (data.LeaveFormList != null && data.LeaveFormList.Count > 0)
            {
                var tempList = data.LeaveFormList.Select(x => new
                {
                    Adj = false,
                    FNo = x.FormNo,
                    BTime = x.StartTime.ToString("yyyy/MM/dd HH:mm"),
                    ETime = x.EndTime.ToString("yyyy/MM/dd HH:mm"),
                    Hours = x.LeaveAmount
                }).ToList();

                result = JsonConvert.SerializeObject(tempList);
            }
            return result;
        }




        private void SetDefaultData(string departmentdata = "", string beginDate = "", string endDate = "", string AbsentCode = "")
        {
            if (string.IsNullOrWhiteSpace(beginDate))
            {
                ViewBag.StartTime = DateTime.Now.ToString("yyyy/MM/dd");
                
            }
            else
            {
                ViewBag.StartTime = beginDate;
            }

            if (string.IsNullOrWhiteSpace(endDate))
            {
                ViewBag.EndTime = new DateTime(DateTime.Now.Year, 12, 31).ToString("yyyy/MM/dd");
            }
            else
            {
                ViewBag.EndTime = endDate;
            }

            ViewData["AbsentCodeList"] = GetAbsentCodeList(AbsentCode);
            ViewBag.AbsentCode = AbsentCode;
            
            ViewData["DepartmentList"] = GetDepartmentList(departmentdata);
            //ViewData["EmployeeList"] = GetEmployeeList(departmentdata, employeedata);
            
            ViewBag.DepartmentData = departmentdata;
           
            ViewBag.LanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

        }

        //20170510 Start 增加 by Daniel
        //個人假單查詢增加假別篩選
        /// <summary>
        /// 取得所有假別清單
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetAbsentCodeList(string AbsentCode = "")
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            //假別只需要顯示BambooHR上有的假別，使用之前的參數BambooHR_AbsentCodeList
            string BambooHRAbsentCodes = Services.GetService<SystemSettingService>().GetSettingValue("BambooHR_AbsentCodeList");
            List<string> bbAbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();

            if (Session["Absents"] != null)
            {
                List<AbsentType> listAbsents = (List<AbsentType>)Session["Absents"];
                listAbsents = listAbsents.Where(x => bbAbsentCodeList.Contains(x.AbsentCode.Trim())).ToList();
                if (listAbsents.Count > 0)
                {
                    //目前中英文全部顯示
                    itemList = listAbsents.Select(x => new SelectListItem
                                        {
                                            //Text = getLanguageCookie == "en-US" ? x.AbsentEnglishName : x.AbsentName,
                                            Text = x.AbsentName + " " + x.AbsentEnglishName,
                                            Value = x.AbsentCode.Trim(),
                                            Selected = x.AbsentCode.Trim() == AbsentCode ? true : false
                                        }).ToList();
                }

                //itemList.Insert(0, new SelectListItem { Text = getLanguageCookie == "en-US" ? "All" : "全部", Value = "" });
              
            }
            return itemList;
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
     
        //    return listItem;
        //}

        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata)
        {
            List<SelectListItem> itemList=new List<SelectListItem>();

            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            //目前只需要查詢創新與創投兩個部門，B315000 & B335000，直接使用之前設定BambooHR_DepartmentList
            string bambooHRDepartments = Services.GetService<SystemSettingService>().GetSettingValue("BambooHR_DepartmentList");
            if (!string.IsNullOrWhiteSpace(bambooHRDepartments))
            {
                List<string> bbDeptList = bambooHRDepartments.Split(';').ToList();
                List<Department> departmentList = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && bbDeptList.Contains(x.DepartmentCode)).OrderBy(x => x.DepartmentCode).ToList();
                if (departmentList.Count>0)
                {
                    itemList = departmentList.Select(x => new SelectListItem() 
                                            { 
                                                Text = getLanguageCookie == "en-US" ? x.DepartmentEnglishName : x.DepartmentName, 
                                                Value = x.DepartmentCode, 
                                                Selected = selecteddata == x.DepartmentCode ? true : false 
                                            }).ToList();
                }
            }
  
            return itemList;
        }

        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeeList(string departmentdata, string selecteddata = "")
        {
            
            if (departmentdata == "")
            {
                return new List<SelectListItem>();
            }

            List<string> queryDepartmentList = departmentdata.Split(',').ToList();
           
            //取得部門
            //List<Department> _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => queryDepartmentList.Contains(x.DepartmentCode) && x.Enabled).OrderBy(y => y.DepartmentCode).ToList();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();

            //取得員工列表
            //data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
            data = Services.GetService<EmployeeService>().GetAllEmployeeByDepartment(CurrentUser.CompanyID, queryDepartmentList);

            //listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });
            foreach (var item in data)
            {
                //string empName = getLanguageCookie == "en-US" ? item.EmployeeEnglishName : item.EmployeeName;
                //目前不分語系，中英文全都要顯示
                string itemText = item.EmployeeNO + " " + item.EmployeeName + " " + item.EmployeeEnglishName;
                listItem.Add(new SelectListItem { Text = itemText, Value = item.EmployeeNO });
             
            }
            return listItem;
        }

        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<KeyValuePair<string, string>> GetEmployeeList2(string departmentdata, string selecteddata = "")
        {

            if (departmentdata == "")
            {
                return new List<KeyValuePair<string, string>>();
            }

            List<string> queryDepartmentList = departmentdata.Split(',').ToList();

            //取得部門
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<Employee> data = new List<Employee>();

            //取得員工列表
            data = Services.GetService<EmployeeService>().GetAllEmployeeByDepartment(CurrentUser.CompanyID, queryDepartmentList);

            List<KeyValuePair<string, string>> listItem = data.Select(x => new KeyValuePair<string, string>
                (
                    x.EmployeeNO + " " + x.EmployeeName + " " + x.EmployeeEnglishName,  //目前不分語系，中英文全都要顯示
                    x.EmployeeNO)).ToList();

            return listItem;
        }

        /// <summary>
        /// 給下拉式選單讀取員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetEmployee(string DepartmentId)
        {
            //List<SelectListItem> result = GetEmployeeList(DepartmentId);
            List<KeyValuePair<string, string>> result = GetEmployeeList2(DepartmentId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
               
        private void FilterLeaveFormByAbsentCode(ref List<AbsentFormData> listAbsentFormData,string AbsentCode)
        {
            if (!string.IsNullOrWhiteSpace(AbsentCode))
            {
                listAbsentFormData = listAbsentFormData.Where(x => x.AbsentCode == AbsentCode).ToList();
            }
        }
       
    }

}

