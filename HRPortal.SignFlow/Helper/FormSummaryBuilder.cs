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

namespace HRPortal.SignFlow.Helper
{
    public class FormSummaryBuilder
    {
        private HRPortal_Services _services;
        Dictionary<string, string> _absent;

        public FormSummaryBuilder(Dictionary<string, string> absent)
        {
            _services = new HRPortal_Services();
            _absent = absent;
        }

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

        public string BuildSummary(object form, FormType formType)
        {
            switch (formType)
            {
                case FormType.Leave:
                    LeaveForm _leaveForm = form as LeaveForm;
                    return string.Format("{0}({1}~{2} {3}{4})",
                            _absent.Keys.Contains(_leaveForm.AbsentCode) ? _absent[_leaveForm.AbsentCode] : _leaveForm.AbsentCode,
                            _leaveForm.StartTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _leaveForm.LeaveAmount.ToString("0.#"),
                            _leaveForm.AbsentUnit == "h" ? Resource.Hour : Resource.Day);
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
                            _patchCardForm.Type == 1? Resource.OnDuty: Resource.OffDuty,
                            _patchCardForm.PatchCardTime.ToString("yyyy/MM/dd HH:mm"));
                    break;
                default:
                    return string.Empty;
                    break;
            }
        }
    }
}