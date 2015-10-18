using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepzScreenshot.Model
{
    class PlayerModel : ModelBase
    {

        private int _id;
        private string _name;


        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                if(value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
    }
}
