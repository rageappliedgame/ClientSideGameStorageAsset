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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnVirtual = new System.Windows.Forms.Button();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnSaveLoadStructure = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.btnSaveData = new System.Windows.Forms.Button();
            this.btnFilter = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnSaveStructure = new System.Windows.Forms.Button();
            this.btnLoadStructure = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(933, 794);
            this.textBox1.TabIndex = 1;
            // 
            // btnVirtual
            // 
            this.btnVirtual.Location = new System.Drawing.Point(1587, 932);
            this.btnVirtual.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnVirtual.Name = "btnVirtual";
            this.btnVirtual.Size = new System.Drawing.Size(333, 55);
            this.btnVirtual.TabIndex = 3;
            this.btnVirtual.Text = "Virtual Values";
            this.btnVirtual.UseVisualStyleBackColor = true;
            this.btnVirtual.Click += new System.EventHandler(this.btnVirtual_Click);
            // 
            // btnLoadData
            // 
            this.btnLoadData.Location = new System.Drawing.Point(597, 932);
            this.btnLoadData.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(333, 55);
            this.btnLoadData.TabIndex = 4;
            this.btnLoadData.Text = "3) Data <- Server";
            this.btnLoadData.UseVisualStyleBackColor = true;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);
            // 
            // textBox2
            // 
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox2.Location = new System.Drawing.Point(0, 0);
            this.textBox2.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(941, 794);
            this.textBox2.TabIndex = 6;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(35, 839);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(200, 55);
            this.btnConnect.TabIndex = 7;
            this.btnConnect.Text = "1) Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnSaveLoadStructure
            // 
            this.btnSaveLoadStructure.Location = new System.Drawing.Point(248, 839);
            this.btnSaveLoadStructure.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnSaveLoadStructure.Name = "btnSaveLoadStructure";
            this.btnSaveLoadStructure.Size = new System.Drawing.Size(682, 55);
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
            this.textBox3.Location = new System.Drawing.Point(35, 1075);
            this.textBox3.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(1462, 327);
            this.textBox3.TabIndex = 9;
            // 
            // btnSaveData
            // 
            this.btnSaveData.Location = new System.Drawing.Point(248, 932);
            this.btnSaveData.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnSaveData.Name = "btnSaveData";
            this.btnSaveData.Size = new System.Drawing.Size(333, 55);
            this.btnSaveData.TabIndex = 10;
            this.btnSaveData.Text = "2) Data -> Server";
            this.btnSaveData.UseVisualStyleBackColor = true;
            this.btnSaveData.Click += new System.EventHandler(this.btnSaveData_Click);
            // 
            // btnFilter
            // 
            this.btnFilter.Location = new System.Drawing.Point(1238, 932);
            this.btnFilter.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(333, 55);
            this.btnFilter.TabIndex = 11;
            this.btnFilter.Text = "Enumerate Filtered";
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(35, 29);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox2);
            this.splitContainer1.Size = new System.Drawing.Size(1885, 794);
            this.splitContainer1.SplitterDistance = 933;
            this.splitContainer1.SplitterWidth = 11;
            this.splitContainer1.TabIndex = 13;
            // 
            // btnSaveStructure
            // 
            this.btnSaveStructure.Location = new System.Drawing.Point(1238, 839);
            this.btnSaveStructure.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnSaveStructure.Name = "btnSaveStructure";
            this.btnSaveStructure.Size = new System.Drawing.Size(333, 55);
            this.btnSaveStructure.TabIndex = 15;
            this.btnSaveStructure.Text = "1) Structure -> Local";
            this.btnSaveStructure.UseVisualStyleBackColor = true;
            this.btnSaveStructure.Click += new System.EventHandler(this.btnSaveStructure_Click);
            // 
            // btnLoadStructure
            // 
            this.btnLoadStructure.Location = new System.Drawing.Point(1587, 839);
            this.btnLoadStructure.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.btnLoadStructure.Name = "btnLoadStructure";
            this.btnLoadStructure.Size = new System.Drawing.Size(333, 55);
            this.btnLoadStructure.TabIndex = 16;
            this.btnLoadStructure.Text = "2) Structure <- Local";
            this.btnLoadStructure.UseVisualStyleBackColor = true;
            this.btnLoadStructure.Click += new System.EventHandler(this.btnLoadStructure_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1952, 1474);
            this.Controls.Add(this.btnLoadStructure);
            this.Controls.Add(this.btnSaveStructure);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnFilter);
            this.Controls.Add(this.btnSaveData);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.btnSaveLoadStructure);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnLoadData);
            this.Controls.Add(this.btnVirtual);
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "GameStorage Client Tester";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnVirtual;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSaveLoadStructure;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button btnSaveData;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnSaveStructure;
        private System.Windows.Forms.Button btnLoadStructure;
    }
}

