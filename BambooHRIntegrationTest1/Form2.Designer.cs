namespace BambooHRIntegrationTest1
{
    partial class Form2
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
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.textBambooHREmployeeID = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.textImportHistoryEmpID = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.button7 = new System.Windows.Forms.Button();
            this.gridLeaveForm = new System.Windows.Forms.DataGridView();
            this.button8 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textResponseHeader = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.textTimeOffRequestID = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridLeaveForm)).BeginInit();
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
            this.textNote.Size = new System.Drawing.Size(260, 54);
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
            this.textRequest.Size = new System.Drawing.Size(298, 82);
            this.textRequest.TabIndex = 7;
            // 
            // textResponse
            // 
            this.textResponse.Location = new System.Drawing.Point(458, 145);
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
            this.label7.Location = new System.Drawing.Point(403, 148);
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
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(458, 299);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(140, 36);
            this.button3.TabIndex = 12;
            this.button3.Text = "查詢請假";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(846, 190);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(846, 225);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(144, 23);
            this.button5.TabIndex = 14;
            this.button5.Text = "取得所有員工資料";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBambooHREmployeeID
            // 
            this.textBambooHREmployeeID.Location = new System.Drawing.Point(1117, 192);
            this.textBambooHREmployeeID.Name = "textBambooHREmployeeID";
            this.textBambooHREmployeeID.Size = new System.Drawing.Size(100, 22);
            this.textBambooHREmployeeID.TabIndex = 15;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(1117, 226);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(100, 23);
            this.button6.TabIndex = 16;
            this.button6.Text = "取得工號";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(1035, 195);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(76, 12);
            this.label9.TabIndex = 17;
            this.label9.Text = "BambooHR ID";
            // 
            // textImportHistoryEmpID
            // 
            this.textImportHistoryEmpID.Location = new System.Drawing.Point(112, 372);
            this.textImportHistoryEmpID.Name = "textImportHistoryEmpID";
            this.textImportHistoryEmpID.Size = new System.Drawing.Size(100, 22);
            this.textImportHistoryEmpID.TabIndex = 18;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(77, 375);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 12);
            this.label10.TabIndex = 19;
            this.label10.Text = "工號";
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(358, 370);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(144, 23);
            this.button7.TabIndex = 20;
            this.button7.Text = " 列出歷史資料";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // gridLeaveForm
            // 
            this.gridLeaveForm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridLeaveForm.Location = new System.Drawing.Point(75, 411);
            this.gridLeaveForm.Name = "gridLeaveForm";
            this.gridLeaveForm.RowTemplate.Height = 24;
            this.gridLeaveForm.Size = new System.Drawing.Size(1219, 183);
            this.gridLeaveForm.TabIndex = 21;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(519, 370);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(142, 23);
            this.button8.TabIndex = 22;
            this.button8.Text = "匯入歷史資料";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(227, 374);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(108, 16);
            this.checkBox1.TabIndex = 23;
            this.checkBox1.Text = "包括簽核中假單";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // textResponseHeader
            // 
            this.textResponseHeader.Location = new System.Drawing.Point(458, 225);
            this.textResponseHeader.Multiline = true;
            this.textResponseHeader.Name = "textResponseHeader";
            this.textResponseHeader.Size = new System.Drawing.Size(298, 54);
            this.textResponseHeader.TabIndex = 24;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(367, 228);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "Response Header";
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(846, 157);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 25;
            this.button9.Text = "button9";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(846, 299);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(130, 23);
            this.button10.TabIndex = 26;
            this.button10.Text = "Test";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(1087, 363);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(130, 31);
            this.button11.TabIndex = 27;
            this.button11.Text = "測試比對假單狀態";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // textTimeOffRequestID
            // 
            this.textTimeOffRequestID.Location = new System.Drawing.Point(967, 363);
            this.textTimeOffRequestID.Name = "textTimeOffRequestID";
            this.textTimeOffRequestID.Size = new System.Drawing.Size(100, 22);
            this.textTimeOffRequestID.TabIndex = 28;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(858, 363);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(103, 12);
            this.label12.TabIndex = 17;
            this.label12.Text = "Time Off Request ID";
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(846, 255);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(144, 23);
            this.button12.TabIndex = 29;
            this.button12.Text = "Get all accounts";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(1117, 289);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(110, 23);
            this.button13.TabIndex = 30;
            this.button13.Text = "Test SignFlow";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1365, 606);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.textTimeOffRequestID);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.textResponseHeader);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.gridLeaveForm);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textImportHistoryEmpID);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.textBambooHREmployeeID);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
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
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textDate2);
            this.Controls.Add(this.textDate1);
            this.Name = "Form2";
            this.Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)(this.gridLeaveForm)).EndInit();
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
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBambooHREmployeeID;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textImportHistoryEmpID;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.DataGridView gridLeaveForm;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textResponseHeader;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.TextBox textTimeOffRequestID;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
    }
}

