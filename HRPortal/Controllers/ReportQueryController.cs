using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using HRPortal.Helper;
using PagedList;
using HRPortal.DBEntities;
using HRPortal.Services;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;


namespace HRPortal.Controllers
{
    public class ReportQueryController : BaseController
    {
        private HRPortal_Services Services = new HRPortal_Services();
        private HttpRequest Request = System.Web.HttpContext.Current.Request;
        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDepartmentList(Guid CompanyID, string selecteddata, bool showAll, bool isAdminOrHR, List<Department> Dept)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = new List<Department>();
            if (isAdminOrHR == false)
            {
                if (Dept.Count() > 0)
                {
                    data = Dept;
                }
                else if (Dept.Count() == 0)
                {
                    data = Services.GetService<DepartmentService>().GetListsByCompany(CompanyID).Where(x => x.Enabled && x.DepartmentCode == selecteddata).OrderBy(x => x.DepartmentCode).ToList();
                }
            }
            else if (isAdminOrHR == true)
            {
                //if (Dept.Count() > 0)
                //{
                //    data = Dept;
                //}
                //else
                //{
                    data = Services.GetService<DepartmentService>().GetListsByCompany(CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
                //}
            }

            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (showAll == true) // 選單中是否要多全部
            {
                listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });
            }

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

            return listItem;
        }

        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selectedEmpNo">被選選取員工工號</param>
        /// <param name="statusData">在離職狀況</param>
        /// <param name="showAll">下拉式清單是否加上 全部選項(true: 加上; false:不加上)</param>
        /// <param name="isAllMember">是否撈取全部人員(true: 全部; false:不全部)</param>
        /// <returns></returns>
        public List<SelectListItem> GetEmployeetList(Guid CompanyID, string departmentData, string selectedEmpNo = "", string statusData = "", bool showAll = true, bool isAllMember = false)
        {
            Department _department = Services.GetService<DepartmentService>().GetListsByCompany(CompanyID).Where(x => x.DepartmentCode == departmentData && x.Enabled).FirstOrDefault();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();

            //取得員工列表
             List<Employee>  data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
             if (isAllMember == false)//如果不需要顯示全部就只顯示個人
             {
                 data = data.Where(x => x.EmployeeNO == selectedEmpNo).ToList();
             }

             if (statusData == "") {//在職
                 data = data.Where(x => x.LeaveDate == null || x.LeaveDate >= DateTime.Now).ToList();
             }
             else if (statusData == "L")//離職
             {
                 data = data.Where(x =>x.LeaveDate < DateTime.Now).ToList();
             }

             if (showAll == true)//下拉式清單是否加上 "全部" 選項
             {
                 listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selectedEmpNo == "All" ? true : false) });
             }

            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selectedEmpNo == item.EmployeeNO ? true : false) });
                }
                else
                {
                    listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selectedEmpNo == item.EmployeeNO ? true : false) });
                }
            }
            return listItem;
        }

        /// <summary>
        /// 取得員工在離職狀態 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        public List<SelectListItem> GetStatusDataList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "顯示在職人員", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "顯示離職人員", Value = "L", Selected = (selecteddata == "L" ? true : false) });
            listItem.Add(new SelectListItem { Text = "全部", Value = "ALL", Selected = (selecteddata == "ALL" ? true : false) });

            //listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }

        /// <summary>
        /// 取得目前可請假時數
        /// </summary>
        /// <param name="CompanyID"></param>
        /// <param name="EmployeeNO"></param>
        /// <returns></returns>
        public async Task<List<AbsentDetail>> GetLeaveAmountList(Guid CompanyID, string EmployeeNO)
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            //獲得公司資訊
            Company companyInfo = Services.GetService<CompanyService>().GetCompanyById(CompanyID);
            //獲得員工資訊
            Employee empInfo = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CompanyID,EmployeeNO);
            List<AbsentDetail> data = new List<AbsentDetail>();
            if (empInfo.LeaveDate == null)//尚未離職
            {
                data = await HRMApiAdapter.GetEmployeeAbsent(companyInfo.CompanyCode, EmployeeNO, DateTime.Now);
            }
            else//離職
            {
                var dataType = empInfo.LeaveDate.GetType();
                data = await HRMApiAdapter.GetEmployeeAbsent(companyInfo.CompanyCode, EmployeeNO, dataType.Name == "DateTime" ? empInfo.LeaveDate.Value : DateTime.Now);
            }

            DateTime selectDate = new DateTime(DateTime.Now.Year, 12, 31);

            Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(empInfo.ID, selectDate);

            foreach (var item in data)
            {
                AbsentUnit _unit = item.Unit == "h" ? AbsentUnit.hour : AbsentUnit.day;
                if (notApprovedAbsentAmount.ContainsKey(item.Code))
                {
                    item.ApprovedHours = notApprovedAbsentAmount[item.Code];
                    item.UseAmount -= notApprovedAbsentAmount[item.Code];
                }

                item.getLanguageCookie = getLanguageCookie;
                item.AbsentNameEn = item.AbsentNameEn != null ? item.AbsentNameEn : item.Name;
                item.LeaveHours = item.AnnualLeaveHours - item.ApprovedHours - item.UseAmount;
            }
            return data;
        }

        /// <summary>
        /// 取得員工個人假別資料標準檔
        /// </summary>
        /// <param name="CompanyCode"></param>
        /// <param name="EmpID"></param>
        /// <param name="AbsentID"></param> 
        /// <returns></returns>
        public async Task<List<EmpHolidayData>> GetEmpHolidayDataList(Guid CompanyID, string EmployeeNO,string AbsentID)
        {
            //獲得公司資訊
            Company companyInfo = Services.GetService<CompanyService>().GetCompanyById(CompanyID);
            List<EmpHolidayData> empHolidayData = await HRMApiAdapter.GetEmpHolidayData(companyInfo.CompanyCode, EmployeeNO, AbsentID);
            return empHolidayData;
        }

        /// <summary>
        /// 取得員工個人空中飄的補休
        /// </summary>
        /// <param name="CompanyCode"></param>
        /// <param name="EmpID"></param>
        /// <returns></returns>
        public async Task<List<EmpHolidayData>> GetUnLockRestLeaveInfo(Guid CompanyID, string EmployeeNO)
        {
            //獲得公司資訊
            Company companyInfo = Services.GetService<CompanyService>().GetCompanyById(CompanyID);
            List<EmpHolidayData> empHolidayData = await HRMApiAdapter.GetUnLockRestLeaveInfo(companyInfo.CompanyCode, EmployeeNO);
            return empHolidayData;
        }
    }
}
