using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class PersonnelspecificationFilesService : BaseCrudService<PersonnelspecificationFiles>
    {

        public PersonnelspecificationFilesService(HRPortal_Services services)
            : base(services)
        {
        }

        public DownloadFile GetFileIDByAnnouncement(Guid ID)
        {
            DownloadFile df = new DownloadFile();
            var data = GetAll().Where(x => x.AnnouncementID == ID).FirstOrDefault();
            if (data != null)
                df = data.DownloadFiles;
            return df;
        }

        public PersonnelspecificationFiles GetFileByAnnouncemrnyID(Guid ID)
        {
            var data = GetAll().Where(x => x.AnnouncementID == ID).FirstOrDefault();

            return data;
        }

        public override int Create(PersonnelspecificationFiles model, bool isSave = true)
        {
            var _result = 0;
            try
            {
                //檢查Announcement是否已經新增，有的話更新，沒有則新增
                var olddata = GetFileByAnnouncemrnyID(model.AnnouncementID);
                if (olddata != null)
                {
                    _result = Update(olddata, model, new string[] { "DownloadFilesID" });
                }
                else
                {
                    model.ID = Guid.NewGuid();
                    isSave = true;
                    _result = base.Create(model, isSave);
                }

            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return _result;
        }

        public override int Update(PersonnelspecificationFiles oldData, PersonnelspecificationFiles newData, string[] includeProperties, bool isSave = true)
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
