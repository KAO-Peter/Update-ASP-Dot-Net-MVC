using HRPortal.DBEntities;

namespace HRPortal.Services
{
    public class EmpALScheduleService : BaseCrudService<EmpALSchedule>
    {
        public EmpALScheduleService(HRPortal_Services services)
            : base(services)
        {
        }
    }
}