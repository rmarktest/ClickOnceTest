using RepzScreenshot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepzScreenshot.Error
{
    class UserOfflineException : ApiException
    {
        public UserOfflineException(Player p) : base(String.Format("{0} is offline", p.Name)) { }
    }
}
