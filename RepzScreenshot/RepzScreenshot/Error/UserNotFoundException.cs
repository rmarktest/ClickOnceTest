using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepzScreenshot.Error
{
    class UserNotFoundException : ApiException
    {

        public UserNotFoundException():base("User could not be found") { }

    }
}
