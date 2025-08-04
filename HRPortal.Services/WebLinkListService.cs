//using HRPortal.Models;
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
    public class WebLinkListService : BaseCrudService<WebLinkList>
    {
        public WebLinkListService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// 取得所有的連結
        /// </summary>
        /// <returns></returns>
        public List<WebLinkList> GetAllLinkList()
        {
            IQueryable<WebLinkList> _webLinkList;

            _webLinkList = GetAll().Where(x => x.IsDeleted == false);

            return _webLinkList.ToList();
        }

        /// <summary>
        /// 刪除目前所選擇連結
        /// </summary>
        /// <returns></returns>
        public int DeleteLink(Guid deletedId, Guid currentUser, bool isSave = true)
        {
            int result = 0;
            try
            {
                WebLinkList oldData = this.GetLinkByID(deletedId);
                WebLinkList newData = oldData;
                newData.IsDeleted = true;
                newData.Deletedby = currentUser;
                newData.DeletedTime=DateTime.Now;
                string[] updataproperties = { "IsUsed", "Deletedby", "DeletedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }
            return result;
        }


        public WebLinkList GetLinkByID(Guid Id)
        {
            WebLinkList model = this.GetAll().FirstOrDefault(x => x.ID == Id);
            return model;
        }

        public bool UpdateStatus(bool ShowStatus, Guid Id, Guid ChangeModifiedbyId)
        {
            bool result = true;
            WebLinkList oldData = this.GetLinkByID(Id);
            WebLinkList newData = new WebLinkList();
            newData.Modifiedby = ChangeModifiedbyId;
            newData.ModifiedTime = DateTime.Now;
            string[] updataproperties = { "IsUsed", "Modifiedby", "ModifiedTime" };
            if (ShowStatus)
            {
                newData.IsUsed = true;
            }
            else
            {
                newData.IsUsed = false;
            }

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


        public int Update(WebLinkList oldData, WebLinkList newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "Number","WebName", "WebLink", "Createdby", "CreatedTime", "Modifiedby", "ModifiedTime", "IsUsed" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }


        public override int Create(WebLinkList model, bool isSave = true)
        {
            try
            {
                model.CreatedTime = DateTime.Now;
                model.ModifiedTime = DateTime.Now;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return base.Create(model, isSave);
        }


        public override int Update(WebLinkList oldData, WebLinkList newData, string[] includeProperties, bool isSave = true)
        {
            int result = 0;
            try
            {
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;

            }
            result = base.Update(oldData, newData, includeProperties, isSave);
            return result;
        }

    }
}
