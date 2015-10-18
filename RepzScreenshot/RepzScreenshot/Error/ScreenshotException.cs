using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepzScreenshot.Error
{
    class ScreenshotException : ApiException
    {
        public ScreenshotException(string msg) : base(msg) { }

    }
}
