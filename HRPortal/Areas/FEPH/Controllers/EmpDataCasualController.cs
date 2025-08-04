using HRPortal.DBEntities;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.ApiAdapter;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRProtal.Core;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Areas.FEPH.Controllers
{
    public class EmpDataCasualController : BaseController
    {
        // GET: FEPH/EmpDataCasual
        public async Task<ActionResult> Index(int page = 1, string Cmd = "", string SearchDepartmentData = "", string SearchEmpID = "", string SearchEmpName = "", string SearchIDNumber = "")
        {
            GetDefaultData(SearchDepartmentData, SearchEmpID, SearchEmpName, SearchIDNumber);

            int currentPage = page < 1 ? 1 : page;

            if (Cmd == "btnQuery")
            {
                //呼叫 WebApi - GetEmpDataCasual
                GetEmpDataCasual data = await HRMApiAdapter.GetEmpDataCasual(CurrentUser.Employee.Company.CompanyCode, SearchDepartmentData, SearchEmpID, SearchEmpName, SearchIDNumber, currentPage, currentPageSize);

                #region Create Page List Data
                int startIndex = 0, endIndex = 0;

                startIndex = (currentPage - 1) * currentPageSize;
                if (startIndex < 0)
                    startIndex = 0;

                endIndex = currentPage * currentPageSize;
                if (endIndex > data.DataCount)
                    endIndex = data.DataCount;

                List<EmpDataCasual> result = new List<EmpDataCasual>();

                for (int i = 0; i < startIndex; i++)
                {
                    result.Add(null);
                }

                foreach (var item in data.EmployeeData)
                {
                    result.Add(item);
                }

                for (int i = endIndex; i < data.DataCount; i++)
                {
                    result.Add(null);
                }
                #endregion

                return View(result.ToPagedList(currentPage, currentPageSize));
            }
            else
            {
                return View();
            }
        }

        // POST: FEPH/EmpDataCasual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string btnQuery, string btnClear, string SearchDepartmentData, string SearchEmpID, string SearchEmpName, string SearchIDNumber)
        {
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                return RedirectToAction("Index", new
                {
                    Cmd = "btnQuery",
                    SearchDepartmentData,
                    SearchEmpID,
                    SearchEmpName,
                    SearchIDNumber
                });
            }

            //重整
            GetDefaultData(SearchDepartmentData, SearchEmpID, SearchEmpName, SearchIDNumber);
            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="departmentdata"></param>
        /// <param name="empid"></param>
        /// <param name="empname"></param>
        private void GetDefaultData(string departmentdata = "", string empid = "", string empname = "", string idnumber = "")
        {
            ViewData["DepartmentList"] = GetDepartmentList(departmentdata);
            ViewBag.SearchDepartmentData = departmentdata;
            ViewBag.SearchEmpID = empid;
            ViewBag.SearchEmpName = empname;
            ViewBag.SearchIDNumber = idnumber;
        }

        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata">被選取的部門</param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
            }

            return listItem;
        }

        /// <summary>
        /// 取得費用別列表
        /// </summary>
        /// <param name="selecteddata">被選取的費用別</param>
        /// <returns></returns>
        private List<SelectListItem> GetCostList(string deptcode, string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<DeptCost> data = Services.GetService<DeptCostService>().GetListsByDeptCode(deptcode).OrderBy(x => x.CostName).ToList();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.CostName, Value = item.CostCode, Selected = (selecteddata == item.CostCode ? true : false) });
            }

            return listItem;
        }

        /// <summary>
        /// 取得國籍列表
        /// </summary>
        /// <param name="selecteddata">被選取的國籍</param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> GetNationList(string selecteddata = "")
        {
            var result = await HRMApiAdapter.GetNationData(CurrentUser.CompanyCode);  //國籍下拉

            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in result)
            {
                listItem.Add(new SelectListItem { Text = item.Name, Value = item.Code, Selected = (selecteddata == item.Code ? true : false) });
            }

            return listItem;
        }

        /// <summary>
        /// 取得外籍身分註記列表
        /// </summary>
        /// <param name="selecteddata">被選取的外籍身分註記</param>
        /// <returns></returns>
        private List<SelectListItem> GetForeignTypeList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            //20170630 Daniel 修改外籍配偶說明文字
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "本人", Value = "Y", Selected = (selecteddata == "Y" ? true : false) });
            listItem.Add(new SelectListItem { Text = "外籍配偶(含港澳配偶)", Value = "1", Selected = (selecteddata == "1" ? true : false) });
            listItem.Add(new SelectListItem { Text = "大陸配偶", Value = "2", Selected = (selecteddata == "2" ? true : false) });

            return listItem;
        }

        /// <summary>
        /// 給下拉式選單讀取費用別列表
        /// </summary>
        /// <param name="DeptCode"></param>
        /// <returns></returns>
        public ActionResult GetCost(string DeptCode)
        {
            List<SelectListItem> result = GetCostList(DeptCode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: FEPH/EmpDataCasual/Create
        public async Task<ActionResult> Create()
        {
            GetEmpDataCasualDetail data = new GetEmpDataCasualDetail();
            data.CompanyCode = CurrentUser.CompanyCode;
            data.CompanyName = CurrentUser.CompanyName;
            data.Sex = "M";
            data.AssumeDate = DateTime.Now;
            data.Birthday = DateTime.Now;
            data.Married = false;
            data.AsAbove = false;
            data.NationType = true;
            data.ForeignType = "";
            data.NationCode = "";

            ViewData["DepartmentList"] = GetDepartmentList();
            ViewData["DeptCostList"] = GetCostList("");
            ViewData["NationList"] = await GetNationList();
            ViewData["ForeignTypeList"] = GetForeignTypeList();

            return PartialView("_CreateEmpDataCasual", data);
        }

        // POST: FEPH/EmpDataCasual/Create
        [HttpPost]
        public async Task<ActionResult> Create(GetEmpDataCasualDetail data)
        {
            //呼叫 WebApi - SaveEmpDataCasual 臨時員工資料存檔
            RequestResult result = await HRMApiAdapter.SaveEmpDataCasual(data);

            if (result.Status)
            {
                WriteLog("Success:" + (result.Status ? result.Message : data.EmpID));
                return Json(new AjaxResult() { status = "success", message = string.Format("送出成功，員工編號為{0}", result.Message) });
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = result.Message });
            }
        }

        // GET: FEPH/EmpDataCasual/Edit
        public async Task<ActionResult> Edit(string deptCode, string empNo)
        {
            //呼叫 WebApi - GetEmpDataCasual 臨時員工資料查詢
            GetEmpDataCasualDetail data = await HRMApiAdapter.GetEmpDataCasualDetail(CurrentUser.Employee.Company.CompanyCode, deptCode, empNo);

            ViewData["DepartmentList"] = GetDepartmentList();
            ViewData["DeptCostList"] = GetCostList(data.DeptCode);
            ViewData["NationList"] = await GetNationList();
            ViewData["ForeignTypeList"] = GetForeignTypeList();

            return PartialView("_EditEmpDataCasual", data);
        }

        // POST: FEPH/EmpDataCasual/Update
        [HttpPost]
        public async Task<ActionResult> Update(GetEmpDataCasualDetail data)
        {
            //呼叫 WebApi - SaveEmpDataCasual 臨時員工資料存檔
            RequestResult result = await HRMApiAdapter.SaveEmpDataCasual(data);

            if (result.Status)
            {
                WriteLog("Success:" + data.EmpID);
                //20170911 Start Daniel 增加顯示刪除勞退資料的訊息(如果有的話)
                //return Json(new AjaxResult() { status = "success", message = "送出成功" });
                return Json(new AjaxResult() { status = "success", message = "送出成功" + result.Message });

                //20170911 End
                
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = result.Message });
            }
        }

        [HttpPost]
        public ActionResult IDNumberCheckExists(string IDNumber)
        {
            return Json(new { Result = IDNumberCheck(IDNumber) });
        }

        /// <summary>
        /// 檢查是否符合台灣身分證字號規則
        /// </summary>
        /// <param name="id" />身分證字號</param>
        /// <returns></returns>
        private static bool IDNumberCheck(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;   //沒有輸入，回傳 ID 錯誤
            id = id.ToUpper();
            var regex = new Regex("^[A-Z]{1}[0-9]{9}$");
            if (!regex.IsMatch(id))
                return false;   //Regular Expression 驗證失敗，回傳 ID 錯誤

            int[] seed = new int[10];       //除了檢查碼外每個數字的存放空間
            string[] charMapping = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "X", "Y", "W", "Z", "I", "O" };
            //A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
            //P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35
            string target = id.Substring(0, 1);
            for (int index = 0; index < charMapping.Length; index++)
            {
                if (charMapping[index] == target)
                {
                    index += 10;
                    seed[0] = index / 10;       //10進制的高位元放入存放空間
                    seed[1] = (index % 10) * 9; //10進制的低位元*9後放入存放空間
                    break;
                }
            }
            for (int index = 2; index < 10; index++)
            {   //將剩餘數字乘上權數後放入存放空間
                seed[index] = Convert.ToInt32(id.Substring(index - 1, 1)) * (10 - index);
            }
            //檢查是否符合檢查規則，10減存放空間所有數字和除以10的餘數的個位數字是否等於檢查碼
            //(10 - ((seed[0] + .... + seed[9]) % 10)) % 10 == 身分證字號的最後一碼
            return (10 - (seed.Sum() % 10)) % 10 == Convert.ToInt32(id.Substring(9, 1));
        }
    }
}