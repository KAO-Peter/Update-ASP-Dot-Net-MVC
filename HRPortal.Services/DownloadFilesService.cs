using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HRPortal.Services
{
    public class DownloadFilesService : BaseCrudService<DownloadFile>
    {
        //private SystemSettingService sysSetting_service = new SystemSettingService();
        public DownloadFilesService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// 檔案列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<DownloadFile> GetDownloadFileList(string keyword = "")
        {
            IQueryable<DownloadFile> data = this.GetAll().Where(x => x.IsDeleted == false).OrderByDescending(x => x.ModifiedTime);
            if (!string.IsNullOrEmpty(keyword))
            {
                data = data.Where(x => x.Name.Contains(keyword));
            }

            return data;
        }

    

        public IQueryable<DownloadFile> GetDownloadFileAllList()
        {
            IQueryable<DownloadFile> data = this.GetAll().OrderByDescending(x => x.IsDeleted).ThenByDescending(x => x.CreatedTime);
            return data;
        }

        public DownloadFile GetDownloadFileByID(Guid Id)
        {
            DownloadFile model = this.GetAll().FirstOrDefault(x => x.Id == Id);
            return model;
        }


        public override int Create(DownloadFile model, bool isSave = true)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.CreatedTime = DateTime.Now;
                model.ModifiedTime = DateTime.Now;
                model.Modifiedby = model.Createdby;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return base.Create(model, isSave);
        }

        public override int Update(DownloadFile oldData, DownloadFile newData, string[] includeProperties, bool isSave = true)
        {
            int result = 0;
            try
            {
                newData.ModifiedTime = DateTime.Now;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;

            }
            result = base.Update(oldData, newData, includeProperties, isSave);
            //if (result == 1)
            {
                //clear cache
                //this.ClearCacheData("GetAnnounceList");
            }
            return result;
        }

        public int DeleteFile(Guid deletedId,Guid currentUser, bool isSave = true)
        {
            int result = 0;
            try
            {
                DownloadFile oldData = this.GetDownloadFileByID(deletedId);
                DownloadFile newData = oldData;
                newData.IsDeleted = true;
                newData.DeletedTime = DateTime.Now;
                newData.Deletedby = currentUser;
                string[] updataproperties = { "IsDeleted", "Deletedby", "DeletedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }
            return result;
        }

        public int UpdateFile(DownloadFile newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                DownloadFile oldData = this.GetDownloadFileByID(newData.Id);
 
                string[] updataproperties = { "Name", "Description", "Modifiedby", "ModifiedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }
            return result;
        }

        public string UploadFile(HttpPostedFileBase file)
        {
            string result ="";
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    string savePath = Services.GetService<SystemSettingService>().GetSettingValue("DownloadFilePath");
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(savePath), fileName);
                    file.SaveAs(path);
                    result = fileName;
                }
                catch (Exception ex)
                {

                    //throw;
                }
            }

            return result;
        }
    }
}
