using HRPortal.DBEntities;
using HRPortal.SignFlow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using YoungCloud.SignFlow.Conditions;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.SignFlow.SignLists
{
    public partial class HRPortalSignList<TEntity> : FormDefaultSignList<SignFlowRec> where TEntity : SignFlowRec
	{
        public FormData _tempFormData = null;
		public HRPortalSignList(FormType formType)
            : base(formType)
		{
            SignFlowRecRepository = new SignFlowRecRepository();
            OnGetSignerId = getSignerId;
		}

        public SignFlowRecModel GetSenderFlowItem(IFormData formData)
        {
            try
            {
                SignflowCreateBase etyInitData = new SignflowCreateBase();
                etyInitData = AutoMapper.Mapper.DynamicMap<SignflowCreateBase>(formData);

                return GetSenderFlowItem(etyInitData);
            }
            catch (System.Exception ex)
            {
                //logger.Error("**取得授權簽核清單(FormDefaultSignList.chkSignFlowSignerID)發生錯誤：" + ex.Message + "**");
                throw new Exception("取得授權簽核清單發生錯誤：" + ex.Message);
                //return null;
            }
        }

        public SignFlowRecModel GetSenderFlowItem(SignflowCreateBase etyInitData)
        {
            return new SignFlowRecModel()
                {
                    ID = "0",
                    FormNumber = etyInitData.FormNumber,
                    FormType = etyInitData.FormType,
                    FormLevelID = "0",
                    SignOrder = "0",
                    SenderID = etyInitData.CUser,
                    SignStatus = "W",
                    SignType = "S",
                    SignerID = etyInitData.CUser,
                    IsUsed = "Y",
                    CUser = etyInitData.CUser,
                    CDate = DateTime.Now,
                    MUser = etyInitData.CUser,
                    MDate = DateTime.Now,
                    DataState = DefaultEnum.SignFlowDataStatus.Add.ToString(),
                    SenderCompanyID = etyInitData.CompanyID, //NEW 20160604 BY BEE
                    SignCompanyID = etyInitData.CompanyID //NEW 20160604 BY BEE
                };
        }

		/// <summary>
		/// 由簽核授權清單，加上 CONDITION 取得之選擇清簽核項目組成簽核清單
		/// </summary>
		/// <param name="formData">用來判斷選擇性簽核項目，因此每種表單會帶進來的欄位都不同</param><returns></returns>
		/// <remarks>
		/// </remarks>
        public override IList<SignFlowRecModel> GetDefaultSignList(IFormData formData)
        {
            return GetDefaultSignList(formData, false);
        }

        public IList<SignFlowRecModel> GetDefaultSignList(IFormData formData, bool startAsSender)
        {
            try
            {
                SignflowCreateBase etyInitData = new SignflowCreateBase();
                etyInitData = AutoMapper.Mapper.DynamicMap<SignflowCreateBase>(formData);

                //取得流程設計表單
                if (etyDesign == null)
                {
                    etyDesign = getDesignByDept(etyInitData.CompanyID, etyInitData.DeptID,etyInitData.CUser);
                }

                //檢查設計檔的正確性
                if (!chkDesignVaild(etyDesign))
                {
                    throw new Exception("簽核流程設計檔中，含有對應不到的簽核層級！");
                }

                string _designId = etyDesign[0].DesignID;
                ConditionCheck _condition = null;

                switch(formData.FormType)
                {
                    case "Leave":
                        _condition = new Conditions.LeaveCdCheck(_designId, string.Empty, formData.AbsentCode);
                        break;
                    case "OverTime":
                        //_condition = new Conditions.OverTimeCdCheck(_designId);
                        break;
                    case "PatchCard":
                        //_condition = new Conditions.PatchCardCdCheck(_designId);
                        break;
                }

                if (_condition != null)
                {
                    foreach(SignFlowFormLevelModel _formLevel in ListFormLevel)
                    {
                        _condition.CheckLevel = _formLevel.LevelID;
                        etyDesign = _condition.CheckCondition(etyDesign, ListFormLevel, formData);
                    }
                }

                //將設計檔，轉成簽核清單
                IList<SignFlowRecModel> _signFlowList = this.ConvertSignRec(etyDesign, etyInitData);
                if (startAsSender)
                {
                    _signFlowList.Insert(0, GetSenderFlowItem(etyInitData));
                }

                //最後需進行排序才能回傳
                //DataTable dtReturn = null;
                //dtReturn = Utility.CopyDataRow2DataTable(dtSignList.Clone(), dtSignList.Select(null, "SN", DataViewRowState.CurrentRows));

                //檢查簽核流程之應簽人員是否均有值
                chkSignFlowSignerID(_signFlowList);

                removeSameSigner(_signFlowList);

                //判斷不可完全無簽核層級
                chkSignListCnt(_signFlowList);

                return _signFlowList;

            }
            catch (System.Exception ex)
            {
                //logger.Error("**取得授權簽核清單(FormDefaultSignList.chkSignFlowSignerID)發生錯誤：" + ex.Message + "**");
                throw new Exception("取得授權簽核清單發生錯誤：" + ex.Message);
                //return null;
            }
        }


        public IList<SignFlowRecModel> GetDefaultSignList_(IFormData formData, bool startAsSender)
        {
            try
            {
                SignflowCreateBase etyInitData = new SignflowCreateBase();
                etyInitData = AutoMapper.Mapper.DynamicMap<SignflowCreateBase>(formData);

                //取得流程設計表單
                if (etyDesign == null)
                {
                    etyDesign = getDesignByDept(etyInitData.CompanyID, etyInitData.DeptID, etyInitData.CUser);
                }

                //檢查設計檔的正確性
                if (!chkDesignVaild(etyDesign))
                {
                    throw new Exception("簽核流程設計檔中，含有對應不到的簽核層級！");
                }

                string _designId = etyDesign[0].DesignID;
                ConditionCheck _condition = null;

                switch (formData.FormType)
                {
                    case "Leave":
                        _condition = new Conditions.LeaveCdCheck(_designId, string.Empty, formData.AbsentCode);
                        break;
                    case "OverTime":
                        //_condition = new Conditions.OverTimeCdCheck(_designId);
                        break;
                    case "PatchCard":
                        //_condition = new Conditions.PatchCardCdCheck(_designId);
                        break;
                }

                if (_condition != null)
                {
                    foreach (SignFlowFormLevelModel _formLevel in ListFormLevel)
                    {
                        _condition.CheckLevel = _formLevel.LevelID;
                        etyDesign = _condition.CheckCondition(etyDesign, ListFormLevel, formData);
                    }
                }

                //將設計檔，轉成簽核清單
                IList<SignFlowRecModel> _signFlowList = this.ConvertSignRec(etyDesign, etyInitData);
                if (startAsSender)
                {
                    _signFlowList.Insert(0, GetSenderFlowItem(etyInitData));
                }

                //最後需進行排序才能回傳
                //DataTable dtReturn = null;
                //dtReturn = Utility.CopyDataRow2DataTable(dtSignList.Clone(), dtSignList.Select(null, "SN", DataViewRowState.CurrentRows));

                //檢查簽核流程之應簽人員是否均有值
                chkSignFlowSignerID(_signFlowList);

                removeSameSigner(_signFlowList);

                //判斷不可完全無簽核層級
                chkSignListCnt(_signFlowList);

                return _signFlowList;

            }
            catch (System.Exception ex)
            {
                //logger.Error("**取得授權簽核清單(FormDefaultSignList.chkSignFlowSignerID)發生錯誤：" + ex.Message + "**");
                throw new Exception("取得授權簽核清單發生錯誤：" + ex.Message);
                //return null;
            }
        }

        /// <summary>
        /// 20201229 Daniel 配合BambooHR整合，取得預設流程(不看簽核條件)
        /// </summary>
        /// <param name="startAsSender"></param>
        /// <returns></returns>
        public IList<SignFlowRecModel> GetDefaultSignListWithoutCondition(IFormData formData, bool startAsSender)
        {
            try
            {
                SignflowCreateBase etyInitData = new SignflowCreateBase();
                etyInitData = AutoMapper.Mapper.DynamicMap<SignflowCreateBase>(formData);
                etyInitData.FormType = "Leave";
                //取得流程設計表單
                if (etyDesign == null)
                {
                    etyDesign = getDesignByDept(etyInitData.CompanyID, etyInitData.DeptID, etyInitData.CUser);
                }

                //檢查設計檔的正確性
                if (!chkDesignVaild(etyDesign))
                {
                    throw new Exception("簽核流程設計檔中，含有對應不到的簽核層級！");
                }

                string _designId = etyDesign[0].DesignID;
                
                //將設計檔，轉成簽核清單
                IList<SignFlowRecModel> _signFlowList = this.ConvertSignRec(etyDesign, etyInitData);
                if (startAsSender)
                {
                    _signFlowList.Insert(0, GetSenderFlowItem(etyInitData));
                }

                //檢查簽核流程之應簽人員是否均有值
                chkSignFlowSignerID(_signFlowList);

                removeSameSigner(_signFlowList);

                //判斷不可完全無簽核層級
                chkSignListCnt(_signFlowList);

                return _signFlowList;

            }
            catch (System.Exception ex)
            {
                //logger.Error("**取得授權簽核清單(FormDefaultSignList.chkSignFlowSignerID)發生錯誤：" + ex.Message + "**");
                throw new Exception("取得授權簽核清單發生錯誤：" + ex.Message);
                //return null;
            }
        }

        protected string getManagerID(FormData formData, int level)
        {
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                string _deptCode = formData.DeptCode;
                Department _dept = _db.Departments.FirstOrDefault(x => x.Company.CompanyCode == formData.CompanyID && x.DepartmentCode == _deptCode && x.Enabled==true);
                
                //if (_dept.Manager != null && _dept.Manager.EmployeeNO == formData.CUser) //20160816 修改為一切交由簽核主管 by Bee
                if (_dept.SignManager != null && _dept.SignManager.EmployeeNO == formData.CUser)
                {
                    _deptCode = getParentDeptId(formData.CompanyID, _deptCode, _db);
                }
                if(_deptCode == null)
                {
                    return null;
                }

                for (int i = 1; i < level; i++)
                {
                    _deptCode = getParentDeptId(formData.CompanyID, _deptCode, _db);
                    if(_deptCode == null)
                    {
                        return null;
                    }
                }

                _dept = _db.Departments.FirstOrDefault(x => x.Company.CompanyCode == formData.CompanyID && x.DepartmentCode == _deptCode);
                if(_dept != null && _dept.SignManager != null)
                {
                    return _dept.SignManager.EmployeeNO;
                }
                else
                {
                    return null;
                }
            }
        }

        protected string getParentDeptId(string CompanyID, string DeptID, HRPortal.DBEntities.NewHRPortalEntities db)
        {
            Department _dept = db.Departments.FirstOrDefault(x => x.Company.CompanyCode == CompanyID && x.DepartmentCode == DeptID);
            if (_dept.SignParent == null)
            {
                return _dept.DepartmentCode;
            }
            return _dept.SignParent.DepartmentCode;
        }

        protected string getDeptManagerId(string CompanyID, string DeptID)
        {
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                Department _dept = _db.Departments.FirstOrDefault(x => x.Company.CompanyCode == CompanyID && x.DepartmentCode == DeptID);
                //20190712 小榜 修改先判斷是否有部門簽核主管，再判斷是否有單位主管
                if (_dept.SignManagerId != null)
                {
                    Employee _manager = _db.Employees.FirstOrDefault(x => x.ID == _dept.SignManagerId );
                    return _manager.EmployeeNO;
                }
                else if (_dept.ManagerId != null)
                {
                    Employee _manager = _db.Employees.FirstOrDefault(x =>  x.ID == _dept.ManagerId);
                    return _manager.EmployeeNO;
                }
                else
                {
                    return null;
                }
            }
        }

        protected string[] getSignerId(SignFlowDesignModel design, SignflowCreateBase etyInitData)
        {
            FormData _formData;

            //20201230 Daniel 修改，可預先傳入準備好的FormData，就不需要再去取得一次
            if (this._tempFormData != null)
            {
                _formData = this._tempFormData;
            }
            else
            {
                _formData = (FormData)GetFormData(etyInitData);
            }

            string[] _tmpSignerId = { "P", null };
            //應簽核人員
            string _signDeptType = (design.DeptType == null ? null : design.DeptType.ToString());
            string _dept = null;
            if (!string.IsNullOrEmpty(_signDeptType))
            {
                switch (_signDeptType)
                {
                    case "5":   //員工第五層主管
                        try
                        {
                            _tmpSignerId[1] = getManagerID(_formData, 5);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("(工號：" + _formData.EmployeeNo + ")，" + ex.Message);
                        }

                        break;
                    case "4":   //員工第四層主管
                        try
                        {
                            _tmpSignerId[1] = getManagerID(_formData, 4);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("(工號：" + _formData.EmployeeNo + ")，" + ex.Message);
                        }

                        break;
                    case "3":   //員工第三層主管
                        try
                        {
                            _tmpSignerId[1] = getManagerID(_formData, 3);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("(工號：" + _formData.EmployeeNo + ")，" + ex.Message);
                        }

                        break;
                    case "2":   //員工第二層主管
                        try
                        {
                            _tmpSignerId[1] = getManagerID(_formData, 2);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("(工號：" + _formData.EmployeeNo + ")，" + ex.Message);
                        }

                        break;
                    case "1":   //員工主管
                        try
                        {
                            _tmpSignerId[1] = getManagerID(_formData, 1);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("(工號：" + design.SignDeptID + ")，" + ex.Message);
                        }

                        break;
                    case "0":   //指定部門主管
                        try
                        {
                            //_tmpSignerId[1] = this.getDeptManagerId(_formData.CompanyID, design.SignDeptID);
                            string SignCompanyCode = _formData.CompanyID;
                            if (design.SignCompanyID != null)
                            { //20160606 針對跨公司簽核判斷
                                SignCompanyCode = design.SignCompanyID;
                            }
                            _tmpSignerId[1] = this.getDeptManagerId(SignCompanyCode, design.SignDeptID);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("(部門編號：" + design.SignDeptID + ")，" + ex.Message);
                        }
                        break;
                    case "15":  //指定部門任一人
                        try
                        {
                            _tmpSignerId[0] = "D";
                            _tmpSignerId[1] = design.SignDeptID;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("(部門編號：" + design.SignDeptID + ")，" + ex.Message);
                        }
                        break;
                    default:
                        throw new Exception("無法對應的簽核部門類型！");
                }
            }
            else    //_signDeptType == null => 直接指定簽核人員
            {
                _tmpSignerId[1] = design.SignerID;
            }

            return _tmpSignerId;

        }

        public override IFormData GetFormData(SignflowCreateBase enty)
        {
            throw new NotImplementedException();
        }

        public IList<SignFlowRecModel> GetAllSignPermission(IFormData formData, bool startAsSender)
        {
            try
            {
                SignflowCreateBase etyInitData = new SignflowCreateBase();
                etyInitData = AutoMapper.Mapper.DynamicMap<SignflowCreateBase>(formData);

                //取得流程設計表單
                if (etyDesign == null)
                {
                    etyDesign = getDesignByDept(etyInitData.CompanyID, etyInitData.DeptID, etyInitData.CUser);
                }

                //檢查設計檔的正確性
                if (!chkDesignVaild(etyDesign))
                {
                    throw new Exception("簽核流程設計檔中，含有對應不到的簽核層級！");
                }
                              
                //將設計檔，轉成簽核清單
                IList<SignFlowRecModel> _signFlowList = this.ConvertSignRec(etyDesign, etyInitData);
                //if (startAsSender)
                //{
                //    //_signFlowList.Insert(0, GetSenderFlowItem(etyInitData));
                //}
                /*
                //最後需進行排序才能回傳
                //DataTable dtReturn = null;
                //dtReturn = Utility.CopyDataRow2DataTable(dtSignList.Clone(), dtSignList.Select(null, "SN", DataViewRowState.CurrentRows));

                //檢查簽核流程之應簽人員是否均有值
                chkSignFlowSignerID(_signFlowList);

                removeSameSigner(_signFlowList);

                //判斷不可完全無簽核層級
                chkSignListCnt(_signFlowList);
                */
                return _signFlowList;

            }
            catch (System.Exception ex)
            {
                //logger.Error("**取得授權簽核清單(FormDefaultSignList.chkSignFlowSignerID)發生錯誤：" + ex.Message + "**");
                throw new Exception("取得授權簽核清單發生錯誤：" + ex.Message);
                //return null;
            }
        }
    }
	// SiteRelateDateSignList
}
