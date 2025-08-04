using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class VacanciesViewModel
    {
        public List<Vacancies> FAQLists { get; set; }
        public Vacancies Data { get; set; }
        public List<VacanciesType> FAQTypeLists { get; set; }

        public string OtherType { get; set; }
    }
}
