using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace HRPortal.Services
{
    public class SerialControlService : BaseCrudService<SerialControl>
    {
        public SerialControlService(HRPortal_Services services)
            : base(services)
        {
        }

        public int GetSerialNumber(string serialName)
        {
            int _number = 0;
            using (TransactionScope _scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }))
            {
                SerialControl _serail = FirstOrDefault(x => x.SerialName == serialName);
                if(_serail == null)
                {
                    _serail = new SerialControl() { ID = Guid.NewGuid(), SerialName = serialName, SerialNumber = _number };
                    Create(_serail);
                }
                _number = ++_serail.SerialNumber;
                Update(_serail);

                _scope.Complete();
            }

            return _number;
        }

    }
}
