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
        public SDG sdg { get; set; }

        public event Action TemplatesClicked;

        public RTF_Manger(LSSERVICEPROVIDERLib.INautilusServiceProvider sp, DataLayer dal, RichSpellCtrl richDiag)
        {
            this._dal = dal;

            this.sp = sp;

            _result2RichText = new Dictionary<string, RichSpellCtrl>
                {
                    {Constants.IMP_DIAGNOS, richDiag},
                };
            foreach (var rtCtrls in _result2RichText.Values.Distinct())
            {

                rtCtrls.ExtraStrip.Text = "Templates";
                rtCtrls.ExtraBtnClciked += Pb_ExtraBtnClciked;
                rtCtrls.AlignChanged += (direction =>
                {
                    //  pb.DocumentRtl = direction;
                });

                rtCtrls.RightToLeft = RightToLeft.No;
                rtCtrls.Load += richTxt_Load;
                rtCtrls.SetDefaultSpelling();
            }
        }


    

   

     

        private string _cytoTemplateSql = null;
        private LSSERVICEPROVIDERLib.INautilusServiceProvider sp;
        private bool frstkk;


    
        #region Init



        private void Pb_ExtraBtnClciked()
        {
            if (TemplatesClicked != null) TemplatesClicked();

        }

        void richTxt_Load(object sender, EventArgs e)
        {
            //Set default zoom to 120
            RichSpellCtrl richSpell = sender as RichSpellCtrl;
            if (richSpell != null)
            {
                //    richSpell.ZoomTo(1.8F);


            }
        }

        internal void EnableControls(bool p)
        {
            foreach (KeyValuePair<string, RichSpellCtrl> rt in _result2RichText)
            {
                rt.Value.Enabled = p;
            }
        }

        internal void ClearScreen()
        {
            //foreach (var rt in pages)
            //{
            //    rt.Text = string.Empty;
            //}

            foreach (KeyValuePair<string, RichSpellCtrl> rtCtrls in _result2RichText)
            {
                rtCtrls.Value.Text = string.Empty;
                rtCtrls.Value.ClearText();// = string.Empty;
            }
        }

        #endregion


    }
}
