//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.Services.Models.BambooHR;

namespace HRPortal.Services
{
    public class BambooHREmployeeMappingService : BaseCrudService<BambooHREmployeeMapping>
    {
        public BambooHREmployeeMappingService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// 由工號找出對應的BambooHR Employee ID
        /// </summary>
        /// <param name="EmpID">工號</param>
        public string GetBambooHREmployeeIDByEmpID(string EmpID)
        {
            string result = "";
            BambooHREmployeeMapping mapping = this.GetAll().AsNoTracking().Where(x => x.EmpID == EmpID).OrderByDescending(y => y.CreateTime).FirstOrDefault();
            if (mapping != null)
            {
                result = mapping.BambooHREmployeeID;
            }

            return result;

        }

        public BambooHREmployeeMapping GetMappingByBambooHREmployeeID(string BambooHREmployeeID, List<BambooHREmployeeMapping> mappingList = null)
        {
            
            BambooHREmployeeMapping mapping;
            if (mappingList != null)
            {
                mapping = mappingList.Where(x => x.BambooHREmployeeID == BambooHREmployeeID).FirstOrDefault();
            }
            else
            {
                mapping = this.GetAll().Include("Employee").Include("Employee.Company").Include("Employee.SignDepartment").Where(x => x.BambooHREmployeeID == BambooHREmployeeID).FirstOrDefault();
            }

            return mapping;
        }

        public Employee GetEmployeeByBambooHREmployeeID(string BambooHREmployeeID, List<BambooHREmployeeMapping> mappingList = null)
        {
            Employee result = null;
            BambooHREmployeeMapping mapping;
            if (mappingList != null)
            {
                mapping = mappingList.Where(x => x.BambooHREmployeeID == BambooHREmployeeID).FirstOrDefault();
            }
            else
            {
                mapping = this.GetAll().Include("Employee").Include("Employee.Company").Include("Employee.SignDepartment").Where(x => x.BambooHREmployeeID == BambooHREmployeeID).FirstOrDefault();
            }

            if (mapping != null && mapping.PortalEmployeeID.HasValue)
            {
                result = mapping.Employee;
            }

            return result;
        }

        public Employee GetEmployeeByBambooHRUserID(string BambooHRUserID, List<BambooHREmployeeMapping> mappingList = null)
        {
            Employee result = null;
            BambooHREmployeeMapping mapping;
            if (mappingList != null)
            {
                mapping = mappingList.Where(x => x.BambooHRUserID == BambooHRUserID).FirstOrDefault();
            }
            else
            {
                mapping = this.GetAll().Include("Employee").Include("Employee.Company").Include("Employee.SignDepartment").Where(x => x.BambooHRUserID == BambooHRUserID).FirstOrDefault();
            }

            if (mapping != null && mapping.PortalEmployeeID.HasValue)
            {
                result = mapping.Employee;
            }

            return result;
        }

        /*
        public override int Update(BambooHREmployeeMapping item, bool isSave = true)
        {
            var p = item.GetType().GetProperties();
            var e = this.Db.Entry(item);
            List<string> s = new List<string>();
            foreach (var x in p)
            {
                if (e.Property(x.Name).IsModified)
                {
                    s.Add(x.Name);
                }
            }

            //int result = this.Update(item, false);
            int result = s.Count;
            return result;

        }
        */

       
    }
}
