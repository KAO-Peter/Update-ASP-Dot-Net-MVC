using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.Services.DDMC_PFA.Models;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private int pageSize = 5;

        //20170306 Start
        private List<Menu> menuCache;
        //20170306 End

        private HRPortal.Services.DDMC_PFA.HRPortal_Services PfaServices = new Services.DDMC_PFA.HRPortal_Services();

        public async Task<ActionResult> Index(String tab = "")
        {
            //20170306 Start
            menuCache = CurrentUser.Menus.ToList();
            //20170306 End
            DateTime nowDate =  DateTime.Parse( DateTime.Now.ToLongDateString());
            //20231120修改 by 小榜，離職人員登入後，只顯示特定功能
            if (CurrentUser.Employee.LeaveDate != null && CurrentUser.Employee.LeaveDate < nowDate)
            {
               //00000014-0002-0000-0000-000000000000	Root
               //00000014-0002-0015-0000-000000000000	績效考核作業
               //00000014-0002-0015-0003-000000000000	績效考核自評
                List<string> isLeaveMenu = new List<string>() { "00000014-0002-0000-0000-000000000000", "00000014-0002-0015-0000-000000000000", "00000014-0002-0015-0003-000000000000" };
                menuCache = CurrentUser.Menus.Where(x => isLeaveMenu.Contains(x.ID.ToString())).ToList();
                ViewBag.isLeaveNotShow = "1";
            }

            Session["CurrentTab"] = tab == "" ? "_index" : tab;//頁面頁簽
            HomeViewModel viewmodel = new HomeViewModel();
            List<HomwLeaveModel> _leavetodaylist = new List<HomwLeaveModel>();

            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW";

            //20190529 Daniel 假別Session調整為取回同時有中文與英文名稱的物件List
            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }
            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);
            Session["AbsentsDataAll"] = await HRMApiAdapter.GetAllAbsentData("");

            ViewBag.LanguageCookie = languageCookie;

            ViewBag.Role = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault().RoleParams;
            ViewBag.Count = CurrentUser.Departments;

            //20180329 首頁顯示 by Frank
            viewmodel.PersonalSettings = Services.GetService<PersonalSettingsService>().GetParameter("HomePage", CurrentUser.CompanyCode, CurrentUser.EmployeeNO).ToList();

            //薪資查詢
            if (IsPersonalSetting(viewmodel.PersonalSettings, "SalarySearch"))
            {
                viewmodel.SalarySearchList = GetSalarySearchList();
            }
            viewmodel.LeaveLinkList = GetLeaveLinkList(); // 假單清單連結 by bee 20161202
            viewmodel.WebLinikList = GetWebLinkList(); // 網頁連結 by bee 20160301

            //個人假別時數彙總表
            if (IsPersonalSetting(viewmodel.PersonalSettings, "PersonalLeavesSummary"))
            {
                viewmodel.PersonalLeavesSummary = await GetPersonalLeavesSummary(); //遠百個人假別彙總表 by Bee 20161107
            }
            viewmodel.SmallElectronicSign = GetBasicElectronicSign(); //遠百首頁登入 by Bee 20161108

            ViewBag.JaString = MultiLanguage.Resource.Test;

            //2019/03/05 Neo 增加判斷HR, 副總, 協理, 權任協理有權限顯示主管按鈕 
            ViewBag.IsSuperintendent = await GetIsSuperintendent();

            //今日部門請假人員
            if (IsPersonalSetting(viewmodel.PersonalSettings, "LeaveToday"))
            {
                //2018/11/5 Neo 依假別調整排序, 1.補休,2.特休, 其他需依假別名稱第一個字的筆劃來排序(遞增)
                List<HomwLeaveModel> leaveList;
                List<HomwLeaveModel> leaveListNew = new List<HomwLeaveModel>();

                if ((ViewBag.Count.Count > 0) || ViewBag.Role != null)
                {
                    //viewmodel.LeaveList = await GetAllLeaveToday();
                    leaveList = await GetAllLeaveToday(1);
                }
                else
                {
                    //viewmodel.LeaveList = await GetLeaveToday();
                    leaveList = await GetLeaveToday();
                }

                if (leaveList != null && leaveList.Count() > 0)
                {
                    //取得假別優先排序
                    string sortLeaveStr = Services.GetService<SystemSettingService>().GetSettingValue("SortLeave");
                    string[] sortLeavAry = null;

                    if (!string.IsNullOrEmpty(sortLeaveStr))
                    {
                        sortLeavAry = sortLeaveStr.Split(';');
                        foreach (var sortLeave in sortLeavAry)
                        {
                            //2018/12/20 Neo 今日部門請假人員 假別優先排序調整
                            var SLeaveList = leaveList.Where(x => x.AbsentName == sortLeave).ToList();
                            foreach (var SLeave in SLeaveList)
                            {
                                leaveListNew.Add(SLeave);
                            }
                            leaveList = leaveList.Where(x => x.AbsentName != sortLeave).ToList();//排除假別優先排序
                        }
                    }
                    leaveList = leaveList.OrderBy(x => x.AbsentName.Substring(0)).ToList();//其他假別需依假別名稱第一個字的筆劃來排序(遞增)
                    if (leaveList != null && leaveList.Count() > 0)
                    {
                        leaveListNew.AddRange(leaveList);
                    }
                }
                viewmodel.LeaveList = leaveListNew;
            }

            //抓取Menus
            foreach (var item in GetMenus())
            {
                if (GetMenus(item).Any())
                {
                    foreach (var subitem in GetMenus(item))
                    {
                        if (subitem.ID.ToString() == "00000014-0002-0010-0013-000000000000" && Session["CurrentTab"].ToString() == "_index")//Menus送件/退簽
                        {
                            ViewBag.Vacancies = "1";
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0002-0002-000000000000" && Session["CurrentTab"].ToString() == "_index")//Menus送件/退簽
                        {
                            ViewBag.PendingItems = "1";
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0002-0004-000000000000" && Session["CurrentTab"].ToString() == "_index")//Menus問題回答
                        {
                            ViewBag.ReplyQuestion = "1";
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0002-0001-000000000000" && Session["CurrentTab"].ToString() == "_index")//Menus待簽核
                        {
                            ViewBag.PendingYourApproval = "1";
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0003-0001-000000000000" && Session["CurrentTab"].ToString() != "_salarySearch")//Menus假單申請
                        {
                            ViewBag.LeaveForm = "1";
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0011-0010-000000000000" && Session["CurrentTab"].ToString() == "_index")//快速連結
                        {
                            ViewBag.WebLinkListFEDS = "1";
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0003-0004-000000000000" && Session["CurrentTab"].ToString() == "_index")//電子簽到
                        {
                            ViewBag.SmallElectronicSign = "1";
                        }
                        if ((subitem.ID.ToString() == "00000014-0002-0004-0001-000000000000" || subitem.ID.ToString() == "00000014-0002-0004-0003-000000000000") && Session["CurrentTab"].ToString() == "_index")//薪資查詢
                        {
                            ViewBag.SalarySearch = "1";
                        }
                        if (Session["CurrentTab"].ToString() != "_salarySearch")
                        {
                            ViewBag.PersonalLeavesSummary = "1";//遠百首頁個人假別時數彙總表
                        }
                    }
                }
            }
            return View(viewmodel);
        }

        public IEnumerable<Menu> GetMenus(Menu parent = null)
        {
            //20170306 Start
            if (menuCache == null)
            {
                menuCache = CurrentUser.Menus.ToList();
            }

            if (parent == null)
            {
                parent = menuCache.Find(x => x.Type == (int)MenuType.ROOT);
            }
            return menuCache.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.MENU);
            //20170306 End

            //20170306 Start 原有的區段Comment掉
            /*
            if (parent == null)
                parent = CurrentUser.Menus.Find(x => x.Type == (int)MenuType.ROOT);
            return CurrentUser.Menus.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.MENU);
            */
            //20170306 End
        }

        public ActionResult AnswerFAQ(Guid id)
        {
            SetBaseUserInfo();
            Vacancies model = new Vacancies();
            model = Services.GetService<VacanciesService>().GetFAQByID(id);
            ViewBag._Title = model.Title;
            Session["Title"] = model.Title;
            return PartialView("_AskFAQ");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateAnswerFAQ(AnswerVacancies model)
        {
            SetBaseUserInfo();
            if (!ModelState.IsValid)
            {
                return PartialView("_AskFAQ", model);
            }
            else
            {
                model.ID = Guid.NewGuid();
                model.EmployeeID = CurrentUser.EmployeeID;
                model.CompanyID = CurrentUser.CompanyID;
                model.DepartmentID = CurrentUser.DepartmentID;
                model.Createdby = CurrentUser.EmployeeID;
                model.Title = Session["Title"].ToString();
                int IsSuccess = Services.GetService<AnswerVacanciesService>().Create(model, true);

                if (IsSuccess == 1)
                {
                    TempData["message"] = "成功";
                    // ModelState.Clear();
                    WriteLog("Success:" + model.ID);
                    string _fromMail = this.Services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
                    SendMail(new string[] { _fromMail },
                    new string[] { CurrentUser.Employee.Email }, new string[] { }, "我要推薦:" + model.Title, string.Format("我要推薦:{0}<br><br>推薦者:【{1}】{2}<br>", model.ContentText.Replace("\r\n", "<br/>"), CurrentUser.Employee.Department.DepartmentName,
                   CurrentUser.Employee.EmployeeName), true);
                    return Json(new { success = true });
                }
            }
            return PartialView("_AskFAQ", model);
        }

        /// <summary>
        /// 公告
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult _IndexAnnounceList(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            var model = Services.GetService<AnnouncementService>().GetAnnounceList().ToPagedList(currentPage, pageSize);
            ViewBag.AnnouncementTotal = model.TotalItemCount;
            return View(model);
        }

        #region 今日必讀

        /// <summary>
        /// 今日必讀
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult _ReadToday(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            var model = Services.GetService<ReadTodayService>().GetAnnounceList().ToPagedList(currentPage, pageSize);
            ViewBag.AnnouncementTotal = model.TotalItemCount;
            return View(model);
        }

        #endregion 今日必讀

        #region 員工福利

        /// <summary>
        /// 員工福利
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult _EmployeeBenefits(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            var model = Services.GetService<EmployeeBenefitsService>().GetAnnounceList().ToPagedList(currentPage, pageSize);
            ViewBag.AnnouncementTotal = model.TotalItemCount;
            return View(model);
        }

        #endregion 員工福利

        #region 職缺公告

        /// <summary>
        /// 職缺公告
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult _Vacancies(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            var model = Services.GetService<VacanciesService>().GetFAQList().ToPagedList(currentPage, pageSize);
            ViewBag.AnnouncementTotal = model.TotalItemCount;
            return View(model);
        }

        #endregion 職缺公告

        #region 人事規範

        /// <summary>
        /// 人事規範
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult _Personnelspecification(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            var model = Services.GetService<PersonnelspecificationService>().GetAnnounceList().ToPagedList(currentPage, pageSize);
            ViewBag.AnnouncementTotal = model.TotalItemCount;
            return View(model);
        }

        #endregion 人事規範

        /// <summary>
        /// 公告詳細內容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult _AnnounceDetail(Guid id)
        {
            AnnouncementViewModel viewmodel = new AnnouncementViewModel();
            Announcement model = new Announcement();
            model = Services.GetService<AnnouncementService>().GetAnnounceByID(id);
            viewmodel.AnnouncementData = model;
            var downloadfile = Services.GetService<AnnouncementFilesService>().GetFileIDByAnnouncement(model.Id);

            if (downloadfile != null)
            {
                viewmodel.FileGuid = downloadfile.Id.ToString();
                viewmodel.FilePath = downloadfile.Path;
                viewmodel.FileDescription = downloadfile.Name;
            }
            return PartialView("_AnnounceDetail", viewmodel);
        }

        #region 今日必讀詳細內容

        /// <summary>
        /// 今日必讀詳細內容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult _ReadTodayDetail(Guid id)
        {
            ReadTodayViewModel viewmodel = new ReadTodayViewModel();
            ReadToday model = new ReadToday();
            model = Services.GetService<ReadTodayService>().GetAnnounceByID(id);
            viewmodel.AnnouncementData = model;
            var downloadfile = Services.GetService<ReadTodayFilesService>().GetFileIDByAnnouncement(model.Id);

            if (downloadfile != null)
            {
                viewmodel.FileGuid = downloadfile.Id.ToString();
                viewmodel.FilePath = downloadfile.Path;
                viewmodel.FileDescription = downloadfile.Name;
            }
            return PartialView("_ReadTodayDetail", viewmodel);
        }

        #endregion 今日必讀詳細內容

        #region 顯示附件內容共用模組

        //2017.03.16增加 by Daniel
        /// <summary>
        ///首頁顯示附件內容共用模組
        /// </summary>
        /// <param name="fileGuid"></param> 附件檔案UID
        /// <param name="filePath"></param> 附件檔案位址
        /// <param name="fileDescription"></param> 附件檔案說明
        /// <returns></returns>
        public ActionResult _ShowAttachmentContent(Guid fileGuid, string filePath, string fileDescription)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                ShowAttachmentContentViewModel viewModel = new ShowAttachmentContentViewModel();
                viewModel.AttachmentGUID = fileGuid;
                viewModel.AttachmentFilePath = filePath;
                viewModel.ContentTitle = fileDescription;
                return PartialView("_ShowAttachmentContent", viewModel);
            }
            else
            {
                return new EmptyResult();
            }
        }

        #endregion 顯示附件內容共用模組

        #region 員工福利詳細內容

        /// <summary>
        /// 員工福利詳細內容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult _EmployeeBenefitsDetail(Guid id)
        {
            EmployeeBenefitsViewModel viewmodel = new EmployeeBenefitsViewModel();
            EmployeeBenefits model = new EmployeeBenefits();
            model = Services.GetService<EmployeeBenefitsService>().GetAnnounceByID(id);
            viewmodel.AnnouncementData = model;
            var downloadfile = Services.GetService<EmployeeBenefitsFilesService>().GetFileIDByAnnouncement(model.Id);

            if (downloadfile != null)
            {
                viewmodel.FileGuid = downloadfile.Id.ToString();
                viewmodel.FilePath = downloadfile.Path;
                viewmodel.FileDescription = downloadfile.Name;
            }
            return PartialView("_EmployeeBenefitsDetail", viewmodel);
        }

        #endregion 員工福利詳細內容

        #region 人事規範詳細內容

        /// <summary>
        /// 人事規範詳細內容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult _PersonnelspecificationDetail(Guid id)
        {
            PersonnelspecificationViewModel viewmodel = new PersonnelspecificationViewModel();
            Personnelspecification model = new Personnelspecification();
            model = Services.GetService<PersonnelspecificationService>().GetAnnounceByID(id);
            viewmodel.AnnouncementData = model;
            var downloadfile = Services.GetService<PersonnelspecificationFilesService>().GetFileIDByAnnouncement(model.Id);

            if (downloadfile != null)
            {
                viewmodel.FileGuid = downloadfile.Id.ToString();
                viewmodel.FilePath = downloadfile.Path;
                viewmodel.FileDescription = downloadfile.Name;
            }
            return PartialView("_PersonnelspecificationDetail", viewmodel);
        }

        #endregion 人事規範詳細內容

        #region 職缺公告詳細內容

        /// <summary>
        /// 職缺公告細內容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult _VacanciesDetail(Guid id)
        {
            VacanciesViewModel viewmodel = new VacanciesViewModel();
            Vacancies model = new Vacancies();
            model = Services.GetService<VacanciesService>().GetFAQByID(id);
            viewmodel.Data = model;
            //var downloadfile = Services.GetService<VacanciesFilesService>().GetFileIDByAnnouncement(model.Id);

            //if (downloadfile != null)
            //{
            //    viewmodel.FileGuid = downloadfile.Id.ToString();
            //    viewmodel.FilePath = downloadfile.Path;
            //    viewmodel.FileDescription = downloadfile.Name;
            //}
            return PartialView("_VacanciesDetail", viewmodel);
        }

        #endregion 職缺公告詳細內容

        delegate HomeToDoModel GetHomeToDoModel(string _name, string _url);

        /// <summary>
        /// 代辦事項
        /// </summary>
        /// <returns></returns>
        public ActionResult _ToDoList()
        {
            GetHomeToDoModel replyModel = GetReply;
            GetHomeToDoModel signModel = GetSign;
            GetHomeToDoModel pendingModel = GetPending;
            GetHomeToDoModel IsSelfModel = GetIsSelf;      // 自評
            GetHomeToDoModel IsFirstModel = GetIsFirst;     // 初核
            GetHomeToDoModel IsSecondModel = GetIsSecond;    // 複核
            GetHomeToDoModel IsThirdModel = GetIsThird;    // 核決
            HomeViewModel viewmodel = new HomeViewModel();
            List<HomeToDoModel> _result = new List<HomeToDoModel>();
            if (CurrentUser.IsAdmin || CurrentUser.IsHR)
            {
                foreach (var item in GetMenus())
                {
                    if (GetMenus(item).Any())
                    {
                        foreach (var subitem in GetMenus(item))
                        {
                            if (subitem.ID.ToString() == "00000014-0002-0002-0004-000000000000")//Menus問題回答
                            {
                                _result.Add(replyModel(HRPortal.MultiLanguage.Resource.ReplyQuestion, Url.Action("ReplyQuestion", "ToDo", null)));
                            }
                        }
                    }
                }
            }
            //抓取Menus
            foreach (var item in GetMenus())
            {
                if (GetMenus(item).Any())
                {
                    foreach (var subitem in GetMenus(item))
                    {
                        if (subitem.ID.ToString() == "00000014-0002-0002-0002-000000000000")//Menus送件/退簽
                        {
                            _result.Add(pendingModel(HRPortal.MultiLanguage.Resource.PendingForms, Url.Action("PendingForms", "ToDo", null)));
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0002-0001-000000000000")//Menus待簽核
                        {
                            _result.Add(signModel(HRPortal.MultiLanguage.Resource.SignForms, Url.Action("SignForms", "ToDo", null)));
                        }
                    }
                }
            }
            foreach (var item in GetMenus())
            {
                if (GetMenus(item).Any())
                {
                    foreach (var subitem in GetMenus(item))
                    {
                        if (subitem.Link.ToString() == "~/DDMC_PFA/PfaCycleSelfEvaluation")    //績效考核自評待處理
                        {
                            _result.Add(IsSelfModel("績效考核自評待處理"
                                , Url.Action("PfaCycleSelfEvaluation", "DDMC_PFA", new { cmd = "Query" })));
                        }
                        if (subitem.Link.ToString() == "~/DDMC_PFA/PfaCycleFirstEvaluation")   //績效考核初核待簽核
                        {
                            _result.Add(IsFirstModel("績效考核初核待簽核"
                                , Url.Action("PfaCycleFirstEvaluation", "DDMC_PFA", new { cmd = "Query" })));
                        }
                        if (subitem.Link.ToString() == "~/DDMC_PFA/PfaCycleSecondEvaluation")  //績效考核複核待簽核
                        {
                            _result.Add(IsSecondModel("績效考核複核待簽核"
                                , Url.Action("PfaCycleSecondEvaluation", "DDMC_PFA", new { cmd = "Query" })));
                        }
                        if (subitem.Link.ToString() == "~/DDMC_PFA/PfaCycleThirdEvaluation")   //績效考核核決待簽核
                        {
                            _result.Add(IsThirdModel("績效考核核決待簽核"
                                , Url.Action("PfaCycleThirdEvaluation", "DDMC_PFA", new { cmd = "Query" })));
                        }
                    }
                }
            }
            viewmodel.ToDoList = _result;
            return View(viewmodel);
        }

        /// <summary>
        /// 網頁連結
        /// </summary>
        /// <returns></returns>
        private List<HomeWebLinkModel> GetWebLinkList()
        {
            List<HomeWebLinkModel> _result = new List<HomeWebLinkModel>();

            foreach (var webLinkList in Services.GetService<WebLinkListService>().GetAllLinkList().Where(x => x.IsUsed == true).OrderBy(y => y.Number))
            {
                _result.Add(new HomeWebLinkModel { Name = webLinkList.WebName, URL = webLinkList.WebLink });
            }

            return _result;
        }

        /// <summary>
        /// 薪資查詢
        /// </summary>
        /// <returns></returns>
        private List<HomeSalarySearchModel> GetSalarySearchList()
        {
            HomeViewModel viewmodel = new HomeViewModel();
            List<HomeSalarySearchModel> _result = new List<HomeSalarySearchModel>();
            //抓取Menus
            foreach (var item in GetMenus())
            {
                if (GetMenus(item).Any())
                {
                    foreach (var subitem in GetMenus(item))
                    {
                        if (subitem.ID.ToString() == "00000014-0002-0004-0001-000000000000")//Menus個人薪資查詢
                        {
                            _result.Add(new HomeSalarySearchModel { Name = subitem.Title, URL = subitem.Link.Substring(2), EngName = subitem.Alias });
                        }
                        if (subitem.ID.ToString() == "00000014-0002-0004-0003-000000000000")//Menus部門薪資彙總查詢
                        {
                            _result.Add(new HomeSalarySearchModel { Name = subitem.Title, URL = subitem.Link.Substring(2), EngName = subitem.Alias });
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 假單功能快速連結
        /// </summary>
        /// <returns></returns>
        private List<LeaveApplicationLeaveLinkModel> GetLeaveLinkList()
        {
            List<LeaveApplicationLeaveLinkModel> _result = new List<LeaveApplicationLeaveLinkModel>();

            string getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW";

            //抓取Menus
            foreach (var item in GetMenus())
            {
                if (GetMenus(item).Any() && item.ID == Guid.Parse("00000014-0002-0003-0000-000000000000"))
                {
                    foreach (var subitem in GetMenus(item))
                    {
                        _result.Add(new LeaveApplicationLeaveLinkModel { Name = (getLanguageCookie == "en-US" ? subitem.Alias : subitem.Title), URL = subitem.Link.Substring(2) });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 今日部門請假人員(一般)
        /// </summary>
        /// <returns></returns>
        private async Task<List<HomwLeaveModel>> GetLeaveToday()
        {
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.RoleParams;    //如果是管理者或是有管理多個部門，前端頁面上會增加顯示(顯示全部)的功能
            ViewBag.Count = CurrentUser.Departments;
            List<HomwLeaveModel> _result = new List<HomwLeaveModel>();
            //取得部門編號
            List<string> deptcodelists = new List<string>();
            foreach (var item in CurrentUser.Departments)
            {
                deptcodelists.Add(item.DepartmentCode);
            }

            //取得登入者以下人員請假資訊
            List<DepartmentLeaveSummaryItem> model = await HRMApiAdapter.GeneralGetLeaveEmployee(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, deptcodelists, CurrentUser.EmployeeNO, DateTime.Today, DateTime.Today.AddDays(1));

            if (model != null && model.Count > 0)
            {
                foreach (var leaveData in model)
                {
                    foreach (var i in leaveData.DeptMember)
                    {
                        //抓取人員英文名字
                        Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, i.EmpID);
                        DateTime StartTime = Convert.ToDateTime(i.BeginTime);//假別開始時間
                        DateTime EndTime = Convert.ToDateTime(i.EndTime);//假別結束時間

                        _result.Add(new HomwLeaveModel
                        {
                            AbsentNameEN = i.AbsentNameEN != null ? i.AbsentNameEN : i.AbsentName,
                            SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                            getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",//抓取語系
                            DepartmentName = leaveData.DeptName,
                            AbsentAmount = i.Unit == "d" ? i.AbsentAmount / 8 : i.AbsentAmount,
                            AbsentName = i.AbsentName,
                            EmployeeNo = i.EmpID,
                            Name = i.EmpName,
                            EndTime = EndTime,
                            StartTime = StartTime,
                            Unit = i.Unit
                        });
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 今日部門請假人員(一般) 前端呼叫用
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetDeptLeaveToday()
        {
            List<HomwLeaveModel> _result = new List<HomwLeaveModel>();

            //2019/03/05 增加是否為主管判斷顯示
            ViewBag.Role = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault().RoleParams;
            ViewBag.Count = CurrentUser.Departments;

            if ((ViewBag.Count.Count > 0) || ViewBag.Role != null)
            {
                _result = await GetAllLeaveToday(1);
            }
            else
            {
                _result = await GetLeaveToday();
            }

            //2019/03/05 Neo 增加判斷HR, 副總, 協理, 權任協理有權限顯示主管按鈕 
            ViewBag.IsSuperintendent = await GetIsSuperintendent();

            return PartialView("_LeaveToday", _result);
        }

        /// <summary>
        ///  所屬被簽核單位及簽核的都會列出。
        /// </summary>
        /// <param name="mode">1:只顯示主管自己簽過的今日請假資料, 2:顯示所有自己有管理的部門所有的今日請假資料</param>
        /// <returns></returns>
        private async Task<List<HomwLeaveModel>> GetAllLeaveToday(int mode)
        {
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.Name;
            ViewBag.Count = CurrentUser.Departments;
            List<HomwLeaveModel> _result = new List<HomwLeaveModel>();
            //取得部門編號
            List<string> deptcodelists = new List<string>();
            List<string> designatedEmpIDs = new List<string>();

            #region 先把所屬要簽核的人或部門資料整理出來 ，塞到deptcodelists跟designatedEmpIDs，之後乎叫web api用

            if (roleDataa.RoleParams != null)
            {
                List<Department> data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).
                    Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
                foreach (var item in data)
                {
                    deptcodelists.Add(item.DepartmentCode);
                }
            }
            else
            {
                foreach (var item in CurrentUser.Departments)
                {
                    deptcodelists.Add(item.DepartmentCode);
                }

                //把所屬要簽核的單位也加入
                foreach (var item in CurrentUser.SignDepartments.Where(x => !deptcodelists.Contains(x.DepartmentCode)).ToList())
                {
                    deptcodelists.Add(item.DepartmentCode);
                }

                #region 找出指定要登入者簽核的人員資料

                DesignatedSignHelper designatedSignHelper = new DesignatedSignHelper();
                designatedEmpIDs = designatedSignHelper.GetSignOfEmpIds(this.CurrentUser.EmployeeNO);

                var designatedEmpWithDepts = Services.GetService<EmployeeService>().GetAll().Where(x => designatedEmpIDs.Contains(x.EmployeeNO)).
                    Select(x => new { x.EmployeeNO, x.Department.DepartmentCode }).ToList();

                designatedEmpIDs = designatedEmpWithDepts.Where(x => !deptcodelists.Contains(x.DepartmentCode)).
                    Select(x => x.EmployeeNO).Distinct().ToList();

                #endregion 找出指定要登入者簽核的人員資料
            }

            #endregion 先把所屬要簽核的人或部門資料整理出來 ，塞到deptcodelists跟designatedEmpIDs，之後乎叫web api用

            //取得登入者以下人員請假資訊
            List<DepartmentLeaveSummaryItem> model = await HRMApiAdapter.GetLeaveEmployee(CurrentUser.CompanyCode, CurrentUser.DepartmentCode,
                deptcodelists, CurrentUser.EmployeeNO, DateTime.Today, DateTime.Today.AddDays(1), designatedEmpIDs);

            if (model != null && model.Count > 0)
            {
                #region 這邊要過濾實際有簽核過的單子。

                SignFlowRecQueryHelper signFlowRecQueryHelper = new SignFlowRecQueryHelper();

                //取得所有DeptMember物件集合。
                IEnumerable<DepartmentMemberForLeaveSummary> deptMembers = model.SelectMany(x => x.DeptMember);

                List<string> allFormNos = deptMembers.Select(x => x.FormNo).Distinct().ToList();

                IList<YoungCloud.SignFlow.Model.SignFlowRecModel> allSignedRecs = signFlowRecQueryHelper.GetSignFlowByFormNumberWithSignerID(allFormNos, this.CurrentUser.Employee.EmployeeNO);
                List<string> allSignedFormNos = allSignedRecs.Where(x => x.FormType == "Leave").Select(x => x.FormNumber).ToList();

                #endregion 這邊要過濾實際有簽核過的單子。

                string dname = "";
                if (CurrentUser.Departments.FirstOrDefault() != null && CurrentUser.Departments.Count() > 0)
                {
                    dname = CurrentUser.Departments.First().DepartmentName;
                }
                else
                {// 20200221 小榜 增加判斷，人資無管理部門時，需帶入自己部門，才看的到今日請假人員
                    dname = CurrentUser.Employee.Department.DepartmentName;
                }
                //2018/12/27 Neo 調整只要是有管理的部門不管是否有簽核, 主管都要能看到今天所管理的部門員工是否有休假
                List<string> dnameList = CurrentUser.Departments.Select(x => x.DepartmentName).ToList();

                //2019/03/05 Neo 增加判斷HR, 副總, 協理, 權任協理有權限顯示副理級以上主管的今日請假
                List<string> emplyeeList = new List<string>();
                //2019/03/12 Neo 增加判斷[顯示]按鈕需排除資料的部門代碼
                List<string> excludeDeptList = new List<string>();

                if (mode == 3)
                {
                    var emplyeeBasicDataList = await HRMApiAdapter.GetEmployeeBasicData(CurrentUser.CompanyCode, null, null);
                    List<EmployeeBasicData> emplyeeBasicDataList2 = new List<EmployeeBasicData>();

                    string showSuperintendentListStr = Services.GetService<SystemSettingService>().GetSettingValue("ShowSuperintendentList");
                    string[] showSuperintendentListAry = null;

                    if (!string.IsNullOrEmpty(showSuperintendentListStr))
                    {
                        showSuperintendentListAry = showSuperintendentListStr.Split(';');
                        foreach (var showSuperintendentList in showSuperintendentListAry)
                        {
                            var showSuperintendentList2 = emplyeeBasicDataList.Where(x => !string.IsNullOrEmpty(x.JobTitleName) && x.JobTitleName.Trim().Contains(showSuperintendentList)).ToList();

                            if (showSuperintendentList2 != null)
                            {
                                emplyeeBasicDataList2.AddRange(showSuperintendentList2);
                            }
                        }
                        emplyeeList = emplyeeBasicDataList2.Select(x => x.EmpID).Distinct().ToList();
                    }

                    //2019/03/12 Neo 增加判斷[顯示]按鈕需排除資料的部門代碼
                    string showExcludeDeptStr = Services.GetService<SystemSettingService>().GetSettingValue("ShowExcludeDept");
                    string[] excludeDeptAry = showExcludeDeptStr.Split(';');
                    excludeDeptList = Services.GetService<DepartmentService>().GetAll().Where(x => excludeDeptAry.Contains(x.DepartmentCode)).Select(x => x.DepartmentName).ToList();
                }

                foreach (var leaveData in model)
                {
                    List<DepartmentMemberForLeaveSummary> deptMemberList = new List<DepartmentMemberForLeaveSummary>();

                    if (mode == 1 || mode == 2)//全部1/簽核2
                    {
                        deptMemberList = leaveData.DeptMember.Where(x => allSignedFormNos.Contains(x.FormNo) || (mode == 2 && dnameList.Contains(leaveData.DeptName) || (mode == 1 && leaveData.DeptName == dname))).ToList();
                    }
                    else//主管3
                    {
                        deptMemberList = leaveData.DeptMember.Where(x => emplyeeList.Contains(x.EmpID)).ToList();
                        //2019/03/12 Neo 增加判斷[主]按鈕需排除資料的部門代碼
                        if (excludeDeptList.Count() > 0)
                        {
                            deptMemberList = deptMemberList.Where(x => !excludeDeptList.Contains(leaveData.DeptName)).ToList();
                        }
                    }

                    foreach (var i in deptMemberList)
                    {
                        //抓取人員英文名字
                        Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, i.EmpID);
                        DateTime StartTime = Convert.ToDateTime(i.BeginTime);//假別開始時間
                        DateTime EndTime = Convert.ToDateTime(i.EndTime);//假別結束時間

                        _result.Add(new HomwLeaveModel
                        {
                            AbsentNameEN = i.AbsentNameEN != null ? i.AbsentNameEN : i.AbsentName,
                            SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                            getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",//抓取語系
                            DepartmentName = leaveData.DeptName,
                            AbsentAmount = i.Unit == "d" ? i.AbsentAmount / 8 : i.AbsentAmount,
                            AbsentName = i.AbsentName,
                            EmployeeNo = i.EmpID,
                            Name = i.EmpName,
                            EndTime = EndTime,
                            StartTime = StartTime,
                            Unit = i.Unit
                        });
                    }
                }
            }

            return _result;
        }

        /// <summary>
        ///  所屬被簽核單位及簽核的都會列出。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetAllSignedLeaveToday()
        {
            List<HomwLeaveModel> _result = new List<HomwLeaveModel>();
            _result = await GetAllLeaveToday(2);

            //2019/03/05 Neo 增加判斷HR, 副總, 協理有權限顯示主管按鈕 
            ViewBag.IsSuperintendent = await GetIsSuperintendent();

            return PartialView("_LeaveToday", _result);
        }

        //2019/03/05 Neo 增加主管按鈕查詢事件
        [HttpPost]
        public async Task<ActionResult> GetSuperintendentLeaveToday()
        {
            List<HomwLeaveModel> _result = new List<HomwLeaveModel>();
            _result = await GetAllLeaveToday(3);

            ViewBag.IsSuperintendent = await GetIsSuperintendent();

            return PartialView("_LeaveToday", _result);
        }

        /// <summary>
        /// 待簽核
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        private HomeToDoModel GetSign(string _name, string _url)
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = _queryHelper.GetToSignList(CurrentUser.CompanyCode, CurrentUser.SignDepartmentCode, CurrentUser.EmployeeNO).Where(x => x.FormStatus > 0).ToList();

                //FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentType>)Session["Absents"]);
                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                foreach (HRPotralFormSignStatus _item in _result)
                {
                    _summaryBuilder.BuildSummary(_item);
                }

                return new HomeToDoModel { Count = _result.Count, Name = _name, URL = _url };
            }
        }

        /// <summary>
        /// 待處理
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        private HomeToDoModel GetPending(string _name, string _url)
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = _queryHelper.GetPendingList(CurrentUser.CompanyCode, CurrentUser.EmployeeNO).Where(x => x.FormStatus >= 0).ToList();

                //FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentType>)Session["Absents"]);
                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                foreach (HRPotralFormSignStatus _item in _result)
                {
                    _summaryBuilder.BuildSummary(_item);
                }

                return new HomeToDoModel { Count = _result.Count, Name = _name, URL = _url };
            }
        }

        /// <summary>
        /// 問題回答
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        private HomeToDoModel GetReply(string _name, string _url)
        {
            int ReplyCount = Services.GetService<AnswerFAQService>().GetAnswerFAQList().Count();
            return new HomeToDoModel { Count = ReplyCount, Name = _name, URL = _url };
        }

        /// <summary>
        /// 績效考核-自評待處理
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        private HomeToDoModel GetIsSelf(string _name, string _url)
        {
            int cnt = 0;
            try
            {
                cnt = PfaServices.GetService<HRPortal.Services.DDMC_PFA.PfaSignProcessService>().GetIsSelfData(CurrentUser.EmployeeID).Count;
            }
            catch (Exception)
            {
                cnt = 0;
            }
            return new HomeToDoModel { Count = cnt, Name = _name, URL = _url };
        }

        /// <summary>
        /// 績效考核-初核待簽核
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        private HomeToDoModel GetIsFirst(string _name, string _url)
        {
            int cnt = 0;
            try
            {
                cnt = PfaServices.GetService<HRPortal.Services.DDMC_PFA.PfaSignProcessService>().GetIsFirstData(CurrentUser.EmployeeID).Count;
                _url = _url + "&txtStatus=" + PfaSignProcess_Status.PendingReview;
            }
            catch (Exception)
            {
                cnt = 0;
            }
            return new HomeToDoModel { Count = cnt, Name = _name, URL = _url };
        }

        /// <summary>
        /// 績效考核-複核待簽核
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        private HomeToDoModel GetIsSecond(string _name, string _url)
        {
            int cnt = 0;
            try
            {
                cnt = PfaServices.GetService<HRPortal.Services.DDMC_PFA.PfaSignProcessService>().GetIsSecondData(CurrentUser.EmployeeID).Count;
                _url = _url + "&txtStatus=" + PfaSignProcess_Status.PendingReview;
            }
            catch (Exception)
            {
                cnt = 0;
            }
            return new HomeToDoModel { Count = cnt, Name = _name, URL = _url };
        }

        /// <summary>
        /// 績效考核-核決待簽核
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        private HomeToDoModel GetIsThird(string _name, string _url)
        {
            int cnt = 0;
            try
            {
                cnt = PfaServices.GetService<HRPortal.Services.DDMC_PFA.PfaSignProcessService>()
                    .GetIsThirdData(CurrentUser.EmployeeID)
                    .Count;
                _url = _url + "&txtStatus=" + PfaSignProcess_Status.PendingThirdReview;
            }
            catch (Exception)
            {
                cnt = 0;
            }
            return new HomeToDoModel { Count = cnt, Name = _name, URL = _url };
        }

        /// <summary>
        /// 個人假別時數彙總表
        /// </summary>
        /// <returns></returns>
        private async Task<List<PersonalLeavesSummaryModel>> GetPersonalLeavesSummary()
        {
            AbsentDetailAll data = await HRMApiAdapter.GetEmployeeAbsent2(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now, "remaining");
            List<PersonalLeavesSummaryModel> personalLeavesSummary = new List<PersonalLeavesSummaryModel>();
            DateTime selectDate = new DateTime(DateTime.Now.Year, 12, 31);
            Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(CurrentUser.EmployeeID, selectDate);
            if (data.AbsentDetail_Now != null)
            {
                //2018/11/5 Neo 依假別調整排序, 1.補休,2.特休, 其他需依假別名稱第一個字的筆劃來排序(遞增)
                var absentDetail_NowList = data.AbsentDetail_Now.Where(x => x.CanUse == true).ToList();

                //取得假別優先排序
                string sortLeaveStr = Services.GetService<SystemSettingService>().GetSettingValue("SortLeave");
                string[] sortLeavAry = null;

                if (!string.IsNullOrEmpty(sortLeaveStr))
                {
                    sortLeavAry = sortLeaveStr.Split(';');
                    foreach (var sortLeave in sortLeavAry)
                    {
                        var SLeave = absentDetail_NowList.Where(x => x.Name == sortLeave).FirstOrDefault();
                        if (SLeave != null)
                        {
                            var SLeaveItem = this.GetPersonalLeavesSummaryModelSign(SLeave, notApprovedAbsentAmount);
                            if (SLeaveItem != null)
                            {
                                personalLeavesSummary.Add(SLeaveItem);
                            }
                            absentDetail_NowList = absentDetail_NowList.Where(x => x.Name != sortLeave).ToList();//排除假別優先排序
                        }
                    }
                }
                absentDetail_NowList = absentDetail_NowList.OrderBy(x => x.Name.Substring(0)).ToList();//依假別名稱第一個字的筆劃來排序(遞增)

                //foreach (var item in data.AbsentDetail_Now.Where(x => x.CanUse == true))//取有給核假的假別 Irving 20161206
                //{
                //    //檢查後台簽核中是否為0
                //    if (item.ApprovedHours == 0)
                //    {
                //        //如果後台無簽核中資料，改檢查前台是否有值
                //        if (notApprovedAbsentAmount.ContainsKey(item.Code))
                //        {
                //            item.ApprovedHours = notApprovedAbsentAmount[item.Code];
                //            item.UseAmount -= notApprovedAbsentAmount[item.Code]; //剩餘可休也要扣除前台簽核中的
                //        }
                //    }
                //    item.LeaveHours = item.AnnualLeaveHours - item.ApprovedHours - item.UseAmount;

                //    personalLeavesSummary.Add(new PersonalLeavesSummaryModel
                //    {
                //        Code = item.Code,
                //        Name = item.Name,
                //        AbsentNameEn = item.AbsentNameEn,
                //        AnnualLeaveHours = item.AnnualLeaveHours,
                //        ApprovedHours = item.ApprovedHours,
                //        LeaveHours = item.LeaveHours,
                //        UseAmount = item.UseAmount,
                //        Unit = item.Unit,
                //        CanUse = item.CanUse,
                //        Remark = item.Remark
                //    });
                //}

                if (absentDetail_NowList != null && absentDetail_NowList.Count() > 0)
                {
                    foreach (var item in absentDetail_NowList)
                    {
                        var resultItem = this.GetPersonalLeavesSummaryModelSign(item, notApprovedAbsentAmount);
                        personalLeavesSummary.Add(resultItem);
                    }
                }
            }
            return personalLeavesSummary;
        }

        //2018/11/5 Neo 增加共用計算 個人假別時數彙總表 時數function
        private PersonalLeavesSummaryModel GetPersonalLeavesSummaryModelSign(AbsentDetail item, Dictionary<string, decimal> notApprovedAbsentAmount)
        {
            PersonalLeavesSummaryModel result;
            //檢查後台簽核中是否為0
            if (item.ApprovedHours == 0)
            {
                //如果後台無簽核中資料，改檢查前台是否有值
                if (notApprovedAbsentAmount.ContainsKey(item.Code))
                {
                    item.ApprovedHours = notApprovedAbsentAmount[item.Code];
                    item.UseAmount -= notApprovedAbsentAmount[item.Code]; //剩餘可休也要扣除前台簽核中的
                }
            }
            item.LeaveHours = item.AnnualLeaveHours - item.ApprovedHours - item.UseAmount;

            result = new PersonalLeavesSummaryModel
            {
                Code = item.Code,
                Name = item.Name,
                AbsentNameEn = item.AbsentNameEn,
                AnnualLeaveHours = item.AnnualLeaveHours,
                ApprovedHours = item.ApprovedHours,
                LeaveHours = item.LeaveHours,
                UseAmount = item.UseAmount,
                Unit = item.Unit,
                CanUse = item.CanUse,
                Remark = item.Remark
            };
            return result;
        }

        /// <summary>
        /// 首頁線上刷卡功能
        /// </summary>
        /// <returns></returns>
        private ElectronicSignViewModel GetBasicElectronicSign()
        {
            ElectronicSignViewModel viewmodel = new ElectronicSignViewModel();
            viewmodel.ClockInWay = GetClockInWay();

            return viewmodel;
        }

        private SelectList GetClockInWay(string selected = "")
        {
            SelectList options = new SelectList(new List<SelectListItem>
         {
             new SelectListItem { Text = GetClockInWayName(ClockInWayData.Work), Value = ((int)ClockInWayData.Work).ToString(),Selected=(selected==((int)ClockInWayData.Work).ToString()?true:false)},
             new SelectListItem { Text = GetClockInWayName(ClockInWayData.GetOffWork), Value =  ((int)ClockInWayData.GetOffWork).ToString(),Selected=(selected==((int)ClockInWayData.GetOffWork).ToString()?true:false)}
        }, "Value", "Text");
            return options;
        }

        private string GetClockInWayName(ClockInWayData CodeData)
        {
            string _result = "";
            switch (CodeData)
            {
                case ClockInWayData.Work:
                    _result = "上班";
                    break;

                case ClockInWayData.GetOffWork:
                    _result = "下班";
                    break;
            }
            return _result;
        }

        [HttpPost]
        public async Task<ActionResult> ElectronicSignToHRM(string ClockInWayType)
        {
            int ClockInReasonType = 0;
            DateTime CurrentTime = DateTime.Now;
            RequestResult _result = await HRMApiAdapter.PostDuty(CurrentUser.Employee.Company.CompanyCode, CurrentUser.EmployeeNO, CurrentTime, int.Parse(ClockInWayType), ClockInReasonType);
            if (_result.Status)
            {
                WriteLog("Success:" + CurrentUser.EmployeeNO + "-" + CurrentTime);
                return Json(new AjaxResult() { status = "success", message = CurrentTime.ToShortTimeString() });
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = _result.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EmpScheduleClassTime(DateTime startTime, DateTime endTime)
        {
            List<GetEmpScheduleClassTimeByStartEndTimeResponse> empScheduleTime = await HRMApiAdapter.GetEmpScheduleClassTimeByStartEndTime(CurrentUser.Employee.Company.CompanyCode, CurrentUser.EmployeeNO, startTime, endTime);

            return Json(empScheduleTime);
        }

        /// <summary>
        /// 個人設定 是否顯示
        /// </summary>
        /// <param name="personalSettings"></param>
        /// <param name="key"></param>
        /// <returns>true/false</returns>
        public static bool IsPersonalSetting(List<PersonalSettings> personalSettings, string key)
        {
            var setting = personalSettings.Where(x => x.SettingKey == key).FirstOrDefault();
            return (setting == null ? true : (setting.SettingValue == "0" ? false : true));
        }

        //2019/03/07 Neo 增加判斷是否顯示主管按鈕
        /// <summary>
        /// 判斷是否顯示主管按鈕
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetIsSuperintendent()
        {
            bool result = false;

            //2019/03/05 Neo 增加判斷HR, 副總, 協理, 權任協理有權限顯示主管按鈕 
            var emplyeeBasicDataList = await HRMApiAdapter.GetEmployeeBasicData(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.EmployeeNO);

            if (emplyeeBasicDataList != null)
            {
                var emplyeeBasicData = emplyeeBasicDataList.FirstOrDefault();

                string isSuperintendentStr = Services.GetService<SystemSettingService>().GetSettingValue("IsSuperintendent");
                string[] isSuperintendentAry = null;

                if (!string.IsNullOrEmpty(isSuperintendentStr))
                {
                    isSuperintendentAry = isSuperintendentStr.Split(';');
                    foreach (var isSuperintendent in isSuperintendentAry)
                    {
                        if (isSuperintendent == "HR")
                        {
                            if (CurrentUser.IsHR)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(emplyeeBasicData.JobTitleName) && emplyeeBasicData.JobTitleName.Trim().Contains(isSuperintendent))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}