namespace UIPlatform
{
    partial class UCControlItem
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tbMain = new System.Windows.Forms.TabControl();
            this.tpDesign = new System.Windows.Forms.TabPage();
            this.tpXML = new System.Windows.Forms.TabPage();
            this.txXML = new System.Windows.Forms.RichTextBox();
            this.tpDesignCode = new System.Windows.Forms.TabPage();
            this.txtDesignCode = new System.Windows.Forms.RichTextBox();
            this.tbMain.SuspendLayout();
            this.tpXML.SuspendLayout();
            this.tpDesignCode.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbMain
            // 
            this.tbMain.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tbMain.Controls.Add(this.tpDesign);
            this.tbMain.Controls.Add(this.tpXML);
            this.tbMain.Controls.Add(this.tpDesignCode);
            this.tbMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMain.HotTrack = true;
            this.tbMain.Location = new System.Drawing.Point(0, 0);
            this.tbMain.Margin = new System.Windows.Forms.Padding(0);
            this.tbMain.Name = "tbMain";
            this.tbMain.SelectedIndex = 0;
            this.tbMain.Size = new System.Drawing.Size(935, 628);
            this.tbMain.TabIndex = 0;
            this.tbMain.SelectedIndexChanged += new System.EventHandler(this.tbMain_SelectedIndexChanged);
            // 
            // tpDesign
            // 
            this.tpDesign.Location = new System.Drawing.Point(4, 4);
            this.tpDesign.Name = "tpDesign";
            this.tpDesign.Padding = new System.Windows.Forms.Padding(3);
            this.tpDesign.Size = new System.Drawing.Size(927, 602);
            this.tpDesign.TabIndex = 0;
            this.tpDesign.Text = "[视图设计]";
            this.tpDesign.UseVisualStyleBackColor = true;
            // 
            // tpXML
            // 
            this.tpXML.Controls.Add(this.txXML);
            this.tpXML.Location = new System.Drawing.Point(4, 4);
            this.tpXML.Name = "tpXML";
            this.tpXML.Padding = new System.Windows.Forms.Padding(3);
            this.tpXML.Size = new System.Drawing.Size(927, 602);
            this.tpXML.TabIndex = 1;
            this.tpXML.Text = "[表单代码]";
            this.tpXML.UseVisualStyleBackColor = true;
            // 
            // txXML
            // 
            this.txXML.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txXML.Location = new System.Drawing.Point(3, 3);
            this.txXML.Name = "txXML";
            this.txXML.Size = new System.Drawing.Size(921, 596);
            this.txXML.TabIndex = 0;
            this.txXML.Text = "";
            // 
            // tpDesignCode
            // 
            this.tpDesignCode.Controls.Add(this.txtDesignCode);
            this.tpDesignCode.Location = new System.Drawing.Point(4, 4);
            this.tpDesignCode.Name = "tpDesignCode";
            this.tpDesignCode.Padding = new System.Windows.Forms.Padding(3);
            this.tpDesignCode.Size = new System.Drawing.Size(927, 602);
            this.tpDesignCode.TabIndex = 2;
            this.tpDesignCode.Text = "[设计器代码]";
            this.tpDesignCode.UseVisualStyleBackColor = true;
            // 
            // txtDesignCode
            // 
            this.txtDesignCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDesignCode.Location = new System.Drawing.Point(3, 3);
            this.txtDesignCode.Name = "txtDesignCode";
            this.txtDesignCode.Size = new System.Drawing.Size(921, 596);
            this.txtDesignCode.TabIndex = 0;
            this.txtDesignCode.Text = "";
            // 
            // UCControlItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbMain);
            this.Name = "UCControlItem";
            this.Size = new System.Drawing.Size(935, 628);
            this.tbMain.ResumeLayout(false);
            this.tpXML.ResumeLayout(false);
            this.tpDesignCode.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tbMain;
        private System.Windows.Forms.TabPage tpDesign;
        private System.Windows.Forms.TabPage tpXML;
        private System.Windows.Forms.RichTextBox txXML;
        private System.Windows.Forms.TabPage tpDesignCode;
        private System.Windows.Forms.RichTextBox txtDesignCode;
    }
}
