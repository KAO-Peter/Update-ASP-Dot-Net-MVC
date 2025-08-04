using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class SignFlowConditionsRepository : SignFlowRepositoryBase<SignFlowConditions>
    {
        public SignFlowConditionsRepository()
            : base()
        {
        }

        public IQueryable<SignFlowConditions> GetConditions(string designID, string levelId)
        {
            return GetByDesignID(designID).Where(x => x.LevelID == levelId);
        }

        public IQueryable<SignFlowConditions> GetConditions(string designID, string levelId, string absentCode)
        {
            var result = GetByDesignID(designID).Where(x => x.LevelID == levelId);

            if (string.IsNullOrEmpty(absentCode))
            {
                result = result.Where(x => x.AbsentCode == null || x.AbsentCode == "");
            }
            else
            {
                var resultList = result.Where(x => x.AbsentCode == absentCode).ToList();

                if (resultList.Count == 0)
                {
                    result = result.Where(x => x.AbsentCode == null || x.AbsentCode == "");
                }
                else
                {
                    result = result.Where(x => x.AbsentCode == absentCode);
                }
            }

            return result;
        }

        public IQueryable<SignFlowConditions> GetByDesignID(string designID)
        {
            return GetAll().Where(x => x.DesignID == designID && x.IsUsed == "Y");
        }
    }
}
