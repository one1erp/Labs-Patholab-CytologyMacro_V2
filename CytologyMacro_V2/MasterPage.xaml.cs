﻿using CytologyMacro_V2.Annotations;
using CytologyMacro_V2Pages.Classes;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using ONE1_richTextCtrl;
using Oracle.ManagedDataAccess.Client;
using Patholab_Common;
using Patholab_DAL_V1;
using Patholab_DAL_V1.Enums;
using Patholab_XmlService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;


namespace CytologyMacro_V2Pages
{

    public partial class MasterPage : UserControl
    {

        #region Private fields

        public INautilusServiceProvider ServiceProvider { get; set; }
        private INautilusProcessXML xmlProcessor;
        private INautilusUser _ntlsUser;
        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider sp;
        private INautilusDBConnection _ntlsCon;
        private DataLayer dal;
        public bool DEBUG;
        private List<PHRASE_ENTRY> RotherStatus;
        private SDG_USER sdg;
        private U_DEBIT_USER debit;
        private Timer _timerFocus;
        private long? _operator_id;
        private double _session_id;

        public System.Collections.ObjectModel.ObservableCollection<CytoDetails> SampleDetails { get; set; }
        public ListData listData { get; set; }
        public List<PHRASE_ENTRY> ListColor { get; set; }
        public List<U_NORGAN_USER> ListOrgans { get; set; }
        public List<PHRASE_ENTRY> ListCytoSlideType { get; set; }
        public List<PHRASE_ENTRY> ListNextStep { get; set; }

        private bool hasCellBlock = false;
        public SDG CurrentSdg { get; set; }

        private SAMPLE currentSample;

        private RTF_Manger _rtfManager;

        public event Func<DateTime?, bool> RequestDateChanged;

        private RichSpellCtrl richTextMacro;

        public Dictionary<string, RichSpellCtrl> _result2RichText;

        private List<int> indexesToAddCellBlock = new List<int>();

        private string _cytoTemplateSql = null;

        private long resId;


        #endregion


        #region initilaizing functions

        public MasterPage()
        {
            InitializeComponent();
            //FirstFocus();


        }

        public MasterPage(INautilusServiceProvider sp, INautilusProcessXML xmlProcessor, INautilusDBConnection _ntlsCon,
            IExtensionWindowSite2 _ntlsSite, INautilusUser _ntlsUser)
        {
            InitializeComponent();
            this.ServiceProvider = sp;
            this.sp = sp;
            this.xmlProcessor = xmlProcessor;
            this._ntlsCon = _ntlsCon;
            this._ntlsSite = _ntlsSite;
            this._ntlsUser = _ntlsUser;
            this.DataContext = this;
        }

        public void Initilaize()
        {
            dal = new DataLayer();

            if (DEBUG)
            {
                dal.MockConnect();
                _operator_id = 1;
                _session_id = 1;

            }
            else
            {
                dal.Connect(_ntlsCon);
                _operator_id = (long)_ntlsUser.GetOperatorId();
                _session_id = _ntlsCon.GetSessionId();

                //Task task1 = Task.Run(() => { InitializeDataInXaml(); });

                InitializeDataInXaml();

            }

        }

        public void InitializeDataInXaml()
        {
            initRichTextControls();

            SampleDetails = new System.Collections.ObjectModel.ObservableCollection<CytoDetails>();

            listData = new ListData(dal);
            listData.LoadCytoData();
            ListColor = listData.ColorPhrase;
            ListCytoSlideType = listData.CytoSlideType;
            ListNextStep = listData.CytoNextStep;
            ListOrgans = listData.CytoOrgans;

            cmpPatholog.ItemsSource = listData.Doctors;
            cmpPatholog.DisplayMemberPath = "OPERATOR_USER.U_HEBREW_NAME";
            cmpPatholog.SelectedValuePath = "OPERATOR_ID";
        }


        #endregion

        #region richtext functions        
        private void initRichTextControls()
        {
            if (richTextMacro == null) richTextMacro = new RichSpellCtrl();

            richTextMacro.SetDefaultSpelling();
            richTextMacro.DocumentRtl = RightToLeft.Yes;

            _rtfManager = new RTF_Manger(sp, dal, richTextMacro);
            _rtfManager.TemplatesClicked += RtfManager_tmcclicked;

            winformsHostMacro.Child = richTextMacro;
        }

        private void RtfManager_tmcclicked()
        {
            if (CurrentSdg == null)
            {
                return;
            }

            var sdg = dal.FindBy<SDG>(s => s.SDG_ID == CurrentSdg.SDG_ID).FirstOrDefault();

            #region extracted to another class

            string All_organs = "ALL";
            string l = sdg.NAME[0].ToString();

            //If has organ get it, else get "All
            var organs4Sdg = sdg.SAMPLEs.Select(SM => SM.SAMPLE_USER.U_ORGAN ?? All_organs).Distinct().ToList();
            if (!organs4Sdg.Contains(All_organs))
                organs4Sdg.Add(All_organs);

            var organsList = (from item in dal.GetAll<U_NLIST_USER>()
                              where item.U_SDG_TYPE == l
                              select item).ToList();

            List<U_NLIST_USER> list4Show = new List<U_NLIST_USER>();
            foreach (var item in organsList)
            {
                foreach (var org in organs4Sdg)
                {
                    if (item.U_ORGANS != null && item.U_ORGANS.Contains(org))
                    {
                        list4Show.Add(item);
                    }
                }
            }

            var organs4Show = list4Show.Select(SM => SM.U_TEXT).ToList().Distinct();
            if (organs4Show.Count() < 1)
            {

                MessageBox.Show(".לא קיימים טמפלייטים");
                return;
            }

            #endregion

            TextTemplateCtrl templateCtrl = new TextTemplateCtrl(organs4Show);
            templateCtrl.ShowDialog();

            RichSpellCtrl r = richTextMacro;
            if (r != null)
            {
                if (templateCtrl.SelectedText != null)
                {
                    r.AppendText(templateCtrl.SelectedText);
                }
            }


        }

        private string GetCytoMacroTemplate(SDG sdg)
        {
            if (string.IsNullOrEmpty(_cytoTemplateSql))
            {
                _cytoTemplateSql = dal.GetPhraseByName("CytologyTemplate").PhraseEntriesDictonary["Macro"];
            }
            string val = "";
            var queries = _cytoTemplateSql.Split(new string[1] { "~^" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string query in queries)
            {
                var sql = query.Replace("#SDG_ID#", sdg.SDG_ID.ToString());
                var res = dal.GetDynamicList(sql);
                if (res != null)
                    val = res.Aggregate(val, (current, s) => current + s);
            }

            return val;

        }

        internal void SaveTxtFromRichText2SdgResult()
        {

            var currentSdgResultMacroTextRes = GetCytoMacroTxtResult(CurrentSdg);

            var currentSdgRtfTbl = dal.GetAll<RTF_RESULT>().FirstOrDefault(x => x.RTF_RESULT_ID == resId);

            string currentRtfFromRichText = richTextMacro.GetRtf();

            string currentText = richTextMacro.GetOriginalText();

            if (currentSdgRtfTbl == null)
            {
                // insert
                var newRTF = new RTF_RESULT();
                newRTF.RTF_RESULT_ID = resId;
                newRTF.RTF_TEXT = currentRtfFromRichText;
                dal.Add(newRTF);
            }

            else
            {
                currentSdgRtfTbl.RTF_TEXT = currentRtfFromRichText; //update
            }

            string first4000Chars = currentText.Substring(0, Math.Min(currentText.Length, 4000));

            currentSdgResultMacroTextRes.ORIGINAL_RESULT = first4000Chars;
            currentSdgResultMacroTextRes.FORMATTED_RESULT = first4000Chars;

            string a = currentSdgResultMacroTextRes.STATUS;

            currentSdgResultMacroTextRes.STATUS = "C";

        }

        public RESULT GetCytoMacroTxtResult(SDG sdg)
        {

            var currentSdgCytoMacroResult = dal.Get_SDG_RESULTS(CurrentSdg.SDG_ID).Find(x => x.RESULT_TEMPLATE_NAME == "Cytology Macro Text");
            if (currentSdgCytoMacroResult == null)
                return null;

            resId = currentSdgCytoMacroResult.RESULT_ID;

            var res = dal.FindBy<RESULT>(x => x.RESULT_ID == resId).FirstOrDefault();
            return res;
        }

        internal void LoadResults(SDG sdg)
        {
            richTextMacro.ClearText();

            string s = GetCytoMacroTemplate(sdg);

            var currentSdgResultMacroTextRes = GetCytoMacroTxtResult(CurrentSdg);

            if (currentSdgResultMacroTextRes != null)
                if (currentSdgResultMacroTextRes.FORMATTED_RESULT == null || currentSdgResultMacroTextRes.FORMATTED_RESULT == "")
                {
                    richTextMacro.AppendText(s);
                }
                else
                {
                    var res2 = (from diag in dal.FindBy<RESULT>(x => x.TEST.ALIQUOT.SAMPLE.SDG_ID == CurrentSdg.SDG_ID)
                                where diag.TEST.STATUS != "X"

                                select diag);

                    foreach (var item in res2)
                    {
                        dal.ReloadEntity(item);
                    }

                    var currentResults = (from rl in res2
                                          select new WrapperRtf()
                                          {
                                              Result_ = rl,
                                              Name = rl.NAME,
                                              ResultId = rl.RESULT_ID,
                                              TestName = rl.TEST.NAME
                                          }).ToList();


                    //Gets current results id
                    var ids = currentResults.Select(x => x.ResultId);

                    //Gets results with RTF
                    var resultsHaveRtf = from res in dal.FindBy<RTF_RESULT>(x => ids.Contains(x.RTF_RESULT_ID)) select res;


                    //LOADS RTF TO LIST
                    foreach (RTF_RESULT rtfResult in resultsHaveRtf)
                    {
                        dal.ReloadEntity(rtfResult);
                        var rr = currentResults.FirstOrDefault(x => x.ResultId == rtfResult.RTF_RESULT_ID);
                        if (rr != null)
                        {
                            rr.RtfText = rtfResult.RTF_TEXT;
                        }
                    }

                    _result2RichText = new Dictionary<string, RichSpellCtrl>
                {
                    { "Cytology Macro Text", richTextMacro },
                };

                    //Sets data into rich text
                    foreach (KeyValuePair<string, RichSpellCtrl> result2RichTextHi in _result2RichText)
                    {

                        var res = currentResults.FirstOrDefault(x => x.Name == result2RichTextHi.Key);
                        if (res != null && res.RtfText != null)
                        {
                            result2RichTextHi.Value.SetRtf(res.RtfText);
                            result2RichTextHi.Value.Focus();
                        }

                    }
                }


        }

        #endregion

        #region displaying request

        private SolidColorBrush originalBrush;

        private void StartFlashingAnimation()
        {
            // Store the original color
            originalBrush = templateLoadingBtn.Background as SolidColorBrush;

            ColorAnimation animation = new ColorAnimation
            {
                To = Colors.LightCoral,
                Duration = TimeSpan.FromSeconds(0.7),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            SolidColorBrush brush = new SolidColorBrush();
            templateLoadingBtn.Background = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void StopFlashingAnimation()
        {
            SolidColorBrush brush = templateLoadingBtn.Background as SolidColorBrush;
            if (brush != null)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
                // Restore the original color
                templateLoadingBtn.Background = originalBrush;
            }
        }

        public void DisplayRequestDetails()
        {
            try
            {

                lbl_sdgName.Content = $"מספר מקרה: {CurrentSdg.SDG_USER.U_PATHOLAB_NUMBER}";

                SetPicture();

                numSamplesdd.IsEnabled = true;
                dtRequestDate.IsEnabled = true;


                SampleDetails.Clear();

                if (CurrentSdg.SAMPLEs != null)
                {
                    numSamplesdd.ValueChanged -= NumSamplesdd_OnValueChanged;
                    numSamplesdd.Value = CurrentSdg.SAMPLEs.Count;
                    numSamplesdd.ValueChanged += NumSamplesdd_OnValueChanged;
                }

                if (CurrentSdg.SDG_USER.PATHOLOG != null)
                    cmpPatholog.SelectedItem = CurrentSdg.SDG_USER.PATHOLOG;


                if (CurrentSdg.SDG_USER.U_REQUEST_DATE != null)
                    dtRequestDate.Value = CurrentSdg.SDG_USER.U_REQUEST_DATE.Value;


                if (CurrentSdg.SAMPLEs != null)
                {

                    foreach (var sample in CurrentSdg.SAMPLEs)
                    {
                        var chk_1 = sample.ALIQUOTs.SelectMany(x => x.ALIQUOT_USER.U_COLOR_TYPE);

                        var su = sample.SAMPLE_USER;


                        var smp = new CytoDetails();

                        smp.NumOfBlocksOnLoading = sample.ALIQUOTs.Where(x => x.ALIQUOT_USER.U_IS_CELL_BLOCK != "T").Count();

                        smp.IsReadOnly = false;
                        smp.MarkAs = su.U_MARK_AS;
                        smp.Mark = su.U_MARK;

                        smp.AssutaNbr = su.U_ASSUTA_NUMBER;

                        smp.CytoSlideType = su.U_CYTOLOGY_SLIDE_TYPE;
                        smp.Organ = su.U_ORGAN;

                        smp.NextStep = su.U_CYTOLOGY_NEXT_STEP;
                        smp.Volume = su.U_VOLUME.HasValue ? su.U_VOLUME.Value : 0;
                        smp.Color = su.U_COLOR;


                        var aliquots = sample.ALIQUOTs.Where(x => x.ALIQ_FORMULATION_PARENT.Count == 0); //PARENTS

                        smp.NumOfBlocks = aliquots.Count();

                        if (aliquots.FirstOrDefault(x => x.ALIQUOT_USER.U_IS_CELL_BLOCK == "T") != null)
                        {
                            smp.CellBlock = true;
                        }

                        //רשימה של כל הALIQ שהם לא CELL BLOCK
                        aliquots = aliquots.Where(x => x.ALIQUOT_USER.U_IS_CELL_BLOCK != "T");
                        foreach (var item in aliquots)
                        {

                            AliqColor a = new AliqColor()
                            {
                                ColorName = item.ALIQUOT_USER.U_COLOR_TYPE
                            };
                            smp.SlidesColor.Add(a);
                        }


                        SampleDetails.Add(smp);


                    }




                }


                templateLoadingBtn.IsEnabled = true;

                StartFlashingAnimation();

                numSamplesdd.IsEnabled = true;

            }
            catch (Exception e)
            {
                MessageBox.Show("Error in display cyto");
                Logger.WriteLogFile(e);

            }
        }
        private void TxtInternalNbr_OnKeyDown(object o, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    var tb = o as TextBox;
                    if (tb == null || string.IsNullOrEmpty(tb.Text))
                    {
                        return;
                    }

                    if (tb.Name == "txtInternalNbr")
                    {
                        txtInternalSamp.Text = String.Empty;
                        txtInternalSamp.Focus();
                        return;
                    }
                    currentSample = null;
                    CurrentSdg = null;
                    authorizeBtn.IsEnabled = true;
                    saveBtn.IsEnabled = true;
                    CurrentSdg = null;

                    string sdg_name = txtInternalNbr.Text;
                    string sample_name = txtInternalSamp.Text;



                    if (sdg_name == String.Empty || sample_name == String.Empty)
                    {
                        System.Windows.Forms.MessageBox.Show("חובה למלא מספר מקרה ומספר צנצנת");
                        return;
                    }

                    if (txtInternalNbr.Text[0] != 'C' || txtInternalNbr.Text[0] != 'C')
                    {
                        System.Windows.Forms.MessageBox.Show("מסך מיועד למקרי ציטולוגיה בלבד");
                        return;
                    }

                    CurrentSdg = dal.FindBy<SDG>(x => x.NAME == sdg_name.ToUpper()).FirstOrDefault();

                    if (CurrentSdg != null)
                    {
                        currentSample =
                            dal.FindBy<SAMPLE>(x => x.NAME == sample_name.ToUpper() && x.SDG_ID == CurrentSdg.SDG_ID)
                                .FirstOrDefault();
                    }

                    if (CurrentSdg == null || currentSample == null)
                    {
                        MessageBox.Show(".אנא בדוק את פרטי המקרה שהוזנו", Constants.MboxCaption, MessageBoxButton.OK,
                            MessageBoxImage.Hand);
                        return;
                    }

                    //richTextMacro.ClearText();
                    //richTextMacro.Enabled = false;

                    //btnOK.IsEnabled = false;
                    //btnSave.IsEnabled = false;

                    SetBtns();
                    DisplayRequestDetails();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public bool CloseQuery()
        {

            if (dal != null) dal.Close();

            return true;
        }


        #endregion

        #region modifying request

        private void NumOfSlidesChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            IntegerUpDown numOfSlidesInput = sender as IntegerUpDown;

            FrameworkElement container = numOfSlidesInput?.Parent as FrameworkElement;

            CytoDetails selectedSample = container?.DataContext as CytoDetails;

            if (selectedSample == null)
                return;


            int newNumOfSlides = e.NewValue != null ? int.Parse(e.NewValue.ToString()) : 0;
            int oldNumOfSlides = e.OldValue != null ? int.Parse(e.OldValue.ToString()) : 0;


            if (numOfSlidesInput.Value != null && numOfSlidesInput.Value.Value > 0 && newNumOfSlides != oldNumOfSlides)
            {
                if (newNumOfSlides > oldNumOfSlides) // If rows were added
                {

                    var newAliqColor = new AliqColor();
                    if (selectedSample.SlidesColor.Count > 0)
                    {
                        newAliqColor.ColorName = selectedSample.SlidesColor[0].ColorName;
                        selectedSample.SlidesColor.Add(newAliqColor);
                    }
                }
                else if (newNumOfSlides < oldNumOfSlides) // If rows were removed
                {

                    var lastSlideAdded = selectedSample.SlidesColor.LastOrDefault();
                    selectedSample.SlidesColor.Remove(lastSlideAdded);

                }
            }
        }

        private void NumSamplesdd_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
            {
                SampleDetails.Clear();
            }

            var newe = e.NewValue != null ? int.Parse(e.NewValue.ToString()) : 0;
            var olde = e.OldValue != null ? int.Parse(e.OldValue.ToString()) : 0;


            if (numSamplesdd.Value != null && numSamplesdd.Value.Value > 0 && newe != olde)
            {
                if (newe > olde) //אם נוספו שורות
                {
                    int sum = (newe - olde);

                    for (int i = 0; i < sum; i++)
                    {
                        var cd = new CytoDetails
                        {
                            NumOfBlocks = 1,
                            AssutaNbr = "",
                            MarkAs = "",
                            Mark = "",
                            Volume = 0

                        };
                        cd.SlidesColor.Add(new AliqColor());

                        SampleDetails.Add(cd);
                    }
                }
                else if (newe < olde) //אם הוסרו שורות
                {
                    if (CurrentSdg != null)
                    {
                        if (newe >= CurrentSdg.SAMPLEs.Count)
                        {
                            RemoveSample(newe, olde);
                        }
                    }
                    else
                    {
                        RemoveSample(newe, olde);
                    }
                }

                if (CurrentSdg != null && newe < CurrentSdg.SAMPLEs.Count)
                {
                    numSamplesdd.ValueChanged -= NumSamplesdd_OnValueChanged;
                    numSamplesdd.Value = CurrentSdg.SAMPLEs.Count;
                    numSamplesdd.ValueChanged += NumSamplesdd_OnValueChanged;
                }
            }
        }

        private void RemoveSample(int newe, int olde)
        {
            int sum = (olde - newe);

            for (int i = 0; i < sum; i++)
            {
                SampleDetails.RemoveAt(olde - 1 - i);
            }
        }

        private void SetBtns()
        {
            authorizeBtn.IsEnabled = !authorizeBtn.IsEnabled;
            saveBtn.IsEnabled = !saveBtn.IsEnabled;
            //lbl_sdgName.Visibility = (lbl_sdgName.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
            richTextMacro.Enabled = !richTextMacro.Enabled;
        }

        private void CleanScreen()
        {

            CurrentSdg = null;
            SampleDetails.Clear();
            richTextMacro.ClearText();
            lbl_sdgName.Content = string.Empty; 


            SetBtns();

            txtInternalSamp.Text = string.Empty;
            txtInternalNbr.Text = string.Empty;

            dtRequestDate.Value = null;

            numSamplesdd.ValueChanged -= NumSamplesdd_OnValueChanged;
            numSamplesdd.Value = 0;
            numSamplesdd.ValueChanged += NumSamplesdd_OnValueChanged;

            cmpPatholog.SelectedIndex = -1;
        }

        public bool UpdateRequest()
        {
            try
            {
                var createdBy = _ntlsUser.GetOperatorName();
                CurrentSdg.SDG_USER.PATHOLOG = cmpPatholog.SelectedItem as OPERATOR;
                CurrentSdg.SDG_USER.U_REQUEST_DATE = dtRequestDate.Value;
                int index = 0;


                // if the user added more samples, this will log them to the sdg in db
                int numOfSamplesToAdd = SampleDetails.Count - CurrentSdg.SAMPLEs.Count;
                int numSamplesAdded = 0;

                for (int i = 0; i < numOfSamplesToAdd; i++)
                {
                    long sampleID = AddSample();

                    if (sampleID != -1)
                    {
                        dal.InsertToSdgLog(CurrentSdg.SDG_ID, "CM.ADD_SAMPLE", (long)_ntlsCon.GetSessionId(), $"sample added by {createdBy}");
                        numSamplesAdded++;
                    }

                }

                if (numSamplesAdded != numOfSamplesToAdd)
                {
                    var failed = numOfSamplesToAdd - numSamplesAdded;
                }

                var currentSdgSamples =
                    dal.FindBy<SAMPLE>(x => x.SDG_ID == CurrentSdg.SDG_ID).OrderBy(x => x.SAMPLE_ID);

                foreach (var item in currentSdgSamples)
                {
                    EditSample(index, item);

                    index++;
                }

                var currentSdgSamples_1 =
                    dal.FindBy<SAMPLE>(x => x.SDG_ID == CurrentSdg.SDG_ID).OrderBy(x => x.SAMPLE_ID);



                foreach (var item in currentSdgSamples_1)
                {
                    foreach (var aliq in item.ALIQUOTs)
                    {
                        if (aliq.STATUS == "U")
                        {
                            aliq.STATUS = "V";
                        }
                        aliq.ALIQUOT_USER.U_ALIQUOT_STATION = "19";
                    }
                }

                SaveTxtFromRichText2SdgResult();

                indexesToAddCellBlock.Clear();
                dal.InsertToSdgLog(CurrentSdg.SDG_ID, "CM.UPDATE_MACRO", (long)_ntlsCon.GetSessionId(), $"MACRO UPDATED BY {createdBy}");
                dal.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בעדכון מאקרו." + Environment.NewLine + ex.Message);
                return false;
            }
        }

        public bool SpecialValidation()
        {
            if (CurrentSdg == null)
            {
                if (SampleDetails.Any(t => string.IsNullOrEmpty(t.AssutaNbr)))
                {
                    MessageBox.Show(Constants.AssutaMandatoryMsg, Constants.MboxCaption, MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }
            }

            foreach (CytoDetails cd in SampleDetails)
            {


                if (string.IsNullOrEmpty(cd.CytoSlideType))
                {
                    MessageBox.Show(Constants.CytologyTypeMandatoryMsg, Constants.MboxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }


            }


            return true;
        }

        private void EditSample(int i, SAMPLE sample)
        {

            try
            {

                sample.SAMPLE_USER.U_MARK_AS = SampleDetails[i].MarkAs;
                sample.SAMPLE_USER.U_MARK = SampleDetails[i].Mark;
                sample.SAMPLE_USER.U_ASSUTA_NUMBER = SampleDetails[i].AssutaNbr;
                sample.SAMPLE_USER.U_CYTOLOGY_SLIDE_TYPE = SampleDetails[i].CytoSlideType;
                sample.SAMPLE_USER.U_ORGAN = SampleDetails[i].Organ;
                sample.SAMPLE_USER.U_CYTOLOGY_NEXT_STEP = SampleDetails[i].NextStep;
                sample.SAMPLE_USER.U_COLOR = SampleDetails[i].Color;
                sample.SAMPLE_USER.U_VOLUME = SampleDetails[i].Volume;


                int existsAliquotsNumber = sample.ALIQUOTs.Count;

                var SampleDetails_aliquots = SampleDetails[i].SlidesColor;

                var SampleDetails_aliquots_cnt = SampleDetails[i].SlidesColor.Count;

                var resultList = SampleDetails_aliquots.Skip(existsAliquotsNumber).ToList();

                bool addCellBlock = indexesToAddCellBlock.Contains(i) ? true : false;

                if (resultList.Count > 0 || addCellBlock)
                {
                    AddNewAliquots(sample, addCellBlock, resultList);
                }

                var sampAliquots = dal.FindBy<ALIQUOT>(x => x.SAMPLE_ID == sample.SAMPLE_ID);

                foreach (var aliquot in sampAliquots)
                {
                    if (aliquot.ALIQUOT_USER.U_IS_CELL_BLOCK != "T")
                    {
                        aliquot.ALIQUOT_USER.U_COLOR_TYPE = "Papanicolao";
                    }
                }

                dal.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void AddNewAliquots(SAMPLE sample, bool addCellBlock, List<AliqColor> aliqList)
        {
            var chk_1 = sample.ALIQUOTs.SelectMany(x => x.ALIQUOT_USER.U_COLOR_TYPE);
            var chk_2 = aliqList.SelectMany(x => x.ColorName);
            try
            {
                if (addCellBlock)
                {
                    FireEventXmlHandler fireEvent = new FireEventXmlHandler(sp);
                    fireEvent.CreateFireEventXml("SAMPLE", sample.SAMPLE_ID, "Add Cell Block");
                    bool returnbool = fireEvent.ProcssXml();

                    if (returnbool)
                    {
                        var aliquots = dal.FindBy<ALIQUOT>(x => x.SAMPLE_ID == sample.SAMPLE_ID);
                        var aliq_cellBlock = aliquots.First(x => x.ALIQUOT_USER.U_IS_CELL_BLOCK == "T");
                        aliq_cellBlock.STATUS = "V";
                        aliq_cellBlock.ALIQUOT_USER.U_COLOR_TYPE = "HE";
                        dal.Ex_Req_Logic(CurrentSdg.SDG_ID, sample.NAME, ExtraRequestType.C,
                            (long)_ntlsUser.GetOperatorId(), "הוספת CELL BLOCK - מסך מאקרו ציטולוגיה", "");


                        //assigning name to slide under cell block
                        var aliquotId = dal.FindBy<ALIQUOT>(x => x.SAMPLE_ID == sample.SAMPLE_ID).OrderByDescending(x => x.ALIQUOT_ID).FirstOrDefault().ALIQUOT_ID;
                        try
                        {
                            OracleConnection _oraCon = GetConnection(_ntlsCon);

                            string query = $"select LIMS.GET_CYTO_SLIDE_NAME({aliquotId}) from dual";

                            string newSlide_name = string.Empty;

                            using (OracleCommand cmd = new OracleCommand(query, _oraCon))
                            {

                                using (var reader = cmd.ExecuteReader())
                                {

                                    while (reader.Read())
                                    {
                                        newSlide_name = reader[0].ToString();
                                    }
                                }

                            }
                            dal.FindBy<ALIQUOT>(x => x.ALIQUOT_ID == aliquotId).FirstOrDefault().NAME = newSlide_name;
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show("Error while updating new slide name");

                        }


                        dal.InsertToSdgLog(CurrentSdg.SDG_ID, "CM.ADD_CB", (long)_ntlsCon.GetSessionId(), $"CELL BLOCK נוסף לצנצנת: {sample.NAME}");

                        dal.SaveChanges();

                    }
                }

                foreach (var item in aliqList)
                {
                    FireEventXmlHandler cassetteEvent = new FireEventXmlHandler(ServiceProvider, "Add Cassette");
                    cassetteEvent.CreateFireEventXml("SAMPLE", sample.SAMPLE_ID, "Add Cassette");
                    bool s = cassetteEvent.ProcssXml();
                    //Todo:print cassette by workflow


                    if (!s)
                    {
                        MessageBox.Show(cassetteEvent.ErrorResponse);
                        break;
                    }


                    var newAliquotId = cassetteEvent.GetValueByTagName("ALIQUOT_ID");

                    //if (newAliquotId != null)
                    //{
                    //    long newAliquotId_1 = Int64.Parse(newAliquotId);
                    //    ALIQUOT aliquot = dal.FindBy<ALIQUOT>(al => al.ALIQUOT_ID == newAliquotId_1)
                    //        .FirstOrDefault();
                    //    aliquot.ALIQUOT_USER.U_COLOR_TYPE = item.ColorName;

                    //}

                }

            }
            catch (Exception e)
            {
                Logger.WriteLogFile(e);
                Console.WriteLine(e);
                throw;
            }
        }

        private long AddSample()
        {
            try
            {
                long sample_id = -1;

                FireEventXmlHandler addSample = new FireEventXmlHandler(ServiceProvider, "Add sample to SDG");
                addSample.CreateFireEventXml("SDG", CurrentSdg.SDG_ID, "Add Sample");
                bool res = addSample.ProcssXml();

                if (!res)
                {
                    MessageBox.Show(addSample.ErrorResponse);
                    return -1;
                }

                try
                {
                    sample_id = Convert.ToInt64(addSample.GetValueByTagName("SAMPLE_ID"));
                }
                catch (Exception ex)
                {
                    sample_id = -1;
                }

                return sample_id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void SaveSDG()
        {
            try
            {
                if (CurrentSdg == null)
                {
                    return;
                }


                if (UpdateRequest())
                {
                    MessageBox.Show("דרישה עודכנה בהצלחה.");
                    txtInternalNbr.Focus();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בעדכון הדרישה  ." + ex.Message);
            }

        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Traverse up the visual tree to find the parent of the specified type
            while (child != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(child);

                if (parent is T result)
                {
                    return result;
                }

                child = parent;
            }

            return null; // No parent of the specified type found
        }

        private void ChkIsCellBlock_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            ListBoxItem listBoxItem = FindVisualParent<ListBoxItem>(checkBox);

            if (listBoxItem != null)
            {
                int index = lb.ItemContainerGenerator.IndexFromContainer(listBoxItem);

                // Add the index to the list if it's not already present

                indexesToAddCellBlock.Add(index);

            }

        }

        private void ChkIsCellBlock_OnUnchecked(object sender, RoutedEventArgs e)
        {

            CheckBox checkBox = sender as CheckBox;

            ListBoxItem listBoxItem = FindVisualParent<ListBoxItem>(checkBox);

            if (listBoxItem != null)
            {
                int index = lb.ItemContainerGenerator.IndexFromContainer(listBoxItem);

                // Remove the index from the list if it's present
                indexesToAddCellBlock.Remove(index);
            }


        }

        private void DtRequestDate_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (RequestDateChanged != null)
            {
                bool b = RequestDateChanged(dtRequestDate.Value);
                if (!b)
                {
                    lbdt.Content = Constants.EqualDateMsg;
                    dtRequestDate.ToolTip = Constants.EqualDateMsg;

                }
                else
                {
                    lbdt.Content = "";
                }
            }
        }

        private void CmbCytoSlideType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            var selectedValue = comboBox.SelectedItem;

            var selectedCyto = (PHRASE_ENTRY)selectedValue;

            // Find the ListBoxItem that contains the ComboBox
            var listBoxItem = FindParent<ListBoxItem>(comboBox);

            if (listBoxItem != null)
            {
                var currentSampleDetails = listBoxItem.Content as CytoDetails;
                foreach (var item in currentSampleDetails.SlidesColor)
                {
                    item.ColorName = selectedCyto.PHRASE_INFO;
                }
            }


        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }

        private void SetPicture()
        {
            try
            {
                var samplesList = CurrentSdg.SAMPLEs;

                UniformGrid uniformGrid = new UniformGrid();
                uniformGrid.Columns = samplesList.Count; // Set the number of columns based on the number of images

                foreach (var item in samplesList)
                {
                    // Create the Image element
                    Image imgStatus = new Image();
                    imgStatus.Name = "imgStatus";
                    imgStatus.Width = 30;
                    imgStatus.Height = 30;
                    imgStatus.HorizontalAlignment = HorizontalAlignment.Center;

                    const string ResourcePath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Thermo\Nautilus\9.2\Directory";
                    var path = Utils.GetResourcePath();

                    if (path != null)
                    {
                        path += "\\";
                        const string _tableName = "Sample";

                        var uri = new Uri(path + _tableName + item.STATUS + ".ico");
                        var bitmap = new BitmapImage(uri);
                        imgStatus.Source = bitmap;
                    }

                    // Add the image to the UniformGrid
                    uniformGrid.Children.Add(imgStatus);
                }

                // Add the UniformGrid to the parent element (ImgGrid in your case)
                //ImgGrid.Children.Add(uniformGrid);
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
            }
        }

        private void PrintInitialLetterCyto(SDG newsdg)
        {
            try
            {
                if (newsdg.SdgType == SdgType.Cytology)
                {
                    FireEventXmlHandler fireEvent = new FireEventXmlHandler(ServiceProvider, "Print");
                    fireEvent.CreateFireEventXml("SDG", newsdg.SDG_ID, "Print an initial letter");
                    var feres = fireEvent.ProcssXml();
                }
            }
            catch (Exception ex)
            {

                System.Windows.Forms.MessageBox.Show("  שגיאה בהדפסת דף מלווה" + ex.Message, "Nautilus", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public OracleConnection GetConnection(INautilusDBConnection ntlsCon)
        {

            OracleConnection connection = null;

            if (ntlsCon != null)
            {


                // Initialize variables
                String roleCommand;
                // Try/Catch block
                try
                {


                    var C = ntlsCon.GetServerIsProxy();
                    var C2 = ntlsCon.GetServerName();
                    var C4 = ntlsCon.GetServerType();

                    var C6 = ntlsCon.GetServerExtra();

                    var C8 = ntlsCon.GetPassword();
                    var C9 = ntlsCon.GetLimsUserPwd();
                    var C10 = ntlsCon.GetServerIsProxy();
                    var DD = _ntlsSite;




                    var u = _ntlsUser.GetOperatorName();
                    var u1 = _ntlsUser.GetWorkstationName();



                    string _connectionString = ntlsCon.GetADOConnectionString();

                    var splited = _connectionString.Split(';');

                    var cs = "";

                    for (int i = 1; i < splited.Count(); i++)
                    {
                        cs += splited[i] + ';';
                    }
                    //<<<<<<< .mine
                    var username = ntlsCon.GetUsername();
                    if (string.IsNullOrEmpty(username))
                    {
                        var serverDetails = ntlsCon.GetServerDetails();
                        cs = "User Id=/;Data Source=" + serverDetails + ";";
                    }


                    //Create the connection
                    connection = new OracleConnection(cs);



                    // Open the connection
                    connection.Open();

                    // Get lims user password
                    string limsUserPassword = ntlsCon.GetLimsUserPwd();

                    // Set role lims user
                    if (limsUserPassword == "")
                    {
                        // LIMS_USER is not password protected
                        roleCommand = "set role lims_user";
                    }
                    else
                    {
                        // LIMS_USER is password protected.
                        roleCommand = "set role lims_user identified by " + limsUserPassword;
                    }

                    // set the Oracle user for this connecition
                    OracleCommand command = new OracleCommand(roleCommand, connection);

                    // Try/Catch block
                    try
                    {
                        // Execute the command
                        command.ExecuteNonQuery();
                    }
                    catch (Exception f)
                    {
                        // Throw the exception
                        throw new Exception("Inconsistent role Security : " + f.Message);
                    }

                    // Get the session id
                    _session_id = ntlsCon.GetSessionId();

                    // Connect to the same session
                    string sSql = string.Format("call lims.lims_env.connect_same_session({0})", _session_id);

                    // Build the command
                    command = new OracleCommand(sSql, connection);

                    // Execute the command
                    command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    // Throw the exception
                    throw e;
                }

                // Return the connection
            }

            return connection;

        }

        private void HeaderBtnsClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid)
            {
                switch (grid.Name)
                {

                    case "exitBtn":
                        {
                            var dg = MessageBox.Show("האם אתה בטוח שברצונך לצאת?", Constants.MboxCaption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            if (dg == MessageBoxResult.Yes)
                            {
                                _ntlsSite.CloseWindow();
                            }
                            break;
                        }
                    case "saveBtn":
                        {
                            if (SpecialValidation())
                            {
                                SaveSDG();
                            };
                            break;
                        }
                    case "templateLoadingBtn":
                        {

                            try
                            {
                                if (SpecialValidation())
                                {
                                    SaveSDG();
                                    LoadResults(CurrentSdg);
                                    SetBtns();
                                    StopFlashingAnimation();
                                };

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                throw;
                            }
                            break;
                        }
                    case "authorizeBtn":
                        {
                            if (SpecialValidation())
                            {
                                SaveSDG();
                                PrintInitialLetterCyto(CurrentSdg);
                                CleanScreen();
                                SetBtns();
                            };
                            break;
                        }

                }
            }
        }



    }
    #endregion





    #region Cytology Details classes

    public class CytoDetails : INotifyPropertyChanged
    {

        private int _numOfBlocks;

        private string _markAs;
        private string _mark;
        private string _assutaNbr;
        public decimal Volume { get; set; }
        public string CytoType { get; set; }

        public string Organ { get; set; }

        public bool CellBlock { get; set; }

        public System.Collections.ObjectModel.ObservableCollection<AliqColor> SlidesColor { get; set; }

        public string CytoSlideType { get; set; }

        public string Color { get; set; }

        public string NextStep { get; set; }
        bool _isReadOnly;
        public int NumOfBlocks
        {
            get { return _numOfBlocks; }

            set
            {
                _numOfBlocks = value;
                OnPropertyChanged("NumOfBlocks");
            }


        }

        private int? numOfBlocksOnLoading;

        public int NumOfBlocksOnLoading
        {
            get { return numOfBlocksOnLoading ?? 1; }
            set { numOfBlocksOnLoading = value; }
        }


        public bool IsReadOnly
        {
            get { return _isReadOnly; }

            set
            {
                _isReadOnly = value;
                OnPropertyChanged("IsReadOnly");
            }


        }


        public string _sdgName { get; set; }

        public string MarkAs
        {
            get { return _markAs; }
            set
            {
                _markAs = value;
                OnPropertyChanged("MarkAs");
            }
        }
        public string Mark
        {
            get { return _mark; }
            set
            {
                _mark = value;
                OnPropertyChanged("Mark");
            }
        }


        public string AssutaNbr
        {
            get { return _assutaNbr; }
            set
            {
                _assutaNbr = value;
                OnPropertyChanged("AssutaNbr");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public CytoDetails()
        {
            SlidesColor = new System.Collections.ObjectModel.ObservableCollection<AliqColor>();
            Volume = 0;
            IsReadOnly = true;

        }



    }

    public class AliqColor : INotifyPropertyChanged
    {
        private string _colorName;



        public string ColorName
        {
            get { return _colorName; }

            set
            {
                _colorName = value;
                OnPropertyChanged("ColorName");
            }


        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public static class Constants
    {

        public const string MboxCaption = "מסך מאקרו";
        public static string EqualDateMsg = "תאריך הפנייה אינו יכול להיות גדול מהיום או מתאריך הגעה.";
        public static string AssutaMandatoryMsg = "חובה להזין מס אסותא .";
        public static string CytologyTypeMandatoryMsg = "חובה להזין סוג ציטולוגיה .";
        public static string OrganMandatoryMsg = "חובה לבחור איבר.";

    }

    #endregion










}