using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.MultiLanguage;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace HRPortal.Helper
{
    public class FormSummaryBuilder
    {
        private HRPortal_Services _services;
        Dictionary<string, string> _absent;
        Dictionary<string, string> _absentEN;

        public FormSummaryBuilder(Dictionary<string, string> absent, Dictionary<string, string> absentEN = null)
        {
            _services = new HRPortal_Services();
            _absent = absent;
            _absentEN = absentEN;
        }

        //此函式目前沒有地方會呼叫
        public void BuildSummary(HRPotralFormSignStatus signFlowStatus)
        {
            switch (signFlowStatus.FormType)
            {
                case FormType.Leave:
                    LeaveForm _leaveForm = _services.GetService<LeaveFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_leaveForm, signFlowStatus.FormType);
                    break;
                case FormType.OverTime:
                    OverTimeForm _overTimeForm = _services.GetService<OverTimeFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_overTimeForm, signFlowStatus.FormType);
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = _services.GetService<PatchCardFormService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_patchCardForm, signFlowStatus.FormType);
                    break;
                case FormType.LeaveCancel:
                    LeaveCancel _leaveCancel = _services.GetService<LeaveCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = Resource.LeaveCancel + " - " + BuildSummary(_leaveCancel.LeaveForm, FormType.Leave);
                    break;
                case FormType.OverTimeCancel:
                    OverTimeCancel _overTimeCancel = _services.GetService<OverTimeCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_overTimeCancel.OverTimeForm, FormType.OverTime).Replace(Resource.OverTime, Resource.OverTimeCancel);
                    break;
                case FormType.PatchCardCancel:
                    PatchCardCancel _patchCardCancel = _services.GetService<PatchCardCancelService>()
                        .FirstOrDefault(x => x.FormNo == signFlowStatus.FormNo);
                    signFlowStatus.FormSummary = BuildSummary(_patchCardCancel.PatchCardForm, FormType.PatchCardCancel).Replace(Resource.PatchCard, Resource.PatchCardCancel);
                    break;
                default:
                    break;
            }
        }

        //此函式目前沒有地方會呼叫
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
        

        public string BuildSummary(object form, FormType formType, string LanguageCookie = "zh-TW")
        {
            switch (formType)
            {
                case FormType.Leave:
                    LeaveForm _leaveForm = form as LeaveForm;
                    string absentName = "";
                    if (LanguageCookie == "en-US" && _absentEN != null)
                    {
                        absentName = _absentEN.Keys.Contains(_leaveForm.AbsentCode) ? _absentEN[_leaveForm.AbsentCode] : _leaveForm.AbsentCode;
                    }
                    else
                    {
                        absentName = _absent.Keys.Contains(_leaveForm.AbsentCode) ? _absent[_leaveForm.AbsentCode] : _leaveForm.AbsentCode;
                    }
                    return string.Format("{0}({1}~{2} {3}{4})",
                        //_absent.Keys.Contains(_leaveForm.AbsentCode) ? _absent[_leaveForm.AbsentCode] : _leaveForm.AbsentCode,
                            absentName,
                            _leaveForm.StartTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.LeaveAmount.ToString("0.#"),
                            //20190710 Daniel 依照傳入語系顯示單位
                            _leaveForm.AbsentUnit == "h" ? Resource.ResourceManager.GetString("Hour", CultureInfo.GetCultureInfo(LanguageCookie)) : Resource.ResourceManager.GetString("Day", CultureInfo.GetCultureInfo(LanguageCookie)));
                    break;
                case FormType.OverTime:
                    OverTimeForm _overTimeForm = form as OverTimeForm;
                    return string.Format("{0}({1}~{2} {3}{4})",
                            //Resource.OverTime,
                            Resource.ResourceManager.GetString("OverTime", CultureInfo.GetCultureInfo(LanguageCookie)),
                            _overTimeForm.StartTime.ToString("yyyy/MM/dd HH:mm"),
                            _overTimeForm.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _overTimeForm.OverTimeAmount.ToString("0.#"),
                            Resource.ResourceManager.GetString("Hour", CultureInfo.GetCultureInfo(LanguageCookie))); ////20190711 Daniel 依照傳入語系顯示單位
                    break;
                case FormType.PatchCard:
                    PatchCardForm _patchCardForm = form as PatchCardForm;
                    return string.Format("{0}({1})",
                            //_patchCardForm.Type == 1 ? Resource.OnDuty : Resource.OffDuty,
                            _patchCardForm.Type == 1 ? Resource.ResourceManager.GetString("OnDuty", CultureInfo.GetCultureInfo(LanguageCookie)) : Resource.ResourceManager.GetString("OffDuty", CultureInfo.GetCultureInfo(LanguageCookie)),
                            _patchCardForm.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                    break;
                default:
                    return string.Empty;
                    break;
            }
        }
    }
}