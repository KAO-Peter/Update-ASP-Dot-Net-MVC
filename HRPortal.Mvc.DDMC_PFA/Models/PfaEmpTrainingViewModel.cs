using System;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核員工訓練紀錄
    /// </summary>
    public class PfaEmpTrainingViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid? ID { get; set; }

        /// <summary>
        /// 績效考核員工ID
        /// </summary>
        public Guid? PfaCycleEmpID { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public string CoursesCode { get; set; }

        /// <summary>
        /// 課程名稱
        /// </summary>
        public string CoursesName { get; set; }

        /// <summary>
        /// 時數
        /// </summary>
        public double? TrainingHours { get; set; }
    }
}
