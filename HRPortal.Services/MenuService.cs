using HRPortal.Core.Utilities;
using HRPortal.DBEntities;
//using HRPortal.Models;
//using HRPortal.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Configuration;
using System.Web;
using HRPortal.Services.Models;

namespace HRPortal.Services
{
    public enum MenuGroup : int
    {
        /// <summary>
        /// 後台
        /// </summary>
        BACK_END = 1,
        /// <summary>
        /// 前台
        /// </summary>
        FRONT_END = 2
    }
    public enum MenuType : int
    {
        /// <summary>
        /// 根
        /// </summary
        ROOT = 0,
        /// <summary>
        /// 選單
        /// </summary>
        MENU = 1,
        /// <summary>
        /// 標籤
        /// </summary>
        TAB = 2
    }
    
    public class MenuService : BaseCrudService<Menu>
    {
        public MenuService(HRPortal_Services services)
            : base(services)
        {
        }
        public List<Menu> GetFullMenu(Role role)
        {
            //if (this.HasCacheData(role.ID.ToString() + "::GetFullMenu") == false)
            {
                List<Menu> menus = this.Where(x => x.RoleMenuMaps.Any(r => r.Role_ID == role.ID)).ToList();
                List<Menu> data = menus.ToList();
                foreach (Menu item in menus)
                {
                    AddParentMenu(ref data, item);
                }
                data = data.OrderBy(x => x.Ordering).ToList();
                foreach (Menu item in data.Where(x => string.IsNullOrWhiteSpace(x.Link) && data.Any(y => y.Parent_ID == x.ID && y.Type == (int)MenuType.TAB) && data.Any(y => y.Parent_ID == x.ID && y.Type == (int)MenuType.MENU) == false))
                {
                    item.Link = data.First(x => x.Parent_ID == item.ID && x.Type == (int)MenuType.TAB).Link;
                }

                //20170324修改 by Daniel，增加切版選單條件判斷，之前是放在View做判斷，因首頁區塊也要進行檢查，所以改成在最早取得Menu的地方檢查
                //20170328修改 by Daniel，改為Global變數
                string siteType = SiteFunctionInfo.SiteType; //目前遠百以外的客戶會是空值，不做過濾
                if (siteType == "mobile" || siteType == "outside" || siteType == "signonly") //行動裝置與外網都需要限制選單 //20171221 Daniel 增加直接登入網站也要限制

                {
                    //List<string> allowedFunctionTitleList = (List<string>)session["AllowedMenu"];
                    List<string> allowedFunctionIDList = (List<string>)SiteFunctionInfo.AllowedMenuID;
                    data = data.Where(x => allowedFunctionIDList.Contains(x.ID.ToString())).ToList();
                }
                
                //this.SetCacheData(role.ID.ToString() + "::GetFullMenu", data);
                return data;
            }
            //return this.GetCacheData(role.ID.ToString() + "::GetFullMenu") as List<Menu>;
        }

        private void AddParentMenu(ref List<Menu> menus, Menu menu)
        {
            if (menu == null)
                return;
            if (menu.Parent_ID == null)
                return;
            if (menus.Any(x => x.ID == menu.Parent_ID))
                return;
            menus.Add(this.FirstOrDefault(x => x.ID == menu.Parent_ID));
            AddParentMenu(ref menus, this.FirstOrDefault(x => x.ID == menu.Parent_ID));
        }

        public string GetDefaultUrl(Role role)
        {
            //if (this.HasCacheData(role.ID.ToString() + "::GetDefaultUrl") == false)
            {
                string data = GetFullMenu(role).First(x => string.IsNullOrWhiteSpace(x.Link) == false).Link;
                //this.SetCacheData(role.ID.ToString() + "::GetDefaultUrl", data);
                return data;
            }
            //return this.GetCacheData(role.ID.ToString() + "::GetDefaultUrl");
        }

        protected override IQueryable<Menu> Where(bool include_disable, params Expression<Func<Menu, bool>>[] filters)
        {
            //MenuGroup menuGroup = (ConfigurationManager.AppSettings["AppType"].ToString() == "backend") ? MenuGroup.BACK_END : MenuGroup.FRONT_END;
            return base.Where(include_disable, filters);//.Where(x => x. == (int)menuGroup);
        }

        //public IQueryable<Menu> GetBackendMenus(params Expression<Func<Menu, bool>>[] filters)
        //{
        //    return base.Where(true, filters).Where(x => x.group == (int)MenuGroup.BACK_END);
        //}

        public IQueryable<Menu> GetMenus(params Expression<Func<Menu, bool>>[] filters)
        {
            return base.Where(true, filters);//.Where(x => x.group == (int)MenuGroup.FRONT_END);
        }
    }
}
