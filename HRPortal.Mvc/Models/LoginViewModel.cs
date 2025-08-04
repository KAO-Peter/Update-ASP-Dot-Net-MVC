using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRPortal.Mvc.Models
{
    public class LoginViewModel
    {
        public Guid Company { get; set; }
        [Required]
        public string Account { get; set; }
        [Required]
        public string Password { get; set; }
    }
}