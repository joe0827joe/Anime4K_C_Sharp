namespace Digital_Image_Processing_HW3
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.開啟檔案ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.儲存ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OrginalimageSave = new System.Windows.Forms.ToolStripMenuItem();
            this.TargetImageSave = new System.Windows.Forms.ToolStripMenuItem();
            this.Targeimage2Save = new System.Windows.Forms.ToolStripMenuItem();
            this.SpectrumimageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FunctionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NearestNeighbor_TSM = new System.Windows.Forms.ToolStripMenuItem();
            this.Bilinear_TSM = new System.Windows.Forms.ToolStripMenuItem();
            this.Bicublic_TSM = new System.Windows.Forms.ToolStripMenuItem();
            this.anime4K_TSM = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox1.Location = new System.Drawing.Point(12, 27);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(320, 320);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox3.Location = new System.Drawing.Point(12, 373);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(320, 320);
            this.pictureBox3.TabIndex = 0;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 352);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "源圖片";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 696);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "目標圖片";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(335, 696);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(74, 15);
            this.label19.TabIndex = 33;
            this.label19.Text = "目標圖片2";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileToolStripMenuItem,
            this.FunctionToolStripMenuItem,
            this.SettingToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(671, 28);
            this.menuStrip1.TabIndex = 34;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileToolStripMenuItem
            // 
            this.FileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.開啟檔案ToolStripMenuItem,
            this.儲存ToolStripMenuItem});
            this.FileToolStripMenuItem.Name = "FileToolStripMenuItem";
            this.FileToolStripMenuItem.Size = new System.Drawing.Size(51, 24);
            this.FileToolStripMenuItem.Text = "檔案";
            // 
            // 開啟檔案ToolStripMenuItem
            // 
            this.開啟檔案ToolStripMenuItem.Name = "開啟檔案ToolStripMenuItem";
            this.開啟檔案ToolStripMenuItem.Size = new System.Drawing.Size(114, 26);
            this.開啟檔案ToolStripMenuItem.Text = "開啟";
            this.開啟檔案ToolStripMenuItem.Click += new System.EventHandler(this.OpenfileToolStripMenuItem_Click);
            // 
            // 儲存ToolStripMenuItem
            // 
            this.儲存ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OrginalimageSave,
            this.TargetImageSave,
            this.Targeimage2Save,
            this.SpectrumimageToolStripMenuItem});
            this.儲存ToolStripMenuItem.Name = "儲存ToolStripMenuItem";
            this.儲存ToolStripMenuItem.Size = new System.Drawing.Size(114, 26);
            this.儲存ToolStripMenuItem.Text = "儲存";
            // 
            // OrginalimageSave
            // 
            this.OrginalimageSave.Name = "OrginalimageSave";
            this.OrginalimageSave.Size = new System.Drawing.Size(153, 26);
            this.OrginalimageSave.Text = "源圖片";
            this.OrginalimageSave.Click += new System.EventHandler(this.OrginalimageSave_Click);
            // 
            // TargetImageSave
            // 
            this.TargetImageSave.Name = "TargetImageSave";
            this.TargetImageSave.Size = new System.Drawing.Size(153, 26);
            this.TargetImageSave.Text = "目標圖片";
            this.TargetImageSave.Click += new System.EventHandler(this.TargetImageSave_Click);
            // 
            // Targeimage2Save
            // 
            this.Targeimage2Save.Name = "Targeimage2Save";
            this.Targeimage2Save.Size = new System.Drawing.Size(153, 26);
            this.Targeimage2Save.Text = "目標圖片2";
            this.Targeimage2Save.Click += new System.EventHandler(this.Targeimage2Save_Click);
            // 
            // SpectrumimageToolStripMenuItem
            // 
            this.SpectrumimageToolStripMenuItem.Name = "SpectrumimageToolStripMenuItem";
            this.SpectrumimageToolStripMenuItem.Size = new System.Drawing.Size(153, 26);
            this.SpectrumimageToolStripMenuItem.Text = "目標圖片3";
            this.SpectrumimageToolStripMenuItem.Click += new System.EventHandler(this.SpectrumimageToolStripMenuItem_Click);
            // 
            // FunctionToolStripMenuItem
            // 
            this.FunctionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NearestNeighbor_TSM,
            this.Bilinear_TSM,
            this.Bicublic_TSM,
            this.anime4K_TSM});
            this.FunctionToolStripMenuItem.Name = "FunctionToolStripMenuItem";
            this.FunctionToolStripMenuItem.Size = new System.Drawing.Size(51, 24);
            this.FunctionToolStripMenuItem.Text = "功能";
            // 
            // NearestNeighbor_TSM
            // 
            this.NearestNeighbor_TSM.Name = "NearestNeighbor_TSM";
            this.NearestNeighbor_TSM.Size = new System.Drawing.Size(216, 26);
            this.NearestNeighbor_TSM.Text = "最鄰近插值法";
            this.NearestNeighbor_TSM.Click += new System.EventHandler(this.NearestNeighbor_TSM_Click);
            // 
            // Bilinear_TSM
            // 
            this.Bilinear_TSM.Name = "Bilinear_TSM";
            this.Bilinear_TSM.Size = new System.Drawing.Size(216, 26);
            this.Bilinear_TSM.Text = "雙線性插值法";
            this.Bilinear_TSM.Click += new System.EventHandler(this.Bilinear_TSM_Click);
            // 
            // Bicublic_TSM
            // 
            this.Bicublic_TSM.Name = "Bicublic_TSM";
            this.Bicublic_TSM.Size = new System.Drawing.Size(216, 26);
            this.Bicublic_TSM.Text = "雙三次插值法";
            this.Bicublic_TSM.Click += new System.EventHandler(this.Bicublic_TSM_Click);
            // 
            // anime4K_TSM
            // 
            this.anime4K_TSM.Name = "anime4K_TSM";
            this.anime4K_TSM.Size = new System.Drawing.Size(216, 26);
            this.anime4K_TSM.Text = "Anime4K";
            this.anime4K_TSM.Click += new System.EventHandler(this.anime4K_TSM_Click);
            // 
            // SettingToolStripMenuItem
            // 
            this.SettingToolStripMenuItem.Name = "SettingToolStripMenuItem";
            this.SettingToolStripMenuItem.Size = new System.Drawing.Size(51, 24);
            this.SettingToolStripMenuItem.Text = "設定";
            this.SettingToolStripMenuItem.Click += new System.EventHandler(this.SettingToolStripMenuItem_Click);
            // 
            // pictureBox4
            // 
            this.pictureBox4.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox4.Location = new System.Drawing.Point(339, 373);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(320, 320);
            this.pictureBox4.TabIndex = 0;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.pictureBox4_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(335, 350);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 15);
            this.label1.TabIndex = 36;
            this.label1.Text = "目標圖片3";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox2.Location = new System.Drawing.Point(339, 27);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(320, 320);
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 719);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "影像處理_Anime4K";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 開啟檔案ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 儲存ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FunctionToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.ToolStripMenuItem OrginalimageSave;
        private System.Windows.Forms.ToolStripMenuItem TargetImageSave;
        private System.Windows.Forms.ToolStripMenuItem Targeimage2Save;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem SpectrumimageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ToolStripMenuItem NearestNeighbor_TSM;
        private System.Windows.Forms.ToolStripMenuItem Bilinear_TSM;
        private System.Windows.Forms.ToolStripMenuItem Bicublic_TSM;
        private System.Windows.Forms.ToolStripMenuItem anime4K_TSM;
    }
}

