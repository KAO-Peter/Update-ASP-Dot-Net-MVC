using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class ResetPasswordRequestModal
    {
        [DisplayName("公司")]
        public Guid Company { get; set; }

        [DisplayName("工號")]
        [Required]
        public string Account { get; set; }

        [DisplayName("Email")]
        [Required]
        public string Email { get; set; }
    }
}
