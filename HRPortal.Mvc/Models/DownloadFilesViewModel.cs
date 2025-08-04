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
    public class DownloadFilesViewModel
    {
        public DownloadFilesViewModel()
        {
            Files = new List<HttpPostedFileBase>();
        }
        public DownloadFile DownloadFileData { get; set; }
     
       [DisplayName("檔案上傳")]
        public HttpPostedFileBase FilePath { get; set; }

        public List<HttpPostedFileBase> Files { get; set; }
    }
}
