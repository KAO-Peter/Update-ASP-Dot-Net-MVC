using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.SignLists
{
    public class DefaultEnum
    {
        public enum SignFlowDataStatus
        {
            Add,
            Modify
        }
        public enum SignStatus
        {
            W,//等待簽核
            A,//已簽核
            R,//駁回
            S,//送出
            B,//修改
        }
        public enum IsUsed
        {
            Y,
            N
        }
    }
}
