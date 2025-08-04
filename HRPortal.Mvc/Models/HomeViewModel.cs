using HRPortal.DBEntities;
using HRPortal.MultiLanguage;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class HomeViewModel
    {
        //public IQueryable<Announcement> AnnouncementList { get; set; }
        public IPagedList<Announcement> AnnouncementList { get; set; }
        public List<HomeToDoModel> ToDoList { get; set; }
        public List<HomwLeaveModel> LeaveList { get; set; }
        public List<HomeSalarySearchModel> SalarySearchList { get; set; }
        public List<LeaveApplicationLeaveLinkModel> LeaveLinkList { get; set; } //20161202 請假加班快速連結頁面
        public List<HomeWebLinkModel> WebLinikList { get; set; } //20160301 建立首頁連結頁面 by Bee
        public List<PersonalLeavesSummaryModel> PersonalLeavesSummary { get; set; } //20161107 建立遠百首頁(個人假別時數彙總表) by Bee
        public List<HomeWebLinkFEDSModel> WebLinikListFEDS { get; set; }//20161108 建立快速連結頁面 by Irving
        public ElectronicSignViewModel SmallElectronicSign { get; set; }//20161108 建立首頁打卡畫面 by Bee
        public List<HomeOptionsModel> OptionsList { get; set; }

        /// <summary>個人化設定</summary>
        /// <remarks>20180329 by Frank</remarks>
        public List<PersonalSettings> PersonalSettings { get; set; }
    }

    public class HomeOptionsModel
    {
        public Guid? OptionGroupID { get; set; }
        public string DisplayName { get; set; }
        public string WebName { get; set; }
        public string WebLinke { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUsed { get; set; }
    }

    public class HomeToDoModel
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public int Count { get; set; }
    }

    public class HomeSalarySearchModel
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string EngName { get; set; }
    }

    //遠百 請假加班頁面 請假加班功能快速連結
    public class LeaveApplicationLeaveLinkModel
    {
        public string Name { get; set; }
        public string URL { get; set; }
    }

    public class HomwLeaveModel
    {
        public string AbsentNameEN { get; set; }
        public string SenderEmployeeEnglishName { get; set; }
        public string getLanguageCookie { get; set; }
        public string AbsentName { get; set; }
        public Double AbsentAmount { get; set; }
        public string Unit { get; set; }
        public string Name { get; set; }
        public string EmployeeNo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DepartmentName { get; set; }
    }

    public class HomeWebLinkModel //20160301 建立首頁連結頁面 by Bee
    {
        public Guid ID { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "Number")]
        public int? Number { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "WebName")]
        public string Name { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "WebLink")]
        public string URL { get; set; }

        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "WebLinkListStatus")]
        public bool IsUsed { get; set; }
    }

    public class HomeWebLinkFEDSModel //20161108 建立快速連結頁面 by Irving
    {
        public IEnumerable<SelectListItem> webLists { get; set; }
        public Guid ID { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "CurrentOptionGroupKey")]
        public string CurrentOptionGroupKey { get; set; }

        public string OptionName { get; set; }
        public Guid? OptionID { get; set; }

        [Display(ResourceType = typeof(Resource), Name = "Number")]
        public int? Number { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "WebName")]
        public string Name { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = "WebLink")]
        public string URL { get; set; }

        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "WebLinkListStatus")]
        public bool IsUsed { get; set; }
    }

    public class PersonalLeavesSummaryModel //20161107 遠百首頁(個人假別時數彙總表) by Bee
    {
        public string getLanguageCookie { get; set; }
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string AbsentNameEn { get; set; }
        public decimal AnnualLeaveHours { get; set; }
        public decimal ApprovedHours { get; set; }
        public decimal LeaveHours { get; set; }
        public decimal UseAmount { get; set; }

        //單位 H/D
        public string Unit { get; set; }
        public bool CanUse { get; set; }
        public string Remark { get; set; }
    }
}