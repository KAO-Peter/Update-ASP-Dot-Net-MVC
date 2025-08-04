using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities;
using RestSharp;
using Newtonsoft.Json;

namespace HRPortal.Services
{
    public class SmartSheetrIntegrationService
    {
        const decimal WorkhoursPerDay = 8;
        public SmartSheetSetting Setting { get; private set; }
        protected HRPortal_Services Services;

        public SmartSheetrIntegrationService()
        {
            Services = new HRPortal_Services();
            string BaseURL = Services.GetService<SystemSettingService>().GetSettingValue("SmartSheet_BaseURL");
            string AccessToken = Services.GetService<SystemSettingService>().GetSettingValue("SmartSheet_AccessToken");

            BaseURL = string.IsNullOrWhiteSpace(BaseURL) ? "https://api.smartsheet.com" : BaseURL;
            BaseURL = BaseURL.Substring(BaseURL.Length - 1) == "/" ? BaseURL.Substring(0, BaseURL.Length - 1) : BaseURL; //移除最後的斜線
            SmartSheetSetting setting = new SmartSheetSetting() { BaseURL = BaseURL, AccessToken = AccessToken };
            this.Setting = setting;
        }

        public string Create(LeaveForm FormData, string AbsentName,string AbsentEngName, decimal AbsentQuota)
        {
            string result = "";
            string SheetID = Services.GetService<SystemSettingService>().GetSettingValue("SmartSheet_LeaveSheetID");
            LeaveDataRow row = new LeaveDataRow()
            {
                FormNo = FormData.FormNo,
                Status = 1, //新增目前一定是申請的時候，所以固定為1
                EmpID = FormData.Employee.EmployeeNO,
                EmpName = FormData.Employee.EmployeeName,
                EmpEngName = FormData.Employee.EmployeeEnglishName,
                BeginTime = FormData.StartTime,
                EndTime = FormData.EndTime,
                AbsentName = AbsentName,
                AbsentEngName = AbsentEngName,
                AbsentUnit = FormData.AbsentUnit,
                LeaveAmount = FormData.LeaveAmount,
                TotalLeaveAmount = AbsentQuota,
                RemainingLeaveAmount = FormData.AfterAmount,
                Comments = FormData.LeaveReason
            };

            try
            {
                result = AddSmartSheetRow(SheetID, row, FormData.ID);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;

        }

        public string Delete(string FormNo, Guid LeaveFormID)
        {
            string result = "";
            string SheetID = Services.GetService<SystemSettingService>().GetSettingValue("SmartSheet_LeaveSheetID");
            try
            {
                //取得對應RowID
                string rowID = Services.GetService<SmartSheetMappingService>().GetRowIDByLeaveFormID(LeaveFormID);

                result = DeleteSmartSheetRow(SheetID, rowID, FormNo, LeaveFormID);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        public string Update(string FormNo, Guid LeaveFormID, int Status)
        {
            string result = "";
            string SheetID = Services.GetService<SystemSettingService>().GetSettingValue("SmartSheet_LeaveSheetID");
            try
            {
                //取得對應RowID
                string rowID = Services.GetService<SmartSheetMappingService>().GetRowIDByLeaveFormID(LeaveFormID);

                result = UpdateSmartSheetRow(SheetID, rowID, Status, FormNo);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        private string AddSmartSheetRow(string SheetID, LeaveDataRow Row,Guid LeaveFormID)
        {

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            //var client = new RestClient("https://api.smartsheet.com/2.0/sheets/8499131585980292/rows");
            //client.AddDefaultHeader("Authorization", "Bearer sepmhry9ghhg00zedue6p1b1ku");

            string smartSheetURL = this.Setting.BaseURL + @"/2.0/sheets/" + SheetID + @"/rows";
            var client = new RestClient(smartSheetURL);

            string authorizationString = "Bearer " + this.Setting.AccessToken;
            client.AddDefaultHeader("Authorization", authorizationString);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.POST;
            request.RequestFormat = DataFormat.Json;

            string status = Row.Status == 1 ? "Submitted" : (Row.Status == 3 ? "Approved" : "");

            string durationDays, durationHours, totalLeaveDays, remainingLeaveDays;
            if (Row.AbsentUnit == "h")
            {
                durationDays = (Row.LeaveAmount / WorkhoursPerDay).ToString("###0.#####");
                durationHours = Row.LeaveAmount.ToString("###0.#####");
                totalLeaveDays = (Row.TotalLeaveAmount / WorkhoursPerDay).ToString("###0.#####");
                remainingLeaveDays = (Row.RemainingLeaveAmount / WorkhoursPerDay).ToString("###0.#####");
            }
            else
            {
                durationDays = Row.LeaveAmount.ToString("###0.#####");
                durationHours = (Row.LeaveAmount * WorkhoursPerDay).ToString("###0.#####");
                totalLeaveDays = Row.TotalLeaveAmount.ToString("###0.#####");
                remainingLeaveDays = Row.RemainingLeaveAmount.ToString("###0.#####");
            }

            durationDays += "d";
            durationHours += "h";
            //totalLeaveDays += "d";
            //remainingLeaveDays += "d";

            var row = new SmartSheetRowAdd();
            var columns = new List<SmartSheetColumn>();
            /*
            //測試環境
            columns.Add(new SmartSheetColumn() { columnId = "842740637427588", value = status }); //Status
            columns.Add(new SmartSheetColumn() { columnId = "5346340264798084", value = Row.EmpEngName }); //Requester
            columns.Add(new SmartSheetColumn() { columnId = "1470286898980740", value = Row.EmpID }); //Employee ID
            columns.Add(new SmartSheetColumn() { columnId = "7598140078483332", value = Row.BeginTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") }); //Start Date
            columns.Add(new SmartSheetColumn() { columnId = "1968640544270212", value = Row.EndTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") }); //End Date
            columns.Add(new SmartSheetColumn() { columnId = "6472240171640708", value = durationDays }); //Duration
            columns.Add(new SmartSheetColumn() { columnId = "2096692041410436", value = durationHours }); //Duration_Calc
            columns.Add(new SmartSheetColumn() { columnId = "4220440357955460", value = Row.AbsentEngName }); //Leave Type
            columns.Add(new SmartSheetColumn() { columnId = "8724039985325956", value = totalLeaveDays }); //Total Leave Days
            columns.Add(new SmartSheetColumn() { columnId = "561265660716932", value = remainingLeaveDays }); //Remaining Leave Days for the Year
            columns.Add(new SmartSheetColumn() { columnId = "5064865288087428", value = Row.Comments }); //Comments
            */

            //正式環境
            columns.Add(new SmartSheetColumn() { columnId = "1831236755842948", value = status }); //Status
            columns.Add(new SmartSheetColumn() { columnId = "4083036569528196", value = Row.EmpEngName }); //Requester
            columns.Add(new SmartSheetColumn() { columnId = "8586636196898692", value = Row.EmpID }); //Employee ID
            columns.Add(new SmartSheetColumn() { columnId = "423861872289668", value = Row.BeginTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") }); //Start Date
            columns.Add(new SmartSheetColumn() { columnId = "4927461499660164", value = Row.EndTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") }); //End Date
            columns.Add(new SmartSheetColumn() { columnId = "2675661685974916", value = durationDays }); //Duration
            columns.Add(new SmartSheetColumn() { columnId = "7179261313345412", value = durationHours }); //Duration_Calc
            columns.Add(new SmartSheetColumn() { columnId = "1549761779132292", value = Row.AbsentEngName }); //Leave Type
            columns.Add(new SmartSheetColumn() { columnId = "6053361406502788", value = totalLeaveDays }); //Total Leave Days
            columns.Add(new SmartSheetColumn() { columnId = "3801561592817540", value = remainingLeaveDays }); //Remaining Leave Days for the Year
            columns.Add(new SmartSheetColumn() { columnId = "8305161220188036", value = Row.Comments }); //Comments
            

            row.cells = columns;

            request.AddJsonBody(row);
            //textBox1.Text = JsonConvert.SerializeObject(row);

            var response = client.Post(request);
            client = null;

            string result = "";
            if (string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                dynamic retObj = JsonConvert.DeserializeObject(response.Content);
                if (retObj.message == "SUCCESS")
                {
                    //取得RowID
                    string rowID = retObj.result.id;

                    SmartSheetMapping data = new SmartSheetMapping()
                    {
                        ID = Guid.NewGuid(),
                        LeaveFormID = LeaveFormID,
                        RowID = rowID
                    };
                    int retValue = Services.GetService<SmartSheetMappingService>().Create(data);

                }
                result = "LeaveForm FormNo=" + Row.FormNo + "，ResponseStatus=" + response.ResponseStatus.ToString() + 
                "，REQUEST=" + JsonConvert.SerializeObject(row) + "，RESPONSE=" + response.Content;
            }
            else
            {
                result = "呼叫REST API發生錯誤，" + response.ErrorMessage;
            }

            //回傳交易訊息
            return result;
        }

        private string DeleteSmartSheetRow(string SheetID, string RowID, string FormNo, Guid LeaveFormID)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            //var client = new RestClient("https://api.smartsheet.com/2.0/sheets/8499131585980292/rows);
            //client.AddDefaultHeader("Authorization", "Bearer sepmhry9ghhg00zedue6p1b1ku");

            string smartSheetURL = this.Setting.BaseURL + @"/2.0/sheets/" + SheetID + @"/rows";
            smartSheetURL += @"?ids=" + RowID;

            var client = new RestClient(smartSheetURL);

            string authorizationString = "Bearer " + this.Setting.AccessToken;
            client.AddDefaultHeader("Authorization", authorizationString);

            var request = new RestRequest();
            request.Method = Method.DELETE;

            var response = client.Delete(request);
            client = null;

            //不論是否成功，都要刪除SmartSheetMapping資料
            int retValue = Services.GetService<SmartSheetMappingService>().DeleteByLeaveFormID(LeaveFormID);

            string result = "";
            if (string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                result = "LeaveForm FormNo=" + FormNo + "，ResponseStatus=" + response.ResponseStatus.ToString() + 
                    "，REQUEST=" + smartSheetURL + "，RESPONSE=" + response.Content;
            }
            else
            {
                result = "呼叫REST API發生錯誤，" + response.ErrorMessage;
            }

            //回傳交易訊息
            return result;

        }

        //更新之前的請假狀態
        private string UpdateSmartSheetRow(string SheetID, string RowID, int SubmitStatus, string FormNo)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            //var client = new RestClient("https://api.smartsheet.com/2.0/sheets/8499131585980292/rows?ids=7792596841457540");
            //client.AddDefaultHeader("Authorization", "Bearer sepmhry9ghhg00zedue6p1b1ku");

            string smartSheetURL = this.Setting.BaseURL + @"/2.0/sheets/" + SheetID + @"/rows";
            smartSheetURL += @"?ids=" + RowID;

            var client = new RestClient(smartSheetURL);

            string authorizationString = "Bearer " + this.Setting.AccessToken;
            client.AddDefaultHeader("Authorization", authorizationString);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.PUT;
            request.RequestFormat = DataFormat.Json;

            string status = SubmitStatus == 1 ? "Submitted" : (SubmitStatus == 3 ? "Approved" : "");

            var row = new SmartSheetRowUpdate();
            var columns = new List<SmartSheetColumn>();
            //columns.Add(new SmartSheetColumn() { columnId = "842740637427588", value = status }); //Status //測試環境
            columns.Add(new SmartSheetColumn() { columnId = "1831236755842948", value = status }); //Status //正式環境

            row.id = RowID;
            row.cells = columns;

            request.AddJsonBody(row);
            //textBox1.Text = JsonConvert.SerializeObject(row);

            var response = client.Put(request);
            client = null;

            string result = "";
            if (string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                result = "LeaveForm FormNo=" + FormNo + "，ResponseStatus=" + response.ResponseStatus.ToString() + 
                    "，REQUEST=" + JsonConvert.SerializeObject(row) + "，RESPONSE=" + response.Content;
            }
            else
            {
                result = "呼叫REST API發生錯誤，" + response.ErrorMessage;
            }

            //回傳交易訊息
            return result;
        }

    }

    public class SmartSheetColumn
    {
        public string columnId { get; set; }
        public string value { get; set; }
    }

    public class SmartSheetRowAdd
    {
        public List<SmartSheetColumn> cells { get; set; }
    }

    public class SmartSheetRowUpdate
    {
        public string id { get; set; }
        public List<SmartSheetColumn> cells { get; set; }
    }

    public class LeaveDataRow
    {
        public string FormNo { get; set; }
        public int Status { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string EmpEngName { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public string AbsentName { get; set; }
        public string AbsentEngName { get; set; }
        public string AbsentUnit { get; set; }
        public decimal LeaveAmount { get; set; }
        public decimal TotalLeaveAmount { get; set; }
        public decimal RemainingLeaveAmount { get; set; }
        public string Comments { get; set; }

    }

    public class SmartSheetSetting
    {
        public string BaseURL { get; set; }
        public string AccessToken { get; set; }
    }

    

}
