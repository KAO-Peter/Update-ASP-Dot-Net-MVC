using LinqKit;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.Models
{
    public class JQGridRequest
    {
        public bool _search { get; set; }
        public string nd { get; set; }
        public int rows { get; set; }
        public int page { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public string keyword { get; set; }
        public string[] colModel { get; set; }
        public string[] colSearch { get; set; }
        public JqGridFilter filters { get; set; }
        public string queryMode { get; set; }
    }

    public class JqGridFilter
    {
        /// <summary>
        /// 所有
        /// </summary>
        public const string FILTER_GROUP_OP_AND = "AND";
        /// <summary>
        /// 任一
        /// </summary>
        public const string FILTER_GROUP_OP_OR = "OR";
        public string groupOp { get; set; }
        public JqGridRule[] rules { get; set; }
        public JqGridFilter[] groups { get; set; }
    }

    public class JqGridRule
    {
        /// <summary>
        /// 等於
        /// </summary>
        public const string RULE_OP_EQ = "eq";
        /// <summary>
        /// 不等於
        /// </summary>
        public const string RULE_OP_EN = "en";
        /// <summary>
        /// 包含
        /// </summary>
        public const string RULE_OP_CN = "cn";
        public string field { get; set; }
        public string op { get; set; }
        public string data { get; set; }
    }
}
