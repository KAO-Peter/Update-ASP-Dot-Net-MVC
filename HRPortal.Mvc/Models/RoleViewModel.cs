using HRPortal.MultiLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class RoleViewModel
    {
        public Guid ID { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "RoleName")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "IsAdmin")]
        public bool IsAdmin { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "IsHR")]
        public bool IsHR { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "RoleDescription")]
        public string Description { get; set; }
    }
}
