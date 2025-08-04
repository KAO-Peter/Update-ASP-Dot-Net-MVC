using System;

namespace YoungCloud.SignFlow.Model
{
    public class SignFlowRecModel
    {
        public string ID { get; set; }
        public string FormNumber { get; set; }
        public string FormType { get; set; }
        public string FormLevelID { get; set; }
        public string SignOrder { get; set; }
        public string SignStatus { get; set; }
        public string SignType { get; set; }
        public string SignerID { get; set; }
        public string ActSignerID { get; set; }
        public string Instruction { get; set; }
        public Nullable<System.DateTime> SignDate { get; set; }
        public string IsUsed { get; set; }
        public decimal GroupID { get; set; }
        public string CUser { get; set; }
        public System.DateTime CDate { get; set; }
        public string MUser { get; set; }
        public System.DateTime MDate { get; set; }
        public string SenderID { get; set; }
        public string DataState { get; set; }
        public string SignCompanyID { get; set; }
        public string SenderCompanyID { get; set; }
        public string OrgSignerID { get; set; }
    }
}
