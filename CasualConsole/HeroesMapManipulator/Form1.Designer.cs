namespace HeroesMapManipulator
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
            this.fileDragLabel = new System.Windows.Forms.Label();
            this.beginButton = new System.Windows.Forms.Button();
            this.randomizeChestCheckBox = new System.Windows.Forms.CheckBox();
            this.checkBoxPanel = new System.Windows.Forms.Panel();
            this.deleteAdditionalMonsterCheckBox = new System.Windows.Forms.CheckBox();
            this.deleteMonolithCheckBox = new System.Windows.Forms.CheckBox();
            this.weakenShipyardsCheckBox = new System.Windows.Forms.CheckBox();
            this.checkBoxPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileDragLabel
            // 
            this.fileDragLabel.AutoSize = true;
            this.fileDragLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileDragLabel.Location = new System.Drawing.Point(31, 108);
            this.fileDragLabel.Name = "fileDragLabel";
            this.fileDragLabel.Size = new System.Drawing.Size(220, 29);
            this.fileDragLabel.TabIndex = 0;
            this.fileDragLabel.Text = "Drag Map File here";
            // 
            // beginButton
            // 
            this.beginButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.beginButton.Location = new System.Drawing.Point(36, 197);
            this.beginButton.Name = "beginButton";
            this.beginButton.Size = new System.Drawing.Size(215, 53);
            this.beginButton.TabIndex = 1;
            this.beginButton.Text = "Begin";
            this.beginButton.UseVisualStyleBackColor = true;
            this.beginButton.Click += new System.EventHandler(this.beginButton_Click);
            // 
            // randomizeChestCheckBox
            // 
            this.randomizeChestCheckBox.AutoSize = true;
            this.randomizeChestCheckBox.Location = new System.Drawing.Point(3, 3);
            this.randomizeChestCheckBox.Name = "randomizeChestCheckBox";
            this.randomizeChestCheckBox.Size = new System.Drawing.Size(114, 17);
            this.randomizeChestCheckBox.TabIndex = 2;
            this.randomizeChestCheckBox.Text = "Randomize Chests";
            this.randomizeChestCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkBoxPanel
            // 
            this.checkBoxPanel.Controls.Add(this.weakenShipyardsCheckBox);
            this.checkBoxPanel.Controls.Add(this.deleteMonolithCheckBox);
            this.checkBoxPanel.Controls.Add(this.deleteAdditionalMonsterCheckBox);
            this.checkBoxPanel.Controls.Add(this.randomizeChestCheckBox);
            this.checkBoxPanel.Location = new System.Drawing.Point(36, 12);
            this.checkBoxPanel.Name = "checkBoxPanel";
            this.checkBoxPanel.Size = new System.Drawing.Size(200, 179);
            this.checkBoxPanel.TabIndex = 3;
            // 
            // deleteAdditionalMonsterCheckBox
            // 
            this.deleteAdditionalMonsterCheckBox.AutoSize = true;
            this.deleteAdditionalMonsterCheckBox.Location = new System.Drawing.Point(4, 27);
            this.deleteAdditionalMonsterCheckBox.Name = "deleteAdditionalMonsterCheckBox";
            this.deleteAdditionalMonsterCheckBox.Size = new System.Drawing.Size(130, 17);
            this.deleteAdditionalMonsterCheckBox.TabIndex = 3;
            this.deleteAdditionalMonsterCheckBox.Text = "Delete Extra Monsters";
            this.deleteAdditionalMonsterCheckBox.UseVisualStyleBackColor = true;
            // 
            // deleteMonolithCheckBox
            // 
            this.deleteMonolithCheckBox.AutoSize = true;
            this.deleteMonolithCheckBox.Location = new System.Drawing.Point(4, 51);
            this.deleteMonolithCheckBox.Name = "deleteMonolithCheckBox";
            this.deleteMonolithCheckBox.Size = new System.Drawing.Size(105, 17);
            this.deleteMonolithCheckBox.TabIndex = 4;
            this.deleteMonolithCheckBox.Text = "Delete Monoliths";
            this.deleteMonolithCheckBox.UseVisualStyleBackColor = true;
            // 
            // weakenShipyardsCheckBox
            // 
            this.weakenShipyardsCheckBox.AutoSize = true;
            this.weakenShipyardsCheckBox.Location = new System.Drawing.Point(4, 73);
            this.weakenShipyardsCheckBox.Name = "weakenShipyardsCheckBox";
            this.weakenShipyardsCheckBox.Size = new System.Drawing.Size(116, 17);
            this.weakenShipyardsCheckBox.TabIndex = 5;
            this.weakenShipyardsCheckBox.Text = "Weaken Shipyards";
            this.weakenShipyardsCheckBox.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.fileDragLabel);
            this.Controls.Add(this.checkBoxPanel);
            this.Controls.Add(this.beginButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.checkBoxPanel.ResumeLayout(false);
            this.checkBoxPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label fileDragLabel;
        private System.Windows.Forms.Button beginButton;
        private System.Windows.Forms.CheckBox randomizeChestCheckBox;
        private System.Windows.Forms.Panel checkBoxPanel;
        private System.Windows.Forms.CheckBox deleteAdditionalMonsterCheckBox;
        private System.Windows.Forms.CheckBox deleteMonolithCheckBox;
        private System.Windows.Forms.CheckBox weakenShipyardsCheckBox;
    }
}

