using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using HRPortal.Services;

namespace BambooHRIntegrationTest1
{
    public class BambooHR
    {
        public static string urlAddTimeOffRequest = "https://api.bamboohr.com/api/gateway.php/{companyDomain}/v1/employees/{employeeId}/time_off/request";

        public string Domain { get; set; }

        protected HRPortal_Services Services;


        public BambooHR(string Domain)
        {
            this.Domain = Domain;
            Services=new HRPortal_Services();
        }

        public IRestResponse Send(RequestItem rItem)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            
            var client = new RestClient(rItem.URL);

            string BBAPIKey = "80e80bc8e20ceccfbaca13bff9cd08a8f06ab0a9";
            string password = "xxx"; //任意字串都可以
            client.Authenticator = new HttpBasicAuthenticator(BBAPIKey, password);

            IRestResponse response = client.Execute(rItem.RestRequest);
            return response;

        }

        public string GetBambooHRTimeOffTypeID(string AbsentCode)
        {
            string timeOffID = Services.GetService<BambooHRTimeOffTypeService>().GetTimeOffTypeIDByAbsentCode(AbsentCode);
            return timeOffID;
        }

        public IRestResponse CreateTimeOffRequest(TimeOffRequest item)
        {
            string url = urlAddTimeOffRequest;
            url = url.Replace("{companyDomain}", Domain);
            url = url.Replace("{employeeId}", item.BambooEmployeeID);
    
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.PUT;
            request.RequestFormat = DataFormat.Json;
            
            List<TimeOffNote> notes = new List<TimeOffNote>();
            notes.Add(new TimeOffNote() { from = "employee", note = item.note });

            TimeOffBody body = new TimeOffBody()
            {
                status = item.status,
                start = item.start,
                end = item.end,
                timeOffTypeId = GetBambooHRTimeOffTypeID(item.AbsentCode),
                amount = item.amount,
                notes = notes
            };

            request.AddJsonBody(body);

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            IRestResponse response = Send(rItem);
            return response;

        }

        /*
        public IRestResponse AlterTimeOffStatus(Guid LeaveFormID)
        { 

        }
        */

        public class RequestItem
        {
            public string URL { get; set; }
            public RestRequest RestRequest{get;set;}
            
        }

        public class TimeOffRequest
        {
            public int EmpData_ID { get; set; }
            public string EmpID { get; set; }
            public string BambooEmployeeID { get; set; }
            public string status { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string AbsentCode { get; set; }
            public string amount { get; set; }
            public string note { get; set; }
        }

        private class TimeOffBody
        {
            public string status { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string timeOffTypeId { get; set; }
            public string amount { get; set; }
            public List<TimeOffNote> notes { get; set; }
        }

        private class TimeOffNote
        {
            public string from { get; set; }
            public string note { get; set; }
        }
    }

    
}
