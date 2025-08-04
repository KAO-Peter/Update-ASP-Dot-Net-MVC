using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.Databases.SqlClient.Repositories;
using System.Data.Entity;
namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class SignFlowRecRepository:SignFlowRecRepositoryBase<SignFlowRec>
    {
        public SignFlowRecRepository()
            : base()
        {
        }
        public override IQueryable<SignFlowRec> GetSignFlowRec(string formNumber, string isUsed)
        {
            return this.GetAll().Where(x => x.FormNumber == formNumber && x.IsUsed == isUsed);
        }
        public override IQueryable<SignFlowRec> GetSignFlowRecOrderBySignOrder(string formNumber, string isUsed)
        {
            return this.GetSignFlowRec(formNumber, isUsed).OrderBy(x => x.SignOrder);
        }
        public override IQueryable<SignFlowRec> GetSignFlowRecByOverSignOrder(string formNumber, string isUsed, string signOrder)
        {
            return  this.GetSignFlowRecOrderBySignOrder(formNumber, isUsed).Where(x => int.Parse(x.SignOrder) > int.Parse(signOrder) && x.SignOrder.Length == signOrder.Length);
        }
        public override IQueryable<SignFlowRec> GetSignFlowRecByGroupID(string formNumber, string isUsed, string groupID)
        {
            return this.GetSignFlowRec(formNumber, isUsed).Where(x => x.GroupId.ToString() == groupID);
        }
        public SignFlowRec GetSingleSignFlowRec(string iD)
        {
            return this.GetAll().FirstOrDefault(x => x.ID == iD);
        }
        public override void UpdateSingOrder(SignFlowRec entity, string m_User, int signorderCount)
        {
            entity.SignOrder = (int.Parse(entity.SignOrder) + signorderCount).ToString();
            entity.MUser = m_User;
            entity.MDate = DateTime.Now;
            m_Context.Entry(entity).State = EntityState.Modified;
            base.Update(entity);
        }
        public override void Update(SignFlowRec entity)
        {
            SignFlowRec _data = this.FindOne(x => x.ID == entity.ID);
            m_Context.Entry(_data).CurrentValues.SetValues(entity);
        }
    }
}
