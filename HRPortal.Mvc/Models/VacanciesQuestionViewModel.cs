using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class VacanciesQuestionViewModel
    {
        public AnswerVacancies AnswerData { get; set; }

        [Required]
        [Display(Name = "回覆")]
        public string ReplyData { get; set; }
    }
}
