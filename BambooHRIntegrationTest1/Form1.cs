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
using System.IO;

namespace BambooHRIntegrationTest1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.comboAbsent.Items.Add(new ComboBoxItem("Annual leave(特休)", "83"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Business trip(公出)", "84"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Deferred annual leave(遞延特休)", "87"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Personal leave(事假)", "85"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Sick leave(病假)", "86"));
            this.comboAbsent.Items.Add(new ComboBoxItem("Marriage leave(婚假)", "89"));

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
    }
    
   
}
