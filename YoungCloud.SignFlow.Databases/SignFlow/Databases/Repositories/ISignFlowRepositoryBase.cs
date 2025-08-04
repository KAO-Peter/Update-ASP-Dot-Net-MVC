using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.Databases.SqlClient.Repositories;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public interface ISignFlowRepositoryBase<TEntity> : IRepositoryBase<TEntity> 
    {
    }
}
