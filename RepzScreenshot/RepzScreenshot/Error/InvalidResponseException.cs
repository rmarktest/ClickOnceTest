using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepzScreenshot.Error
{
    class InvalidResponseException : ApiException
    {
        public InvalidResponseException() : base("Invalid response from server") { }
    }
}
