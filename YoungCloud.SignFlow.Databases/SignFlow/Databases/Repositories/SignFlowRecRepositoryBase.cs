using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public abstract class SignFlowRecRepositoryBase<TEntity> : SignFlowRepositoryBase<TEntity>, ISignFlowRecRepositoryBase<TEntity> where TEntity : class
    {
        public  abstract IQueryable<TEntity> GetSignFlowRec(string formNumber, string isUsed);
        public  abstract IQueryable<TEntity> GetSignFlowRecOrderBySignOrder(string formNumber, string isUsed);
        public  abstract IQueryable<TEntity> GetSignFlowRecByOverSignOrder(string formNumber, string isUsed, string signOrder);
        public  abstract IQueryable<TEntity> GetSignFlowRecByGroupID(string formNumber, string isUsed, string groupID);
        public  abstract void UpdateSingOrder(TEntity entity, string m_User, int signorderCount);
    }
}
