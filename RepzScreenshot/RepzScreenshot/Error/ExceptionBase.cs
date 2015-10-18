using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepzScreenshot.Error
{
    class ExceptionBase : Exception
    {
        public ExceptionBase(string msg) : base(msg) { }
        public ExceptionBase() { }
    }
}
