namespace ConfigDataExpoter
{
    partial class DataExpoterForm
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.exportAllExcelBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.languageDirectoryText = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.codeTypeDropDown = new System.Windows.Forms.ComboBox();
            this.unityDataDirectoryText = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.unityCodeDirectoryText = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.copyFromDirectoryPathText = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.typeEnumCodeNameText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.loaderCodeNameText = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.configDataNameText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.dataDirectoryNameText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.codeDirectoryNameText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rootDirectoryText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.exportLanguageBtn = new System.Windows.Forms.Button();
            this.unityLanguageDirectoryText = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.removeExpiredLanguageItemChecked = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // exportAllExcelBtn
            // 
            this.exportAllExcelBtn.Location = new System.Drawing.Point(83, 359);
            this.exportAllExcelBtn.Name = "exportAllExcelBtn";
            this.exportAllExcelBtn.Size = new System.Drawing.Size(75, 23);
            this.exportAllExcelBtn.TabIndex = 0;
            this.exportAllExcelBtn.Text = "全部导出";
            this.exportAllExcelBtn.UseVisualStyleBackColor = true;
            this.exportAllExcelBtn.Click += new System.EventHandler(this.ParseAllExcel);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.removeExpiredLanguageItemChecked);
            this.groupBox1.Controls.Add(this.languageDirectoryText);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.codeTypeDropDown);
            this.groupBox1.Controls.Add(this.unityDataDirectoryText);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.unityCodeDirectoryText);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.copyFromDirectoryPathText);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.typeEnumCodeNameText);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.loaderCodeNameText);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.configDataNameText);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.dataDirectoryNameText);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.codeDirectoryNameText);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.rootDirectoryText);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.exportAllExcelBtn);
            this.groupBox1.Location = new System.Drawing.Point(12, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(371, 400);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "自动生成设置";
            // 
            // languageDirectoryText
            // 
            this.languageDirectoryText.Location = new System.Drawing.Point(104, 101);
            this.languageDirectoryText.Name = "languageDirectoryText";
            this.languageDirectoryText.Size = new System.Drawing.Size(244, 21);
            this.languageDirectoryText.TabIndex = 22;
            this.languageDirectoryText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(19, 104);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 21;
            this.label11.Text = "语言文件夹";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(19, 293);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 12);
            this.label10.TabIndex = 20;
            this.label10.Text = "面向端";
            // 
            // codeTypeDropDown
            // 
            this.codeTypeDropDown.FormattingEnabled = true;
            this.codeTypeDropDown.Location = new System.Drawing.Point(104, 290);
            this.codeTypeDropDown.Name = "codeTypeDropDown";
            this.codeTypeDropDown.Size = new System.Drawing.Size(121, 20);
            this.codeTypeDropDown.TabIndex = 2;
            this.codeTypeDropDown.SelectedIndexChanged += new System.EventHandler(this._SelectedIndexChanged);
            // 
            // unityDataDirectoryText
            // 
            this.unityDataDirectoryText.Location = new System.Drawing.Point(104, 263);
            this.unityDataDirectoryText.Name = "unityDataDirectoryText";
            this.unityDataDirectoryText.Size = new System.Drawing.Size(244, 21);
            this.unityDataDirectoryText.TabIndex = 19;
            this.unityDataDirectoryText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 266);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(83, 12);
            this.label9.TabIndex = 18;
            this.label9.Text = "UnityData目录";
            // 
            // unityCodeDirectoryText
            // 
            this.unityCodeDirectoryText.Location = new System.Drawing.Point(104, 236);
            this.unityCodeDirectoryText.Name = "unityCodeDirectoryText";
            this.unityCodeDirectoryText.Size = new System.Drawing.Size(244, 21);
            this.unityCodeDirectoryText.TabIndex = 17;
            this.unityCodeDirectoryText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 239);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(83, 12);
            this.label8.TabIndex = 16;
            this.label8.Text = "UnityCode目录";
            // 
            // copyFromDirectoryPathText
            // 
            this.copyFromDirectoryPathText.Location = new System.Drawing.Point(104, 209);
            this.copyFromDirectoryPathText.Name = "copyFromDirectoryPathText";
            this.copyFromDirectoryPathText.Size = new System.Drawing.Size(244, 21);
            this.copyFromDirectoryPathText.TabIndex = 15;
            this.copyFromDirectoryPathText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 212);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 14;
            this.label7.Text = "待拷贝文件夹";
            // 
            // typeEnumCodeNameText
            // 
            this.typeEnumCodeNameText.Location = new System.Drawing.Point(104, 182);
            this.typeEnumCodeNameText.Name = "typeEnumCodeNameText";
            this.typeEnumCodeNameText.Size = new System.Drawing.Size(244, 21);
            this.typeEnumCodeNameText.TabIndex = 13;
            this.typeEnumCodeNameText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 185);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "类型枚举文件";
            // 
            // loaderCodeNameText
            // 
            this.loaderCodeNameText.Location = new System.Drawing.Point(104, 155);
            this.loaderCodeNameText.Name = "loaderCodeNameText";
            this.loaderCodeNameText.Size = new System.Drawing.Size(244, 21);
            this.loaderCodeNameText.TabIndex = 11;
            this.loaderCodeNameText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "加载器文件";
            // 
            // configDataNameText
            // 
            this.configDataNameText.Location = new System.Drawing.Point(104, 128);
            this.configDataNameText.Name = "configDataNameText";
            this.configDataNameText.Size = new System.Drawing.Size(244, 21);
            this.configDataNameText.TabIndex = 9;
            this.configDataNameText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 131);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "数据类文件";
            // 
            // dataDirectoryNameText
            // 
            this.dataDirectoryNameText.Location = new System.Drawing.Point(104, 74);
            this.dataDirectoryNameText.Name = "dataDirectoryNameText";
            this.dataDirectoryNameText.Size = new System.Drawing.Size(244, 21);
            this.dataDirectoryNameText.TabIndex = 7;
            this.dataDirectoryNameText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "数据文件夹";
            // 
            // codeDirectoryNameText
            // 
            this.codeDirectoryNameText.Location = new System.Drawing.Point(104, 47);
            this.codeDirectoryNameText.Name = "codeDirectoryNameText";
            this.codeDirectoryNameText.Size = new System.Drawing.Size(244, 21);
            this.codeDirectoryNameText.TabIndex = 5;
            this.codeDirectoryNameText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "代码文件夹";
            // 
            // rootDirectoryText
            // 
            this.rootDirectoryText.Location = new System.Drawing.Point(104, 20);
            this.rootDirectoryText.Name = "rootDirectoryText";
            this.rootDirectoryText.Size = new System.Drawing.Size(244, 21);
            this.rootDirectoryText.TabIndex = 3;
            this.rootDirectoryText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "导出目录";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.exportLanguageBtn);
            this.groupBox2.Controls.Add(this.unityLanguageDirectoryText);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Location = new System.Drawing.Point(418, 34);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(343, 100);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "翻译表导入Unity";
            // 
            // exportLanguageBtn
            // 
            this.exportLanguageBtn.Location = new System.Drawing.Point(144, 61);
            this.exportLanguageBtn.Name = "exportLanguageBtn";
            this.exportLanguageBtn.Size = new System.Drawing.Size(75, 23);
            this.exportLanguageBtn.TabIndex = 3;
            this.exportLanguageBtn.Text = "导出翻译";
            this.exportLanguageBtn.UseVisualStyleBackColor = true;
            this.exportLanguageBtn.Click += new System.EventHandler(this.ParseLanguageExcel);
            // 
            // unityLanguageDirectoryText
            // 
            this.unityLanguageDirectoryText.Location = new System.Drawing.Point(93, 20);
            this.unityLanguageDirectoryText.Name = "unityLanguageDirectoryText";
            this.unityLanguageDirectoryText.Size = new System.Drawing.Size(244, 21);
            this.unityLanguageDirectoryText.TabIndex = 24;
            this.unityLanguageDirectoryText.TextChanged += new System.EventHandler(this._TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 23);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 12);
            this.label12.TabIndex = 23;
            this.label12.Text = "输出目录";
            // 
            // checkBox1
            // 
            this.removeExpiredLanguageItemChecked.AutoSize = true;
            this.removeExpiredLanguageItemChecked.Location = new System.Drawing.Point(21, 327);
            this.removeExpiredLanguageItemChecked.Name = "checkBox1";
            this.removeExpiredLanguageItemChecked.Size = new System.Drawing.Size(108, 16);
            this.removeExpiredLanguageItemChecked.TabIndex = 23;
            this.removeExpiredLanguageItemChecked.Text = "删除过期翻译项";
            this.removeExpiredLanguageItemChecked.UseVisualStyleBackColor = true;
            this.removeExpiredLanguageItemChecked.CheckedChanged += new System.EventHandler(this._CheckedChanged);
            // 
            // DataExpoterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "DataExpoterForm";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button exportAllExcelBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox loaderCodeNameText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox configDataNameText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox dataDirectoryNameText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox codeDirectoryNameText;
        private System.Windows.Forms.TextBox rootDirectoryText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox typeEnumCodeNameText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox copyFromDirectoryPathText;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox unityDataDirectoryText;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox unityCodeDirectoryText;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox codeTypeDropDown;
        private System.Windows.Forms.TextBox languageDirectoryText;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox unityLanguageDirectoryText;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button exportLanguageBtn;
        private System.Windows.Forms.CheckBox removeExpiredLanguageItemChecked;
    }
}

