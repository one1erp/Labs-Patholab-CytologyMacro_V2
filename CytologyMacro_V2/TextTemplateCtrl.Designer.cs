namespace CytologyMacro_V2Pages
{
    partial class TextTemplateCtrl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.radListView1 = new Telerik.WinControls.UI.RadListView();
            ((System.ComponentModel.ISupportInitialize)(this.radListView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // radListView1
            // 
            this.radListView1.AllowEdit = false;
            this.radListView1.AutoScroll = true;
            this.radListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radListView1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.radListView1.ItemSpacing = 5;
            this.radListView1.Location = new System.Drawing.Point(0, 0);
            this.radListView1.Name = "radListView1";
            this.radListView1.Size = new System.Drawing.Size(993, 463);
            this.radListView1.TabIndex = 0;
            // 
            // TextTemplateCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 463);
            this.Controls.Add(this.radListView1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TextTemplateCtrl";
            // 
            // 
            // 
            //   this.RootElement.ApplyShapeToControl = true;
            this.Text = "TextTemplateCtrl";
            ((System.ComponentModel.ISupportInitialize)(this.radListView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }
        private Telerik.WinControls.UI.RadListView radListView1;

        #endregion
    }
}