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
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using HRPortal.MultiLanguage;
using System.Globalization;

namespace HRPortal.Areas.FEPH.Controllers
{
    public class EmergencyStaffListController : BaseController
    {
        //GET: /FEPH/EmergencyStaffList/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> createEmergencyStaffList(string downloadDate,string compamyArea)
        {
            List<EmergencyEmpDutyCardViewModel> tempEmpDutyCard = new List<EmergencyEmpDutyCardViewModel>();

            try
            {   
                // Open the text file using a stream reader.
                StreamReader sr = new StreamReader(@"D:\TW9500\TextLog\" + downloadDate + ".txt");
                String line;
                int cardFlag = 1;
                while ((line = sr.ReadLine()) != null)
                {
                    string cardNo = line.Substring(32, 10); //卡號
                    string inOrOut = line.Substring(30, 2); //進出別
                    string dutyCardTime = line.Substring(2, 12); //刷卡時間
                    int flagAddCard = 0;

                    //20170517 Start Daniel，修改原來邏輯的Bug，重寫判斷
                    
                    //檢查之前是否有本筆卡號
                    int CardExistedIndex=tempEmpDutyCard.FindIndex(x => x.cardNo == cardNo);

                    //清單已經有本筆卡號，先移除該卡號資料
                    if (CardExistedIndex>=0) 
                    {
                        tempEmpDutyCard.RemoveAt(CardExistedIndex);
                    }
                    
                    //判斷本筆刷卡紀錄是否為刷進，刷進就新增一筆
                    if (inOrOut == "01")
                    {
                        tempEmpDutyCard.Add(new EmergencyEmpDutyCardViewModel { dutyCardTime = dutyCardTime, cardNo = cardNo, empID = "" });
                    }
                    
                    
                    /* 註解掉原來的
                    foreach (var dutyCard in tempEmpDutyCard)
                    {
                        if (dutyCard.cardNo == cardNo)
                        {
                            cardFlag = 0;
                            tempEmpDutyCard.Remove(dutyCard);
                            if (inOrOut == "01") //重複刷卡取最後一筆
                            {
                                cardFlag = 1;
                            }
                            break;
                        }
                        else if (dutyCard.cardNo != cardNo && inOrOut != "02")
                        {
                            cardFlag = 1;
                        }
                    }
                    if (cardFlag == 1)
                    {
                        tempEmpDutyCard.Add(new EmergencyEmpDutyCardViewModel { dutyCardTime = dutyCardTime, cardNo = cardNo, empID = "" });
                        cardFlag = 0;
                    }
                    */
                    //20170517 End
                }
                sr.Close();

                #region 呼叫 WebApi - GetEmpDutyCard 取得使用中出勤卡號
                DateTime ExcuteDate = DateTime.Now;
                List<EmpDutyCardInfoData> empDutyCardData = await HRMApiAdapter.GetAllEmpDutyCard(CurrentUser.CompanyCode, ExcuteDate);
                #endregion
                List<EmergencyEmpDutyCardViewModel> empDutyCard = new List<EmergencyEmpDutyCardViewModel>();
                List<EmployeeData> _hrmEmployee = await HRMApiAdapter.GetDeptEmployee(CurrentUser.CompanyCode,null);
                //List<DepartmentData> _hrmDepartment = await HRMApiAdapter.GetDepartment(CurrentUser.CompanyCode);
                foreach (var tempCard in tempEmpDutyCard)
                {
                    
                    //員工工號
                    var curEmpDutyInfo = empDutyCardData.FirstOrDefault(x => x.CardNo == tempCard.cardNo);
                    tempCard.empID = curEmpDutyInfo!=null?curEmpDutyInfo.EmpID:"error";
                    if (tempCard.empID != "error")
                    {
                        
                        EmployeeData EmpData = _hrmEmployee.FirstOrDefault(x => x.EmpID == tempCard.empID);
                        //員工姓名
                        tempCard.empName = EmpData.EmpName;
                        //員工英文姓名
                        tempCard.empNameEN = EmpData.EmpNameEN;
                        //部門代碼
                        tempCard.empDeptCode = EmpData.DeptCode;
                        //部門名稱
                        tempCard.empDeptName = EmpData.DeptName;

                        int empDeptCode= Int32.Parse(tempCard.empDeptCode);
                        if (compamyArea == "tpe" && empDeptCode < 100)// tpe:台北公司 itabashi:板橋公司
                        {
                            empDutyCard.Add(tempCard);
                        }
                        else if (compamyArea == "itabashi" && empDeptCode >= 100)
                        {
                            empDutyCard.Add(tempCard);
                        }
                    }
                    else 
                    {
                        var CardNo = tempCard.cardNo;
                        empDutyCard.Add(tempCard);
                    }
                    
                }
                tempEmpDutyCard = empDutyCard;
            }
            catch (Exception e)
            {
                //return Json(new AjaxResult() { status = "error", message = "The file could not be read" });
                return Json(new AjaxResult() { status = "error", message = e.Message});
            }

            

            // Create the workbook
            XLWorkbook workbook = new XLWorkbook();
            var ListSheet = workbook.Worksheets.Add("EmergencyStaffList");
            ListSheet.Columns(1,7).Width = 15;
            ListSheet.Columns(4,4).Width = 19;
            ListSheet.Columns(6,6).Width = 25;
            int colIdx = 1;
            int rowIdy = 2; 
            foreach (string colName in "員工工號;卡號;員工姓名;員工英文姓名;部門代碼;部門名稱;刷卡時間".Split(';'))
            {
                ListSheet.Cell(1, colIdx).Value = colName;
                ListSheet.Cell(1, colIdx).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                colIdx++;
            }

            foreach (EmergencyEmpDutyCardViewModel tempCard in tempEmpDutyCard)
            {
                DateTime dt = DateTime.ParseExact(tempCard.dutyCardTime, "yyyyMMddHHmm", null);
                //員工工號(1)
                ListSheet.Cell(rowIdy, 1).Value = tempCard.empID;
                ListSheet.Cell(rowIdy, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ListSheet.Cell(rowIdy, 1).DataType = XLCellValues.Text;
                //員工卡號(2)
                ListSheet.Cell(rowIdy, 2).Value = tempCard.cardNo;
                ListSheet.Cell(rowIdy, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ListSheet.Cell(rowIdy, 2).DataType = XLCellValues.Text;
                //員工姓名(3)
                ListSheet.Cell(rowIdy, 3).Value = tempCard.empName;
                ListSheet.Cell(rowIdy, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ListSheet.Cell(rowIdy, 3).DataType = XLCellValues.Text;
                //員工英文姓名(4)
                ListSheet.Cell(rowIdy, 4).Value = tempCard.empNameEN;
                ListSheet.Cell(rowIdy, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ListSheet.Cell(rowIdy, 4).DataType = XLCellValues.Text;
                //部門代碼(5)
                ListSheet.Cell(rowIdy, 5).Value = tempCard.empDeptCode;
                ListSheet.Cell(rowIdy, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ListSheet.Cell(rowIdy, 5).DataType = XLCellValues.Text;
                //部門名稱(6)
                ListSheet.Cell(rowIdy, 6).Value = tempCard.empDeptName;
                ListSheet.Cell(rowIdy, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ListSheet.Cell(rowIdy, 6).DataType = XLCellValues.Text;
                //刷入時間(7)
                ListSheet.Cell(rowIdy, 7).Value = dt.ToString("yyyy/MM/dd HH:mm");
                ListSheet.Cell(rowIdy, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ListSheet.Cell(rowIdy, 7).Style.DateFormat.SetFormat("yyyy/MM/dd HH:mm");
                rowIdy++;
            }

            // Send the file
            MemoryStream excelStream = new MemoryStream();
            workbook.SaveAs(excelStream);
            excelStream.Position = 0;

            string exportFileName = string.Concat("EmergencyStaffList_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
            Session[exportFileName] = excelStream;
            return Json(new AjaxResult() { status = "success", message = exportFileName });
        }

        [HttpGet]
        public ActionResult DownloadExcelReport(string fName)
        {
            var ms = Session[fName] as MemoryStream;
            if (ms == null)
                return new EmptyResult();
            Session[fName] = null;
            return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fName);
        }
    }
}
