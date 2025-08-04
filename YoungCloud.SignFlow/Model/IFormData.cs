using System;
namespace YoungCloud.SignFlow.Model
{
    public interface IFormData
    {
        string FormNumber { get; set; }
        string FormType { get; set; }
        string AbsentCode { get; set; }
    }
}
