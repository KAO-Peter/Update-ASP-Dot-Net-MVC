using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using YoungCloud.Configurations;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;

namespace YoungCloud.SignFlow.SignLists
{
    /// <summary>
	/// 簽核清單的抽象類別，範本模式
	/// </summary>
    public abstract class FormDefaultSignList<TEntity> :ClassBase where TEntity : class
    {
        public FormDefaultSignList(Enum formType)
		{
			this.m_formType = formType;
		}

		#region "Property"
        //表單簽核流程Repository
        public ISignFlowRecRepositoryBase<TEntity> SignFlowRecRepository
        {
            get;
            set;
        }
        //簽核ID流水號Repository
        private SignFlowSeqRepository m_signFlowSeqRepository;
        public SignFlowSeqRepository SignFlowSeqRepository
        {
            get
            {
                if (m_signFlowSeqRepository == null) m_signFlowSeqRepository = new SignFlowSeqRepository();
                return m_signFlowSeqRepository;
            }
        }
        //簽核層級Repository
        private SignFlowLevelRepository m_signFlowLevelRepository;
        public SignFlowLevelRepository SignFlowLevelRepository
        {
            get
            {
                if (m_signFlowLevelRepository == null) m_signFlowLevelRepository = new SignFlowLevelRepository();
                return m_signFlowLevelRepository;
            }
        }
        //表單簽核層級Repository
        private SignFlowFormLevelRepository m_signFlowFormLevelRepository;
        public SignFlowFormLevelRepository SignFlowFormLevelRepository
        {
            get
            {
                if (m_signFlowFormLevelRepository == null) m_signFlowFormLevelRepository = new SignFlowFormLevelRepository();
                return m_signFlowFormLevelRepository;
            }
        }
        //部門表單簽核設計流程Repository
        private SignFlowAssignDeptRepository m_signFlowAssignDeptRepository;
        public SignFlowAssignDeptRepository SignFlowAssignDeptRepository
        {
            get
            {
                if (m_signFlowAssignDeptRepository == null) m_signFlowAssignDeptRepository = new SignFlowAssignDeptRepository();
                return m_signFlowAssignDeptRepository;
            }
        }
        //表單簽核設計流程Repository
        private SignFlowDesignRepository m_signFlowDesignRepository;
        public SignFlowDesignRepository SignFlowDesignRepository
        {
            get
            {
                if (m_signFlowDesignRepository == null) m_signFlowDesignRepository = new SignFlowDesignRepository();
                return m_signFlowDesignRepository;
            }
        }
        protected Enum m_formType;
        //public abstrac LoginRepo { get; set; }
		protected NameValueCollection m_nvcOrder;
		public NameValueCollection nvcOrder {
			get {
				if (m_nvcOrder == null) {
					m_nvcOrder = new NameValueCollection();
				}
				return m_nvcOrder;
			}
			set { m_nvcOrder = value; }
		}

		protected NameValueCollection m_nvcOther;
		public NameValueCollection nvcOther {
			get {
				if (m_nvcOther == null) {
					m_nvcOther = new NameValueCollection();
				}
				return m_nvcOther;
			}
			set { m_nvcOther = value; }
		}

		protected IList<SignFlowDesignModel> m_etyDesign;
        public IList<SignFlowDesignModel> etyDesign
        {
			get { return m_etyDesign; }
			set { m_etyDesign = value; }
		}

		protected IList<SignFlowFormLevelModel> m_ListFormLevel;
        public IList<SignFlowFormLevelModel> ListFormLevel
        {
			get {
				if ((m_ListFormLevel == null)) {
					m_ListFormLevel = getSignFlow_FormLevels();
				}
				return m_ListFormLevel;
			}
			set { m_ListFormLevel = value; }
		}
		#endregion

        public virtual IList<SignFlowRecModel> LoadSigningFlow(string formNumber, DefaultEnum.IsUsed isUsed)
        {
            var _tbsignFlowRec = SignFlowRecRepository.GetSignFlowRecOrderBySignOrder(formNumber, isUsed.ToString()).ToList();
            IList<SignFlowRecModel> _signFlowRec = new List<SignFlowRecModel>();
            foreach (var _signFlow in _tbsignFlowRec) {
                _signFlowRec.Add(AutoMapper.Mapper.DynamicMap<SignFlowRecModel>(_signFlow));
            }
            return _signFlowRec;
        }

        /// <summary>
        /// Desc: 預設不為空的欄位
        /// Author: JerryLin
        /// Date: 2015/09/09
        /// </summary>
        /// <param name="signFlowRec">簽核清單</param>
        /// <returns>簽核清單</returns>
        protected IList<SignFlowRecModel> SetNotNnullColumn(IList<SignFlowRecModel> signFlowRec)
        {
            if (signFlowRec != null && signFlowRec.Count > 0) {
                for (int i = 0; i <= signFlowRec.Count - 1; i++) {
                    if (string.IsNullOrEmpty(signFlowRec[i].FormType))signFlowRec[i].FormType=this.CurrentFormType.ToString();
                    if (string.IsNullOrEmpty(signFlowRec[i].SignStatus)) signFlowRec[i].SignStatus = "W";
                    if (string.IsNullOrEmpty(signFlowRec[i].SignType)) signFlowRec[i].SignType = "P";
                    if (signFlowRec[i].CDate==null) signFlowRec[i].CDate = DateTime.Now;
                    if (signFlowRec[i].MDate==null) signFlowRec[i].MDate = DateTime.Now;
                    if (string.IsNullOrEmpty(signFlowRec[i].IsUsed)) signFlowRec[i].IsUsed = "Y";
                }
            }
            return signFlowRec;
        }

        protected IList<SignFlowRecModel> UpdateSNColumn(IList<SignFlowRecModel> signFlow)
		{
            string sSEQ = SignFlowSeqRepository.GetSignFlowSeq();
            SignFlowSeqRepository.SaveChanges();
            sSEQ = (int.Parse(sSEQ) + 1000000000).ToString();
			int iRowCount = 0;
            var _temp =SignFlowRecRepository.GetSignFlowRecByGroupID(signFlow.First().FormNumber,"Y",signFlow.First().GroupID.ToString());
            if ((_temp != null) && _temp.Count() > 0)
            {
                iRowCount = _temp.Count();
			} else {
				iRowCount = 0;
			}
			for (int i = 0; i <= signFlow.Count - 1; i++) {
				//最終 SN 格式(14碼) = 前置兩碼 & 流水號(8碼) & 筆數(2碼)
				//111000000101, 121000000202
				if (signFlow[i].DataState == DefaultEnum.SignFlowDataStatus.Add.ToString()) {
					iRowCount += 1;
					signFlow[i].ID = (iRowCount + 10).ToString() + sSEQ + iRowCount.ToString().PadLeft(2, Convert.ToChar("0"));
					//SignFlow.Rows(i).Item("SN") = (i + 10).ToString & sSEQ & iRowCount.ToString.PadLeft(2, CChar("0"))
				}
			}
			return signFlow;
		}

		public Enum CurrentFormType {
			get {
                //if ((this.m_formType == null)) {
                //    throw new System.Exception("尚未設定表單類型，無法執行作業。");
                //}
				return m_formType;
			}
		}


		protected void UpdDBSignOrder(string signOrder, NameValueCollection nvcOrder, NameValueCollection nvcOther)
		{
            IQueryable<TEntity> _lSignFlow = SignFlowRecRepository.GetSignFlowRecByOverSignOrder(nvcOther.Get("FORM_NUMBER").ToString(), DefaultEnum.IsUsed.Y.ToString(), signOrder);
            if (_lSignFlow != null && _lSignFlow.Count() > 0)
            {
                foreach (var _entity in _lSignFlow)
                {
                    SignFlowRecRepository.UpdateSingOrder(_entity, nvcOther.Get("M_USER").ToString(), int.Parse(nvcOrder.Get(signOrder)));
                }
            }
		}

        public void removeSameSigner(IList<SignFlowRecModel> _signList)
        {
            List<int> _removeIndex = new List<int>();
            int i = 0, j = 0;
            while (true)
            {
                while (i < _signList.Count && (_removeIndex.Contains(i) || _signList[i].IsUsed == "N" || _signList[i].SignStatus == "A" || _signList[i].SignStatus == "R"))
                {
                    i++;
                }
                j = i + 1;
                while (j < _signList.Count && (_removeIndex.Contains(j) || _signList[j].IsUsed == "N" || _signList[i].SignStatus == "A" || _signList[j].SignStatus == "R"))
                {
                    j++;
                }

                if (i >= _signList.Count || j >= _signList.Count) break;

                if(_signList[i].SignerID == _signList[j].SignerID)
                {
                    if(_signList[i].SignType == "S" && _signList[j].SignType != "S")
                    {
                        _removeIndex.Add(j);
                    }
                    else if (_signList[i].SignType != "S")
                    {
                        _removeIndex.Add(i);
                        i = j;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }

            _removeIndex = _removeIndex.OrderByDescending(x => x).ToList();
            foreach(int index in _removeIndex)
            {
                _signList.RemoveAt(index);
            }
        }

		/// <summary>
		/// Desc: 檢查是否有簽核層級
		/// </summary>  
		/// <param name="_signList">簽核流程</param> 
		/// <returns>回傳: Boolean。</returns>
        public bool chkSignListCnt(IList<SignFlowRecModel> _signList)
		{
            if (_signList.Count == 0 || (_signList.Count == 1 && _signList[0].SignType == "S"))
            {
				throw new Exception("無任何簽核層級！");
			}
			return true;
		}

        #region "動態簽核使用"
        public abstract IList<SignFlowRecModel> GetDefaultSignList(IFormData formData);
		/// <summary>
		/// Desc: 透過傳入的 Entity 取得表單資料並建立簽核清單
		/// </summary>
		/// <param name="enty"></param>
        public virtual IList<SignFlowRecModel> GetDefaultSignList(SignflowCreateBase enty)
		{
			return GetDefaultSignList(this.GetFormData(enty));
		}

		/// <summary>
		/// Desc: 透過傳入的 Entity 取得表單資料
		/// </summary>
        public abstract IFormData GetFormData(SignflowCreateBase enty);

		/// <summary>
		/// 將設計檔，轉成簽核清單(Sign List)
		/// </summary>
		/// <param name="entyDesign">簽核流程設計檔</param>
		/// <param name="etyInitData"></param>
		/// <returns>DataTable</returns>
        public IList<SignFlowRecModel> ConvertSignRec(IList<SignFlowDesignModel> entyDesign, SignflowCreateBase etyInitData)
		{
			try {
                IList<SignFlowRecModel> _listSignFlow = new List<SignFlowRecModel>();

		    	string[] _signerId = null;
				//應簽核人員
				//'依設計檔逐筆寫入簽核清單
                int i = 0;
                foreach (SignFlowDesignModel _design in entyDesign)
                {
                    i++;
                    _signerId = GetSignerId(_design, etyInitData);
                    if (_signerId != null)
                    {
                        SignFlowRecModel _signFlowRec = new SignFlowRecModel();
                        _signFlowRec.ID = i.ToString();
                        _signFlowRec.FormNumber = etyInitData.FormNumber;
                        //NEW
                        _signFlowRec.FormType = this.CurrentFormType.ToString();
                        _signFlowRec.FormLevelID = _design.FormLevelID;
                        //表單層級編號
                        _signFlowRec.SignOrder = i.ToString();
                        _signFlowRec.SenderID = etyInitData.CUser;
                        //NEW
                        _signFlowRec.SignStatus = "W";
                        //NEW
                        _signFlowRec.SignType = _signerId[0];
                        //NEW
                        _signFlowRec.SignerID = _signerId[1];
                        //簽核人員 所屬公司別
                        _signFlowRec.SignCompanyID = _design.SignCompanyID;
                        //申請人員 所屬公司別
                        _signFlowRec.SenderCompanyID = etyInitData.CompanyID;
                        //應簽核人員
                        _signFlowRec.IsUsed = "Y";
                        //NEW
                        _signFlowRec.CUser = etyInitData.CUser;
                        _signFlowRec.CDate = DateTime.Now;
                        //NEW
                        _signFlowRec.MUser = etyInitData.CUser;
                        _signFlowRec.MDate = DateTime.Now;
                        _signFlowRec.DataState = DefaultEnum.SignFlowDataStatus.Add.ToString();
                        //NEW
                        _signFlowRec.OrgSignerID = _signerId[1];
                        _listSignFlow.Add(_signFlowRec);
                    }
				}
				return _listSignFlow;
			} catch (System.Exception ex) {
				throw new Exception("設計檔轉成簽核清單發生錯誤：" + ex.Message);

			}
		}

		/// <summary>
		/// Desc: 查詢部門主管
		/// </summary>
		/// <param name="DeptID">部門編號</param>
		/// <returns>String</returns>
        protected virtual string getDeptManagerId(string DeptID)
        {
            try
            {
                DeptRepository _repo = new DeptRepository();
                return _repo.GetManagerID(DeptID);
            }
            catch (System.Exception ex)
            {
                throw new Exception("查詢部門主管發生錯誤：" + ex.Message);
            }
        }

		/// <summary>
		/// Desc: 檢查簽核流程之應簽人員是否均有值,無值則移除
		/// </summary>  
        /// <param name="signFlowRec">預設簽核流程</param> 
		/// <returns>Boolean</returns>
        public bool chkSignFlowSignerID(IList<SignFlowRecModel> signFlowRec)
		{
            try
            {
                if (signFlowRec.Count > 0)
                {
                    for (int i = signFlowRec.Count - 1; i >= 0; i--)
                    {
                        if (string.IsNullOrEmpty(signFlowRec[i].SignerID))
                        {
                            signFlowRec.RemoveAt(i);
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                throw new Exception("檢查簽核流程之應簽人員是否均有值發生錯誤：" + ex.Message);
            }
		}

		/// <summary>
		/// 取得表單簽核層級
		/// </summary>
        /// <returns>IList<SignFlowFormLevelModel></returns>
        protected IList<SignFlowFormLevelModel> getFormLevelInfo()
        {

            IList<SignFlowFormLevelModel> _resultList = new List<SignFlowFormLevelModel>();
            try
            {
                var _dbData = SignFlowFormLevelRepository.GetSignFlowFormLevelByFormType(this.CurrentFormType.ToString());
                foreach (var _temp in _dbData)
                {
                    _resultList.Add(AutoMapper.Mapper.DynamicMap<SignFlowFormLevelModel>(_temp));
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("取得表單簽核層級發生錯誤：" + ex.Message);
            }
            return _resultList;
        }
		/// <summary>
		/// 儲存簽核流程
		/// </summary>
		/// <param name="signFlow">簽核流程</param>
		/// <param name="CurrentOrderNum">執行核可之簽核層級的SignOrder</param>
		/// <returns>Boolean</returns>
        public virtual bool SaveSigningFlow(IList<SignFlowRecModel> signFlow, string CurrentOrderNum)
        {
            if (signFlow == null || signFlow.Count == 0)
            {
                throw new Exception("沒有可寫入的簽核流程！");
            }

            try
            {
                //取得SN流水號
                signFlow = this.UpdateSNColumn(signFlow);
                signFlow = SetNotNnullColumn(signFlow);

                //設定SignOrder
                signFlow = SetSignOrder(signFlow, CurrentOrderNum);
                //dtSignFlow.PrimaryKey = new DataColumn[] { dtSignFlow.Columns["SN"] };

                for (int i = 0; i <= signFlow.Count - 1; i++)
                {
                    switch (signFlow[i].DataState)
                    {
                        case "Add":
                            //新增簽核清單主檔資料
                            AddSignFlowRec(signFlow[i]);
                            break;
                        case "Modify":
                            UpdSignRec(signFlow[i]);
                            break;
                        default:
                            break;
                        //do nothing
                    }
                }
                SignFlowRecRepository.SaveChanges();
                return true;
            }
            catch (System.Exception ex)
            {
                throw new Exception("儲存簽核流程發生錯誤：" + ex.Message);
            }

        }

		/// <summary>
		/// 寫入至各簽核清單
		/// </summary>
        /// <param name="_signFlowRec">欲新增之資料</param>
		/// <returns>Boolean</returns>
        protected bool AddSignFlowRec(SignFlowRecModel _signFlowRec)
		{
			try {
                SignFlowRecRepository.Create(AutoMapper.Mapper.DynamicMap<TEntity>(_signFlowRec));
				return true;
			} catch (System.Exception ex) {
				throw new Exception("寫入簽核清單發生錯誤：" + ex.Message);
			}
		}

		/// <summary>
		/// 更新簽核清單
		/// </summary>
        /// <param name="_signFlowRec">欲更新之資料</param>
		/// <returns>Boolean</returns>
        protected bool UpdSignRec(SignFlowRecModel _signFlowRec)
		{
			try {
                SignFlowRecRepository.Update(AutoMapper.Mapper.DynamicMap<TEntity>(_signFlowRec));
				return true;

			}  catch (System.Exception ex) {
				throw new Exception("更新簽核清單發生錯誤：" + ex.Message);
			}

		}

		/// <summary>
		/// 取得表單層級檔資料
		/// Author: JerryLin
		/// Date: 2015/09/09
		/// </summary>
		/// <returns>表單簽核層級</returns>
		protected IList<SignFlowFormLevelModel> getSignFlow_FormLevels()
		{
			//取得表單層級檔資料
            SignFlowFormLevelRepository _repo = new SignFlowFormLevelRepository();
            IList<SignFlowFormLevelModel> _SignFlowFormLevel = new List<SignFlowFormLevelModel>();
            var _dbData = _repo.GetSignFlowFormLevelByFormType(this.CurrentFormType.ToString());
            if (_dbData == null || _dbData.Count == 0)
            {
                throw new Exception("無法取得表單簽核層級資料！");
            }
            else
            {
                foreach (var _temp in _dbData)
                {
                    _SignFlowFormLevel.Add(AutoMapper.Mapper.DynamicMap<SignFlowFormLevelModel>(_temp));
                }
            }
            return _SignFlowFormLevel;
		}

		/// <summary>
		/// 取得層級檔資料
		/// Author: JerryLin
		/// Date: 2015/09/09
		/// </summary>
        /// <returns>簽核層級</returns>
        protected IList<SignFlowLevelModel> getSignFlow_Levels()
		{
			//取得層級檔資料
            SignFlowLevelRepository _repo = new SignFlowLevelRepository();
            IList<SignFlowLevelModel> _SignFlowLevel =new  List<SignFlowLevelModel>();
            var _dbData = _repo.GetIsUsedSignFlowLevel();
            if (_dbData == null || _dbData.Count == 0)
            {
                throw new Exception("無法取得簽核層級資料！");
            }
            else { 
                foreach(var _temp in _dbData ){
                    _SignFlowLevel.Add(AutoMapper.Mapper.DynamicMap<SignFlowLevelModel>(_temp));
                }
            }
            return _SignFlowLevel;
		}

		/// <summary>
		/// 設定簽核順序
		/// </summary>
		/// <param name="signFlowRec">簽核清單</param>
		/// <param name="CurrentOrderNum">執行核可之簽核層級的SignOrder</param>
		/// <returns>DataTable</returns>
        protected IList<SignFlowRecModel> SetSignOrder(IList<SignFlowRecModel> signFlowRec, string CurrentOrderNum)
		{
			NameValueCollection nvcTmp = new NameValueCollection();
			string rowSignOrder = null;
			string rowSignOrderNum = null;
            try
            {
                if (!string.IsNullOrEmpty(CurrentOrderNum))
                {
                    int iRowCount = 0;

                    //取得層級檔資料
                    IList<SignFlowLevelModel> entyLevels = null;
                    entyLevels = getSignFlow_Levels();

                    //取得表單層級檔資料
                    IList<SignFlowFormLevelModel> entyFLevels = null;
                    entyFLevels = getSignFlow_FormLevels();

                    int iFormLevelId = 0;
                    for (int i = 0; i <= signFlowRec.Count - 1; i++)
                    {
                        iFormLevelId = Convert.ToInt32(signFlowRec[i].FormLevelID);
                        if (signFlowRec[i].DataState == DefaultEnum.SignFlowDataStatus.Add.ToString())
                        {
                            iRowCount += 1;
                            signFlowRec[i].SignOrder = CurrentOrderNum + iRowCount.ToString().PadLeft(2, Convert.ToChar("0"));
                        }
                    }

                }
                else
                {
                    //第一次送審，簽核順序 等於 序號
                    for (int i = 0; i <= signFlowRec.Count - 1; i++)
                    {
                        if (signFlowRec[i].DataState == DefaultEnum.SignFlowDataStatus.Add.ToString())
                        {
                            signFlowRec[i].SignOrder = signFlowRec[i].ID;
                        }
                    }
                }
                return signFlowRec;
            }
            catch (System.Exception ex)
            {
                throw new Exception("設定簽核順序發生錯誤：" + ex.Message);
            }

		}

        private string[] GetSignerId(SignFlowDesignModel design, SignflowCreateBase etyInitData)
        {
            if(OnGetSignerId != null)
            {
                return OnGetSignerId(design, etyInitData);
            }
            else
            {
                return getSignerId(design, etyInitData);
            }
        }

        protected Func<SignFlowDesignModel, SignflowCreateBase, string[]> OnGetSignerId;

		/// <summary>
		/// 取得應簽核人員id
		/// </summary>
		/// <param name="design">設計檔</param>
		/// <param name="etyInitData">The ety init data.</param>
		/// <returns>String</returns>
		private string[] getSignerId(SignFlowDesignModel design, SignflowCreateBase etyInitData)
		{
            string[] _tmpSignerId = { "P", null };
			//應簽核人員
            string _signDeptType = (design.DeptType == null ? null : design.DeptType.ToString());
			string _dept = null;
			if (!string.IsNullOrEmpty(_signDeptType)) {
				switch (_signDeptType) {
					case "5":
					case "4":
					case "3":
					case "2":
						if (etyInitData.DeptID.Length < Convert.ToInt32(_signDeptType)) {
							throw new Exception("無法判斷應簽核之部門！");
						}

                        _dept = etyInitData.DeptID.Substring(0, Convert.ToInt32(_signDeptType));
						try {
							_tmpSignerId[1] = this.getDeptManagerId(_dept);
						} catch (Exception ex) {
							throw new Exception("(部門編號：" + _dept + ")，" + ex.Message);
						}

						break;
					case "0":
						try {
							_tmpSignerId[1] = this.getDeptManagerId(design.SignDeptID);
						} catch (Exception ex) {
                            throw new Exception("(部門編號：" + design.SignDeptID + ")，" + ex.Message);
						}

						break;
					default:
						throw new Exception("無法對應的簽核部門類型！");
				}
			} else {
				_tmpSignerId[1] = design.SignerID;
			}

			return _tmpSignerId;

		}

		/// <summary>
		/// 取得流程設計表單
		/// </summary><returns></returns>
        public IList<SignFlowDesignModel> getDesignByDept(string companyId, string deptId, string CUser)
		{
            var _signFlowAssignDept = SignFlowAssignDeptRepository.GetDesignID(this.CurrentFormType.ToString(), companyId, deptId).ToList();
                
            if (_signFlowAssignDept.Where(x => x.EmpID == CUser).Count() >= 1)
            {
                _signFlowAssignDept = _signFlowAssignDept.Where(x => x.EmpID == CUser).ToList();
                _signFlowAssignDept.RemoveAll(x => x.EmpID == null);
            }
            else
            {
                _signFlowAssignDept.RemoveAll(x => x.EmpID != null);
                if (_signFlowAssignDept.Count > 1)
                {
                    _signFlowAssignDept.RemoveAll(x => x.DeptID == "ALL");
                }
            }

         

            IList<SignFlowDesignModel> _list = (from _sFAD in _signFlowAssignDept
                                                join _sFD in SignFlowDesignRepository.GetAll().Where(x => x.IsUsed == "Y").ToList() on _sFAD.DesignID equals _sFD.DesignID into tmp1
                                                from _sFD in tmp1.DefaultIfEmpty()
                                                select new SignFlowDesignModel
                                                {
                                                    ID = _sFD.ID,
                                                    DesignID = _sFD.DesignID,
                                                    FormLevelID = _sFD.FormLevelID,
                                                    DeptType = _sFD.DeptType,
                                                    SignDeptID = _sFD.SignDeptID,
                                                    SignerID = _sFD.SignerID,
                                                    SignOrder = _sFD.SignOrder,
                                                    SignCompanyID=_sFD.SignCompanyID
                                                }).OrderBy(x => x.SignOrder).ToList();
            if (_list == null ||_list.Count==0)
            {
				throw new Exception("無法取得洽談人員所屬部門之簽核授權清單！");
			}

            return _list;
		}

		/// <summary>
		/// 檢查設計檔層級是否存在
		/// </summary>
		/// <param name="design">設計檔</param>
		/// <returns>Boolean</returns>
		public bool chkDesignVaild(IList<SignFlowDesignModel> design)
		{
			IList<SignFlowLevelModel> _levals = null;
			_levals = getSignFlow_Levels();
			IList<SignFlowFormLevelModel> _fLevels = null;
			_fLevels = getSignFlow_FormLevels();

            var _query = (from a in _fLevels 
                join b in _levals on a.LevelID equals b.LevelID 
                select new{ a.FormLevelID, b.Name}).ToList();

            var _vaild = (from a in design
                         join b in _query on a.FormLevelID equals b.FormLevelID into temp1
                         from _result in temp1.DefaultIfEmpty()
                         where _result == null
                         select 1);

			if (_vaild.Any()) {
				return false;
			}
			return true;
        }

        #endregion

        public Action<SignFlowRecModel> OnFlowAccepted;
        public Action<SignFlowRecModel> OnFormApproved;
        public Action<SignFlowRecModel> OnFlowRejected;
        public Action<SignFlowRecModel> OnFormReturned;

        public virtual bool IsApprovingLevel(string formNumber, string signFlowRecId)
        {
            IList<SignFlowRecModel> _signFlow = LoadSigningFlow(formNumber, DefaultEnum.IsUsed.Y);
            SignFlowRecModel _currentFlow = _signFlow.FirstOrDefault(x => x.ID == signFlowRecId);
            return _signFlow.Last() == _currentFlow;
        }
        public bool IsFlowAllowSigning(string formNumber, string signFlowRecId)
        {
            IList<SignFlowRecModel> _signFlow = LoadSigningFlow(formNumber, DefaultEnum.IsUsed.Y);
            SignFlowRecModel _currentFlow = _signFlow.FirstOrDefault(x => x.ID == signFlowRecId);
            return (_currentFlow != null && _currentFlow.SignStatus == "W");
        }

        public virtual bool Accept(string formNumber, string signFlowRecId, string signerId, string instruction)
        {
            if (!IsFlowAllowSigning(formNumber, signFlowRecId))
            {
                throw new Exception("此關卡已被簽核或已被刪除");
            }

            IList<SignFlowRecModel> _signFlow = LoadSigningFlow(formNumber, DefaultEnum.IsUsed.Y);
            SignFlowRecModel _currentFlow = _signFlow.FirstOrDefault(x => x.ID == signFlowRecId);
            _currentFlow.ActSignerID = signerId;
            _currentFlow.Instruction = instruction;
            _currentFlow.SignDate = DateTime.Now;
            if (_currentFlow.SignType == "S")
            {
                _currentFlow.SignStatus = DefaultEnum.SignStatus.S.ToString();
            }
            else
            {
                _currentFlow.SignStatus = DefaultEnum.SignStatus.A.ToString();
            }
            _currentFlow.MUser = signerId;
            _currentFlow.MDate = DateTime.Now;
            _currentFlow.DataState = DefaultEnum.SignFlowDataStatus.Modify.ToString();
            SaveSigningFlow(_signFlow, _currentFlow.SignOrder);

            bool _isApproved = _signFlow.Last() == _currentFlow;

            if (_isApproved)
            {
                SignFlowRecModel _lastFlow = LoadSigningFlow(formNumber, DefaultEnum.IsUsed.Y)
                    .FirstOrDefault(x => x.SignStatus == DefaultEnum.SignStatus.A.ToString());
                if (OnFormApproved != null)
                {
                    OnFormApproved(_lastFlow);
                }
            }
            else
            {
                SignFlowRecModel _nextFlow = LoadSigningFlow(formNumber, DefaultEnum.IsUsed.Y)
                    .FirstOrDefault(x => x.SignStatus == DefaultEnum.SignStatus.W.ToString());
                if (_nextFlow != null)
                {
                    if (OnFlowAccepted != null)
                    {
                        OnFlowAccepted(_nextFlow);
                    }
                }
            }

            return _isApproved;
        }

        public virtual bool Reject(string formNumber, string signFlowRecId, string signerId, string instruction,
            bool isPullBack = false)
        {
            if (!IsFlowAllowSigning(formNumber, signFlowRecId))
            {
                throw new Exception("此關卡已被簽核或已被刪除");
            }

            IList<SignFlowRecModel> _signFlow = LoadSigningFlow(formNumber, DefaultEnum.IsUsed.Y)
                .OrderBy(x => x.GroupID).ThenBy(x => x.SignOrder).ToList();
            SignFlowRecModel _currentFlow = _signFlow.FirstOrDefault(x => x.ID == signFlowRecId);
            _currentFlow.ActSignerID = signerId;
            _currentFlow.Instruction = instruction;
            _currentFlow.SignDate = DateTime.Now;
            _currentFlow.SignStatus = isPullBack ? DefaultEnum.SignStatus.B.ToString() : DefaultEnum.SignStatus.R.ToString();
            _currentFlow.DataState = DefaultEnum.SignFlowDataStatus.Modify.ToString();
            _currentFlow.MUser = signerId;
            _currentFlow.MDate = DateTime.Now;

            int _index = _signFlow.IndexOf(_currentFlow);
            bool _backToSender = true;

            for (int i = _index + 1; i < _signFlow.Count(); i++)
            {
                _signFlow[i].IsUsed = DefaultEnum.IsUsed.N.ToString();
                _signFlow[i].DataState = DefaultEnum.SignFlowDataStatus.Modify.ToString();
            }

            while (_index >= 0 && _signFlow[_index].SignType != "S")
            {
                _index--;
            }

            _signFlow.Add(new SignFlowRecModel()
            {
                ID = _signFlow[_index].ID,
                FormNumber = _signFlow[_index].FormNumber,
                FormType = _signFlow[_index].FormType,
                FormLevelID = _signFlow[_index].FormLevelID,
                SignOrder = _signFlow[_index].SignOrder,
                SenderID = _signFlow[_index].SenderID,
                SignStatus = "W",
                SignType = _signFlow[_index].SignType,
                SignerID = _signFlow[_index].SignerID,
                IsUsed = "Y",
                GroupID = _currentFlow.GroupID + 1,
                CUser = _signFlow[_index].CUser,
                CDate = DateTime.Now,
                MUser = _signFlow[_index].MUser,
                MDate = DateTime.Now,
                DataState = DefaultEnum.SignFlowDataStatus.Add.ToString(),
                SignCompanyID = _signFlow[_index].SignCompanyID,
                SenderCompanyID = _signFlow[_index].SenderCompanyID
            });

            SaveSigningFlow(_signFlow, _currentFlow.SignOrder);

            SignFlowRecModel _nextFlow = LoadSigningFlow(formNumber, DefaultEnum.IsUsed.Y)
                .FirstOrDefault(x => x.SignStatus == DefaultEnum.SignStatus.W.ToString());
            if (_nextFlow != null)
            {
                if (!isPullBack && OnFormReturned != null)
                {
                    OnFormReturned(_nextFlow);
                }
            }

            return _backToSender;
        }
    }
	// FormDefaultSignList

}
//SignLists