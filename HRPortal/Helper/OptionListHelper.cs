using HRPortal.DBEntities;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Helper
{
    public class OptionListHelper
    {
        private HRPortal_Services _services;

        public OptionListHelper()
        {
            _services = new HRPortal_Services();
        }

        public List<SelectListItem> GetOptionList(string optionGroupKey, string selectedValue = null)
        {
            List<SelectListItem> _listItem = new List<SelectListItem>();
            ResourceManager _resourceManager = new ResourceManager("HRPortal.MultiLanguage.Resource", typeof(HRPortal.MultiLanguage.Resource).Assembly);
            var getLanguageCookie = System.Web.HttpContext.Current.Request.Cookies["lang"] != null ? System.Web.HttpContext.Current.Request.Cookies["lang"].Value : "";
            IList<Option> _options = _services.GetService<OptionService>().GetOptionListByGroup(optionGroupKey);
            foreach (Option _option in _options)
            {
                SelectListItem _item = new SelectListItem();
                _item.Value = _option.OptionValue;
                if(!string.IsNullOrEmpty(selectedValue) && selectedValue == _option.OptionValue)
                {
                    _item.Selected = true;
                }
                if (!string.IsNullOrEmpty(_option.DisplayResourceName)
                    && !string.IsNullOrEmpty(_resourceManager.GetString(_option.DisplayResourceName)))
                {
                    _item.Text = _resourceManager.GetString(_option.DisplayResourceName);
                }
                else if(!string.IsNullOrEmpty(_option.DisplayName))
                {
                    if (getLanguageCookie == "en-US")
                    {
                        _item.Text = _option.DisplayResourceName;
                    }
                    else
                    {
                        _item.Text = _option.DisplayName;
                    }
                }
                else
                {
                    _item.Text = _option.OptionValue;
                }

                _listItem.Add(_item);
            }

            return _listItem;
        }
    }
}