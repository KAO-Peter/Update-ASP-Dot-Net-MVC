using HRPortal.DBEntities;
using HRPortal.Services;
using HRPortal.Services.Models;
//using HRPortal.Services.CourseServices;
using System;
//using HRPortal.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace HRPortal.Mvc.Views
{
    public abstract class BaseAppViewPage<TModel> : WebViewPage<TModel>
    {
        protected HRPortal_Services Services;
        public BaseAppViewPage()
        {
            Services = new HRPortal_Services();
            //if (this.CurrentUser != null)
            //{
            //    Services.SetSysId(this.CurrentUser.SysId);
            //}
            //else if (this.ViewBag.sys_id != null)
            //{
            //    Services.SetSysId(Guid.Parse(this.ViewBag.sys_id));
            //}
        }

        public string GetCurrentControllerName()
        {
            return HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
        }

        private UserClaimsPrincipal _userInfo;
        public virtual UserClaimsPrincipal CurrentUser
        {
            get
            {
                try
                {
                    if (_userInfo == null)
                        _userInfo = new UserClaimsPrincipal(this.User as ClaimsPrincipal, Services);
                    return _userInfo;
                }
                catch
                {
                    return null;
                }
            }
        }

        private Menu _currentMenu;

        public Menu GetCurrentMenu()
        {
            if (Request.AppRelativeCurrentExecutionFilePath.Contains("/Home"))
            {
                return new Menu()
                {
                    Alias = "Home",
                    Link = "~/Home",
                };
            }
            if (_currentMenu == null)
            {
                _currentMenu = this.GetCurrentPage();
                if (_currentMenu.Type == (int)MenuType.TAB)
                    _currentMenu = CurrentUser.Menus.Find(x => x.ID == _currentMenu.Parent_ID);
            }
            return _currentMenu;
        }

        private Menu _currentPage;

        public Menu GetCurrentPage()
        {
            if (_currentPage == null)
            {
                _currentPage = CurrentUser.Menus.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Link) == false && Url.Content(x.Link) == Url.Content(Request.AppRelativeCurrentExecutionFilePath) && x.Type == (int)MenuType.TAB);
                if (_currentPage == null)
                    _currentPage = CurrentUser.Menus.Find(x => string.IsNullOrWhiteSpace(x.Link) == false && Url.Content(x.Link) == Url.Content(Request.AppRelativeCurrentExecutionFilePath) && x.Type == (int)MenuType.MENU);
            }
            return _currentPage;
        }

        public string GetCurrentMenuParam(string name)
        {
            //MenuGroup menuGroup = (ConfigurationManager.AppSettings["AppType"].ToString() == "backend") ? MenuGroup.BACK_END : MenuGroup.FRONT_END;
            RoleMenuMap map = this.CurrentUser.Employee.Role.RoleMenuMaps.First(x => x.Menu_ID == this.GetCurrentMenu().ID);//&& x.Menu.group == (int)menuGroup);
            string value = string.Empty;
            if (string.IsNullOrWhiteSpace(map.MenuParams) == false)
            {
                Dictionary<string, string> menu_params = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(map.MenuParams);
                if (menu_params.ContainsKey(name) == true)
                    value = menu_params[name];
            }
            return value;
        }
        public List<OptionGroup> GetOptionsList()
        {
            IQueryable<OptionGroup> OptionsList;

            OptionsList = Services.GetService<OptionGroupService>().GetAll().Where(x => x.OptionGroupKey == "weblist");

            return OptionsList.ToList();
        }

        private List<Menu> menuCache;
        //20170321修改 by Daniel，依網站種類檢查選單
        //20170324修改 by Daniel，選單判斷改在MenuService處做篩選，這邊就不需要再判斷，改回原來的方式
        public IEnumerable<Menu> GetMenus(Menu parent = null)
        {
            string tab = Session["CurrentTab"] == null ? "_index" : Session["CurrentTab"].ToString();

            menuCache = CurrentUser.Menus.ToList();
            if (CurrentUser.Employee.LeaveDate != null && CurrentUser.Employee.LeaveDate < DateTime.Now)
            {
                //00000014-0002-0000-0000-000000000000	Root	Root
                //00000014-0002-0006-0000-000000000000	統計報表	Reports
                //00000014-0002-0002-0003-000000000000	個人假單查詢	FormQuery
                //00000014-0002-0006-0001-000000000000	員工休假時數彙總表	DeptEmpHolidaySummary
                List<string> isLeaveMenu = new List<string>() { "00000014-0002-0000-0000-000000000000", "00000014-0002-0015-0000-000000000000", "00000014-0002-0015-0003-000000000000" };
                menuCache = CurrentUser.Menus.Where(x => isLeaveMenu.Contains(x.ID.ToString())).ToList();
            }
            //if (parent == null && tab == "_leaveApplication")
            //{
            //    //parent = CurrentUser.Menus.Find(x => x.ID == Guid.Parse("00000014-0002-0003-0000-000000000000"));
            //    return CurrentUser.Menus.Where(x => x.ID == Guid.Parse("00000014-0002-0003-0000-000000000000") && x.Type == (int)MenuType.MENU);

            //}
            //else 
            
            //20180227 Daniel 選單不用再單獨過濾，直接顯示全部的
            if (parent == null)
            {
                //parent = CurrentUser.Menus.Find(x => x.Type == (int)MenuType.ROOT);
                parent = menuCache.Find(x => x.Type == (int)MenuType.ROOT);
            }

            //return CurrentUser.Menus.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.MENU);
            return menuCache.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.MENU);
            
            /*
                if (parent == null && tab == "_salarySearch")
                {
                    //parent = CurrentUser.Menus.Find(x => x.ID == Guid.Parse("00000014-0002-0004-0000-000000000000"));
                    return CurrentUser.Menus.Where(x => x.ID == Guid.Parse("00000014-0002-0004-0000-000000000000") && x.Type == (int)MenuType.MENU);
                }
                //else if (parent == null)
                //{
                //    parent = CurrentUser.Menus.Find(x => x.Type == (int)MenuType.ROOT);
                //}

                if (tab == "_leaveApplication" && parent != null)
                {
                    return CurrentUser.Menus.Where(x => x.Parent_ID == Guid.Parse("00000014-0002-0003-0000-000000000000") && x.Type == (int)MenuType.MENU);
                }
                else if (tab == "_salarySearch" && parent != null)
                {
                    return CurrentUser.Menus.Where(x => x.Parent_ID == Guid.Parse("00000014-0002-0004-0000-000000000000") && x.Type == (int)MenuType.MENU);
                }
                return CurrentUser.Menus.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.MENU);
            */
            
            //parent = CurrentUser.Menus.Find(x => x.Type == (int)MenuType.ROOT);
            //return CurrentUser.Menus.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.MENU);

        }
        /*
        public IEnumerable<Menu> GetMenus(Menu parent = null)
        {
            string tab = Session["CurrentTab"]==null?"_index":Session["CurrentTab"].ToString();
           
            if (parent == null && tab == "_leaveApplication")
            {
                //parent = CurrentUser.Menus.Find(x => x.ID == Guid.Parse("00000014-0002-0003-0000-000000000000"));
                return CurrentUser.Menus.Where(x => x.ID == Guid.Parse("00000014-0002-0003-0000-000000000000") && x.Type == (int)MenuType.MENU);
            }
            else if (parent == null && tab == "_salarySearch")
            {
                //parent = CurrentUser.Menus.Find(x => x.ID == Guid.Parse("00000014-0002-0004-0000-000000000000"));
                return CurrentUser.Menus.Where(x => x.ID == Guid.Parse("00000014-0002-0004-0000-000000000000") && x.Type == (int)MenuType.MENU);
            }
            else if (parent == null)
            {
                parent = CurrentUser.Menus.Find(x => x.Type == (int)MenuType.ROOT);
            }

            if (tab == "_leaveApplication" && parent!=null)
            {
                return CurrentUser.Menus.Where(x => x.Parent_ID == Guid.Parse("00000014-0002-0003-0000-000000000000") && x.Type == (int)MenuType.MENU);
            }
            else if (tab == "_salarySearch" && parent!=null)
            {
                return CurrentUser.Menus.Where(x => x.Parent_ID == Guid.Parse("00000014-0002-0004-0000-000000000000") && x.Type == (int)MenuType.MENU);
            }
            return CurrentUser.Menus.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.MENU);
        }
        */

        public IEnumerable<Menu> GetTabs(Menu parent = null)
        {
            if (parent == null)
                parent = CurrentUser.Menus.Find(x => x.Type == (int)MenuType.ROOT);

            return CurrentUser.Menus.Where(x => x.Parent_ID == parent.ID && x.Type == (int)MenuType.TAB);
        }

        public IHtmlString JsonConvert(object value)
        {
            return Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(value));
        }

        public string getLanguageCookie()
        {
            if (Request.Cookies["lang"] == null)
            {
                return null;
            }
            return Request.Cookies["lang"].Value;
        }

        //public IEnumerable<ValueText> GetDietFoodOptions()
        //{
        //    return Services.GetService<OptionService>().GetValueText(OptionType.DIET_FOOD);
        //}

        //public IEnumerable<ValueText> GetDietDrinkOptions()
        //{
        //    return Services.GetService<OptionService>().GetValueText(OptionType.DIET_DRINK);
        //}

        //public IEnumerable<ValueText> GetGenderOptions()
        //{
        //    return Services.GetService<OptionService>().GetValueText(OptionType.GENDER);
        //}

        //public IEnumerable<ValueText> GetEducationalOptions()
        //{
        //    return Services.GetService<OptionService>().GetValueText(OptionType.EDUCATIONAL);
        //}

        //public IEnumerable<ValueText> GetExpenditureOptions()
        //{
        //    return Services.GetService<OptionService>().GetValueText(OptionType.EXPENDITURE);
        //}

        //public IEnumerable<ValueText> GetFAQTypeOptions()
        //{
        //    return Services.GetService<OptionService>().GetValueText(OptionType.FAQ_TYPE);
        //}

        //public IEnumerable<ValueText> GetJobLevelOptions()
        //{
        //    return Services.GetService<OptionService>().GetValueText(OptionType.JOB_LEVEL);
        //}

        //public IEnumerable<ValueText> GetAffiliateOptions()
        //{
        //    if (this.CurrentUser.Employee.company.is_affiliate)
        //        return Services.GetService<AffiliateService>().GetValueText().Where(x => x.v == this.CurrentUser.Employee.company.id.ToString());
        //    else
        //        return Services.GetService<AffiliateService>().GetValueText();
        //}

        //public IEnumerable<ValueText> GetCourseCategoryOptions()
        //{
        //    return Services.GetService<CourseCategoryService>().GetValueText();
        //}

        public IEnumerable<ValueText> GetCompanyOptions()
        {
            //if (this.CurrentUser.Employee.Company.is_affiliate)
                return Services.GetService<CompanyService>().GetValueText().Where(x => x.v == this.CurrentUser.Employee.CompanyID.ToString());
            //else
                //return Services.GetService<CompanyService>().GetValueText();
        }

        //public IEnumerable<ValueText> GetCourseOrganizerOptions()
        //{
        //    return Services.GetService<CourseOrganizerService>().GetValueText();
        //}

        //public IEnumerable<ValueText> GetCourseCharacterOptions()
        //{
        //    return Services.GetService<CourseCharacterService>().GetValueText();
        //}

        //public IEnumerable<ValueText> GetCourseTargetOptions()
        //{
        //    return Services.GetService<CourseTargetService>().GetValueText();
        //}

        //public IEnumerable<ValueText> GetCourseLocationOptions()
        //{
        //    return Services.GetService<CourseLocationService>().GetValueText();
        //}

        //public IEnumerable<ValueText> GetRoleOptions()
        //{
        //    return Services.GetService<RoleService>().GetValueText();
        //}

        public IEnumerable<ValueText> GetDepartmentOptions()
        {
            return Services.GetService<DepartmentService>().GetValueText(this.CurrentUser.Employee.CompanyID);
        }

        //public string GetConfigByKey(string key)
        //{
        //    return Services.GetService<ConfigService>().GetConfig(key);
        //}

        //public string GetLanguage(string key)
        //{
        //    return Services.GetService<LanguageService>().GetLanguage(key);
        //}

        public string GetAntiForgeryToken()
        {
            string cookieToken, formToken;
            AntiForgery.GetTokens(null, out cookieToken, out formToken);
            return string.Concat(cookieToken, ":", formToken);
        }
    }
    public abstract class BaseAppViewPage : BaseAppViewPage<dynamic>
    {
    }
}