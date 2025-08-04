using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.Exceptions;

namespace YoungCloud.Databases.Exceptions
{
    [Serializable]
    public partial class DbEntityValidationException : ExceptionBase
    {

        public DbEntityValidationException(string message, Exception innerException) : base(message, innerException) { }

    }
}
