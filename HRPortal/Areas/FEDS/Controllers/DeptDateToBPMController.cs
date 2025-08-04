using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using DocumentFormat.OpenXml.Drawing;
using System.Net;
using System.IO;
using HRPortal.Mvc.Controllers;
using System.Threading.Tasks;
using HRPortal.Mvc.Models;
using PagedList;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.Services;
using Microsoft.AspNet.Identity;
using HRPortal.Mvc;
using HRPortal.Mvc.Results;
using HRPortal.DBEntities;
using System.Configuration;
namespace HRPortal.Areas.FEDS.Controllers
{
    [AllowAnonymous]
    public class DeptDateToBPMController : BaseController
    {


        private DepartmentService _departmentService;
        public DepartmentService Service
        {
            get
            {
                if (_departmentService == null)
                {
                    _departmentService = Services.GetService<DepartmentService>();
                }
                return _departmentService;
            }
        }

        // GET: /FEDS/DeptDateToBPM/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DeptInfo()
        {
            string NowDate = DateTime.Now.ToString("yyyyMMdd");
            // string path = Request.PhysicalApplicationPath + @"FileUpload\Temp\";
            //string path = Request.PhysicalApplicationPath ;
            string path = @"C:\temp\";
            ExportDeptExcel(NowDate, path);

            UploadFTP(path + "FEDS_Dept_" + NowDate + ".xlsx", "FEDS_Dept_" + NowDate + ".xlsx", "Dept");

            return Content("1");
        }

        public void ExportDeptExcel(string NowDate, string Path)
        {

            IWorkbook wb = new XSSFWorkbook();
            ISheet ws = wb.CreateSheet("FEDS_DEPT_" + NowDate);
            List<Department> DeptList = Services.GetService<DepartmentService>().GetAllLists().Where(x => x.Enabled).ToList();

            //設定欄位名稱
            List<string> expenditure_name = new List<string>() { "公司別", "分公司別名稱", "系統別", "部門代碼", "部門名稱", "上層部門代號", "上層部門全名", "主管NOTES帳號", "主管姓名", "覆核主管帳號", "覆核主管姓名", "預留欄位帳號_1", "預留欄位姓名_1", "預留欄位帳號_2", "預留欄位姓名_2", "可會辦單位(零用金專用)", "部門層級", "刪除註記" };
            //設定表頭
            ws.CreateRow(0);
            for (int i = 0; i < expenditure_name.Count; i++)
            {
                ws.GetRow(0).CreateCell(i).SetCellValue(expenditure_name[i]);
            }
            int count = 1;

            foreach (Department dept in DeptList)
            {
                if (dept.Company.CompanyName != "裕民股份有限公司")//BPM寫死排除非遠百公司匯入資料 Irving 2017/01/12
                {
                    if (dept.DepartmentCode != "5310003" && dept.DepartmentCode != "1100000" && dept.DepartmentCode != "5310001" && dept.DepartmentCode != "5310002" && dept.DepartmentCode != "7000000")
                    {

                        //上層單位   
                        Department deptLast = Services.GetService<DepartmentService>().GetParentDepartmentDate(dept.SignParentID).FirstOrDefault();
                        //所有部門
                        List<Department> newallDept = new List<Department>();
                        List<Department> allDeptList = GetFullDept(dept, newallDept);
                        string strDept = "";
                        foreach (Department alldept in allDeptList)
                        {
                            strDept = alldept.DepartmentName + "\\" + strDept;
                        }
                        strDept = strDept.Substring(0, strDept.Length - 1);

                        //上層部門所有部門
                        string strDeptUP = "";
                        if (dept.SignParentID.HasValue)
                        {
                            List<Department> newUPallDept = new List<Department>();
                            Department UpDept = Services.GetService<DepartmentService>().GetParentDepartmentDate(dept.SignParentID).FirstOrDefault();
                            List<Department> allUPDeptList = GetFullDept(UpDept, newUPallDept);
                            foreach (Department alldept in allUPDeptList)
                            {
                                strDeptUP = alldept.DepartmentName + "\\" + strDeptUP;
                            }
                            strDeptUP = strDeptUP.Substring(0, strDeptUP.Length - 1);
                        }
                        //分公司別名稱
                        var CompanyID = Services.GetService<DepartmentService>().GetParentDepartmentDate(dept.ParentDepartmentID).Select(x => x.CompanyID).FirstOrDefault();
                        var CompanyName = Services.GetService<CompanyService>().GetCompanyDate(CompanyID).Select(x => x.CompanyName).FirstOrDefault();
                        ws.CreateRow(count);

                        ws.GetRow(count).CreateCell(0).SetCellValue("FEDS"); //公司別
                        ws.GetRow(count).CreateCell(1).SetCellValue(dept.Company.CompanyName); //分公司別名稱
                        ws.GetRow(count).CreateCell(2).SetCellValue("PRPO"); //系統別
                        ws.GetRow(count).CreateCell(3).SetCellValue(dept.DepartmentCode); //部門代碼
                        ws.GetRow(count).CreateCell(4).SetCellValue(dept.DepartmentName); //部門名稱

                        if (deptLast != null)
                        {
                            ws.GetRow(count).CreateCell(5).SetCellValue(deptLast.DepartmentCode); //上層部門代號
                        }
                        if (dept.DepartmentCode == "1200000")//董事會
                        {
                            ws.GetRow(count).CreateCell(6).SetCellValue(strDeptUP.Replace("股東大會", "遠東集團")); //上層部門全名
                        }
                        else
                        {
                            ws.GetRow(count).CreateCell(6).SetCellValue(strDeptUP.Replace("股東大會\\董事會", "遠東集團\\遠東百貨")); //上層部門全名
                        }
                        if (dept.Company.CompanyCode != "1010")
                        {
                            if (deptLast == null)
                            {
                                ws.GetRow(count).CreateCell(6).SetCellValue("遠東集團\\遠東百貨\\董事長室\\總經理室\\營運本部" + strDeptUP); //上層部門全名    
                            }
                            else
                            {
                                ws.GetRow(count).CreateCell(6).SetCellValue("遠東集團\\遠東百貨\\董事長室\\總經理室\\營運本部\\" + strDeptUP); //上層部門全名 
                            }
                        }
                        if (dept.SignManager != null)
                        {
                            if (dept.SignManager.EmployeeNO != null)
                            {
                                ws.GetRow(count).CreateCell(7).SetCellValue("FEDS" + dept.SignManager.EmployeeNO); //主管NOTES帳號
                            }
                            else
                            {
                                ws.GetRow(count).CreateCell(7).SetCellValue(""); //主管NOTES帳號
                            }
                            ws.GetRow(count).CreateCell(8).SetCellValue(dept.SignManager.EmployeeName); //主管姓名
                        }
                        ws.GetRow(count).CreateCell(9).SetCellValue(""); //覆核主管帳號
                        ws.GetRow(count).CreateCell(10).SetCellValue(""); //覆核主管姓名
                        ws.GetRow(count).CreateCell(11).SetCellValue(""); //預留欄位帳號_1
                        ws.GetRow(count).CreateCell(12).SetCellValue(""); //預留欄位姓名_1
                        ws.GetRow(count).CreateCell(13).SetCellValue(""); //預留欄位帳號_2
                        ws.GetRow(count).CreateCell(14).SetCellValue(""); //預留欄位姓名_2
                        ws.GetRow(count).CreateCell(15).SetCellValue(""); //可會辦單位(零用金專用)
                        ws.GetRow(count).CreateCell(16).SetCellValue(Convert.ToString(dept.DepartmentsLevel));//部門層級
                        //ws.GetRow(count).CreateCell(17).SetCellValue(""); //是否自動轉檔(新增)
                        ws.GetRow(count).CreateCell(17).SetCellValue(""); //刪除註記

                        count++;
                    }
                }
            }
            string file_path = Path + "FEDS_DEPT_" + NowDate + ".xlsx";
            FileStream file = new FileStream(file_path, FileMode.Create, FileAccess.ReadWrite);
            wb.Write(file);
            file.Close();
        }


        private List<Department> GetFullDept(Department dept, List<Department> deptList)
        {
            deptList.Add(dept);
            IEnumerable<Department> LastDept = Services.GetService<DepartmentService>().GetParentDepartmentDate(dept.SignParentID);
            if (LastDept.Any())
            {
                GetFullDept(LastDept.FirstOrDefault(), deptList);
            }
            return deptList;
        }
        public static void UploadFTP(string source, string sourcename, string passpath)
        {
            //關閉測試機
            //try
            //{
            //    // Get the object used to communicate with the server.
            //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.2.2.230/" + passpath + "/" + sourcename);
            //    request.Method = WebRequestMethods.Ftp.UploadFile;
            //    // This example assumes the FTP site uses anonymous logon.
            //    request.Credentials = new NetworkCredential("hrftp01", "h5ft901#");
            //    request.UseBinary = true;

            //    byte[] fileContents = System.IO.File.ReadAllBytes(source);
            //    request.ContentLength = fileContents.Length;
            //    Stream requestStream = request.GetRequestStream();
            //    requestStream.Write(fileContents, 0, fileContents.Length);
            //    requestStream.Close();

            //    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            //    response.Close();
            //}
            //catch
            //{}

            //正式機
            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request_p = (FtpWebRequest)WebRequest.Create("ftp://10.1.7.183/" + passpath + "/" + sourcename);
                request_p.Method = WebRequestMethods.Ftp.UploadFile;
                // This example assumes the FTP site uses anonymous logon.
                request_p.Credentials = new NetworkCredential("hrftp01", "h5ft901#");
                request_p.UseBinary = true;

                byte[] fileContents_p = System.IO.File.ReadAllBytes(source);
                request_p.ContentLength = fileContents_p.Length;
                Stream requestStream_p = request_p.GetRequestStream();
                requestStream_p.Write(fileContents_p, 0, fileContents_p.Length);
                requestStream_p.Close();

                FtpWebResponse response_p = (FtpWebResponse)request_p.GetResponse();
                response_p.Close();
            }
            catch
            { }

        }
    }
}