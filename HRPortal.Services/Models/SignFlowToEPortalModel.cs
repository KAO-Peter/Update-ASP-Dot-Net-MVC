using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.Models
{
    public class SignFlowToEPortalModel
    {
        public string compcode { get; set; }
        public List<SignFlowToEPortalDetailModel> data { get; set; }
    }

    public class SignFlowToEPortalDetailModel
    {
        public string empno { get; set; }
        public string sysid { get; set; }
        public string docno { get; set; }
        public string sysname { get; set; }
        public string formdesc { get; set; }
        public string sendtime { get; set; }
        public string sendname { get; set; }
        public string appdate { get; set; }
        public string memo { get; set; }
        public string fieldid { get; set; }
        public string fieldpwd { get; set; }
        public string doclink { get; set; }
        public string serverlink { get; set; }
        public string rstate { get; set; }
        public string create_time { get; set; }
        

    }
    public class SignFlowToEPortalResponseModel 
    {
        public string returnCode { get; set; }
        public string message { get; set; }
        public string responsec { get; set; }
    }

    public enum SendFormType
    {
        Added, //新增表單送出
        SignedEnRoute, //中間關卡簽核完成要送下一關
        Approved, //核決
        Rejected, //退回
        Retracted, //自行拉回
        Deleted //自行刪除
    }
}
