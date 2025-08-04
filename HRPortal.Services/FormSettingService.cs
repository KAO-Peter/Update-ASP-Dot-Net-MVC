using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class FormSettingService : BaseCrudService<FormSetting>
    {
        public FormSettingService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// 取得表單的參數根據公司別(如果沒丟公司別就撈全部)
        /// </summary>
        /// <returns></returns>
        public List<FormSetting> GetFormParameter(string FormType, string CompanyCode = "")
        {
            IQueryable<FormSetting> _formSetting;
            if (CompanyCode != "")
            {
                _formSetting = GetAll().Where(x => x.CompanyCode == CompanyCode && x.FormType == FormType);
            }
            else
            {
                _formSetting = GetAll().Where(x => x.FormType == FormType);
            }

            return _formSetting.ToList();
        }


        /// <summary>
        ///  取得表單的參數根據ID
        /// </summary>
        /// <returns></returns>
        public FormSetting GetFormParameterByID(Guid Id)
        {
            FormSetting model = GetAll().FirstOrDefault(x => x.ID == Id);
            return model;
        }



        /// <summary>
        /// 取得表單的參數根據公司別(如果沒丟公司別就撈全部)
        /// </summary>
        /// <returns></returns>
        public List<FormSetting> GetFormParameterByCompamyCode(string CompanyCode="")
        {
            IQueryable<FormSetting> _formSetting;

            if (CompanyCode != "")
            {
                _formSetting = GetAll().Where(x => x.CompanyCode == CompanyCode);
            }
            else
            {
                _formSetting = GetAll();
            }

            return _formSetting.ToList();
        }


        /// <summary>
        /// 更新表單上的參數
        /// </summary>
        /// <returns></returns>
        public bool UpdateStatus(string SettingValue, Guid Id)
        {
            bool result = true;
            FormSetting oldData = GetFormParameterByID(Id);
            FormSetting newData = new FormSetting();
            string[] updataproperties = { "SettingValue" };

            newData.SettingValue = SettingValue;


            try
            {
                Update(oldData, newData, updataproperties, true);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

    }
}
