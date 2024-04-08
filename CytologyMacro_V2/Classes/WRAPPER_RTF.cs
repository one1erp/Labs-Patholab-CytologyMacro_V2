using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace CytologyMacro_V2Pages.Classes
{
    public class WrapperRtf
    {



        public long ResultId;
        public string RtfText;
        public string Name
        {
            get { return _name; }
            set
            {

                _name = value;

            }
        }

        //


        private string _name;

        public string TestName { get; set; }
        public RadControl Ctl { get; set; }

        public Patholab_DAL_V1.RESULT Result_ { get; set; }
    }
}
