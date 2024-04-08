using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace CytologyMacro_V2Pages
{
    public partial class TextTemplateCtrl : Telerik.WinControls.UI.RadForm
    {
        private IEnumerable<string> organs4Show;



        public TextTemplateCtrl(IEnumerable<string> organs4Show)
        {
            this.organs4Show = organs4Show;
            InitializeComponent();

            radListView1.DoubleClick += radListView1_DoubleClick;
            this.radListView1.DataSource = organs4Show;
        }

        private void radListView1_DoubleClick(object sender, EventArgs e)
        {
            SelectedText = "";
            if (radListView1.SelectedItem != null)

                SelectedText = radListView1.SelectedItem.Text.ToString();
            this.Hide();
        }

        public string SelectedText { get; set; }
    }
}
