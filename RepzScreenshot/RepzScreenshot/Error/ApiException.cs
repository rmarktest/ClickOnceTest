using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepzScreenshot.Error
{
    class ApiException : ExceptionBase
    {
        public ApiException(string msg) : base(msg) { }
    }
}
