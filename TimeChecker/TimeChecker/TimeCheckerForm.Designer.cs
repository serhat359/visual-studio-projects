namespace TimeChecker
{
    partial class TimeCheckerForm
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
            this.startTimeInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.endTimeInput = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkTimeInput = new System.Windows.Forms.TextBox();
            this.checkButton = new System.Windows.Forms.Button();
            this.messageLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // startTimeInput
            // 
            this.startTimeInput.Location = new System.Drawing.Point(108, 39);
            this.startTimeInput.Name = "startTimeInput";
            this.startTimeInput.Size = new System.Drawing.Size(100, 20);
            this.startTimeInput.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(29, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "開始時刻";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(271, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "終了時刻";
            // 
            // endTimeInput
            // 
            this.endTimeInput.Location = new System.Drawing.Point(350, 39);
            this.endTimeInput.Name = "endTimeInput";
            this.endTimeInput.Size = new System.Drawing.Size(100, 20);
            this.endTimeInput.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(127, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "チェックする時刻";
            // 
            // checkTimeInput
            // 
            this.checkTimeInput.Location = new System.Drawing.Point(243, 107);
            this.checkTimeInput.Name = "checkTimeInput";
            this.checkTimeInput.Size = new System.Drawing.Size(100, 20);
            this.checkTimeInput.TabIndex = 4;
            // 
            // checkButton
            // 
            this.checkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkButton.Location = new System.Drawing.Point(179, 156);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(139, 35);
            this.checkButton.TabIndex = 6;
            this.checkButton.Text = "チェック";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // messageLabel
            // 
            this.messageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageLabel.Location = new System.Drawing.Point(12, 203);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(487, 75);
            this.messageLabel.TabIndex = 7;
            this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 300);
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.checkButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkTimeInput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.endTimeInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startTimeInput);
            this.Name = "Form1";
            this.Text = "Time Checker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox startTimeInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox endTimeInput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox checkTimeInput;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.Label messageLabel;
    }
}

