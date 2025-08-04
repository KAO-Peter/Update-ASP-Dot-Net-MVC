using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Collections.Specialized;
using YoungCloud.Configurations;
using YoungCloud.SignFlow.Model;
using System;

namespace YoungCloud.SignFlow.Conditions
{
	public abstract class ConditionHandler : ClassBase
	{
		protected ConditionHandler _Handler;
		private bool _MatchCondition = false;

		public ConditionHandler MatchObject = null;

        public ConditionHandler()
		{
			//nothing
		}

		public bool MatchCondition {
			get { return _MatchCondition; }
			set {
				if (value != _MatchCondition) {
					_MatchCondition = value;
					if (value == true && (MatchObject == null)) {
						MatchObject = this;
					}
					if ((this._Handler != null)) {
						_Handler.MatchObject = MatchObject;
						_Handler.MatchCondition = value;
					}
				}
			}
		}

		/// <summary>
		/// Desc: 設定下一個檢核處理類別
		/// </summary>
		public void SetNextConditionHandler(ConditionHandler CurrentHandler)
		{
			this._Handler = CurrentHandler;
		}

		/// <summary>
		/// Desc: 檢查簽核條件是否符合
		/// </summary>
		/// <param name="signList">原始簽核清單</param>
		/// <param name="checkRowIndex">要調整之簽核項目索引值</param>
		/// <param name="formData">表單資料</param>
		/// <returns>調整過的簽核清單</returns>
        public abstract IList<SignFlowDesignModel> CheckConditionItem(IList<SignFlowDesignModel> signList, int checkRowIndex, IFormData formData);

        protected virtual void VerifyNothing(IList<SignFlowDesignModel> signList, IFormData formData)
		{
            if (signList == null || signList.Count == 0) throw new Exception("原始簽核清單不存在！");
            if (formData == null) throw new Exception("表單資料不存在！");
            //this.CheckDataInstance(SignList, "原始簽核清單不存在！", true);
            //this.CheckDataInstance(FormData, "表單資料不存在！", true);
		}

		/// <summary>
		/// Desc: 條件符合時應執行的動作
		/// </summary>
		/// <param name="signList">原始簽核清單</param>
		/// <param name="CheckRowIndex">要調整之簽核項目索引值</param>
		/// <returns>調整過的簽核清單</returns>
        protected virtual IList<SignFlowDesignModel> RunMatchAction(IList<SignFlowDesignModel> signList, int CheckRowIndex)
		{
			//SignList.Rows(CheckRowIndex).Item("IS_USED") = "Y"
			this.MatchCondition = true;
			return signList;
		}

		/// <summary>
		/// Desc: 移除的動作
		/// </summary>
		/// <param name="signList">原始簽核清單</param>
		/// <param name="CheckRowIndex">要調整之簽核項目索引值</param>
		/// <returns>調整過的簽核清單</returns>
        protected virtual IList<SignFlowDesignModel> RunRemoveAction(IList<SignFlowDesignModel> signList, int CheckRowIndex)
		{
			//SignList.Rows(CheckRowIndex).Item("IS_USED") = "Y"          
			signList.RemoveAt(CheckRowIndex);
			this.MatchCondition = true;
			return signList;
		}

		/// <summary>
		/// Desc: 條件不符合時應執行的動作
		/// </summary>
		/// <param name="signList">原始簽核清單</param>
		/// <param name="CheckRowIndex">要調整之簽核項目索引值</param>
        /// <param name="formData">表單資料</param>
		/// <returns>調整過的簽核清單</returns>
        protected virtual IList<SignFlowDesignModel> RunNoMatchAction(IList<SignFlowDesignModel> signList, int CheckRowIndex, IFormData formData)
		{
			this.MatchCondition = false;
			if ((this._Handler != null)) {
				//如果有下一個檢核處理項目，就丟給下一個去做
				signList = this._Handler.CheckConditionItem(signList, CheckRowIndex, formData);
			} else {
				//沒有下一個處理項目，表示指定索引之簽核項目不用做，要移除
				signList.RemoveAt(CheckRowIndex);
			}

			return signList;
		}

		/// <summary>
		/// Desc: 下一個檢核的動作
		/// </summary>
		/// <param name="signList">原始簽核清單</param>
		/// <param name="checkRowIndex">要調整之簽核項目索引值</param>
        /// <param name="formData">表單資料</param>
		/// <returns>調整過的簽核清單</returns>
        protected virtual IList<SignFlowDesignModel> RunNextConditionAction(IList<SignFlowDesignModel> signList, int checkRowIndex, IFormData formData)
		{
			this.MatchCondition = false;
			if ((this._Handler != null)) {
				//如果有下一個檢核處理項目，就丟給下一個去做
				signList = this._Handler.CheckConditionItem(signList, checkRowIndex, formData);
                ////Else
                ////沒有下一個處理項目，表示指定索引之簽核項目不用做，要移除
                ////SignList.Rows.RemoveAt(CheckRowIndex)
			}

			return signList;
		}
	}
}
