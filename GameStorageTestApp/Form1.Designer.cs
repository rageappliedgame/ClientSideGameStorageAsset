namespace UserModel
{
    partial class Form1
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
            this.btnSaveStructure = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnLoadStructure = new System.Windows.Forms.Button();
            this.btnVirtual = new System.Windows.Forms.Button();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnBinarySaveLoad = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnSaveLoadStructure = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.btnSaveData = new System.Windows.Forms.Button();
            this.btnFilter = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSaveStructure
            // 
            this.btnSaveStructure.Location = new System.Drawing.Point(615, 407);
            this.btnSaveStructure.Name = "btnSaveStructure";
            this.btnSaveStructure.Size = new System.Drawing.Size(75, 23);
            this.btnSaveStructure.TabIndex = 0;
            this.btnSaveStructure.Text = "toXml()";
            this.btnSaveStructure.UseVisualStyleBackColor = true;
            this.btnSaveStructure.Click += new System.EventHandler(this.btnSaveStructure_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(13, 13);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(381, 334);
            this.textBox1.TabIndex = 1;
            // 
            // btnLoadStructure
            // 
            this.btnLoadStructure.Location = new System.Drawing.Point(615, 380);
            this.btnLoadStructure.Name = "btnLoadStructure";
            this.btnLoadStructure.Size = new System.Drawing.Size(75, 23);
            this.btnLoadStructure.TabIndex = 2;
            this.btnLoadStructure.Text = "fromXml()";
            this.btnLoadStructure.UseVisualStyleBackColor = true;
            this.btnLoadStructure.Click += new System.EventHandler(this.btnLoadStructure_Click);
            // 
            // btnVirtual
            // 
            this.btnVirtual.Location = new System.Drawing.Point(224, 353);
            this.btnVirtual.Name = "btnVirtual";
            this.btnVirtual.Size = new System.Drawing.Size(125, 23);
            this.btnVirtual.TabIndex = 3;
            this.btnVirtual.Text = "3) Virtual";
            this.btnVirtual.UseVisualStyleBackColor = true;
            this.btnVirtual.Click += new System.EventHandler(this.btnVirtual_Click);
            // 
            // btnLoadData
            // 
            this.btnLoadData.Location = new System.Drawing.Point(224, 381);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(125, 23);
            this.btnLoadData.TabIndex = 4;
            this.btnLoadData.Text = "Data <- Server";
            this.btnLoadData.UseVisualStyleBackColor = true;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);
            // 
            // btnBinarySaveLoad
            // 
            this.btnBinarySaveLoad.Location = new System.Drawing.Point(355, 381);
            this.btnBinarySaveLoad.Name = "btnBinarySaveLoad";
            this.btnBinarySaveLoad.Size = new System.Drawing.Size(107, 23);
            this.btnBinarySaveLoad.TabIndex = 5;
            this.btnBinarySaveLoad.Text = "Binary()";
            this.btnBinarySaveLoad.UseVisualStyleBackColor = true;
            this.btnBinarySaveLoad.Click += new System.EventHandler(this.btnBinarySaveLoad_Click);
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(411, 12);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(279, 335);
            this.textBox2.TabIndex = 6;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(12, 352);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 7;
            this.btnConnect.Text = "1) Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnSaveLoadStructure
            // 
            this.btnSaveLoadStructure.Location = new System.Drawing.Point(93, 352);
            this.btnSaveLoadStructure.Name = "btnSaveLoadStructure";
            this.btnSaveLoadStructure.Size = new System.Drawing.Size(125, 23);
            this.btnSaveLoadStructure.TabIndex = 8;
            this.btnSaveLoadStructure.Text = "2) Stucture <-> Server";
            this.btnSaveLoadStructure.UseVisualStyleBackColor = true;
            this.btnSaveLoadStructure.Click += new System.EventHandler(this.btnSaveLoadStructure_Click);
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.Location = new System.Drawing.Point(13, 410);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(597, 180);
            this.textBox3.TabIndex = 9;
            // 
            // btnSaveData
            // 
            this.btnSaveData.Location = new System.Drawing.Point(93, 381);
            this.btnSaveData.Name = "btnSaveData";
            this.btnSaveData.Size = new System.Drawing.Size(125, 23);
            this.btnSaveData.TabIndex = 10;
            this.btnSaveData.Text = "Data -> Server";
            this.btnSaveData.UseVisualStyleBackColor = true;
            this.btnSaveData.Click += new System.EventHandler(this.btnSaveData_Click);
            // 
            // btnFilter
            // 
            this.btnFilter.Location = new System.Drawing.Point(355, 352);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(107, 23);
            this.btnFilter.TabIndex = 11;
            this.btnFilter.Text = "4) Enumate Filtered";
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 618);
            this.Controls.Add(this.btnFilter);
            this.Controls.Add(this.btnSaveData);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.btnSaveLoadStructure);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.btnBinarySaveLoad);
            this.Controls.Add(this.btnLoadData);
            this.Controls.Add(this.btnVirtual);
            this.Controls.Add(this.btnLoadStructure);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnSaveStructure);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "GameStorage Client Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSaveStructure;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnLoadStructure;
        private System.Windows.Forms.Button btnVirtual;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.Button btnBinarySaveLoad;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSaveLoadStructure;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button btnSaveData;
        private System.Windows.Forms.Button btnFilter;
    }
}

