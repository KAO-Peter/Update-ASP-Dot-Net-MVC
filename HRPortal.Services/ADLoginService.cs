using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

namespace HRPortal.Services
{
    public class ADLoginService
    {
        //protected HRPortal_Services Services;
        
        public ADLoginService()

        {
            //Services = services;
        }

        public bool AuthenticateActiveDirectoryAccount(string Account, string Password, string DomainName = "HQFEG.COM",string PrefixAccount="ddmc")
        {
            bool authResult = false;
            using (PrincipalContext PContext = new PrincipalContext(ContextType.Domain, DomainName))
            {
                string adAccount = "";
                //string Domain = "HQFEG";
                //string prefixAccount = "ddmc"; 
                //adAccount = Domain + @"\" + PrefixAccount + Account;

                /* 此處不處理直接輸入如ddmc11111格式的帳號的狀況，改由登入頁先移除帳號的前置字串(如果相符的話)
                if (!string.IsNullOrWhiteSpace(PrefixAccount))
                {
                    if (Account.Substring(0, PrefixAccount.Length).ToUpper() == PrefixAccount.ToUpper()) //如果帳號前幾碼和設定的前置字串相同，就直接用帳號登入
                    {
                        adAccount = Account;
                    }
                    else //如果前幾碼與前置字串不同，就用前置字串+帳號登入
                    {
                        adAccount = PrefixAccount + Account; 
                    }
                }
                else //沒設定前置字串，直接用帳號登入
                {
                    adAccount = Account;
                }
                */
                
                adAccount = PrefixAccount + Account;
                authResult = PContext.ValidateCredentials(adAccount, Password, ContextOptions.Negotiate);
            }
            return authResult;
        }
        public string AuthenticateActiveDirectoryAccount2(string Account, string Password, string DomainName = "HQFEG.COM", string PrefixAccount = "ddmc")
        {
            string result = "";
            string adAccount = "";
            adAccount = PrefixAccount + Account;
            //20190422 Daniel 增加一層try catch，並加上錯誤代碼區隔
            try
            {
                using (PrincipalContext PContext = new PrincipalContext(ContextType.Domain, DomainName, adAccount, Password))
                {
                    try
                    {
                        using (UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(PContext, adAccount))
                        {

                            /*
                            DirectoryEntry DE = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
                            using (MemoryStream s = new MemoryStream(DE.Properties["thumbnailPhoto"].Value as byte[]))
                            {
                                return Bitmap.FromStream(s);
                            }
                            */
                        }
                    }
                    catch (Exception innerException)
                    {
                        result = "E0001(AD登入)：" + innerException.Message;
                    }

                }
            }
            catch (Exception ex)
            {
                result = "E0002(AD登入)：" + ex.Message;
            }
            return result;
        }
    }
}
