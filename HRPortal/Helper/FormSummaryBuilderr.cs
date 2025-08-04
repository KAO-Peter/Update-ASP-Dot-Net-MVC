using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.MultiLanguage;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;

namespace HRPortal.Helper
{
    public class FormSummaryBuilderr
    {
        private HRPortal_Services _services;
        //List<AbsentType> _absent;
      
        //public FormSummaryBuilderr(List<AbsentType> absent)
        //{
        //    _services = new HRPortal_Services();
        //    _absent = absent;
        //}
        List<AbsentDetail> _absent;
        List<AbsentDetail> _absentAll;
        public FormSummaryBuilderr(List<AbsentDetail> absent, List<AbsentDetail> absentAll)
        {
            _services = new HRPortal_Services();
            _absent = absent;
            _absentAll = absentAll;
        }

        //20170510 Start 增加 by Daniel
        //過濾假別，如果有傳入假別，假別代碼不一樣的就將移除標記設定為true，之後要移掉
        public bool BuildSummaryForFormQuery(HRPotralFormSignStatus signFlowStatus, string AbsentCode)
        {
            bool flagRemove = false;
            string[] sArray;// item.Summary.Split(new char[2] { '(', ')' });
            switch (signFlowStatus.FormType)
            {
                case FormType.Leave:
                    //20181129 Daniel 增加銷假狀態判斷，增加備註文字
                    LeaveForm _leaveForm = _services.GetService<LeaveFormService>().Where(x => x.FormNo == signFlowStatus.FormNo).Include("LeaveCancels").FirstOrDefault();                    
                    signFlowStatus.FormSummary = BuildSummary(_leaveForm, signFlowStatus.FormType);
                    sArray = signFlowStatus.FormSummary.Split(new char[2] { '(', ')' });
                    signFlowStatus.FormSummary = signFlowStatus.FormSummary;//sArray[1];
                    signFlowStatus.AbsentName = sArray[0];
                    signFlowStatus.BeDate = _leaveForm.StartTime;
                    flagRemove = !(string.IsNullOrWhiteSpace(AbsentCode) || _leaveForm.AbsentCode == AbsentCode);
                    break;
                case FormType.OverTime:
                    OverTimeForm _overTimeForm = _services.GetService<OverTimeFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_overTimeForm, signFlowStatus.FormType);
                    sArray = signFlowStatus.FormSummary.Split(new char[2] { '(', ')' });
                    signFlowStatus.FormSummary = signFlowStatus.FormSummary;//sArray[1];
                    signFlowStatus.AbsentName = sArray[0];
                    signFlowStatus.BeDate = _overTimeForm.StartTime;
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = _services.GetService<PatchCardFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_patchCardForm, signFlowStatus.FormType);
                    sArray = signFlowStatus.FormSummary.Split(new char[2] { '(', ')' });
                    signFlowStatus.FormSummary = signFlowStatus.FormSummary;//sArray[1];
                    signFlowStatus.AbsentName = sArray[0];
                    signFlowStatus.BeDate = _patchCardForm.PatchCardTime;
                    break;
                case FormType.LeaveCancel:
                    LeaveCancel _leaveCancel = _services.GetService<LeaveCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = Resource.LeaveCancel + " - " + BuildSummary(_leaveCancel.LeaveForm, FormType.Leave, false);
                    sArray = signFlowStatus.FormSummary.Split(new char[2] { '(', ')' });
                    signFlowStatus.FormSummary = signFlowStatus.FormSummary;//sArray[1];
                    signFlowStatus.AbsentName = sArray[0];
                    signFlowStatus.BeDate = _leaveCancel.LeaveForm.StartTime;
                    break;
                case FormType.OverTimeCancel:
                    OverTimeCancel _overTimeCancel = _services.GetService<OverTimeCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_overTimeCancel.OverTimeForm, FormType.OverTime).Replace(Resource.OverTime, Resource.OverTimeCancel);
                    sArray = signFlowStatus.FormSummary.Split(new char[2] { '(', ')' });
                    signFlowStatus.FormSummary = signFlowStatus.FormSummary;//sArray[1];
                    signFlowStatus.AbsentName = sArray[0];
                    signFlowStatus.BeDate = _overTimeCancel.OverTimeForm.StartTime;
                    break;
                case FormType.PatchCardCancel:
                    PatchCardCancel _patchCardCancel = _services.GetService<PatchCardCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_patchCardCancel.PatchCardForm, FormType.PatchCardCancel).Replace(Resource.PatchCard, Resource.PatchCardCancel);
                    sArray = signFlowStatus.FormSummary.Split(new char[2] { '(', ')' });
                    signFlowStatus.FormSummary = signFlowStatus.FormSummary;//sArray[1];
                    signFlowStatus.AbsentName = sArray[0];
                    signFlowStatus.BeDate = _patchCardCancel.PatchCardForm.PatchCardTime;
                    break;
                default:
                    break;
            }

            return flagRemove;
        }
        //20170510 End

        public void BuildSummary(HRPotralFormSignStatus signFlowStatus)
        {
            switch (signFlowStatus.FormType)
            {
                case FormType.Leave:
                    //20181129 Daniel 增加銷假狀態判斷，增加備註文字
                    LeaveForm _leaveForm = _services.GetService<LeaveFormService>().Where(x => x.FormNo == signFlowStatus.FormNo).Include("LeaveCancels").FirstOrDefault();
                    
                    signFlowStatus.FormSummary = BuildSummary(_leaveForm, signFlowStatus.FormType);
                    var tmpLeave = BuildData(_leaveForm, signFlowStatus.FormType);
                    signFlowStatus.AbsentCode = tmpLeave.AbsentCode;
                    signFlowStatus.AbsentName = tmpLeave.AbsentName;
                    signFlowStatus.AbsentUnit = tmpLeave.AbsentUnit;
                    signFlowStatus.Amount = tmpLeave.Amount;
                    signFlowStatus.StartTime = tmpLeave.StartTime;
                    signFlowStatus.EndTime = tmpLeave.EndTime;
                    signFlowStatus.AbsentEnglishName = tmpLeave.AbsentEnglishName;
                    break;
                case FormType.OverTime:
                    OverTimeForm _overTimeForm = _services.GetService<OverTimeFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_overTimeForm, signFlowStatus.FormType);
                    var tmpOverTime = BuildData(_overTimeForm, signFlowStatus.FormType);
                    signFlowStatus.AbsentCode = tmpOverTime.AbsentCode;
                    signFlowStatus.AbsentName = tmpOverTime.AbsentName;
                    signFlowStatus.AbsentUnit = tmpOverTime.AbsentUnit;
                    signFlowStatus.Amount = tmpOverTime.Amount;
                    signFlowStatus.StartTime = tmpOverTime.StartTime;
                    signFlowStatus.EndTime = tmpOverTime.EndTime;
                    signFlowStatus.AbsentEnglishName = tmpOverTime.AbsentEnglishName;
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = _services.GetService<PatchCardFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_patchCardForm, signFlowStatus.FormType);
                    var tmpPatchCard = BuildData(_patchCardForm, signFlowStatus.FormType);
                    signFlowStatus.AbsentCode = tmpPatchCard.AbsentCode;
                    signFlowStatus.AbsentName = tmpPatchCard.AbsentName;
                    signFlowStatus.AbsentUnit = tmpPatchCard.AbsentUnit;
                    signFlowStatus.Amount = tmpPatchCard.Amount;
                    signFlowStatus.StartTime = tmpPatchCard.StartTime;
                    signFlowStatus.EndTime = tmpPatchCard.EndTime;
                    signFlowStatus.AbsentEnglishName = tmpPatchCard.AbsentEnglishName;
                    break;
                case FormType.LeaveCancel:
                    LeaveCancel _leaveCancel = _services.GetService<LeaveCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = Resource.LeaveCancel + " - " + BuildSummary(_leaveCancel.LeaveForm, FormType.Leave, false);
                    var tmpLeaveCancel = BuildData(_leaveCancel.LeaveForm, FormType.Leave);
                    signFlowStatus.AbsentCode = Resource.LeaveCancel;
                    signFlowStatus.AbsentName = tmpLeaveCancel.AbsentName;
                    signFlowStatus.AbsentUnit = tmpLeaveCancel.AbsentUnit;
                    signFlowStatus.Amount = tmpLeaveCancel.Amount * -1;
                    signFlowStatus.StartTime = tmpLeaveCancel.StartTime;
                    signFlowStatus.EndTime = tmpLeaveCancel.EndTime;
                    signFlowStatus.AbsentEnglishName = tmpLeaveCancel.AbsentEnglishName;
                    break;
                case FormType.OverTimeCancel:
                    OverTimeCancel _overTimeCancel = _services.GetService<OverTimeCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_overTimeCancel.OverTimeForm, FormType.OverTime).Replace(Resource.OverTime, Resource.OverTimeCancel);
                    var tmpOTC = BuildData(_overTimeCancel.OverTimeForm, FormType.OverTime);
                    signFlowStatus.AbsentCode = tmpOTC.AbsentCode;
                    signFlowStatus.AbsentName = tmpOTC.AbsentName;
                    signFlowStatus.AbsentUnit = tmpOTC.AbsentUnit;
                    signFlowStatus.Amount = tmpOTC.Amount;
                    signFlowStatus.StartTime = tmpOTC.StartTime;
                    signFlowStatus.EndTime = tmpOTC.EndTime;
                    signFlowStatus.AbsentEnglishName = tmpOTC.AbsentEnglishName;
                    break;
                case FormType.PatchCardCancel:
                    PatchCardCancel _patchCardCancel = _services.GetService<PatchCardCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_patchCardCancel.PatchCardForm, FormType.PatchCardCancel).Replace(Resource.PatchCard, Resource.PatchCardCancel);
                    var tmpPCC = BuildData(_patchCardCancel.PatchCardForm, FormType.PatchCardCancel);
                       signFlowStatus.AbsentCode = tmpPCC.AbsentCode;
                       signFlowStatus.AbsentName = tmpPCC.AbsentName;
                       signFlowStatus.AbsentUnit = tmpPCC.AbsentUnit;
                       signFlowStatus.Amount = tmpPCC.Amount;
                       signFlowStatus.StartTime = tmpPCC.StartTime;
                       signFlowStatus.EndTime = tmpPCC.EndTime;
                       signFlowStatus.AbsentEnglishName = tmpPCC.AbsentEnglishName;
                    break;
                default:
                    break;
            }
        }

        //20180322 Daniel 待簽核清單頁面欄位調整，表單摘要需拆解為假別、時數、及起訖區間
        public FormSummaryDetailData BuildSummaryForSign(HRPotralFormSignStatus signFlowStatus)
        {
            FormSummaryDetailData result = new FormSummaryDetailData();

            switch (signFlowStatus.FormType)
            {
                case FormType.Leave:
                    LeaveForm _leaveForm = _services.GetService<LeaveFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    result = BuildSummaryForSign(_leaveForm, signFlowStatus.FormType);
                    break;
                case FormType.OverTime:
                    OverTimeForm _overTimeForm = _services.GetService<OverTimeFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    result = BuildSummaryForSign(_overTimeForm, signFlowStatus.FormType);
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = _services.GetService<PatchCardFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    result = BuildSummaryForSign(_patchCardForm, signFlowStatus.FormType);
                    break;
                case FormType.LeaveCancel:
                    LeaveCancel _leaveCancel = _services.GetService<LeaveCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    result = BuildSummaryForSign(_leaveCancel.LeaveForm, FormType.Leave);
                    result.Description += (signFlowStatus.getLanguageCookie == "en-US" ? " " : "") + HRPortal.MultiLanguage.Resource.Text_LeaveCancellation;
                    break;
                case FormType.OverTimeCancel:
                    OverTimeCancel _overTimeCancel = _services.GetService<OverTimeCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    result = BuildSummaryForSign(_overTimeCancel.OverTimeForm, FormType.OverTime);
                    result.Description += (signFlowStatus.getLanguageCookie == "en-US" ? " " : "") + HRPortal.MultiLanguage.Resource.Text_FormCancellation;
                    break;
                case FormType.PatchCardCancel:
                    PatchCardCancel _patchCardCancel = _services.GetService<PatchCardCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    result = BuildSummaryForSign(_patchCardCancel.PatchCardForm, FormType.PatchCardCancel);
                    result.Description += (signFlowStatus.getLanguageCookie == "en-US" ? " " : "") + HRPortal.MultiLanguage.Resource.Text_FormCancellation;
                    break;
                default:
                    break;

            }
            return result;
        }

        public string BuildSummary(object form)
        {
            Type _formType = form.GetType();

            if (_formType == typeof(LeaveForm))
            {
                return BuildSummary(form, FormType.Leave);
            }
            else if (_formType == typeof(OverTimeForm))
            {
                return BuildSummary(form, FormType.OverTime);
            }
            else if (_formType == typeof(PatchCardForm))
            {
                return BuildSummary(form, FormType.PatchCard);
            }

            return string.Empty;
        }

        public string BuildSummary(object form, FormType formType, bool includeCancelInfo=true)
        {

            switch (formType)
            {
                case FormType.Leave:
                    LeaveForm _leaveForm = form as LeaveForm;
                    string getLanguageCookie = HttpContext.Current.Request.Cookies["lang"] != null ? HttpContext.Current.Request.Cookies["lang"].Value : "zh-TW";
                    string AbsentName = null;
                    string AbsentNameEn = null;
                    foreach (var i in _absent)
                    {
                        if (i.Code == _leaveForm.AbsentCode)
                        {
                            AbsentName = i.Name;
                            AbsentNameEn = i.AbsentNameEn;
                        }
                    }

                    string result = string.Format("{0}({1}~{2} {3}{4})",
                             getLanguageCookie == "en-US" ? AbsentNameEn : AbsentName,
                            _leaveForm.StartTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.LeaveAmount.ToString("0.#"),
                            _leaveForm.AbsentUnit == "h" ? Resource.Hour : Resource.Day);

                    //20181129 Daniel 增加銷假狀態判斷，增加備註文字
                    if (includeCancelInfo)
                    {
                        if (_leaveForm.LeaveCancels.Count > 0)
                        {
                            int status = _leaveForm.LeaveCancels.First().Status;
                            switch (status)
                            {
                                case 1:
                                    result += " --" + HRPortal.MultiLanguage.Resource.Text_LeaveCancellationApplied; 
                                    break;
                                case 3:
                                    result += " --" + HRPortal.MultiLanguage.Resource.Text_LeaveFormDeleted;
                                    break;

                            }

                        }
                    }

                    return result;
                    break;

                case FormType.OverTime:
                    OverTimeForm _overTimeForm = form as OverTimeForm;
                    return string.Format("{0}({1}~{2} {3}{4})",
                            Resource.OverTime,
                            _overTimeForm.StartTime.ToString("yyyy/MM/dd HH:mm"),
                            _overTimeForm.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _overTimeForm.OverTimeAmount.ToString("0.#"),
                            Resource.Hour);
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = form as PatchCardForm;
                    return string.Format("{0}({1})",
                            _patchCardForm.Type == 1 ? Resource.OnDuty : Resource.OffDuty,
                            _patchCardForm.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                    break;
                case FormType.PatchCardCancel:
                     PatchCardForm _patchCardForm_ = form as PatchCardForm;
                    return string.Format("{0}({1})",
                            _patchCardForm_.Type == 1 ? Resource.OnDuty : Resource.OffDuty,
                            _patchCardForm_.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                    break;
                default:
                    return string.Empty;
                    break;
            }
        }

        //20180323 產生表單起訖日期顯示字串
        public string GenerateFormPeriod(DateTime BeginTime, DateTime? EndTime = null,string LanguageCookie="zh-TW")
        {
            //string bstr = BeginTime.ToString("yyyy-MM-dd|ddd|HH:mm", new CultureInfo("zh-TW"));

            string bstr = BeginTime.ToString("yyyy-MM-dd|ddd|HH:mm", new CultureInfo(LanguageCookie));

            string[] arrayBSTR = bstr.Split('|');

            if (LanguageCookie != "en-US")
            {
                arrayBSTR[1] = arrayBSTR[1].Substring(1); //因為中文有週一週二，移除第一個字
            }

            bstr = string.Format("<span class=\"signdate\">{0} ({1})</span> <span class=\"signtime\">{2}</span>", arrayBSTR);

            string estr = "";
            if (EndTime != null)
            {
                //estr = EndTime.Value.ToString("yyyy-MM-dd|ddd|HH:mm", new CultureInfo("zh-TW"));
                
                estr = EndTime.Value.ToString("yyyy-MM-dd|ddd|HH:mm", new CultureInfo(LanguageCookie));
                
                string[] arrayESTR = estr.Split('|');

                if (LanguageCookie != "en-US")
                {
                    arrayESTR[1] = arrayESTR[1].Substring(1); //因為中文有週一週二，移除第一個字
                }

                if (BeginTime.Date == EndTime.Value.Date) //起訖日日期相同不再重複出現
                {
                    estr = string.Format("<span class=\"signtime\">{0}</span>", arrayESTR[2]);
                }
                else
                {
                    estr = string.Format("<span class=\"signdate2\">{0} ({1})</span> <span class=\"signtime\">{2}</span>", arrayESTR);
                }
                estr = " ～ " + estr;
            }

            return bstr + estr;
        }

      

        //20180322 Daniel 待簽核清單頁面欄位調整，表單摘要需拆解為假別、時數、及起訖區間
        public FormSummaryDetailData BuildSummaryForSign(object form, FormType formType)
        {
            FormSummaryDetailData result = new FormSummaryDetailData();
            string getLanguageCookie = HttpContext.Current.Request.Cookies["lang"] != null ? HttpContext.Current.Request.Cookies["lang"].Value : "zh-TW";
             
            switch (formType)
            {
                case FormType.Leave:
                    LeaveForm _leaveForm = form as LeaveForm;
                     string AbsentName = null;
                    string AbsentNameEN = null;
                    foreach (var i in _absent)
                    {
                        if (i.Code == _leaveForm.AbsentCode)
                        {
                            AbsentName = i.Name;
                            AbsentNameEN = i.AbsentNameEn;
                        }
                    }
                    result.Description = getLanguageCookie == "en-US" ? AbsentNameEN : AbsentName;
                    result.Hours= _leaveForm.LeaveAmount.ToString("0.#")+(_leaveForm.AbsentUnit == "h" ? Resource.Hour : Resource.Day);
                    result.Period = GenerateFormPeriod(_leaveForm.StartTime, _leaveForm.EndTime, getLanguageCookie);
                    /*
                    return string.Format("{0}({1}~{2} {3}{4})",
                             getLanguageCookie == "en-US" ? AbsentNameEn : AbsentName,
                            _leaveForm.StartTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.LeaveAmount.ToString("0.#"),
                            _leaveForm.AbsentUnit == "h" ? Resource.Hour : Resource.Day);
                    */
                    break;

                case FormType.OverTime:
                    OverTimeForm _overTimeForm = form as OverTimeForm;

                    result.Description = HRPortal.MultiLanguage.Resource.OverTime;
                    result.Hours = _overTimeForm.OverTimeAmount.ToString("0.#") + Resource.Hour;
                    result.Period = GenerateFormPeriod(_overTimeForm.StartTime, _overTimeForm.EndTime, getLanguageCookie);
                    /*
                    return string.Format("{0}({1}~{2} {3}{4})",
                            Resource.OverTime,
                            _overTimeForm.StartTime.ToString("yyyy/MM/dd HH:mm"),
                            _overTimeForm.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _overTimeForm.OverTimeAmount.ToString("0.#"),
                            Resource.Hour);
                    */
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = form as PatchCardForm;

                    result.Description = HRPortal.MultiLanguage.Resource.PatchCard;
                    result.Hours = "";
                    result.Period = (_patchCardForm.Type == 1 ? Resource.OnDuty : Resource.OffDuty) + " " + GenerateFormPeriod(_patchCardForm.PatchCardTime, null, getLanguageCookie);
                    /*
                    return string.Format("{0}({1})",
                            _patchCardForm.Type == 1 ? Resource.OnDuty : Resource.OffDuty,
                            _patchCardForm.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                     */
                    break;
                case FormType.PatchCardCancel:
                    PatchCardForm _patchCardForm_ = form as PatchCardForm;

                    result.Description = HRPortal.MultiLanguage.Resource.PatchCard;
                    result.Hours = "";
                    result.Period = (_patchCardForm_.Type == 1 ? Resource.OnDuty : Resource.OffDuty) + " " + GenerateFormPeriod(_patchCardForm_.PatchCardTime, null, getLanguageCookie);
              
                    /*
                    return string.Format("{0}({1})",
                            _patchCardForm_.Type == 1 ? Resource.OnDuty : Resource.OffDuty,
                            _patchCardForm_.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                    */

                    break;
                default:
                    result.Description = "";
                    result.Hours = "";
                    result.Period = "";
                    
                    break;
            }

            return result;
        }

        public HRPotralFormSignStatus BuildData(object form, FormType formType)
        {
            HRPotralFormSignStatus buildData = new HRPotralFormSignStatus();
            switch (formType)
            {
                case FormType.Leave:
                    LeaveForm _leaveForm = form as LeaveForm;
                    string getLanguageCookie = HttpContext.Current.Request.Cookies["lang"] != null ? HttpContext.Current.Request.Cookies["lang"].Value : "zh-TW";
                    string AbsentName = null;
                    string AbsentNameEn = null;
                    foreach (var i in _absent)
                    {
                        if (i.Code == _leaveForm.AbsentCode)
                        {
                            AbsentName = i.Name;
                            AbsentNameEn = i.AbsentNameEn;
                        }
                    }
                    buildData.AbsentCode = _leaveForm.AbsentCode;
                    buildData.AbsentName = AbsentName;
                    buildData.Amount =_leaveForm.LeaveAmount;
                    buildData.StartTime = DateTime.Parse( _leaveForm.StartTime.ToString("yyyy/MM/dd HH:mm"));
                    buildData.EndTime =  DateTime.Parse( _leaveForm.EndTime.ToString("yyyy/MM/dd HH:mm"));
                    buildData.AbsentUnit = _leaveForm.AbsentUnit == "h" ? Resource.Hour : Resource.Day;
                    buildData.AbsentEnglishName = AbsentNameEn;
                    return buildData;
                    break;

                case FormType.OverTime:
                    OverTimeForm _overTimeForm = form as OverTimeForm;
                    buildData.AbsentCode = "OT";
                    buildData.AbsentName = Resource.OverTime;
                    buildData.Amount = _overTimeForm.OverTimeAmount;
                    buildData.StartTime = DateTime.Parse(_overTimeForm.StartTime.ToString("yyyy/MM/dd HH:mm"));
                    buildData.EndTime = DateTime.Parse(_overTimeForm.EndTime.ToString("yyyy/MM/dd HH:mm"));
                    buildData.AbsentUnit = Resource.Hour;
                    buildData.AbsentEnglishName = Resource.OverTime;
                    return buildData;
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = form as PatchCardForm;
                    buildData.AbsentCode = "PC";
                    buildData.AbsentName = _patchCardForm.Type == 1 ? Resource.OnDuty : Resource.OffDuty;
                    buildData.Amount = 0;
                    buildData.StartTime = DateTime.Parse(_patchCardForm.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                    buildData.EndTime = null;
                    buildData.AbsentUnit = "";
                    buildData.AbsentEnglishName = _patchCardForm.Type == 1 ? Resource.OnDuty : Resource.OffDuty;
                    return buildData;
                    break;
                case FormType.PatchCardCancel:
                    PatchCardForm _patchCardForm_ = form as PatchCardForm;
                    buildData.AbsentCode = "PCC";
                    buildData.AbsentName = _patchCardForm_.Type == 1 ? Resource.OnDuty : Resource.OffDuty;
                    buildData.Amount = 0;
                    buildData.StartTime = DateTime.Parse(_patchCardForm_.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                    buildData.EndTime = null;
                    buildData.AbsentUnit = "";
                    buildData.AbsentEnglishName = _patchCardForm_.Type == 1 ? Resource.OnDuty : Resource.OffDuty;
                    return buildData;
                    break;
                default:
                    return buildData;
                    break;
            }
        }


    }

    public class FormSummaryDetailData
    {
        public string Description { get; set; }
        public string Hours { get; set; }
        public string Period { get; set; }
    }

}