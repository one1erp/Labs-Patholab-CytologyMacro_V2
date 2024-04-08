using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_Common;
using Patholab_DAL;

namespace Recive_Request
{
    /// <summary>
    /// Interaction logic for ReciveCtrl.xaml
    /// </summary>
    public partial class ReciveCtrl : UserControl
    {







        public ReciveCtrl(INautilusServiceProvider sp, INautilusProcessXML xmlProcessor, INautilusDBConnection _ntlsCon, IExtensionWindowSite2 _ntlsSite, INautilusUser _ntlsUser)
        {
            InitializeComponent();
            //      this.SetResourceReference(Control.BackgroundProperty, System.Drawing.Color.FromName("Control"));
            this.sp = sp;
            this.xmlProcessor = xmlProcessor;
            this._ntlsCon = _ntlsCon;
            this._ntlsSite = _ntlsSite;
            this._ntlsUser = _ntlsUser;
        }


        #region Private fields

        private INautilusProcessXML xmlProcessor;
        private INautilusUser _ntlsUser;
        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider sp;
        private INautilusDBConnection _ntlsCon;
        private SDG _currentSdg;
        private DataLayer dal;
        public bool DEBUG;



        #endregion






        public bool CloseQuery()
        {
            if (dal != null) dal.Close();

            return true;
        }





        public void CreateConnection()
        {

            try
            {
                dal = new DataLayer();

                if (DEBUG)
                {
                    dal.MockConnect();
                    _currentSdg = dal.FindBy<SDG>(x => x.SDG_ID == 266).SingleOrDefault();
                    txtInternalNbr.Text = _currentSdg.NAME;


                }
                else
                    dal.Connect(_ntlsCon);



                LoadStaticData();

            }
            catch (Exception e)
            {


                MessageBox.Show("Error in  InitializeData " + "/n" + e.Message);
                Logger.WriteLogFile(e);
            }

        }

        private int GeneratePatholabNbr()
        {
            return 0;
        }

        private void LoadStaticData()
        {

            //            Debugger.Launch();
            phraseHeaders = new List<PHRASE_HEADER>();
            SetPhrase2Combo("Gender", cmbGender);
            //   SetPhrase2Combo("Phone Code", cmbPhoneCode);
            SetPhrase2Combo("Priority", cmbPriority);
            SetPhrase2Combo("Suspend Reason", cmbSuspensioCause);








            var customers = dal.GetAll<U_CUSTOMER>().ToList();
            var clinics = dal.GetAll<U_CLINIC>().ToList();
            var suppliers = dal.GetAll<SUPPLIER>().ToList();
            //   var clients = dal.GetAll<CLIENT>().ToList();
            var otherSup = suppliers;
            cmbCustomer.ItemsSource = customers;
            cmbCustomer.DisplayMemberPath = "NAME";



            cmbSecondCustomer.ItemsSource = customers;
            cmbSecondCustomer.DisplayMemberPath = "NAME";

            CmbImplementingphysician.ItemsSource = suppliers;
            CmbImplementingphysician.DisplayMemberPath = "NAME";

            CmbReferringphysician.ItemsSource = otherSup;
            CmbReferringphysician.DisplayMemberPath = "NAME";



            cmbClinic.ItemsSource = clinics;
            cmbClinic.DisplayMemberPath = "NAME";

            CmbreferringClinic.ItemsSource = clinics;
            CmbreferringClinic.DisplayMemberPath = "NAME";
        }

        private List<PHRASE_HEADER> phraseHeaders;
        private void SetPhrase2Combo(string PhraseName, ComboBox comboBox)
        {
            try
            {


                var ph = dal.GetPhraseByName(PhraseName);
                phraseHeaders.Add(ph);
                comboBox.ItemsSource = ph.PHRASE_ENTRY;
                comboBox.DisplayMemberPath = "PHRASE_DESCRIPTION";
                comboBox.SelectedValuePath = "NAME";
            }
            catch (Exception e)
            {

                System.Windows.Forms.MessageBox.Show("Error in load " + PhraseName + " Phrase " + e.Message);
            }
        }






        public void DisplaySdg()
        {

            try
            {
                DisplaySdgGroupA();
                DisplaySdgGroupB();
                DisplaySdgGroupC();
                DisplaySdgGroupD();

            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error in Display Sdg " + e.Message);
            }

        }

        private void DisplaySdgGroupA()
        {
            var sdgUser = _currentSdg.SDG_USER;
            if (sdgUser.U_CONTAINER != null)
                txtReciveNumber.Text = sdgUser.U_CONTAINER.NAME;
            txtRecivedContainer.Text = GetCountForContainer();


            txtExternalRef.Text = _currentSdg.EXTERNAL_REFERENCE;
            txtPathoNbr.Text = sdgUser.U_PATHOLAB_NUMBER;
            cbQc.IsChecked = sdgUser.U_RECEIVE_QC == "T";
            SetSdgType();
        }

        private string GetCountForContainer()
        {
            if (_currentSdg.SDG_USER.U_CONTAINER != null)
            {
                int count = 0;
                //var count=  from item in  _currentSdg.SDG_USER.U_CONTAINER.SDG_USER where item.SDG.STATUS 
                //TODO:לשאול את זיו מה השאילתא

                string f = string.Format("התקבלו {0} /{1} הפניות מתוך קבלת המשלוח",
                                         _currentSdg.SDG_USER.U_CONTAINER.SDG_USER.Count, count);
                return f;
            }
            return "";


        }

        private void DisplaySdgGroupB()
        {
            var sdgUser = _currentSdg.SDG_USER;
            txtHospitalNbr.Text = "";//sdgUser.hospital number
            cmbClinic.SelectedValue = sdgUser.COLLECTION_STATION;


            U_ORDER currentOrder =
                dal.FindBy<U_ORDER>(ord => ord.U_ORDER_USER.U_SDG_NAME == _currentSdg.NAME).SingleOrDefault();//TODO האם יכול להיות להזמנה 2 ORDERS
            if (currentOrder != null)
            {
                cmbCustomer.SelectedValue = currentOrder.U_ORDER_USER.U_CUSTOMER1;
                cmbSecondCustomer.SelectedValue = currentOrder.U_ORDER_USER.U_CUSTOMER2;
                cbInAdvance.IsChecked = currentOrder.U_ORDER_USER.U_IN_ADVANCE == "T";
            }
            SetValueInPhrase("Priority", sdgUser.U_PRIORITY.ToString(), cmbPriority);

            cbObligation.IsChecked = sdgUser.U_NO_OBLIGATION == "T";


            txtHospitalNbr.Text = sdgUser.U_HOSPITAL_NUMBER.ToString();
            dtRecived_on.SelectedDate = _currentSdg.CREATED_ON;
        }


        private void DisplaySdgGroupC()
        {
            var client = _currentSdg.SDG_USER.CLIENT;

            var clientUser = client.CLIENT_USER;

            txtClientIdentity.Text = client.NAME;
            txtLastName.Text = clientUser.U_LAST_NAME;
            txtFirtName.Text = clientUser.U_FIRST_NAME;
            txtClientPhone.Text = clientUser.U_PHONE;
            //cmbPhoneCode.Text = TODO;

            txtAge.Text = _currentSdg.SDG_USER.U_AGE_AT_ARRIVAL.ToString();
            SetValueInPhrase("Gender", clientUser.U_GENDER, cmbGender);
            dtDateBirth.SelectedDate = clientUser.U_DATE_OF_BIRTH;
            cbPasport.IsChecked = _currentSdg.SDG_USER.U_PASSPORT == "T";

        }

        private void SetValueInPhrase(string phraseName, string entryName, ComboBox cmb)
        {

            try
            {


                var phrase = (from item in phraseHeaders where item.NAME == phraseName select item).FirstOrDefault();
                if (phrase != null && entryName != null)
                {
                    cmb.Text = phrase.PhraseEntriesDictonary[entryName];
                }


            }
            catch (Exception e)
            {

                System.Windows.Forms.MessageBox.Show("Error in SetValueInPhrase");
            }
        }

        private void DisplaySdgGroupD()
        {
            var sdgUser = _currentSdg.SDG_USER;
            CmbImplementingphysician.SelectedItem = sdgUser.IMPLEMENTING_PHYSICIAN;
            CmbReferringphysician.SelectedItem = sdgUser.REFERRING_PHYSIC;
            CmbreferringClinic.SelectedItem = sdgUser.REFERRING_PHYSIC;
            cmbSuspensioCause.SelectedItem = sdgUser.U_SUSPENSION_CAUSE;
        }

        private const string TODO = "TODO";

        private void CalculateAge(DateTime bday)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - bday.Year;

            if (bday > today.AddYears(-age))
                age--;

        }
        private void SetSdgType()
        {
            switch (_currentSdg.SdgType)
            {
                case "B":
                    rdbHis.IsChecked = true;
                    break;

                case "C":
                    rdbCyt.IsChecked = true;
                    break;

                case "P":
                    rdbPap.IsChecked = true;
                    break;
            }
        }



        #region Events of Controls

        private void TxtInternalNbr_OnKeyDown(object o, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                var tb = o as TextBox;
                if (tb.Name == "txtInternalNbr" || tb.Name == "txtBarcode")
                {
                    _currentSdg = dal.FindBy<SDG>(x => x.NAME == txtInternalNbr.Text).SingleOrDefault();
                    if (_currentSdg == null) return;
                    txtPathoNbr.Text = _currentSdg.SDG_USER.U_PATHOLAB_NUMBER;
                    DisplaySdg();

                }
                else if (tb.Name == "txtPathoNbr")
                {
                    var currentSDgUser =
                    dal.FindBy<SDG_USER>(x => x.U_PATHOLAB_NUMBER == txtPathoNbr.Text).SingleOrDefault();
                    if (currentSDgUser == null) return;
                    _currentSdg = currentSDgUser.SDG;
                    txtInternalNbr.Text = _currentSdg.NAME;
                    DisplaySdg();
                }

                if (_currentSdg == null)
                {
                    MessageBox.Show("Request doesn't Exist");

                }
            }


        }




        private void UIElement_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //Make sure sender is the correct Control.
            if (sender is TextBox)
            {
                //If nothing was entered, reset default text.
                if (((TextBox)sender).Text.Trim().Equals(""))
                {
                    ((TextBox)sender).Foreground = Brushes.Gray;
                    ((TextBox)sender).Text = "Text";
                }
            }
        }

        private void UIElement_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

            if (sender is TextBox)
            {
                //If nothing has been entered yet.
                if (((TextBox)sender).Foreground == Brushes.Gray)
                {
                    ((TextBox)sender).Text = "";
                    ((TextBox)sender).Foreground = Brushes.Black;
                }
            }
        }


        private void CbRefferingPhisician_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            var cb = sender as ComboBox;
            var supp = cb.SelectedValue as SUPPLIER;
            if (cb == null || supp == null) return;


            if (cb.Name == "CmbReferringphysician")
            {
                TxtReferringLicense.Text = supp.SUPPLIER_USER.U_LICENSE_NBR;
                TxtReferringName.Text = supp.NAME;
            }
            else if (cb.Name == "CmbImplementingphysician")
            {
                txtImplementingLicense.Text = supp.SUPPLIER_USER.U_LICENSE_NBR;
                txtImplementingName.Text = supp.NAME;
            }
        }
        #endregion

        private void CmbCustomer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var grids = MainGrid.Children.OfType<Grid>();
            foreach (Grid grid in grids)
            {
                var textboxes = grid.Children.OfType<TextBox>();
                foreach (var textBox in textboxes)
                    textBox.Text = String.Empty;
            }



        }


        private void BtnAddSupplier_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void BtnRemarks_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}