using System;
using System.Configuration;

namespace HRPortal.Services.DDMC_PFA.Common
{
    public class ExceptionHelper
    {
        public static string GetMsg(Exception ex)
        {
            return ex.Message.IndexOf("請參閱內部例外狀況") > -1 ?
                                GetMsg(ex.InnerException) : ex.Message;
        }

    }

}
