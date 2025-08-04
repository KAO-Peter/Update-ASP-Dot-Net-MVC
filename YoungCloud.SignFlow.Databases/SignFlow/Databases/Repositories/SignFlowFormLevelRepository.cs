using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class SignFlowFormLevelRepository : SignFlowRepositoryBase<SignFlowFormLevel>
    {
        public SignFlowFormLevelRepository()
            : base()
        {
        }
        public IList<SignFlowFormLevel> GetSignFlowFormLevelByFormType(string fromType)
        {
            List<SignFlowFormLevel> _list = base.GetAll().Where(x => x.FormType == fromType && x.IsUsed == "Y").ToList();
            return _list;
        }
    }
}
