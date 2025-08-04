using System;
using System.Collections;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 將例外物件資料寫入文字檔的物件。
    /// </summary>
    [Serializable]
    public class TxtFileExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// 檔案鎖定物件。
        /// </summary>
        private static Hashtable s_LockObjectConatiner = new Hashtable();

        /// <summary>
        /// 處理例外物件的功能。
        /// </summary>
        /// <param name="typeEntity">發生例外的組件類別。</param>
        /// <param name="exceptionEntity">例外物件。</param>
        /// <param name="iInfo">例外處理機制的額外資訊需求介面定義。</param>
        public void Handle(Type typeEntity, Exception exceptionEntity, IExceptionHandlerInfoData iInfo)
        {

            ITxtFileExceptionHandlerInfoData _Info = iInfo as ITxtFileExceptionHandlerInfoData;
            if (_Info == null)
            {
                return;
            }

            //<==檢查參數
            if (typeEntity == null) { return; }
            if (exceptionEntity == null) { return; }
            if (iInfo == null) { return; }
            //==>

            string _FileName = _Info.Path + "\\" + _Info.FileName;

            //設定檔案存取權限
            FileIOPermission _FileIOPermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, _FileName);
            _FileIOPermission.Assert();
            //設定檔案存取權限
            _FileIOPermission.AllFiles = FileIOPermissionAccess.Write;

            if (!Directory.Exists(_Info.Path))
            { //檢查目錄是否存在，不存在就先建立
                Directory.CreateDirectory(_Info.Path);
            }

            if (!File.Exists(_FileName))
            { //檢查檔案是否存在，不存在就先建立
                using (FileStream _FileStream = new FileStream(_FileName, FileMode.CreateNew))
                {
                }
            }
            ExceptionMessageBuilder _MessageBuilder = new ExceptionMessageBuilder(HandleType.LogTxtFile);

            if (!s_LockObjectConatiner.ContainsKey(_FileName))
            {
                s_LockObjectConatiner.Add(_FileName, new Object());
            }
            Object _LockObject = s_LockObjectConatiner[_FileName];

            //把Message寫進檔案
            Monitor.Enter(_LockObject);
            try
            {
                using (StreamWriter _StreamWriter = File.AppendText(_FileName))
                {
                    _StreamWriter.Write(_MessageBuilder.Build(typeEntity, exceptionEntity, iInfo));
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Monitor.Exit(_LockObject);
            }
        }
    }
}