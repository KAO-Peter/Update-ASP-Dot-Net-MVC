using System;
using System.Collections.Generic;
using System.Linq;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.Model;

namespace YoungCloud.SignFlow.SignLists
{
    public class SignFlowRecQueryHelper : IDisposable
    {
        private SignFlowRecRepository m_signFlowRecRepository;
        public SignFlowRecRepository SignFlowRecRepository
        {
            get
            {
                if (m_signFlowRecRepository == null) m_signFlowRecRepository = new SignFlowRecRepository();
                return m_signFlowRecRepository;
            }
        }
        
        public SignFlowRecQueryHelper()
        {
        }

        public SignFlowRecQueryHelper(SignFlowRecRepository repository)
        {
            m_signFlowRecRepository = repository;
        }

        public IList<SignFlowRecModel> GetSignFlowBySignerId(string signerId)
        {
            IQueryable<SignFlowRec> _query = SignFlowRecRepository.GetAll().Where(x =>
                x.SignType == "P" && x.SignerID == signerId && x.IsUsed == DefaultEnum.IsUsed.Y.ToString());
            return TransferToModelList(_query);
        }

        public IList<SignFlowRecModel> GetLastSignFlowByFormNumber(IList<string> formNumbers)
        {
            IQueryable<SignFlowRec> _query = GetLastSignFlow().Where(x => formNumbers.Contains(x.FormNumber));
            return TransferToModelList(_query);
        }

        public IList<SignFlowRecModel> GetSignFlowByFormNumber(string formNumber)
        {
            IQueryable<SignFlowRec> _query = SignFlowRecRepository.GetAll().Where(x =>
               x.FormNumber == formNumber && x.IsUsed == DefaultEnum.IsUsed.Y.ToString())
               .OrderBy(x => x.GroupId).ThenBy(x => x.SignOrder);
            return TransferToModelList(_query);
        }

        //20210525 Daniel 增加查詢核准時間的函式，注意狀態是A的會有多個(多關簽核)，取最後一個日期
        public Dictionary<string, DateTime?> GetApprovedTime(List<string> FormNoList, string FormType="Leave") 
        {
            Dictionary<string, DateTime?> result = SignFlowRecRepository.GetAll().Where(x => x.FormType == FormType && FormNoList.Contains(x.FormNumber) && x.IsUsed == "Y" && x.SignStatus == "A")
                                                                                    .GroupBy(y => y.FormNumber)
                                                                                    //.Select(g => new { FormNumber = g.Key, SignDate = g.Max(m => m.SignDate) })
                                                                                    .ToDictionary(k => k.Key, v => v.Max(m => m.SignDate));
            return result;
 
        }


        /// <summary>
        ///  依照表單號碼集合跟簽核人ID查簽核紀錄
        /// </summary>
        /// <param name="formNumbers">請假單號物件集合</param>
        /// <param name="signerId">簽核人員的員編。</param>
        /// <returns></returns>
        public IList<SignFlowRecModel> GetSignFlowByFormNumberWithSignerID(List<string> formNumbers, string signerId)
        {
            IQueryable<SignFlowRec> _query = SignFlowRecRepository.GetAll().Where(x =>
               formNumbers.Contains(x.FormNumber) && x.IsUsed == DefaultEnum.IsUsed.Y.ToString() && 
               x.SignerID == signerId).
               OrderBy(x => x.GroupId).ThenBy(x => x.SignOrder);
            return TransferToModelList(_query);
        }


        public IList<SignFlowRecModel> GetSignFlowByCurrentSignerId(string signerId)
        {
            //20170525 小榜：修改原簽核人與代理人可同時看到假單
            IQueryable<SignFlowRec> _query = GetCurrentSignFlow().Where(x => x.SignType == "P" && (x.SignerID == signerId || x.OrgSignerID == signerId));
            return TransferToModelList(_query);
        }
        //20151215 增加待處理 by Bee
        public IList<SignFlowRecModel> GetPendingSignFlowByCurrentSignerId(string SenderID)
        {
            
            IQueryable<SignFlowRec> _query = GetCurrentSignFlow().Where(x => x.SignStatus == "W" && x.SenderID == SenderID);
            return TransferToModelList(_query);
        }

        public IList<SignFlowRecModel> GetSignFlowByCurrentDepartmentId(string departmentId)
        {
            IQueryable<SignFlowRec> _query = GetCurrentSignFlow().Where(x => x.SignType == "D" && x.SignerID == departmentId);
            return TransferToModelList(_query);
        }

        public IList<SignFlowRecModel> GetCurrentSignFlowByFormNumber(IList<string> formNumbers)
        {
            IQueryable<SignFlowRec> _query = GetCurrentSignFlow().Where(x => formNumbers.Contains(x.FormNumber));
            return TransferToModelList(_query);
        }

        private IQueryable<SignFlowRec> GetCurrentSignFlow()
        {
            return m_signFlowRecRepository.GetAll()
                .Where(x => x.SignStatus == DefaultEnum.SignStatus.W.ToString()
                    && x.IsUsed == DefaultEnum.IsUsed.Y.ToString())
                .GroupBy(x => x.FormNumber)
                .Select(x => x.OrderBy(z => z.GroupId).ThenBy(z => z.SignOrder))
                .Select(x => x.FirstOrDefault());
        }

        private IQueryable<SignFlowRec> GetLastSignFlow()
        {
            return m_signFlowRecRepository.GetAll()
                .Where(x => x.IsUsed == DefaultEnum.IsUsed.Y.ToString())
                .GroupBy(x => x.FormNumber)
                .Select(x => x.OrderByDescending(z => z.GroupId).ThenByDescending(z => z.SignOrder))
                .Select(x => x.FirstOrDefault());
        }

        private static IList<SignFlowRecModel> TransferToModelList(IQueryable<SignFlowRec> query)
        {
            IList<SignFlowRec> _signFlowRec = query.ToList();
            IList<SignFlowRecModel> _signFlowRecModel = new List<SignFlowRecModel>();
            foreach (SignFlowRec _signFlow in _signFlowRec)
            {
                _signFlowRecModel.Add((AutoMapper.Mapper.DynamicMap<SignFlowRecModel>(_signFlow)));
            }
            return _signFlowRecModel;
        }

        public void Dispose()
        {

        }
    }
}
