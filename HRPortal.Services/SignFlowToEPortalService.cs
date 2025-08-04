using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System.Configuration;
using Newtonsoft.Json;

namespace HRPortal.Services
{
    public class SignFlowToEPortalService:BaseCrudService<Company>
    {
        public SignFlowToEPortalService(HRPortal_Services services)
            : base(services)
        {
        }

        public string GenerateEncrytedData(AESKey key, string EmpID, int Company_ID = 1)
        {
            string randomString = CryptoService.GetRandomString(5);
            //傳入ePortal簽核箱FIELDPWD欄位資料內容為 亂數字串-工號-公司ID-日期
            string target = randomString + "-" + EmpID + "-" + Company_ID.ToString() + "-" + DateTime.Now.ToString("yyyyMMdd");
            string result = CryptoService.EncryptString(key, target);
            return result;
        }

        public SignFlowToEPortalDetailModel PrepareModel(string updateMode, SignFlowToEPortalDetailModel detailModel)
        {
            const string SysID = "C00E1"; //HRPortal系統代號
           
            const string SysName = "鼎鼎請假加班系統"; //系統名稱
            //const string RedirectURL = "https://ddmchr.feg.com.tw/SSOLogin/ToSign"; //待簽核頁面直接登入網址
            string RedirectURL = Services.GetService<SystemSettingService>().GetSettingValue("ePortalRedirectURL");
            //const string RedirectURL = "http://10.2.2.30:8888/HRPortal/SSOLogin/ToSign"; //待簽核頁面直接登入網址


            SignFlowToEPortalDetailModel model = new SignFlowToEPortalDetailModel();

            //新增或搬移(移除)都需要設定底下這三個屬性
            model.sysid = SysID;
            model.docno = detailModel.docno;
            model.rstate = updateMode;

            if (updateMode == "A") //新增需要多設定其他屬性
            {
                string AESKeyString = ConfigurationManager.AppSettings["URLEncryptKey"];
                string[] keySection = AESKeyString.Split('-'); //Config一定會有值，不再檢查
                AESKey aesKey = new AESKey();
                aesKey.Key = CryptoService.HexToByteArray(keySection[0]); //AES Key
                aesKey.IV = CryptoService.HexToByteArray(keySection[1]); //AES IV

                model.empno = detailModel.empno;
                model.sysname = SysName;
                model.formdesc = detailModel.formdesc;
                model.sendtime = detailModel.sendtime;
                model.sendname = detailModel.sendname;
                model.appdate = detailModel.appdate;
                model.memo = (detailModel.memo == null) ? "" : detailModel.memo;
                model.fieldid = "ES";
                model.fieldpwd = GenerateEncrytedData(aesKey, detailModel.empno);
                model.doclink = RedirectURL;
                model.serverlink = RedirectURL;
                model.create_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
               
            }
            
            return model;

        }

        //依據表單送簽狀況，更新ePortal
        public string UpdateEPortal(SendFormType ActionType,SignFlowToEPortalDetailModel data,Guid User_ID,string IP,string ControllerName,string ActionName)
        {
          
            List<SignFlowToEPortalDetailModel> listFlowData=new List<SignFlowToEPortalDetailModel>();

            //如果是簽核中間的關卡要送下一關，或是已經核決，退回或自行拉回，都需要先將簽核箱內的表單搬走(移除)
            if (ActionType == SendFormType.SignedEnRoute || ActionType == SendFormType.Approved || ActionType == SendFormType.Rejected || ActionType==SendFormType.Retracted)
            {
                listFlowData.Add(PrepareModel("M", data));
            }

            //如果是新增表單，或是簽核中間的關卡要送下一關，此處都要新增一筆到簽核箱，直接用傳入的data物件就可以
            if (ActionType == SendFormType.Added || ActionType==SendFormType.SignedEnRoute)
            {
                listFlowData.Add(PrepareModel("A",data));
            }

            string result = "";

            if (listFlowData.Count > 0) //有資料才發送
            {
                SignFlowToEPortalModel modelFlow = new SignFlowToEPortalModel()
                {
                    compcode = "DDMC",
                    data = listFlowData
                };

                result = Send(JsonConvert.SerializeObject(modelFlow), User_ID, IP, ControllerName, ActionName);              
            }
            return result;
        }

        //實際發送，透過ePortal Web Service處理，並將發送資料與結果記錄到SystemLog
        public string Send(string Data,Guid User_ID,string IP,string ControllerName,string ActionName)
        {
            string result = "";
            try
            {
                EPORTALWebService.Service WS = new EPORTALWebService.Service();
                result = WS.UpdateSignBox(Data);
                
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            var logService = Services.GetService<SystemLogService>();
            logService.WriteLog(User_ID, IP, ControllerName, ActionName, "SendToEPortal：result=" + result + "，data=" + Data);

            return result;
        }

    }
}
