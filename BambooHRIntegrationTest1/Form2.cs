using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using HRPortal.Services;
using HRPortal.Services.Models.BambooHR;
using System.Net;
using HRPortal.ApiAdapter;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using ImapX;
using System.Security.Authentication;
using HRPortal.BambooHRIntegration;
using YoungCloud.Configurations.AutofacSettings;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;


namespace BambooHRIntegrationTest1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.comboAbsent.Items.Add(new ComboBoxItem("Annual leave(特休)", "83"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Business trip(公出)", "84"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Deferred annual leave(遞延特休)", "87"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Personal leave(事假)", "85"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Sick leave(病假)", "86"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Marriage leave(婚假)", "89"));
            HRMApiAdapter.GetHostUri = GetHostUri;
            AutofacInitializer.Configure(AutofacInitializer.InitializeRegister);
        }

        private string GetHostUri()
        {
            return "http://localhost:52530/";
        }

        struct ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public ComboBoxItem(string text, string value):this()
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textRequest.Text = "";
            this.textResponse.Text = "";
            string EmpBBID = "114"; //Test Test在BambooHR的Employee ID

            string date1 = this.textDate1.Text;
            string date2 = this.textDate2.Text;

            string absentBBID = ((ComboBoxItem)this.comboAbsent.SelectedItem).Value;
            string amount = this.textAmount.Text;
            string note = this.textNote.Text;

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var client = new RestClient("https://api.bamboohr.com/api/gateway.php/fegdrive/v1/employees/" + EmpBBID + "/time_off/request");

            string BBAPIKey = "80e80bc8e20ceccfbaca13bff9cd08a8f06ab0a9";
            string password = "xxx"; //任意字串都可以
            client.Authenticator = new HttpBasicAuthenticator(BBAPIKey, password);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.PUT;
            request.RequestFormat = DataFormat.Json;

            string status = "requested"; //狀態有requested/denied=declined/approved (另外更新狀態的API有一個canceled)
            List<BambooHR_TimeOffNote> notes = new List<BambooHR_TimeOffNote>();
            notes.Add(new BambooHR_TimeOffNote() { from = "employee", note = this.textNote.Text });
            BambooHR_TimeOffItem item = new BambooHR_TimeOffItem()
                                        {
                                            status = status,
                                            start = date1,
                                            end = string.IsNullOrWhiteSpace(date2) ? date1 : date2,
                                            timeOffTypeId = absentBBID,
                                            amount = amount,
                                            notes = notes

                                        };
            request.AddJsonBody(item);
            Parameter body = request.Parameters.Where(x => x.Type == ParameterType.RequestBody).FirstOrDefault();
            if (body != null)
            {
                this.textRequest.Text = body.Value.ToString();
            }

            IRestResponse response = client.Put(request);
            this.textResponse.Text = string.Format("HTTP Status Code={0} {1}", (int)response.StatusCode, response.StatusCode);
            var objLocation = response.Headers.Where(x => x.Name.ToLower() == "location").Select(y => y.Value).FirstOrDefault();
            string location = "";
            if (objLocation != null)
            {
                location = objLocation.ToString();
            }
            else
            {
                location = "沒有回傳location";
            }
            this.textResponseHeader.Text = location;
        }

        private class BambooHR_TimeOffItem
        {
            public string status { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string timeOffTypeId { get; set; }
            public string amount { get; set; }
            public List<BambooHR_TimeOffNote> notes { get; set; }
        }

        private class BambooHR_TimeOffNote
        {
            public string from { get; set; }
            public string note { get; set; }
        }

        private void buttonFileBrowser_Click(object sender, EventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textFilePath.Text = ofd.FileName;
                //string filePath = ofd.FileName;
                //byte[] b = File.ReadAllBytes(filePath);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string EmpBBID = "114"; //Test Test在BambooHR的Employee ID
            var client = new RestClient("https://api.bamboohr.com/api/gateway.php/fegdrive/v1/employees/" + EmpBBID + "/photo");

            string BBAPIKey = "80e80bc8e20ceccfbaca13bff9cd08a8f06ab0a9";
            string password = "xxx"; //任意字串都可以
            client.Authenticator = new HttpBasicAuthenticator(BBAPIKey, password);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "multipart/form-data");
            request.Method = Method.POST;
            request.RequestFormat = DataFormat.Json;

            string filePath = this.textFilePath.Text;
            byte[] b = File.ReadAllBytes(filePath);

            MemoryStream ms = new MemoryStream(b);

            request.Files.Add(new FileParameter()
            {
                Name = "file",
                Writer = (x) => { ms.CopyTo(x); },
                FileName = "Pic1.jpg",
                ContentLength = ms.Length
            });

            IRestResponse response = client.Execute(request);
            this.textResponse.Text = string.Format("HTTP Status Code={0} {1}", (int)response.StatusCode, response.StatusCode);

            ms.Dispose();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string absentCode = "01"; //特休
            DateTime beginDate = new DateTime(2020, 11, 1);
            DateTime endDate = new DateTime(2020, 11, 30);
            //string typeID=Services.GetService<BambooHRTimeOffTypeService>().GetTimeOffTypeIDByAbsentCode(absentCode)
            TimeOffRequestQuery query = new TimeOffRequestQuery()
            {
                BambooEmployeeID = "114",
                AbsentCode = absentCode,
                BeginDate = beginDate,
                EndDate = endDate

            };

            BambooHRIntegrationService queryService = new BambooHRIntegrationService();
            List<TimeOffRequestQueryResult> result = queryService.API_QueryTimeOffRequests(query);
            this.textResponse.Text = result.Count.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string jsonstr = "{number:1000, str:'bbb', array: [{abc:1},{abc:2},{abc:3},{abc:4},{abc:5},{abc:6}]}";
            //dynamic obj = JObject.Parse(jsonstr);
            dynamic obj = JArray.Parse(jsonstr);
            testJSON t = new testJSON()
            {
                AAA = "測試",
                body = obj
            };
            this.textResponse.Text = JsonConvert.SerializeObject(t);
        }

        public class testJSON
        {
            public string AAA { get; set; }
            public object body { get; set; }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService();
            string result = BambooHRService.SyncBambooHREmployee();

            //this.textResponse.Text = JsonConvert.SerializeObject(result);
            this.textResponse.Text = string.IsNullOrWhiteSpace(result) ? "同步完成" : result;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.textBambooHREmployeeID.Text))
            {
                string currentIP = "";
                foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ip.ToString().StartsWith("10."))
                    {
                        currentIP = ip.ToString();
                    }
                }

                LogInfo info = new LogInfo()
                {
                    UserIP = currentIP,
                    Controller = "Form2_Test",
                    Action = "button6_Click"
                };

                BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService(info);
                string EmpID = BambooHRService.GetBamboooEmployeeNumberByEmployeeID(this.textBambooHREmployeeID.Text);
                this.textResponse.Text = EmpID;

            }
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            this.textResponse.Text = "";
            this.gridLeaveForm.DataSource = null;
            DateTime BeginDate=new DateTime(2020,01,01);
            DateTime EndDate=new DateTime(2020,12,31);
            string CompanyCode="ddmc";
            string empID = this.textImportHistoryEmpID.Text;
            bool includeSigning = this.checkBox1.Checked;

            if (!string.IsNullOrWhiteSpace(empID))
            {
                string currentIP = "";
                foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ip.ToString().StartsWith("10."))
                    {
                        currentIP = ip.ToString();
                    }
                }

                LogInfo info = new LogInfo()
                {
                   UserIP = currentIP,
                   Controller = "Form2_Test",
                   Action = "button7_Click"
                };

                BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService(info);
                bool onlyForViewing = true;
                ImportHistoryResult result = await BambooHRService.ImportHistoryLeaveForm(empID, CompanyCode, BeginDate, EndDate, includeSigning, onlyForViewing);
                this.textResponse.Text = string.IsNullOrWhiteSpace(result.Result) ? "執行完成，共" + result.HistoryFormList.Count.ToString() + "筆資料" : result.Result;
                this.gridLeaveForm.DataSource = result.HistoryFormList;
            }
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            this.textResponse.Text = "";
            this.gridLeaveForm.DataSource = null;
            DateTime BeginDate = new DateTime(2020, 01, 01);
            DateTime EndDate = new DateTime(2020, 12, 31);
            string CompanyCode = "ddmc";
            string empID = this.textImportHistoryEmpID.Text;
            bool includeSigning = this.checkBox1.Checked;

            if (!string.IsNullOrWhiteSpace(empID))
            {
                string currentIP = "";
                foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ip.ToString().StartsWith("10."))
                    {
                        currentIP = ip.ToString();
                    }
                }

                LogInfo info = new LogInfo()
                {
                    UserIP = currentIP,
                    Controller = "Form2_Test",
                    Action = "button8_Click"
                };

                BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService(info);
                bool onlyForViewing = false;
                ImportHistoryResult result = await BambooHRService.ImportHistoryLeaveForm(empID, CompanyCode, BeginDate, EndDate, includeSigning, onlyForViewing);
                this.textResponse.Text = string.IsNullOrWhiteSpace(result.Result) ? "執行完成，共" + result.HistoryFormList.Count.ToString() + "筆資料" : result.Result;
                this.gridLeaveForm.DataSource = result.HistoryFormList;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            /*
            BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService();
            BambooHRService.UpdateTest();
            */
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 80); //連到google的DNS，只是一個dummy的動作，目的是要取得目前用來連結的IP
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                this.textResponse.Text = endPoint.Address.ToString();
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            //測試IMAP
            var client = new ImapX.ImapClient("imap.gmail.com",true);
            if (client.Connect())
            {
                this.textResponse.Text = "Gmail connect OK." + Environment.NewLine;

                //if (client.Login("bamboohrddmc1@gmail.com", "ddmc1qaz@WSX1234"))
                if (client.Login("danielchen.ddmc@gmail.com", "ZopyQQ6Z1234"))
                {
                    this.textResponse.Text += "Gmail login OK." + Environment.NewLine;
                    var folder = client.Folders.Inbox;
                    var messages = folder.Search("UID 979:* SUBJECT \"time off\" ");
                    if (messages.Length > 0)
                    {
                        this.textResponse.Text += "找到" + messages.Length.ToString() + "則郵件" + Environment.NewLine;
                        
                        foreach (var msg in messages)
                        {
                            if (msg.Body.HasHtml)
                            {
                                //this.textResponse.Text += msg.Body.Html + Environment.NewLine;
                            }
                            else if (msg.Body.HasText)
                            {
                                //this.textResponse.Text += msg.Body.Text + Environment.NewLine;
                            }
                            
                        }
                        
                    }
                }
                else
                {
                    this.textResponse.Text = "Gmail login failed.";
                }
            }
            else
            {
                this.textResponse.Text = "Gmail connect failed.";
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            LogInfo info = new LogInfo();
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 80); //連到google的DNS，只是一個dummy的動作，目的是要取得目前用來連結的IP
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                string localIP = endPoint.Address.ToString();

                info.UserIP = localIP;
                info.Controller = "Form2_Test";
                info.Action = "button11_Click";

            }

            BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService(info);
            DateTime begin = DateTime.Now;
            BackgroundServiceResult finalResult;
            string result = "";
            if (!string.IsNullOrWhiteSpace(this.textTimeOffRequestID.Text))
            {
                finalResult = BambooHRService.BambooHRCheckTimeOffStatusAndSync(this.textTimeOffRequestID.Text);
            }
            else
            {
                //string result = BambooHRService.BambooHRCheckAndProcess("1728");
                finalResult = BambooHRService.BambooHRCheckTimeOffStatusAndSync();
            }

            if (finalResult.Success)
            {
                DateTime end = DateTime.Now;
                this.textResponse.Text = "本次檢查假單共花費" + (end - begin).TotalSeconds.ToString() + "秒" + Environment.NewLine + "(" + begin.ToString("yyyy-MM-dd HH:mm:ss") + " ~ " + end.ToString("yyyy-MM-dd HH:mm:ss") + ")"
                    + Environment.NewLine + result;
            }
            else
            {
                this.textResponse.Text = "檢查假單發生異常：" + finalResult.ErrorMessage;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            LogInfo info = new LogInfo();
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 80); //連到google的DNS，只是一個dummy的動作，目的是要取得目前用來連結的IP
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                string localIP = endPoint.Address.ToString();

                info.UserIP = localIP;
                info.Controller = "Form2_Test";
                info.Action = "button11_Click";

            }

            BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService(info);
            var result = BambooHRService.SyncBambooHRUser();
            this.textResponse.Text = string.IsNullOrWhiteSpace(result) ? "更新BambooHR User對應完成" : result;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            LeaveSignList _signList = new LeaveSignList();
            
            var _formData = new LeaveFormData()
                                {
                                    FormNumber = "***", //虛擬表單編號
                                    FormType = FormType.Leave.ToString(),
                                    EmployeeNo = "01422",
                                    DeptCode = "B315000",
                                    CompanyCode = "ddmc",
                                    AbsentCode = "01",
                                    StartTime = DateTime.Now.Date.AddHours(9),
                                    EndTime = DateTime.Now.Date.AddHours(18),
                                    LeaveAmount = 8,
                                    AfterAmount = 0,
                                    IsAbroad = false, //目前BambooHR無法輸入出國與否
                                    AgentNo = "",
                                    WorkHours = 8,
                                    AbsentUnit = "h"
                                };
            _signList._tempFormData = _formData;
            try
            {
                IList<SignFlowRecModel> _signFlow = _signList.GetDefaultSignListWithoutCondition(_formData, true);
                if (_signFlow.Count > 1) //第一筆是申請人
                {
                    string ManagerEmpID = _signFlow[1].SignerID; //取第一關主管
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                int len = errMsg.Length;
            }
        }

    }
    
   
}
