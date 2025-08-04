//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HRPortal.Services.Models.BambooHR;

namespace HRPortal.Services
{
    public class BambooHRLeaveFormRecordService : BaseCrudService<BambooHRLeaveFormRecord>
    {
        public BambooHRLeaveFormRecordService(HRPortal_Services services)
            : base(services)
        {
        }

        public LogInfo LogInfo { get; set; }

        public BambooHRLeaveFormRecord GetRecordByFormNo(string FormNo)
        {
            //20210223 Daniel 因雙向整合目前會有多筆記錄，調整為抓最新一筆
            var query = GetAll().Where(x => x.FormNo == FormNo).OrderByDescending(y => y.CreateTime).FirstOrDefault();
            return query;
        }

        public string GetTimeOffIDByFormNo(string FormNo)
        {
            var query = GetRecordByFormNo(FormNo);
            return query != null ? query.BambooHRTimeOffID : "";
        }

        //新增發送紀錄時也要記錄一筆Log內容
        public override int Create(BambooHRLeaveFormRecord model, bool isSave = true)
        {
           
            int result = base.Create(model, isSave);

            //記錄到BambooHRLeaveFormRecordLog表格
            BambooHRLeaveFormRecordLog log = new BambooHRLeaveFormRecordLog()
            {
                BambooHRLeaveFormRecord_ID = model.ID,
                UserID = this.LogInfo.UserID,
                UserIP = this.LogInfo.UserIP,
                Controller = this.LogInfo.Controller,
                Action = this.LogInfo.Action,
                LogSource = "Create",
                LogText = JsonConvert.SerializeObject(model),
                CreateTime = DateTime.Now
            };
            this.Services.GetService<BambooHRLeaveFormRecordLogService>().Create(log);

            return result;
        }
       
        //修改發送紀錄時要記錄異動內容
        public override int Update(BambooHRLeaveFormRecord model, bool isSave = true)
        {
            JObject json = new JObject();

            var properties = model.GetType().GetProperties();
            var entry = this.Db.Entry(model);
            
            foreach (var p in properties)
            {
                if (!p.GetGetMethod().IsVirtual)
                {
                    var ep = entry.Property(p.Name);
                    if (ep.IsModified)
                    {
                        json.Add(p.Name, ep.OriginalValue + " -> " + ep.CurrentValue);
                    }
                }
            }

            //記錄到BambooHRLeaveFormRecordLog表格
            BambooHRLeaveFormRecordLog log = new BambooHRLeaveFormRecordLog()
            {
                BambooHRLeaveFormRecord_ID = model.ID,
                UserID = this.LogInfo.UserID,
                UserIP = this.LogInfo.UserIP,
                Controller = this.LogInfo.Controller,
                Action = this.LogInfo.Action,
                LogSource = "Update",
                LogText = JsonConvert.SerializeObject(json),
                CreateTime = DateTime.Now
            };

            this.Services.GetService<BambooHRLeaveFormRecordLogService>().Create(log);

            return base.Update(model, isSave);

        }
 
    }
}
