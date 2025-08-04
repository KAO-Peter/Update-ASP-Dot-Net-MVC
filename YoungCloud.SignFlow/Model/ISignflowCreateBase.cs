using System;
namespace YoungCloud.SignFlow.Model
{
    public interface ISignflowCreateBase
    {
        string CUser { get; set; }
        string DeptID { get; set; }
        string FormNumber { get; set; }
        string FormType { get; set; }
        string Negotiator { get; set; }
    }
}
