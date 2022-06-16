
namespace FeaCard
{
    partial class Form1
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.defButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.filebox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // defButton
            // 
            this.defButton.BackColor = System.Drawing.SystemColors.HotTrack;
            this.defButton.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.defButton.ForeColor = System.Drawing.SystemColors.Info;
            this.defButton.Location = new System.Drawing.Point(12, 12);
            this.defButton.Name = "defButton";
            this.defButton.Size = new System.Drawing.Size(278, 48);
            this.defButton.TabIndex = 2;
            this.defButton.Text = "Choose Definition File";
            this.defButton.UseVisualStyleBackColor = false;
            this.defButton.Click += new System.EventHandler(this.defButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.statusLabel.Location = new System.Drawing.Point(12, 73);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(42, 21);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "        ";
            // 
            // filebox
            // 
            this.filebox.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.filebox.Location = new System.Drawing.Point(12, 97);
            this.filebox.Multiline = true;
            this.filebox.Name = "filebox";
            this.filebox.ReadOnly = true;
            this.filebox.Size = new System.Drawing.Size(278, 79);
            this.filebox.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(302, 188);
            this.Controls.Add(this.filebox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.defButton);
            this.Name = "Form1";
            this.Text = "FeaCard 1.0";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button defButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.TextBox filebox;
    }
}

