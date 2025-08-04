using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Helper
{
    public interface IFormNoGenerator
    {
        string GetLeaveFormNo();

        string GetOverTimeFormNo();

        string GetPatchCardFormNo();
        string GetLeaveCancelFormNo();
        string GetOverTimeCancelFormNo();
    }
}
