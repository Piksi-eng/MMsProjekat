namespace MMSProject
{
    partial class Gama
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.RedTxtBox = new System.Windows.Forms.TextBox();
            this.GreenTxtBox = new System.Windows.Forms.TextBox();
            this.BlueTxtBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.OkBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Red :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Green :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Blue :";
            // 
            // RedTxtBox
            // 
            this.RedTxtBox.Location = new System.Drawing.Point(94, 49);
            this.RedTxtBox.Name = "RedTxtBox";
            this.RedTxtBox.Size = new System.Drawing.Size(100, 23);
            this.RedTxtBox.TabIndex = 3;
            // 
            // GreenTxtBox
            // 
            this.GreenTxtBox.Location = new System.Drawing.Point(94, 88);
            this.GreenTxtBox.Name = "GreenTxtBox";
            this.GreenTxtBox.Size = new System.Drawing.Size(100, 23);
            this.GreenTxtBox.TabIndex = 4;
            // 
            // BlueTxtBox
            // 
            this.BlueTxtBox.Location = new System.Drawing.Point(94, 128);
            this.BlueTxtBox.Name = "BlueTxtBox";
            this.BlueTxtBox.Size = new System.Drawing.Size(100, 23);
            this.BlueTxtBox.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(121, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "0.2-5.0";
            // 
            // OkBtn
            // 
            this.OkBtn.Location = new System.Drawing.Point(48, 199);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(75, 23);
            this.OkBtn.TabIndex = 7;
            this.OkBtn.Text = "Ok";
            this.OkBtn.UseVisualStyleBackColor = true;
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(152, 202);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 8;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // Gama
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 249);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BlueTxtBox);
            this.Controls.Add(this.GreenTxtBox);
            this.Controls.Add(this.RedTxtBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Gama";
            this.Text = "Gama";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox RedTxtBox;
        private TextBox GreenTxtBox;
        private TextBox BlueTxtBox;
        private Label label4;
        private Button OkBtn;
        private Button CancelBtn;
    }
}