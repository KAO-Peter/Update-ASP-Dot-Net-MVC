using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HRPortal.Mvc.Models
{
    public enum PatchCardFormStatus : int
    {
        Writing = 0,
        Waiting = 1,
        Reject = 2
    }
    public enum ClockInReasonType: int
    {
        Normal = 0,
        ForgotCreditCard = 1,
        ForgotBringCard = 2
    }
    
    public enum PatchCardFormType : int
    {
        StartWork = 1,
        EndWork = 2
    }
    public class PatchCardFormViewModel
    {
        public PatchCardForm FormData { get; set; }
        public string getLanguageCookie { get; set; }
        
        [DisplayName("附件檔案")]
        public HttpPostedFileBase UploadFilePath { get; set; }

    }
}
