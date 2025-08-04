using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public enum AnnouncementStatuss : int
    {
        Hide = 0,
        Show = 1,
        All = 2
    }
    public class EmployeeBenefitsViewModel
    {
        public EmployeeBenefits AnnouncementData { get; set; }
        public bool IsShowEndTime { get; set; }
        public List<EmployeeBenefits> AnnouncementList { get; set; }

        public IEnumerable<SelectListItem> FilesLists { get; set; }

        public string FileGuid { get; set; }
        public string FilePath { get; set; }
        public string FileDescription { get; set; }

    }
}
