using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;

namespace HRPortal.Services.DDMC_PFA
{
    public class DepartmentService : BaseCrudService<PfaDepartment>
    {
        public DepartmentService(HRPortal_Services services)
            : base(services)
        {
        }

        public IEnumerable<ValueText> GetValueText(Guid company_id)
        {
            return GetAll().Where(x => x.CompanyID == company_id).Select(x => new ValueText { id = x.ID, v = x.ID.ToString(), t = x.DepartmentName }).ToArray();
        }

        /// <summary>
        /// 模糊查詢列表
        /// </summary>
        /// <param name="PfaDepartmentId"></param>
        /// <param name="CompanyID"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<PfaDepartment> GetSearchPfaDepartmentLists(string keyword,Guid companyID)
        {
            IQueryable<PfaDepartment> _PfaDepartments;
            _PfaDepartments = GetAll().Where(x => x.CompanyID == companyID && x.Enabled);
           
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                _PfaDepartments = _PfaDepartments.Where(x => x.DepartmentName.Contains(keyword) || x.DepartmentCode.Contains(keyword));
            }
            return _PfaDepartments.OrderBy(x => x.DepartmentCode).ToList();

        }

        /// <summary>
        /// 取得該部門底下的所有部門
        /// </summary>
        /// <param name="PfaDepartmentID"></param>
        /// <returns></returns>
        public List<PfaDepartment> GetParentPfaDepartmentLists(Guid PfaDepartmentID)
        {
            List<PfaDepartment> _result = new List<PfaDepartment>();
            List<PfaDepartment> _allPfaDepartment = GetAll().Where(x => x.Enabled).ToList();
            List<PfaDepartment> _foundDepartmnet = GetAll().Where(x => x.ID == PfaDepartmentID && !x.OnlyForSign && x.Enabled).ToList();

            while (_foundDepartmnet != null && _foundDepartmnet.Count > 0)
            {
                _result.AddRange(_foundDepartmnet);
                _foundDepartmnet = _allPfaDepartment.Where(x => x.ParentDepartmentID.HasValue
                    && _foundDepartmnet.Select(y => y.ID).Contains(x.ParentDepartmentID.Value)).ToList();
            }

            return _result.Distinct().ToList();
        }

        public List<PfaDepartment> GetSignPfaDepartmentLists(Guid PfaDepartmentID,int DeptCount)
        {
            List<PfaDepartment> _result = new List<PfaDepartment>();
            List<PfaDepartment> _allPfaDepartment = GetAll().Where(x => x.Enabled).ToList();
            List<PfaDepartment> _foundDepartmnet = _allPfaDepartment.Where(x => x.ID == PfaDepartmentID).ToList();

            if (DeptCount != 0 && _foundDepartmnet != null && _foundDepartmnet.Count > 0)//代表非後臺主管
            {
                while (_foundDepartmnet != null && _foundDepartmnet.Count > 0)
                {
                    _result.AddRange(_foundDepartmnet);
                    _foundDepartmnet = _allPfaDepartment.Where(x => x.SignParentID.HasValue
                        && _foundDepartmnet.Select(y => y.ID).Contains(x.SignParentID.Value)).ToList();
                }
            }
            else if (DeptCount == 0 && _foundDepartmnet != null && _foundDepartmnet.Count > 0)
            {
                _result.AddRange(_foundDepartmnet);
            }

            return _result.Distinct().OrderBy(x => x.DepartmentCode).ToList();
        }


        public IEnumerable<PfaDepartment> GetAllLists()
        {
            return this.GetAll();
        }
        public IEnumerable<PfaDepartment> GetParentPfaDepartmentDate(Guid? ID)
        {
            return GetAll().Where(x => x.ID == ID);
        }
        public IEnumerable<PfaDepartment> GetListsByCompany(Guid? CompanyID)
        {
            if (CompanyID != null && CompanyID != Guid.Empty)
            {
                return this.GetAll().Where(x => x.CompanyID != null && x.CompanyID == CompanyID);
            }
            return this.GetAll();
        }

        public int ImportSap(Stream stream, bool isSave = true)
        {
            List<string> importErrorMessages = new List<string>();
            int rowCount = 0;
            bool isUpdate = true;

            Dictionary<string, bool> dictField = new Dictionary<string, bool>() { { "PfaDepartment_no", true }, { "name", true }, { "manager", false }, { "begin_date", true }, { "disable_date", true }, { "parent_id", false }, { "cost_center", true }, { "human_range", true }, { "human_sub_range", true } };
            Guid company_id = Services.GetService<CompanyService>().FirstOrDefault().ID;
            using (StreamReader sr = new StreamReader(stream, Encoding.Default))
            {
                while (sr.Peek() >= 0)
                {
                    try
                    {
                        rowCount++;
                        isUpdate = true;
                        List<string> read_fields = sr.ReadLine().Split('\t').ToList();
                        if (read_fields.Count != dictField.Count)
                            throw new Exception(string.Format("欄位數錯誤。"));

                        String val = read_fields[dictField.Keys.ToList().IndexOf("PfaDepartment_no")].Trim();
                        PfaDepartment PfaDepartment = this.Services.GetService<DepartmentService>().FirstOrDefault(x => x.DepartmentCode == val);
                        if (PfaDepartment == null)
                        {
                            PfaDepartment = new PfaDepartment();
                            PfaDepartment.CompanyID = company_id;
                            isUpdate = false;
                        }

                        for (int i = 0; i < dictField.Count; i++)
                        {
                            if (!dictField.ElementAt(i).Value)
                                continue;

                            string propertie = dictField.ElementAt(i).Key;
                            String propertie_value = read_fields[i].Trim();

                            PropertyInfo propertyInfo = PfaDepartment.GetType().GetProperty(propertie);
                            if (propertie == "begin_date" || propertie == "disable_date")
                            {
                                propertie_value = Regex.Replace(propertie_value, "(?<=^\\d{4})(?=.+$)|(?<=^.+)(?=\\d{2}$)", "/");
                                try
                                {
                                    if (propertie_value != "0000/00/00" && propertie_value != "9999/12/31")
                                        propertyInfo.SetValue(PfaDepartment, DateTime.Parse(propertie_value));
                                }
                                catch
                                {
                                    throw new Exception(string.Format(propertie + "格式錯誤"));
                                }
                            }
                            else
                                propertyInfo.SetValue(PfaDepartment, propertie_value);
                        }
                        if (isUpdate)
                            this.Services.GetService<DepartmentService>().Update(PfaDepartment, false);
                        else
                            this.Services.GetService<DepartmentService>().Create(PfaDepartment, false);

                    }
                    catch (Exception ex)
                    {
                        importErrorMessages.Add(string.Format("第 {0} 列錯誤：{1}", rowCount + 1, ex.Message));
                        logger.Error(ex);
                    }
                }
                sr.Close();
            }

            if (importErrorMessages.Count > 0)
            {
                throw new Exception(string.Join("\n", importErrorMessages.ToArray()));
            }

            return this.SaveChanges(isSave);
        }

        public PfaDepartment GetPfaDepartmentByID(Guid ID)
        {
           return GetAll().Where(x => x.ID == ID).FirstOrDefault();
        }

        //20180502 Start 小榜 增加傳入部門代碼，回傳PfaDepartment物件清單功能
        public List<PfaDepartment> GetDeptAgentListByDeptCodeList(List<string> DeptCodeList)
        {
            var result = this.GetAll().Where(x => DeptCodeList.Contains(x.DepartmentCode)).ToList();
            return result;
        }
    }
}
