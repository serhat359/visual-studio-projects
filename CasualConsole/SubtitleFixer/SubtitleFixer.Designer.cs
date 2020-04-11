namespace SubtitleFixer
{
    partial class SubtitleFixer
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
            this.fixButton = new System.Windows.Forms.Button();
            this.folderPathTextBox = new System.Windows.Forms.TextBox();
            this.msTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // fixButton
            // 
            this.fixButton.Enabled = false;
            this.fixButton.Location = new System.Drawing.Point(122, 165);
            this.fixButton.Name = "fixButton";
            this.fixButton.Size = new System.Drawing.Size(127, 23);
            this.fixButton.TabIndex = 3;
            this.fixButton.Text = "Fix";
            this.fixButton.UseVisualStyleBackColor = true;
            this.fixButton.Click += new System.EventHandler(this.fixButton_Click);
            // 
            // folderPathTextBox
            // 
            this.folderPathTextBox.Location = new System.Drawing.Point(122, 71);
            this.folderPathTextBox.Name = "folderPathTextBox";
            this.folderPathTextBox.Size = new System.Drawing.Size(361, 20);
            this.folderPathTextBox.TabIndex = 2;
            // 
            // msTextbox
            // 
            this.msTextbox.Location = new System.Drawing.Point(122, 119);
            this.msTextbox.Name = "msTextbox";
            this.msTextbox.Size = new System.Drawing.Size(127, 20);
            this.msTextbox.TabIndex = 4;
            this.msTextbox.TextChanged += new System.EventHandler(this.msTextbox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "File";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Milliseconds";
            // 
            // SubtitleFixer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.msTextbox);
            this.Controls.Add(this.fixButton);
            this.Controls.Add(this.folderPathTextBox);
            this.Name = "SubtitleFixer";
            this.Text = "Subtitle Fixer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button fixButton;
        private System.Windows.Forms.TextBox folderPathTextBox;
        private System.Windows.Forms.TextBox msTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

