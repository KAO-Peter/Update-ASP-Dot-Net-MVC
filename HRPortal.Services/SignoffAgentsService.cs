using HRPortal.DBEntities;
using System;
using System.Linq;

namespace HRPortal.Services
{
    public class SignoffAgentsService : BaseCrudService<SignoffAgents>
    {
        public SignoffAgentsService(HRPortal_Services services)
            : base(services)
        {
        }

        public SignoffAgents GetSignoffAgentsByID(Guid id)
        {
            SignoffAgents data = Db.SignoffAgents.FirstOrDefault(x => x.ID == id);
            return data;
        }
        public SignoffAgents GetSignoffAgentsByFormNo(string EmpID)
        {
            SignoffAgents data = Db.SignoffAgents.FirstOrDefault(x => x.EmployeeNO == EmpID);
            return data;
        }

        public override int Create(SignoffAgents model, bool isSave = true)
        {
            model.ID = Guid.NewGuid();
            return base.Create(model, isSave);
        }

        public override int Update(SignoffAgents model, bool isSave = true)
        {
            return base.Update(model, isSave); ;
        }
    }
}
