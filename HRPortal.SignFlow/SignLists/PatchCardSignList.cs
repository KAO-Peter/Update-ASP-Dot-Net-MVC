using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using YoungCloud.SignFlow.Conditions;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.Model;
using YoungCloud.Configurations;
using Autofac;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.SignLists;
using HRPortal.SignFlow.Conditions;
using HRPortal.DBEntities;
using System.Linq;
using HRPortal.SignFlow.Model;

namespace HRPortal.SignFlow.SignLists
{
    public partial class PatchCardSignList : HRPortalSignList<SignFlowRec>
	{
        public PatchCardSignList()
            : base(FormType.PatchCard)
		{
            SignFlowRecRepository = new SignFlowRecRepository();
		}

        public override IFormData GetFormData(SignflowCreateBase enty)
        {
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                PatchCardForm _patchCardForm = _db.PatchCardForms.FirstOrDefault(x => x.FormNo == enty.FormNumber);
                return new PatchCardFormData(_patchCardForm);
            }
        }
	}
	// SiteRelateDateSignList
}
