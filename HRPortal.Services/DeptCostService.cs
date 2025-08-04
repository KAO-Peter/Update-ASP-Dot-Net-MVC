//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class DeptCostService : BaseCrudService<DeptCost>
    {
        public DeptCostService(HRPortal_Services services)
            : base(services)
        {
        }

        public IEnumerable<ValueText> GetValueText(string deptCode)
        {
            return GetAll().Where(x => x.DeptCode == deptCode).Select(x => new ValueText { id = x.ID, v = x.CostCode, t = x.CostName }).ToArray();
        }

        public IEnumerable<DeptCost> GetAllLists()
        {
            return this.GetAll();
        }

        public IEnumerable<DeptCost> GetListsByDeptCode(string deptCode)
        {
            return this.GetAll().Where(x => x.DeptCode == deptCode);
        }
    }
}
