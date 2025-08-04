using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaOrgViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public Guid CompanyID { get; set; }

        /// <summary>
        /// 組織代碼
        /// </summary>
        public string PfaOrgCode { get; set; }

        /// <summary>
        /// 組織名稱
        /// </summary>
        public string PfaOrgName { get; set; }

        /// <summary>
        /// 組織主管ID
        /// </summary>
        public Guid? OrgManagerId { get; set; }

        /// <summary>
        /// 組織主管
        /// </summary>
        public string OrgManagerName { get; set; }

        /// <summary>
        /// 適用部門List
        /// </summary>
        public List<Guid> PfaDeptIDList { get; set; }

        /// <summary>
        /// 適用部門
        /// </summary>
        public string PfaDeptName { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }
    }

    public class CreatePfaOrg
    {
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請輸入績效考核組織ID")]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "請輸入公司別")]
        public System.Guid CompanyID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請輸入組織代碼")]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string PfaOrgCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請輸入組織名稱")]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string PfaOrgName { get; set; }

        /// <summary>
        /// 組織主管
        /// </summary>
        public Guid? OrgManagerId { get; set; }

        /// <summary>
        /// 組織主管
        /// </summary>
        public string OrgManagerName { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<SelectListItem> OrgDeptList { get; set; }
    }

    public class SelDeptFunction
    {
        [Required(ErrorMessage = "請輸入績效考核組織ID")]
        public Guid PfaOrgID { get; set; }
        public string PfaOrgCode { get; set; }
        public string PfaOrgName { get; set; }
        public List<DeptItem> DeptItems { get; set; }
    }

    public class DeptItem
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Chk { get; set; }
        public bool Setucompleted { get; set; }
    }
}