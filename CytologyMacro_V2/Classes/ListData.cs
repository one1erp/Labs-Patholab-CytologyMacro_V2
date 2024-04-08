using Patholab_Common;
using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CytologyMacro_V2Pages.Classes
{
    public class ListData
    {
        private DataLayer dal; 
        public List<U_NORGAN_USER> CytoOrgans { get; set; }
        public List<OPERATOR> Doctors { get; set; }
        public List<PHRASE_HEADER> PhraseHeaders;
        public List<PHRASE_ENTRY> ColorPhrase;
        public List<PHRASE_ENTRY> CytoNextStep;
        public List<PHRASE_ENTRY> CytoSlideType;
        public List<PHRASE_ENTRY> LiquidTypePhrase;

        public ListData(DataLayer _dal)
        {
            this.dal = _dal;
            LoadDoctors();
        }


        private void LoadDoctors()
        {
            string q = "";
            try
            {
                var qDoctors =
                    dal.FindBy<OPERATOR>(o => o.LIMS_ROLE.NAME == "DOCTOR" && o.OPERATOR_USER.U_ORDER > 0).Include(x => x.OPERATOR_USER).OrderBy(x => x.NAME);
        
                q = qDoctors.ToString();
                Doctors = qDoctors.ToList();
            }
            catch (Exception e)
            {
                Logger.WriteQueries(e.Message + " נופל אצל זיו " + " " + q);
            }
        }

        public void LoadCytoData()
        {
            //Cytology
            CytoOrgans = dal.FindBy<U_NORGAN_USER>(o => o.U_ORGAN_TYPE == "C").OrderBy(x => x.U_ORGAN_HEBREW_NAME).ToList();
            ColorPhrase = dal.GetPhraseEntries("Color").ToList();
            CytoNextStep = dal.GetPhraseEntries("Cytology Next Step").ToList();
            LiquidTypePhrase = dal.GetPhraseEntries("Liquid Type").ToList();
            CytoSlideType = dal.GetPhraseEntries("Cytology Slide Type").ToList();
        }
    }
}
