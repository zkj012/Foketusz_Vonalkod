namespace Vonalkod
{
    partial class CimInput
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
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbNev = new System.Windows.Forms.ComboBox();
            this.cbJelleg = new System.Windows.Forms.ComboBox();
            this.cbIrsz = new System.Windows.Forms.ComboBox();
            this.txtJel1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtJel2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtLepcsohaz = new System.Windows.Forms.TextBox();
            this.txtSzam1 = new System.Windows.Forms.TextBox();
            this.txtSzam2 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtEpulet = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(88, 180);
            this.btnSave.Name = "btnSave";
            this.btnSave.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnSave.Size = new System.Drawing.Size(186, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Cím mentése";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Irányítószám:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Név:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Jelleg:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Házszám:";
            // 
            // cbNev
            // 
            this.cbNev.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbNev.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cbNev.FormattingEnabled = true;
            this.cbNev.Location = new System.Drawing.Point(88, 38);
            this.cbNev.Name = "cbNev";
            this.cbNev.Size = new System.Drawing.Size(186, 21);
            this.cbNev.TabIndex = 2;
            this.cbNev.SelectedIndexChanged += new System.EventHandler(this.cbNev_SelectedIndexChanged);
            // 
            // cbJelleg
            // 
            this.cbJelleg.DisplayMember = "Nev";
            this.cbJelleg.FormattingEnabled = true;
            this.cbJelleg.Location = new System.Drawing.Point(88, 67);
            this.cbJelleg.Name = "cbJelleg";
            this.cbJelleg.Size = new System.Drawing.Size(186, 21);
            this.cbJelleg.TabIndex = 3;
            this.cbJelleg.ValueMember = "Kod";
            // 
            // cbIrsz
            // 
            this.cbIrsz.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbIrsz.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cbIrsz.FormattingEnabled = true;
            this.cbIrsz.Location = new System.Drawing.Point(88, 6);
            this.cbIrsz.Name = "cbIrsz";
            this.cbIrsz.Size = new System.Drawing.Size(186, 21);
            this.cbIrsz.TabIndex = 1;
            this.cbIrsz.SelectedIndexChanged += new System.EventHandler(this.cbIrsz_SelectedIndexChanged);
            // 
            // txtJel1
            // 
            this.txtJel1.Location = new System.Drawing.Point(139, 97);
            this.txtJel1.Name = "txtJel1";
            this.txtJel1.Size = new System.Drawing.Size(27, 20);
            this.txtJel1.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(172, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "-";
            // 
            // txtJel2
            // 
            this.txtJel2.Location = new System.Drawing.Point(241, 98);
            this.txtJel2.Name = "txtJel2";
            this.txtJel2.Size = new System.Drawing.Size(25, 20);
            this.txtJel2.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 157);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Lépcsőház:";
            // 
            // txtLepcsohaz
            // 
            this.txtLepcsohaz.Location = new System.Drawing.Point(88, 154);
            this.txtLepcsohaz.Name = "txtLepcsohaz";
            this.txtLepcsohaz.Size = new System.Drawing.Size(78, 20);
            this.txtLepcsohaz.TabIndex = 9;
            // 
            // txtSzam1
            // 
            this.txtSzam1.Location = new System.Drawing.Point(88, 97);
            this.txtSzam1.Name = "txtSzam1";
            this.txtSzam1.Size = new System.Drawing.Size(37, 20);
            this.txtSzam1.TabIndex = 4;
            this.txtSzam1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSzam1_KeyPress);
            // 
            // txtSzam2
            // 
            this.txtSzam2.Location = new System.Drawing.Point(188, 97);
            this.txtSzam2.Name = "txtSzam2";
            this.txtSzam2.Size = new System.Drawing.Size(38, 20);
            this.txtSzam2.TabIndex = 6;
            this.txtSzam2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSzam2_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(125, 100);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(12, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "/";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(228, 101);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(12, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "/";
            // 
            // txtEpulet
            // 
            this.txtEpulet.Location = new System.Drawing.Point(89, 124);
            this.txtEpulet.Name = "txtEpulet";
            this.txtEpulet.Size = new System.Drawing.Size(78, 20);
            this.txtEpulet.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(42, 127);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Épület:";
            // 
            // CimInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 215);
            this.Controls.Add(this.txtEpulet);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtSzam2);
            this.Controls.Add(this.txtSzam1);
            this.Controls.Add(this.txtLepcsohaz);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtJel2);
            this.Controls.Add(this.txtJel1);
            this.Controls.Add(this.cbJelleg);
            this.Controls.Add(this.cbIrsz);
            this.Controls.Add(this.cbNev);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSave);
            this.Name = "CimInput";
            this.Text = "Cim";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CimInput_FormClosing);
            this.Load += new System.EventHandler(this.CimInput_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbNev;
        private System.Windows.Forms.ComboBox cbJelleg;
        private System.Windows.Forms.ComboBox cbIrsz;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtJel2;
        private System.Windows.Forms.TextBox txtJel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtLepcsohaz;
        private System.Windows.Forms.TextBox txtSzam1;
        private System.Windows.Forms.TextBox txtSzam2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtEpulet;
        private System.Windows.Forms.Label label6;
    }
}