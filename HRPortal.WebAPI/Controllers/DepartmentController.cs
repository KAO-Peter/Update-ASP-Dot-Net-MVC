using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using HRPortal.DBEntities;
using HRPortal.Services;

namespace HRPortal.WebAPI.Controllers
{
    /// <summary>
    /// Department API 項目.
    /// </summary>
    public class DepartmentController : ApiController
    {
        private DepartmentService _departmentService;
        private CompanyService _companyService;


        /// <summary>
        /// DepartmentController
        /// </summary>
        /// <param name="departmentService"></param>
        public DepartmentController(DepartmentService departmentService, CompanyService companyService)
        {
            this._departmentService = departmentService;
            this._companyService = companyService;
        }
        //Irving 2016/10/26
        // GET  api/Department/GetDepartmentList 
        /// <summary>
        /// 查詢部門資訊
        /// </summary>
        /// <param name="none">none</param>
        /// <returns>
        /// CompanyID : 分公司
        /// CompanyName : 分公司名稱
        /// SystemID : 系統別
        /// DepartmentCode : 部門代碼
        /// DepartmentName : 部門名稱
        /// SignManagerCode : 上層部門代碼
        /// SignManagerName : 上層部門全名
        /// ManagerEmployeeNO : 主管工號
        /// ManagerEmployeeName : 主管姓名
        /// FirstReservedEmployeeNO : 覆核主管工號
        /// FirstReservedEmployeeName : 覆核主管姓名
        /// FirstReservedEmployeeNO : 預留欄位帳號_1
        /// FirstReservedEmployeeName : 預留欄位帳號姓名_1
        /// SecondReservedEmployeeNO : 預留欄位帳號_2
        /// SecondReservedEmployeeName : 預留欄位帳號姓名_2
        /// ForCash : 可會辦單位(零用金專用)
        /// DepartmentLevel : 部門層級
        /// Transfer : 是否自動轉檔(新增)
        /// Mark : 是否註記
        /// </returns>
        [HttpGet]
        public IList<DepartmentRes> GetDepartmentList()
        {
            var tempDept = this._departmentService.GetAllLists().ToList();
          
            IList<DepartmentRes> DeptList = new List<DepartmentRes>();
           
                foreach (var item in tempDept)
                {
                    DepartmentRes temp = new DepartmentRes();
                    //分公司
                    temp.CompanyID = "FEDS";
                    //分公司名稱
                    var CompanyID = this._departmentService.GetParentDepartmentDate(item.ID).Select(x => x.CompanyID).FirstOrDefault();
                    var CompanyName = this._companyService.GetCompanyDate(CompanyID).Select(x => x.CompanyName).FirstOrDefault();
                    temp.CompanyName = CompanyName;
                    //系統別
                    temp.SystemID = "PRPO";
                    //部門代碼
                    temp.DepartmentCode = item.DepartmentCode;
                    //部門名稱
                    temp.DepartmentName = item.DepartmentName;
                    //上層部門代碼
                    var ParentDepartmentCode = this._departmentService.GetParentDepartmentDate(item.ParentDepartmentID).Select(x => x.DepartmentCode).FirstOrDefault();
                    var ParentDepartmentName = this._departmentService.GetParentDepartmentDate(item.ParentDepartmentID).Select(x => x.DepartmentName).FirstOrDefault();

                    var ParentDepartmentID_1 = this._departmentService.GetParentDepartmentDate(item.ParentDepartmentID).Select(x => x.ParentDepartmentID).FirstOrDefault();
                    var ParentDepartmentName_1 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_1).Select(x => x.DepartmentName).FirstOrDefault();

                    var ParentDepartmentID_2 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_1).Select(x => x.ParentDepartmentID).FirstOrDefault();
                    var ParentDepartmentName_2 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_2).Select(x => x.DepartmentName).FirstOrDefault();

                    var ParentDepartmentID_3 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_2).Select(x => x.ParentDepartmentID).FirstOrDefault();
                    var ParentDepartmentName_3 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_3).Select(x => x.DepartmentName).FirstOrDefault();

                    var ParentDepartmentID_4 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_3).Select(x => x.ParentDepartmentID).FirstOrDefault();
                    var ParentDepartmentName_4 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_4).Select(x => x.DepartmentName).FirstOrDefault();

                    var ParentDepartmentID_5 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_4).Select(x => x.ParentDepartmentID).FirstOrDefault();
                    var ParentDepartmentName_5 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_5).Select(x => x.DepartmentName).FirstOrDefault();

                    var ParentDepartmentID_6 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_5).Select(x => x.ParentDepartmentID).FirstOrDefault();
                    var ParentDepartmentName_6 = this._departmentService.GetParentDepartmentDate(ParentDepartmentID_6).Select(x => x.DepartmentName).FirstOrDefault();
                    temp.SignManagerCode = ParentDepartmentCode;
                    //上層部門全名

                    if (ParentDepartmentName_4 == "董事會" && ParentDepartmentName_5 == "股東大會")
                    {
                        ParentDepartmentName_4 = "遠東百貨";
                        ParentDepartmentName_5 = "遠東集團";
                        temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3 + "/" + ParentDepartmentName_4 + "/" + ParentDepartmentName_5;
                    }
                    else
                    {
                        if (ParentDepartmentName != null)
                        {
                            if (ParentDepartmentName == "股東大會")
                            {
                                ParentDepartmentName = "遠東集團";
                                temp.SignManagerName = ParentDepartmentName;
                            }
                            else { 
                                temp.SignManagerName = ParentDepartmentName; 
                            }
                        }
                        if (ParentDepartmentName_1 != null)
                        {
                            if (ParentDepartmentName == "董事會" && ParentDepartmentName_1 == "股東大會")
                            {
                                ParentDepartmentName = "遠東百貨";
                                ParentDepartmentName_1 = "遠東集團";
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1;
                            }
                            else
                            {
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1;
                            }
                        } if (ParentDepartmentName_2 != null)
                        {
                            if (ParentDepartmentName_1 == "董事會" && ParentDepartmentName_2 == "股東大會")
                            {
                                ParentDepartmentName_1 = "遠東百貨";
                                ParentDepartmentName_2 = "遠東集團";
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2;
                            }
                            else {
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2;
                            }
                        } if (ParentDepartmentName_3 != null)
                        {
                            if (ParentDepartmentName_2 == "董事會" && ParentDepartmentName_3 == "股東大會")
                            {
                                ParentDepartmentName_2 = "遠東百貨";
                                ParentDepartmentName_3 = "遠東集團";
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3;
                            }
                            else
                            {
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3;
                            }
                        } if (ParentDepartmentName_4 != null)
                        {
                            if (ParentDepartmentName_3 == "董事會" && ParentDepartmentName_4 == "股東大會")
                            {
                                ParentDepartmentName_3 = "遠東百貨";
                                ParentDepartmentName_4 = "遠東集團";
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3 + "/" + ParentDepartmentName_4;
                            }
                            else {
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3 + "/" + ParentDepartmentName_4;
                            }
                        } if (ParentDepartmentName_5 != null)
                        {
                            temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3 + "/" + ParentDepartmentName_4 + "/" + ParentDepartmentName_5;
                        } if (ParentDepartmentName_6 != null)
                        {
                            if (ParentDepartmentName_5 == "董事會" && ParentDepartmentName_6 == "股東大會")
                            {
                                ParentDepartmentName_5 = "遠東百貨";
                                ParentDepartmentName_6 = "遠東集團";
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3 + "/" + ParentDepartmentName_4 + "/" + ParentDepartmentName_5 + "/" + ParentDepartmentName_6;
                            }
                            else
                            {
                                temp.SignManagerName = ParentDepartmentName + "/" + ParentDepartmentName_1 + "/" + ParentDepartmentName_2 + "/" + ParentDepartmentName_3 + "/" + ParentDepartmentName_4 + "/" + ParentDepartmentName_5 + "/" + ParentDepartmentName_6;
                            }
                        }
                       
                            
                        
                    }
                    if (item.Manager != null)
                    {
                        //主管工號
                        temp.ManagerEmployeeNO = "FEDS" + item.Manager.EmployeeNO;
                        //主管姓名
                        temp.ManagerEmployeeName = item.Manager.EmployeeName;
                    }
                    //覆核主管工號
                    temp.FirstReservedEmployeeNO = null;
                    //覆核主管姓名
                    temp.FirstReservedEmployeeName = null;
                    //預留欄位帳號_1
                    temp.FirstReservedEmployeeNO = null;
                    //預留欄位帳號姓名_1
                    temp.FirstReservedEmployeeName = null;
                    //預留欄位帳號_2
                    temp.SecondReservedEmployeeNO = null;
                    //預留欄位帳號姓名_2
                    temp.SecondReservedEmployeeName = null;
                    //可會辦單位(零用金專用)
                    temp.ForCash = null;
                    //部門層級
                    if (item.DepartmentsLevel != null)
                    {                      
                        temp.DepartmentLevel = item.DepartmentsLevel;
                    }
                    else {
                        temp.DepartmentLevel = null;                    
                    }
                    //是否自動轉檔(新增)
                    temp.Transfer = null;
                    //是否註記
                    temp.Mark = null;



                    ////後台部門主管資訊
                    //if (item.Manager != null)
                    //{
                    //    temp.ManagerEmpData = new BasicInfo();
                    //    temp.ManagerEmpData.ID = item.Manager.ID;
                    //    temp.ManagerEmpData.Name = item.Manager.EmployeeName;
                    //    temp.ManagerEmpData.NameEN = item.Manager.EmployeeEnglishName;
                    //    temp.ManagerEmpData.Code = item.Manager.EmployeeNO;
                    //}
                    ////簽核部門主管資訊
                    //if (item.SignManager != null)
                    //{
                    //    temp.SignManagerEmpData = new BasicInfo();
                    //    temp.SignManagerEmpData.ID = item.SignManager.ID;
                    //    temp.SignManagerEmpData.Name = item.SignManager.EmployeeName;
                    //    temp.SignManagerEmpData.NameEN = item.SignManager.EmployeeEnglishName;
                    //    temp.SignManagerEmpData.Code = item.SignManager.EmployeeNO;
                    //}

                    ////上層簽核部門
                    //if (item.Parent != null)
                    //{
                    //    temp.ParentDepartmentData = new BasicInfo();
                    //    temp.ParentDepartmentData.ID = item.Parent.ID;
                    //    temp.ParentDepartmentData.Name = item.Parent.DepartmentName;
                    //    temp.ParentDepartmentData.NameEN = item.Parent.DepartmentEnglishName;
                    //    temp.ParentDepartmentData.Code = item.Parent.DepartmentCode;
                    //}

                    ////部門生效日期
                    //temp.BeginDate = item.BeginDate;
                    ////部門失效日期
                    //if (item.EndDate != null)
                    //{
                    //    temp.EndDate = item.EndDate.Value;
                    //}
                    //部門前台建立日期
                    //temp.CreatedTime = item.CreatedTime;
                    DeptList.Add(temp);
                }
            
            return DeptList;
        }
    }
}
