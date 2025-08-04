namespace BambooHRIntegrationTest1
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
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
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
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.textDate1 = new System.Windows.Forms.TextBox();
            this.textDate2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textAmount = new System.Windows.Forms.TextBox();
            this.comboAbsent = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textNote = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textRequest = new System.Windows.Forms.TextBox();
            this.textResponse = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textFilePath = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonFileBrowser = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textDate1
            // 
            this.textDate1.Location = new System.Drawing.Point(75, 40);
            this.textDate1.Name = "textDate1";
            this.textDate1.Size = new System.Drawing.Size(130, 22);
            this.textDate1.TabIndex = 0;
            // 
            // textDate2
            // 
            this.textDate2.Location = new System.Drawing.Point(75, 81);
            this.textDate2.Name = "textDate2";
            this.textDate2.Size = new System.Drawing.Size(130, 22);
            this.textDate2.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "開始日期";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "結束時間";
            // 
            // textAmount
            // 
            this.textAmount.Location = new System.Drawing.Point(75, 154);
            this.textAmount.Name = "textAmount";
            this.textAmount.Size = new System.Drawing.Size(130, 22);
            this.textAmount.TabIndex = 3;
            // 
            // comboAbsent
            // 
            this.comboAbsent.FormattingEnabled = true;
            this.comboAbsent.Location = new System.Drawing.Point(75, 117);
            this.comboAbsent.Name = "comboAbsent";
            this.comboAbsent.Size = new System.Drawing.Size(192, 20);
            this.comboAbsent.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "假別";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "時數";
            // 
            // textNote
            // 
            this.textNote.Location = new System.Drawing.Point(75, 195);
            this.textNote.Multiline = true;
            this.textNote.Name = "textNote";
            this.textNote.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textNote.Size = new System.Drawing.Size(285, 54);
            this.textNote.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(40, 198);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "備註";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(75, 277);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(130, 36);
            this.button1.TabIndex = 6;
            this.button1.Text = "送出";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textRequest
            // 
            this.textRequest.Location = new System.Drawing.Point(458, 40);
            this.textRequest.Multiline = true;
            this.textRequest.Name = "textRequest";
            this.textRequest.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textRequest.Size = new System.Drawing.Size(298, 117);
            this.textRequest.TabIndex = 7;
            // 
            // textResponse
            // 
            this.textResponse.Location = new System.Drawing.Point(458, 187);
            this.textResponse.Multiline = true;
            this.textResponse.Name = "textResponse";
            this.textResponse.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textResponse.Size = new System.Drawing.Size(298, 62);
            this.textResponse.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(410, 43);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "Request";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(403, 190);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 12);
            this.label7.TabIndex = 2;
            this.label7.Text = "Response";
            // 
            // textFilePath
            // 
            this.textFilePath.Location = new System.Drawing.Point(846, 51);
            this.textFilePath.Name = "textFilePath";
            this.textFilePath.Size = new System.Drawing.Size(236, 22);
            this.textFilePath.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(844, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "圖片路徑";
            // 
            // buttonFileBrowser
            // 
            this.buttonFileBrowser.Location = new System.Drawing.Point(1089, 51);
            this.buttonFileBrowser.Name = "buttonFileBrowser";
            this.buttonFileBrowser.Size = new System.Drawing.Size(75, 23);
            this.buttonFileBrowser.TabIndex = 10;
            this.buttonFileBrowser.Text = "選擇檔案";
            this.buttonFileBrowser.UseVisualStyleBackColor = true;
            this.buttonFileBrowser.Click += new System.EventHandler(this.buttonFileBrowser_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(846, 99);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(92, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "上傳圖片";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1242, 394);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.buttonFileBrowser);
            this.Controls.Add(this.textFilePath);
            this.Controls.Add(this.textResponse);
            this.Controls.Add(this.textRequest);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textNote);
            this.Controls.Add(this.comboAbsent);
            this.Controls.Add(this.textAmount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textDate2);
            this.Controls.Add(this.textDate1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textDate1;
        private System.Windows.Forms.TextBox textDate2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textAmount;
        private System.Windows.Forms.ComboBox comboAbsent;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textNote;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textRequest;
        private System.Windows.Forms.TextBox textResponse;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textFilePath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonFileBrowser;
        private System.Windows.Forms.Button button2;
    }
}

