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
    public class PatchCardFormService : BaseCrudService<PatchCardForm>
    {
        public PatchCardFormService(HRPortal_Services services)
            : base(services)
        {
        }

        public PatchCardForm GetOverTimeFormByID(Guid id)
        {
            PatchCardForm data = Db.PatchCardForms.FirstOrDefault(x => x.ID == id);
            return data;
        }

        public PatchCardForm GetPatchCardFormByID(Guid id)
        {
            PatchCardForm data = Db.PatchCardForms.FirstOrDefault(x => x.ID == id);
            return data;
        }

        public PatchCardForm GetOverTimeFormByFormNo(string formNo)
        {
            PatchCardForm data = Db.PatchCardForms.FirstOrDefault(x => x.FormNo == formNo);
            return data;
        }

        public int Create(PatchCardForm model,HttpPostedFileBase filedata)
        {
            int result = 0;
            try
            {
                //upload file
                if (filedata != null && filedata.ContentLength > 0)
                {
                    try
                    {
                        string savePath = Services.GetService<SystemSettingService>().GetSettingValue("PatchCardFormFiles");
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(filedata.FileName);
                        var path = Path.Combine(HttpContext.Current.Server.MapPath(savePath), fileName);
                        filedata.SaveAs(path);
                        model.FilePath = fileName;
                        model.FileName = filedata.FileName;
                        model.FileFormat = filedata.ContentType;
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }
                }
             
                result=this.Create(model, true);
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public int Update(PatchCardForm model, HttpPostedFileBase filedata)
        {
            int result = 0;
            try
            {
                //upload file
                if (filedata != null && filedata.ContentLength > 0)
                {
                    try
                    {
                        string savePath = Services.GetService<SystemSettingService>().GetSettingValue("PatchCardFormFiles");
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(filedata.FileName);
                        var path = Path.Combine(HttpContext.Current.Server.MapPath(savePath), fileName);
                        filedata.SaveAs(path);
                        model.FilePath = fileName;
                        model.FileName = filedata.FileName;
                        model.FileFormat = filedata.ContentType;
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }
                }

                result = this.Update(model, true);
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public override int Create(PatchCardForm model, bool isSave = true)
        {
            try
            {
                
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

        public override int Update(PatchCardForm oldData, PatchCardForm newData, string[] includeProperties, bool isSave = true)
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

            return result;
        }

        public int Update(PatchCardForm oldData, PatchCardForm newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "Status", "PatchCardTime", "Type", "Reason", "FilePath", "FileName", "Modifiedby", "ModifiedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }

        public int Delete(PatchCardForm oldData, PatchCardForm newData, Guid employeeID, bool isSave = true)
        {
            int result = 0;
            try
            {
                newData.IsDeleted = true;
                newData.DeletedTime = DateTime.Now;
                newData.ModifiedTime = DateTime.Now;
                newData.Deletedby = employeeID;
                newData.Modifiedby = employeeID;
                string[] updataproperties = { "IsDeleted", "Deletedby", "DeletedTime", "Modifiedby", "ModifiedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }

    }
}
