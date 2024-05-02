using Patholab_Common;
using Patholab_DAL_V1.Enums;
using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ONE1_richTextCtrl;

namespace CytologyMacro_V2Pages.Classes
{
    public class Constants
    {
        public static string DIAGNOSIS = "Diagnosis";
        public static string IMP_DIAGNOS = "Imp_Diagnos";

    }
    public class RTF_Manger
    {

        public readonly Dictionary<string, RichSpellCtrl> _result2RichText;

        public DataLayer _dal { get; set; }

        public event Action TemplatesClicked;

        public RTF_Manger(DataLayer dal, RichSpellCtrl richDiag)
        {
            this._dal = dal;

            _result2RichText = new Dictionary<string, RichSpellCtrl>
                {
                    {Constants.IMP_DIAGNOS, richDiag},
                };

            foreach (var rtCtrls in _result2RichText.Values.Distinct())
            {
                rtCtrls.ExtraStrip.Text = "Templates";
                rtCtrls.ExtraBtnClciked += Pb_ExtraBtnClciked;
                rtCtrls.RightToLeft = RightToLeft.No;
                rtCtrls.SetDefaultSpelling();
            }
        }


    
        #region Init



        private void Pb_ExtraBtnClciked()
        {
            TemplatesClicked?.Invoke();
        }



        #endregion


    }
}
