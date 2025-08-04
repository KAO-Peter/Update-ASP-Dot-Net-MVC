using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.Databases.SqlClient;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class SignFlowLevelRepository : SignFlowRepositoryBase<SignFlowLevel>
    {
        public SignFlowLevelRepository()
            : base()
        {
        }
        public IList<SignFlowLevel> GetIsUsedSignFlowLevel()
        {
            IList<SignFlowLevel> _list = base.GetAll().Where(x => x.IsUsed == "Y").OrderBy(x => x.LevelID).ToList();
            return _list;
        }
    }
}
