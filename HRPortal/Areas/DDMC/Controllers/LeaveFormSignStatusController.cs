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
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.Areas.DDMC.Controllers
{
    public class LeaveFormSignStatusController : BaseController
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
        public ActionResult QueryData(string DepartmentCode, string QueryBeginDate, string QueryEndDate, List<string> EmpIDList, List<string> AbsentCodeList, bool isUsed)
        {
            DateTime beginDate;
            DateTime endDate;
            string dateFormat = "yyyy/MM/dd";
            CultureInfo culture = CultureInfo.InvariantCulture;
            string CompanyCode = CurrentUser.CompanyCode;

            QueryLeaveFormSignStatusViewModel model = new QueryLeaveFormSignStatusViewModel()
            {
                DetailData = new List<LeaveFormSignStatusData>(),
                QueryCondition = new QueryLeaveFormSignStatusObj()
            };

            //轉換EmpIDList與AbsentCodeList，沒選擇的時候會變成有一個空字串元素，轉為空List
            if (EmpIDList.Count == 1 && string.IsNullOrWhiteSpace(EmpIDList.First()))
            {
                EmpIDList.Clear();
            }

            if (AbsentCodeList == null)
            {
                AbsentCodeList = new List<string>();
            }
            /*
            if (AbsentCodeList.Count == 1 && string.IsNullOrWhiteSpace(AbsentCodeList.First()))
            {
                AbsentCodeList.Clear();
            }
            */

            if (DateTime.TryParseExact(QueryBeginDate, dateFormat, culture, DateTimeStyles.None, out beginDate)
                 && DateTime.TryParseExact(QueryEndDate, dateFormat, culture, DateTimeStyles.None, out endDate))
            {
                /*
                //如果未傳入假別代碼，改取得BambooHR上有的假別，使用之前的參數BambooHR_AbsentCodeList
                if (AbsentCodeList.Count == 0)
                {
                    string BambooHRAbsentCodes = Services.GetService<SystemSettingService>().GetSettingValue("BambooHR_AbsentCodeList");
                    AbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();
                }
                */

                QueryLeaveFormSignStatusObj queryObj = new QueryLeaveFormSignStatusObj()
                {
                    CompanyCode = CompanyCode,
                    DepartmentCode = DepartmentCode,
                    QueryBeginDate = beginDate,
                    QueryEndDate = endDate,
                    EmpIDList = EmpIDList,
                    AbsentCodeList = AbsentCodeList,
                    isUsed = isUsed
                };

                //取得前台假單資訊
                List<LeaveForm> leaveFormData = Services.GetService<LeaveFormService>().GetValidLeaveForm(queryObj);

                List<string> formNoList = leaveFormData.Select(x => x.FormNo).ToList();

                //取得假單流程資訊(目前核准時間還是只能看流程，LeaveForm的ModifiedTime在核准時不會更新)
                SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();
                Dictionary<string, DateTime?> dictApproveTime = _queryHelper.GetApprovedTime(formNoList);

                //產生View需要的資料
                List<AbsentType> listAbsents = new List<AbsentType>();
                if (Session["Absents"] != null)
                {
                    listAbsents = (List<AbsentType>)Session["Absents"];
                }

                DateTime? approveTime;
                var portalViewData = leaveFormData.Select(x => new LeaveFormSignStatusData()
                                                        {
                                                            EmpData_ID = x.Employee.Employee_ID ?? 0,
                                                            EmpID = x.Employee.EmployeeNO,
                                                            EmpName = x.Employee.EmployeeName,
                                                            EmpNameEN = x.Employee.EmployeeEnglishName,
                                                            FormNo = x.FormNo,
                                                            AbsentCode = x.AbsentCode,
                                                            Absent = listAbsents.Count > 0 ? listAbsents.Where(l => l.AbsentCode == x.AbsentCode).First() : new AbsentType() { AbsentCode = x.AbsentCode, AbsentName = "", AbsentEnglishName = "" },
                                                            AbsentUnit = x.AbsentUnit,
                                                            BeginTime = x.StartTime,
                                                            EndTime = x.EndTime,
                                                            CreateTime = x.CreatedTime,
                                                            ApproveTime = dictApproveTime.TryGetValue(x.FormNo, out approveTime) ? (approveTime.HasValue ? (DateTime?)approveTime.Value : null) : null,
                                                            LeaveHours = (double)x.LeaveAmount,
                                                            Reason = x.LeaveReason
                                                        }).ToList();

                List<LeaveFormSignStatusData> data = new List<LeaveFormSignStatusData>();

                if (portalViewData.Count > 0)
                {
                    data.AddRange(portalViewData);
                }

                //取得後台假單資料
                List<string> excludeFormNoList = portalViewData.Select(x => x.FormNo).ToList();
                QueryAbsentDataForLeaveFormSignStatusObj queryAPIObj = new QueryAbsentDataForLeaveFormSignStatusObj()
                {
                    ExcludeFormNoList = excludeFormNoList,
                    QueryCondition = queryObj
                };

                List<LeaveFormSignStatusBackendData> backendData = Task.Run(() => HRMApiAdapter.GetEmpAbsentDataForLeaveFormSignStatus(queryAPIObj)).Result;

                var backendViewData = backendData.Select(x => new LeaveFormSignStatusData()
                                                        {
                                                            EmpData_ID = x.EmpData_ID,
                                                            EmpID = x.EmpID,
                                                            EmpName = x.EmpName,
                                                            EmpNameEN = x.EmpNameEN,
                                                            FormNo = (x.isAdjust ? "ADJ_" : "") + x.FormNo, //追補假單FormNo前面加上ADJ供區隔
                                                            AbsentCode = x.AbsentCode,
                                                            Absent = new AbsentType() { AbsentCode = x.AbsentCode, AbsentName = x.AbsentName, AbsentEnglishName = x.AbsentNameEN },
                                                            AbsentUnit = x.AbsentUnit,
                                                            BeginTime = x.BeginTime,
                                                            EndTime = x.EndTime,
                                                            CreateTime = x.CreateTime,
                                                            ApproveTime = x.CreateTime, //後台資料核准時間直接用建立時間，因沒走簽核流程
                                                            LeaveHours = x.AbsentAmount,
                                                            Reason = x.AbsentReason
                                                        }).ToList();


                if (backendViewData.Count > 0)
                {
                    data.AddRange(backendViewData);
                }

                data = data.OrderBy(x => x.EmpID).ThenBy(y => y.BeginTime).ThenBy(z => z.AbsentCode).ToList();
                
                model.DetailData = data;
                model.QueryCondition = queryObj;

            }
       
            return PartialView("_SignStatusList", model);
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
            //ViewData["EmployeeList"] = GetEmployeetList(departmentdata, employeedata);
            
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
            //List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();

            //取得員工列表
            //data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
            data = Services.GetService<EmployeeService>().GetAllEmployeeByDepartment(CurrentUser.CompanyID, queryDepartmentList);

           
            List<SelectListItem> listItem = data.Select(x => new SelectListItem
                                                        {
                                                            Text = x.EmployeeNO + " " + x.EmployeeName + " " + x.EmployeeEnglishName,  //目前不分語系，中英文全都要顯示
                                                            Value = x.EmployeeNO
                                                        }).ToList();
           
            return listItem;
        }

        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<KeyValuePair<string,string>> GetEmployeeList2(string departmentdata, string selecteddata = "")
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

