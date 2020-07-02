namespace DuplicateFinder
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
            this.sourcePathTxt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.destinationPathTxt = new System.Windows.Forms.TextBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.correctSourceCreationDateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // sourcePathTxt
            // 
            this.sourcePathTxt.Location = new System.Drawing.Point(162, 79);
            this.sourcePathTxt.Name = "sourcePathTxt";
            this.sourcePathTxt.Size = new System.Drawing.Size(406, 20);
            this.sourcePathTxt.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(74, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Source Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(74, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Destination Path";
            // 
            // destinationPathTxt
            // 
            this.destinationPathTxt.Location = new System.Drawing.Point(162, 128);
            this.destinationPathTxt.Name = "destinationPathTxt";
            this.destinationPathTxt.Size = new System.Drawing.Size(406, 20);
            this.destinationPathTxt.TabIndex = 2;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(162, 176);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(210, 23);
            this.deleteButton.TabIndex = 4;
            this.deleteButton.Text = "Delete Duplicate Files";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // correctSourceCreationDateButton
            // 
            this.correctSourceCreationDateButton.Location = new System.Drawing.Point(162, 226);
            this.correctSourceCreationDateButton.Name = "correctSourceCreationDateButton";
            this.correctSourceCreationDateButton.Size = new System.Drawing.Size(210, 23);
            this.correctSourceCreationDateButton.TabIndex = 6;
            this.correctSourceCreationDateButton.Text = "Correct Source Creation Date";
            this.correctSourceCreationDateButton.UseVisualStyleBackColor = true;
            this.correctSourceCreationDateButton.Click += new System.EventHandler(this.correctSourceCreationDateButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.correctSourceCreationDateButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.destinationPathTxt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sourcePathTxt);
            this.Name = "Form1";
            this.Text = "Duplicate Finder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox sourcePathTxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox destinationPathTxt;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button correctSourceCreationDateButton;
    }
}

