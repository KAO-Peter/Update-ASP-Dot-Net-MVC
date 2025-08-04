using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using HRPortal.DBEntities;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;

namespace HRPortal.Areas.M06.Controllers
{
    public class RoleManagerController : BaseController
    {
        //
        // GET: /M06/RoleManager/
        public ActionResult Index()
        {
            List<Role> _roles = Services.GetService<RoleService>().GetAll().ToList();
            List<RoleViewModel> _model = new List<RoleViewModel>();

            foreach (Role _role in _roles)
            {
                RoleViewModel _item = new RoleViewModel()
                {
                    ID = _role.ID,
                    Name = _role.Name,
                    IsAdmin = false,
                    IsHR = false,
                    Description = _role.Description,
                };

                if (!string.IsNullOrEmpty(_role.RoleParams))
                {
                    dynamic _roleParams = System.Web.Helpers.Json.Decode(_role.RoleParams);
                    _item.IsAdmin = (_roleParams.is_admin != null && _roleParams.is_admin);
                    _item.IsHR = (_roleParams.is_hr != null && _roleParams.is_hr);
                }

                _model.Add(_item);
            }

            return View(_model);
        }

        public ActionResult Create()
        {
            return PartialView("_CreateRole");
        }

        [HttpPost]
        public ActionResult Create(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_CreateRole", model);
            }
            else
            {
                Dictionary<string, bool> _roleParams = new Dictionary<string, bool>();
                if (model.IsAdmin)
                {
                    _roleParams.Add("is_admin", true);
                }
                if (model.IsHR)
                {
                    _roleParams.Add("is_hr", true);
                }

                Role _role = new Role()
                {
                    ID = Guid.NewGuid(),
                    Name = model.Name,
                    RoleParams = _roleParams.Count > 0 ? System.Web.Helpers.Json.Encode(_roleParams) : null,
                    Description = model.Description,
                    CreatedTime = DateTime.Now,
                };

                int IsSuccess = Services.GetService<RoleService>().Create(_role);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "新增成功";
                    WriteLog("Success:" + _role.ID);
                    return Json(new { success = true });
                }
            }
            return PartialView("_CreateRole", model);
        }
        
        public ActionResult Edit(Guid id)
        {
            Role _role = Services.GetService<RoleService>().FirstOrDefault(x => x.ID == id);
            
            RoleViewModel _model = new RoleViewModel()
            {
                ID = _role.ID,
                Name = _role.Name,
                IsAdmin = false,
                IsHR = false,
                Description = _role.Description,
            };

            if (!string.IsNullOrEmpty(_role.RoleParams))
            {
                dynamic _roleParams = System.Web.Helpers.Json.Decode(_role.RoleParams);
                _model.IsAdmin = (_roleParams.is_admin != null && _roleParams.is_admin);
                _model.IsHR = (_roleParams.is_hr != null && _roleParams.is_hr);
            }

            return PartialView("_EditRole", _model);
        }

        [HttpPost]
        public ActionResult Edit(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_EditRole", model);
            }
            else
            {
                Dictionary<string, bool> _roleParams = new Dictionary<string, bool>();
                if (model.IsAdmin)
                {
                    _roleParams.Add("is_admin", true);
                }
                if (model.IsHR)
                {
                    _roleParams.Add("is_hr", true);
                }

                Role _role = Services.GetService<RoleService>().FirstOrDefault(x => x.ID == model.ID);
                _role.Name = model.Name;
                _role.RoleParams = _roleParams.Count > 0 ? System.Web.Helpers.Json.Encode(_roleParams) : null;
                _role.Description = model.Description;

                int IsSuccess = Services.GetService<RoleService>().Update(_role);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "修改成功";
                    WriteLog("Success:" + model.ID);
                    return Json(new { success = true });
                }
            }
            return PartialView("_EditRole", model);
        }
                
        public ActionResult RoleMenuMap(Guid roleID)
        {
            ViewBag.RoleID = roleID;
            return PartialView("_RoleMenuMap", GetMenuMap(roleID));
        }

        [HttpPost]
        public ActionResult UpdateRoleMenuMap(UpdateRoleMenuModel model)
        {
            try
            {
                List<Guid> _menuList = new List<Guid>();
                if (model.MenuList != null)
                {
                    _menuList = model.MenuList.ToList();
                }
                
                Services.GetService<RoleMenuMapsService>().SetRoleMenus(model.RoleID, _menuList);
                WriteLog("Success:" + model.RoleID);

                return Json(new AjaxResult() { status = "success", message = "更新成功" });
            }
            catch(Exception ex)
            {
                return Json(new AjaxResult() { status = "failed", message = "更新失敗" });
            }

        }

        private RoleMenuMapViewModel GetMenuMap()
        {
            RoleMenuMapViewModel _menuMap = new RoleMenuMapViewModel();

            List<Menu> _allMenu = Services.GetService<MenuService>().Where(x => !x.DisableDate.HasValue).OrderBy(x=>x.Ordering).ToList();

            Guid _rootId = _allMenu.FirstOrDefault(x => !x.Parent_ID.HasValue).ID;
            foreach (Menu _group in _allMenu.Where(x => x.Parent_ID == _rootId))
            {
                MenuGroupItem _groupItem = new MenuGroupItem();
                _groupItem.Title = _group.Title;

                foreach (Menu _menu in _allMenu.Where(x => x.Parent_ID == _group.ID))
                {
                    RoleMenuItem _item = new RoleMenuItem();
                    _item.ID = _menu.ID;
                    _item.Title = _menu.Title;
                    _item.Active = false;

                    _groupItem.Items.Add(_item);
                }

                _menuMap.RoleMenus.Add(_groupItem);
            }

            return _menuMap;
        }

        private RoleMenuMapViewModel GetMenuMap(Guid roleID)
        {
            RoleMenuMapViewModel _menuMap = GetMenuMap();
            List<Guid> _roleMenuList = Services.GetService<RoleMenuMapsService>().GetByRole(roleID).Select(x => x.Menu_ID).ToList();

            foreach (RoleMenuItem _item in _menuMap.RoleMenus.SelectMany(x => x.Items).Where(x => _roleMenuList.Contains(x.ID)))
            {
                _item.Active = true;
            }

            return _menuMap;
        }
	}
}