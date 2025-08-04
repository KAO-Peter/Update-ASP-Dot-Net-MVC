using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HRPortal.Models
{
    /// <summary>
    /// 公告
    /// </summary>
    [Table("Announcement")]
    public class Announcement :BaseEntity
    {
        /// <summary>
        /// ID
        /// </summary>
        //public Guid ID { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementTitle")]
        [Required, StringLength(50)]
        public string Title { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementContentext")]
        public string CotentText { get; set; }

        /// <summary>
        /// 起始時間
        /// </summary>
        [Required(ErrorMessage ="必填")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementStartTime")]
        public DateTime AnnounceStartTime { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary> 
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementEndTime")]
        public DateTime? AnnounceEndTime { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementStatus")]
        public bool Status { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        //[Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementCreatedBy")]
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// 修改者
        /// </summary>
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementModifiedBy")]
        public Guid ModifiedBy { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Column(TypeName = "DateTime2")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementModifiedTime")]
        public DateTime? ModifiedTime { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Column(TypeName = "DateTime2")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementCreatedTime")]
        public DateTime CreatedTime { get; set; }
    }
}
