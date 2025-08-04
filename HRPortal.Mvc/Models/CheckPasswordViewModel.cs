using HRPortal.MultiLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class CheckPasswordViewModel
    {
        //[Display(ResourceType = typeof(Resource), Name = "CurrentPassword")]
        [Display(Name ="密碼")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PasswordRequiredError")]
        public string CheckPassword { get; set; }
    }
}
