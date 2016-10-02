namespace Nobetci
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
            this.nobetTable = new System.Windows.Forms.DataGridView();
            this.myCalendar = new System.Windows.Forms.DataGridView();
            this.Pzt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Çar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Per = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Paz = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.nobetTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myCalendar)).BeginInit();
            this.SuspendLayout();
            // 
            // nobetTable
            // 
            this.nobetTable.AllowUserToResizeRows = false;
            this.nobetTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.nobetTable.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.nobetTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.nobetTable.Location = new System.Drawing.Point(12, 12);
            this.nobetTable.Name = "nobetTable";
            this.nobetTable.Size = new System.Drawing.Size(550, 405);
            this.nobetTable.TabIndex = 0;
            this.nobetTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.nobetTable_CellClick);
            this.nobetTable.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.nobetTable_CellEndEdit);
            // 
            // myCalendar
            // 
            this.myCalendar.AllowUserToAddRows = false;
            this.myCalendar.AllowUserToDeleteRows = false;
            this.myCalendar.AllowUserToResizeColumns = false;
            this.myCalendar.AllowUserToResizeRows = false;
            this.myCalendar.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.myCalendar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.myCalendar.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Pzt,
            this.Sal,
            this.Çar,
            this.Per,
            this.Cum,
            this.Cts,
            this.Paz});
            this.myCalendar.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
            this.myCalendar.Location = new System.Drawing.Point(582, 12);
            this.myCalendar.Name = "myCalendar";
            this.myCalendar.ReadOnly = true;
            this.myCalendar.Size = new System.Drawing.Size(240, 157);
            this.myCalendar.TabIndex = 2;
            this.myCalendar.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.myCalendar_CellContentDoubleClick);
            // 
            // Pzt
            // 
            this.Pzt.HeaderText = "Pzt";
            this.Pzt.Name = "Pzt";
            this.Pzt.ReadOnly = true;
            // 
            // Sal
            // 
            this.Sal.HeaderText = "Sal";
            this.Sal.Name = "Sal";
            this.Sal.ReadOnly = true;
            // 
            // Çar
            // 
            this.Çar.HeaderText = "Çar";
            this.Çar.Name = "Çar";
            this.Çar.ReadOnly = true;
            // 
            // Per
            // 
            this.Per.HeaderText = "Per";
            this.Per.Name = "Per";
            this.Per.ReadOnly = true;
            // 
            // Cum
            // 
            this.Cum.HeaderText = "Cum";
            this.Cum.Name = "Cum";
            this.Cum.ReadOnly = true;
            // 
            // Cts
            // 
            this.Cts.HeaderText = "Cts";
            this.Cts.Name = "Cts";
            this.Cts.ReadOnly = true;
            // 
            // Paz
            // 
            this.Paz.HeaderText = "Paz";
            this.Paz.Name = "Paz";
            this.Paz.ReadOnly = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 429);
            this.Controls.Add(this.myCalendar);
            this.Controls.Add(this.nobetTable);
            this.Name = "Form1";
            this.Text = "Nöbet Düzenleme";
            ((System.ComponentModel.ISupportInitialize)(this.nobetTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.myCalendar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView nobetTable;
        private System.Windows.Forms.DataGridView myCalendar;
        private System.Windows.Forms.DataGridViewTextBoxColumn Pzt;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sal;
        private System.Windows.Forms.DataGridViewTextBoxColumn Çar;
        private System.Windows.Forms.DataGridViewTextBoxColumn Per;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cum;
        private System.Windows.Forms.DataGridViewTextBoxColumn Cts;
        private System.Windows.Forms.DataGridViewTextBoxColumn Paz;
    }
}

