using System;
using System.Collections.Generic;
using System.Linq;
using YoungCloud.Databases.SqlClient.Repositories;
namespace YoungCloud.SignFlow.Databases.Repositories
{
    public interface ISignFlowRecRepositoryBase<TEntity> : ISignFlowRepositoryBase<TEntity>
     where TEntity : class
    {
        System.Linq.IQueryable<TEntity> GetSignFlowRec(string formNumber, string isUsed);
        System.Linq.IQueryable<TEntity> GetSignFlowRecOrderBySignOrder(string formNumber, string isUsed);
        System.Linq.IQueryable<TEntity> GetSignFlowRecByOverSignOrder(string formNumber, string isUsed, string signOrder);
        System.Linq.IQueryable<TEntity> GetSignFlowRecByGroupID(string formNumber, string isUsed, string groupID);

        void UpdateSingOrder(TEntity entity, string m_User, int signorderCount);
        //IList<TEntity> GetSignFlowRecByOverSignOrder(string p, global::YoungCloud.SignFlow.SignLists.DefaultEnum.IsUsed globalYoungCloudSignFlowSignListsDefaultEnumIsUsed, string signOrder);
    }
}
