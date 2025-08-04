using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public interface IDeptRepositoryBase<TEntity> : ISignFlowRepositoryBase<TEntity> where TEntity : class
    {
      string GetManagerID(string deptID);
    }
}
