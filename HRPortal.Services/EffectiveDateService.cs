using HRPortal.DBEntities;
using System;
using System.Linq;

namespace HRPortal.Services
{
    /// <summary> 生效日期設定 </summary>
    public class EffectiveDateService : BaseCrudService<EffectiveDate>
    {
        public EffectiveDateService(HRPortal_Services services)
            : base(services)
        {
        }

        public EffectiveDate GetEffectiveDateByID(Guid id)
        {
            EffectiveDate data = Db.EffectiveDate.FirstOrDefault(x => x.ID == id);
            return data;
        }

        /// <summary>
        /// 讀取資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EffectiveDate GetEffectiveDateByFunctionID(Guid id,string changeType)
        {
            EffectiveDate data = Db.EffectiveDate.FirstOrDefault(x => x.Function_ID == id && x.Change_Type == changeType);
            return data;
        }

        /// <summary> 讀取變動資料 </summary>
        /// <param name="ChangeID"></param>
        /// <returns></returns>
        public EffectiveDate GetEffectiveDateByChangeID(Guid ChangeID)
        {
            EffectiveDate data = Db.EffectiveDate.FirstOrDefault(x => x.Change_ID == ChangeID);
            return data;
        }

        /// <summary> 新增 </summary>
        /// <param name="model"></param>
        /// <param name="isSave"></param>
        /// <returns></returns>
        public override int Create(EffectiveDate model, bool isSave = true)
        {
            model.ID = Guid.NewGuid();
            return base.Create(model, isSave);
        }

        /// <summary> 修改 </summary>
        /// <param name="model"></param>
        /// <param name="isSave"></param>
        /// <returns></returns>
        public override int Update(EffectiveDate model, bool isSave = true)
        {
            return base.Update(model, isSave); ;
        }
        
        /// <summary> 刪除 </summary>
        /// <param name="model"></param>
        /// <param name="isSave"></param>
        /// <returns></returns>
        public override int Delete(EffectiveDate model, bool isSave = true)
        {
            return base.Delete(model, isSave); ;
        }
    }
}
