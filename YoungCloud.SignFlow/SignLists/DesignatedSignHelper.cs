using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace YoungCloud.SignFlow.SignLists
{

    /// <summary>
    /// 取得指定簽核人資訊。
    /// </summary>
    public class DesignatedSignHelper : IDisposable
    {
        private SignFlowAssignDeptRepository _gSignFlowAssignDeptRepository;
        private SignFlowDesignRepository _gSignFlowDesignRepository;


        public DesignatedSignHelper()
        {
            _gSignFlowDesignRepository = new SignFlowDesignRepository();
            _gSignFlowAssignDeptRepository = new SignFlowAssignDeptRepository();
        }


        /// <summary>
        ///  取得指定簽核人要簽核的人員資訊。
        /// </summary>
        /// <param name="pSignerId">簽核人ID</param>
        /// <returns></returns>
        public List<String> GetSignOfEmpIds(string pSignerId)
        {
            List<string> empIDs = new List<string>();

            List<string> designIDs = _gSignFlowDesignRepository.GetAll().
                Where(x => x.IsUsed == "Y" && x.SignerID == pSignerId).Select(x => x.DesignID).Distinct().ToList();

            return _gSignFlowAssignDeptRepository.GetAll().
                Where(x => designIDs.Contains(x.DesignID) && x.IsUsed == "Y" && x.EmpID != null).
                Select( x => x.EmpID).ToList();

        }


        public void Dispose()
        {

        }
    }
}
