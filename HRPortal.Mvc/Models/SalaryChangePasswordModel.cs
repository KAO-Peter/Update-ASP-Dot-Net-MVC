using HRPortal.MultiLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class SalaryChangePasswordModel
    {
        [Display(ResourceType = typeof(Resource), Name = "CurrentPassword")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PasswordRequiredError")]
        public string CurrentPassword { get; set; }
        [Remote("SalaryCheckPassword", "Login",
            ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "WrongPassword")]
        public string EncryptedPassword { get; set; }
        [Display(ResourceType = typeof(Resource), Name = "NewPassword")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "NewPasswordRequiredError")]
        public string NewPassword { get; set; }
        [Display(ResourceType = typeof(Resource), Name = "ConfirmPassword")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword",
            ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordError")]
        public string ConfirmPassword { get; set; }
    }
}
