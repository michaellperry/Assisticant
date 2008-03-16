using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assisticant.Collections;
using Assisticant.Fields;

namespace $rootnamespace$.Models
{
    public class Item
    {
        private Observable<string> _name = new Observable<string>();

        public string Name
        {
            get { return _name; }
            set { _name.Value = value; }
        }
    }
}
