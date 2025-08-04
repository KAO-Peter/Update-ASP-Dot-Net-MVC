using HRPortal.DBEntities.DDMC_PFA;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HRPortal.Common.PFA
{
    public class PfaOption
    {
        
        public static List<SelectListItem> GetPfaSignStatusOption(string selectedData
            , bool flag = false
            , string exclude = null)
        {
            var listItem = new List<SelectListItem>();

            List<DBEntities.DDMC_PFA.PfaOption> pfaOptions = new List<DBEntities.DDMC_PFA.PfaOption>();
            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                pfaOptions = db.PfaOption.Where(r => r.PfaOptionGroup.OptionGroupCode == "SignStatus")
                    .OrderBy(r=>r.Ordering)
                    .ToList();

                if(exclude != null)
                {
                    pfaOptions = pfaOptions.Where(r=>r.OptionCode != exclude)
                        .ToList();
                }
            }

            if (flag)
                listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            foreach (var item in pfaOptions)
            {
                listItem.Add(new SelectListItem { 
                    Text = item.OptionName, 
                    Value = item.OptionCode, 
                    Selected = selectedData == item.OptionCode ? true : false
                });
            }
                

            return listItem;
        }
    }
}