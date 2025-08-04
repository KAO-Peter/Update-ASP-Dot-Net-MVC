using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class PendingFormsController : BaseController
    {
        // GET: /ToDo/SignForms/
        public async Task<ActionResult> Index(int page = 1)
        {
            
            int currentPage = page < 1 ? 1 : page;
            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }

            Session["AbsentsData"]  = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);
            Session["AbsentsDataAll"] = await HRMApiAdapter.GetAllAbsentData("");
            //var b =await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode);
            return View(QueryForPendding().ToPagedList(currentPage, currentPageSize));
        }
        //取附件檔案 Irving 20161209
        public ActionResult DownloadLeaveFormFile(string formNo, string formType = "Leave")
        {
            string _path = System.Web.HttpContext.Current.Server.MapPath(Services.GetService<SystemSettingService>().GetSettingValue("LeaveFormFiles"));
            LeaveForm _form = new LeaveForm();
            OverTimeForm _overTimeForm = new OverTimeForm();
            string FilePath = null;
            string FileFormat = null;
            string FileName = null;
            if (formType == "Leave")
            {
                _form = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);
                FilePath = _form.FilePath;
                FileFormat = _form.FileFormat;
                FileName = _form.FileName;
            }
            else if (formType == "OverTime")
            {
                _overTimeForm = this.Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == formNo);
                FilePath = _overTimeForm.FilePath;
                FileFormat = _overTimeForm.FileFormat;
                FileName = _overTimeForm.FileName;
            }

            try
            {
                using (FileStream _file = new FileStream(Path.Combine(_path, FilePath), FileMode.Open))
                {
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream _ms = new MemoryStream())
                    {
                        int _read;
                        while ((_read = _file.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            _ms.Write(buffer, 0, _read);
                        }
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        return File(_ms.ToArray(), FileFormat, FileName);
                    }
                }
            }
            catch
            {
                Response.Write("<script >alert('此檔案已過期或無此檔案,請回上一頁');</script>");
                return null;
            }

        }
        //20151215 增加待處理 by Bee
        public List<HRPotralFormSignStatus> QueryForPendding()
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = _queryHelper.GetPendingList(CurrentUser.CompanyCode, CurrentUser.EmployeeNO).Where(x => x.FormStatus >= 0).ToList();


                //FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentType>)Session["Absents"]);
                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                foreach (HRPotralFormSignStatus _item in _result)
                {
                    //抓取申請人英文名字
                    Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _item.SenderEmployeeNo);
                    _item.SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName;
                    //抓取簽核者人英文名字
                    Employee SignerEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _item.SignerEmployeeNo);
                    if (SignerEmployeeEnglishName != null) // 如果是 Null代表有可能是給HR部門簽核
                    {
                        _item.SignerEmployeeEnglishName = SignerEmployeeEnglishName.EmployeeEnglishName;
                        
                    }
                    //抓取語系
                    _item.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
                    _summaryBuilder.BuildSummary(_item);

                   //增加附件檔案連結 Irving 20161209
                    LeaveForm _form = new LeaveForm();
                    OverTimeForm _OverTimeForm = new OverTimeForm();
                    if (_item.FormNo.Substring(0, 1) == "P")
                    {
                        string FilePath = null;
                        if (_item.FormType.ToString() == "Leave")
                        {
                            _form = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                            FilePath = _form.FilePath;
                        }
                        else if (_item.FormType.ToString() == "OverTime")
                        {
                            _OverTimeForm = this.Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                            FilePath = _OverTimeForm.FilePath;
                        }
                        _item.FilePath = FilePath;
                    }     
                    //End
                }
                return _result.OrderByDescending(x => x.FormCreateDate).ThenBy(x => x.FormType).ToList();
            }
        }
    }
}
