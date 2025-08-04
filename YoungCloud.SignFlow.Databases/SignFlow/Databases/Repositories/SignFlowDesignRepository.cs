using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class SignFlowDesignRepository : SignFlowRepositoryBase<SignFlowDesign>
    {
        public SignFlowDesignRepository()
            : base()
        {
        }

        public IQueryable<SignFlowDesign> GetByDesignID(string designID)
        {
            return GetAll().Where(x => x.DesignID == designID && x.IsUsed == "Y");
        }
    }
}
