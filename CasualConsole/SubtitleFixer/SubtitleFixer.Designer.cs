namespace SubtitleFixer
{
    partial class SubtitleFixer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            this.simpleFixButton = new System.Windows.Forms.Button();
            this.simpleFolderPathTextBox = new System.Windows.Forms.TextBox();
            this.msTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.simpleFixTab = new System.Windows.Forms.TabPage();
            this.complexFixTab = new System.Windows.Forms.TabPage();
            this.complexFixButton = new System.Windows.Forms.Button();
            this.expected2Textbox = new System.Windows.Forms.TextBox();
            this.inFile2Textbox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.expected1Textbox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.inFile1Textbox = new System.Windows.Forms.TextBox();
            this.complexFolderPathTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.simpleFixTab.SuspendLayout();
            this.complexFixTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // simpleFixButton
            // 
            this.simpleFixButton.Enabled = false;
            this.simpleFixButton.Location = new System.Drawing.Point(91, 115);
            this.simpleFixButton.Name = "simpleFixButton";
            this.simpleFixButton.Size = new System.Drawing.Size(127, 23);
            this.simpleFixButton.TabIndex = 3;
            this.simpleFixButton.Text = "Fix";
            this.simpleFixButton.UseVisualStyleBackColor = true;
            this.simpleFixButton.Click += new System.EventHandler(this.simpleFixButton_Click);
            // 
            // simpleFolderPathTextBox
            // 
            this.simpleFolderPathTextBox.Location = new System.Drawing.Point(91, 21);
            this.simpleFolderPathTextBox.Name = "simpleFolderPathTextBox";
            this.simpleFolderPathTextBox.Size = new System.Drawing.Size(361, 20);
            this.simpleFolderPathTextBox.TabIndex = 2;
            // 
            // msTextbox
            // 
            this.msTextbox.Location = new System.Drawing.Point(91, 69);
            this.msTextbox.Name = "msTextbox";
            this.msTextbox.Size = new System.Drawing.Size(127, 20);
            this.msTextbox.TabIndex = 4;
            this.msTextbox.TextChanged += new System.EventHandler(this.msTextbox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "File";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Millis Add";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.simpleFixTab);
            this.tabControl1.Controls.Add(this.complexFixTab);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(776, 426);
            this.tabControl1.TabIndex = 7;
            // 
            // simpleFixTab
            // 
            this.simpleFixTab.Controls.Add(this.msTextbox);
            this.simpleFixTab.Controls.Add(this.label2);
            this.simpleFixTab.Controls.Add(this.simpleFolderPathTextBox);
            this.simpleFixTab.Controls.Add(this.label1);
            this.simpleFixTab.Controls.Add(this.simpleFixButton);
            this.simpleFixTab.Location = new System.Drawing.Point(4, 22);
            this.simpleFixTab.Name = "simpleFixTab";
            this.simpleFixTab.Padding = new System.Windows.Forms.Padding(3);
            this.simpleFixTab.Size = new System.Drawing.Size(768, 400);
            this.simpleFixTab.TabIndex = 0;
            this.simpleFixTab.Text = "Simple Fix";
            this.simpleFixTab.UseVisualStyleBackColor = true;
            // 
            // complexFixTab
            // 
            this.complexFixTab.Controls.Add(this.complexFixButton);
            this.complexFixTab.Controls.Add(this.expected2Textbox);
            this.complexFixTab.Controls.Add(this.inFile2Textbox);
            this.complexFixTab.Controls.Add(this.label5);
            this.complexFixTab.Controls.Add(this.expected1Textbox);
            this.complexFixTab.Controls.Add(this.label4);
            this.complexFixTab.Controls.Add(this.inFile1Textbox);
            this.complexFixTab.Controls.Add(this.complexFolderPathTextBox);
            this.complexFixTab.Controls.Add(this.label3);
            this.complexFixTab.Location = new System.Drawing.Point(4, 22);
            this.complexFixTab.Name = "complexFixTab";
            this.complexFixTab.Padding = new System.Windows.Forms.Padding(3);
            this.complexFixTab.Size = new System.Drawing.Size(768, 400);
            this.complexFixTab.TabIndex = 1;
            this.complexFixTab.Text = "Complex Fix";
            this.complexFixTab.UseVisualStyleBackColor = true;
            // 
            // complexFixButton
            // 
            this.complexFixButton.Enabled = false;
            this.complexFixButton.Location = new System.Drawing.Point(91, 169);
            this.complexFixButton.Name = "complexFixButton";
            this.complexFixButton.Size = new System.Drawing.Size(127, 23);
            this.complexFixButton.TabIndex = 8;
            this.complexFixButton.Text = "Fix";
            this.complexFixButton.UseVisualStyleBackColor = true;
            this.complexFixButton.Click += new System.EventHandler(this.complexFixButton_Click);
            // 
            // expected2Textbox
            // 
            this.expected2Textbox.Location = new System.Drawing.Point(236, 120);
            this.expected2Textbox.Name = "expected2Textbox";
            this.expected2Textbox.Size = new System.Drawing.Size(127, 20);
            this.expected2Textbox.TabIndex = 7;
            this.expected2Textbox.PlaceholderText = "Expected";
            this.expected2Textbox.TextChanged += new System.EventHandler(this.complexTextbox_TextChanged);
            // 
            // inFile2Textbox
            // 
            this.inFile2Textbox.Location = new System.Drawing.Point(91, 120);
            this.inFile2Textbox.Name = "inFile2Textbox";
            this.inFile2Textbox.Size = new System.Drawing.Size(127, 20);
            this.inFile2Textbox.TabIndex = 6;
            this.inFile2Textbox.PlaceholderText = "In file";
            this.inFile2Textbox.TextChanged += new System.EventHandler(this.complexTextbox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Subtitle2";
            // 
            // expected1Textbox
            // 
            this.expected1Textbox.Location = new System.Drawing.Point(236, 69);
            this.expected1Textbox.Name = "expected1Textbox";
            this.expected1Textbox.Size = new System.Drawing.Size(127, 20);
            this.expected1Textbox.TabIndex = 4;
            this.expected1Textbox.PlaceholderText = "Expected";
            this.expected1Textbox.TextChanged += new System.EventHandler(this.complexTextbox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Subtitle1";
            // 
            // inFile1Textbox
            // 
            this.inFile1Textbox.Location = new System.Drawing.Point(91, 69);
            this.inFile1Textbox.Name = "inFile1Textbox";
            this.inFile1Textbox.Size = new System.Drawing.Size(127, 20);
            this.inFile1Textbox.TabIndex = 2;
            this.inFile1Textbox.PlaceholderText = "In file";
            this.inFile1Textbox.TextChanged += new System.EventHandler(this.complexTextbox_TextChanged);
            // 
            // complexFolderPathTextBox
            // 
            this.complexFolderPathTextBox.Location = new System.Drawing.Point(91, 21);
            this.complexFolderPathTextBox.Name = "complexFolderPathTextBox";
            this.complexFolderPathTextBox.Size = new System.Drawing.Size(361, 20);
            this.complexFolderPathTextBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "File";
            // 
            // SubtitleFixer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "SubtitleFixer";
            this.Text = "Subtitle Fixer";
            this.tabControl1.ResumeLayout(false);
            this.simpleFixTab.ResumeLayout(false);
            this.simpleFixTab.PerformLayout();
            this.complexFixTab.ResumeLayout(false);
            this.complexFixTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button simpleFixButton;
        private System.Windows.Forms.TextBox simpleFolderPathTextBox;
        private System.Windows.Forms.TextBox msTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage simpleFixTab;
        private System.Windows.Forms.TabPage complexFixTab;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox complexFolderPathTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox inFile1Textbox;
        private System.Windows.Forms.TextBox expected1Textbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox expected2Textbox;
        private System.Windows.Forms.TextBox inFile2Textbox;
        private System.Windows.Forms.Button complexFixButton;
    }
}