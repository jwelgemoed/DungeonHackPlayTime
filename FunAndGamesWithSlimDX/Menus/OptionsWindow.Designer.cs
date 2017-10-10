namespace FunAndGamesWithSharpDX.Menus
{
    partial class OptionsWindow
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
            this.chkMSA = new System.Windows.Forms.CheckBox();
            this.btnDone = new System.Windows.Forms.Button();
            this.lblFps = new System.Windows.Forms.Label();
            this.valFps = new System.Windows.Forms.Label();
            this.chkWireframe = new System.Windows.Forms.CheckBox();
            this.chkRestrictMovementXZ = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMouseSensitivity = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkMSA
            // 
            this.chkMSA.AutoSize = true;
            this.chkMSA.Location = new System.Drawing.Point(10, 24);
            this.chkMSA.Margin = new System.Windows.Forms.Padding(2);
            this.chkMSA.Name = "chkMSA";
            this.chkMSA.Size = new System.Drawing.Size(184, 17);
            this.chkMSA.TabIndex = 0;
            this.chkMSA.Text = "Use 4X Multi-Sample Anti Aliasing";
            this.chkMSA.UseVisualStyleBackColor = true;
            this.chkMSA.CheckedChanged += new System.EventHandler(this.chkMSA_CheckedChanged);
            // 
            // btnDone
            // 
            this.btnDone.Location = new System.Drawing.Point(10, 232);
            this.btnDone.Margin = new System.Windows.Forms.Padding(2);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(61, 21);
            this.btnDone.TabIndex = 1;
            this.btnDone.Text = "Save";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // lblFps
            // 
            this.lblFps.AutoSize = true;
            this.lblFps.Location = new System.Drawing.Point(251, 26);
            this.lblFps.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFps.Name = "lblFps";
            this.lblFps.Size = new System.Drawing.Size(68, 13);
            this.lblFps.TabIndex = 2;
            this.lblFps.Text = "Frame Rate :";
            // 
            // valFps
            // 
            this.valFps.AutoSize = true;
            this.valFps.Location = new System.Drawing.Point(362, 27);
            this.valFps.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.valFps.Name = "valFps";
            this.valFps.Size = new System.Drawing.Size(0, 13);
            this.valFps.TabIndex = 3;
            // 
            // chkWireframe
            // 
            this.chkWireframe.AutoSize = true;
            this.chkWireframe.Location = new System.Drawing.Point(10, 46);
            this.chkWireframe.Margin = new System.Windows.Forms.Padding(2);
            this.chkWireframe.Name = "chkWireframe";
            this.chkWireframe.Size = new System.Drawing.Size(74, 17);
            this.chkWireframe.TabIndex = 4;
            this.chkWireframe.Text = "Wireframe";
            this.chkWireframe.UseVisualStyleBackColor = true;
            this.chkWireframe.CheckedChanged += new System.EventHandler(this.chkWireframe_CheckedChanged);
            // 
            // chkRestrictMovementXZ
            // 
            this.chkRestrictMovementXZ.AutoSize = true;
            this.chkRestrictMovementXZ.Location = new System.Drawing.Point(10, 69);
            this.chkRestrictMovementXZ.Margin = new System.Windows.Forms.Padding(2);
            this.chkRestrictMovementXZ.Name = "chkRestrictMovementXZ";
            this.chkRestrictMovementXZ.Size = new System.Drawing.Size(173, 17);
            this.chkRestrictMovementXZ.TabIndex = 5;
            this.chkRestrictMovementXZ.Text = "Restrict Movement to XZ plane";
            this.chkRestrictMovementXZ.UseVisualStyleBackColor = true;
            this.chkRestrictMovementXZ.CheckedChanged += new System.EventHandler(this.chkRestrictMovementXZ_CheckedChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(10, 92);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(184, 21);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(201, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Shader Technique";
            // 
            // txtMouseSensitivity
            // 
            this.txtMouseSensitivity.Location = new System.Drawing.Point(10, 120);
            this.txtMouseSensitivity.MaxLength = 5;
            this.txtMouseSensitivity.Name = "txtMouseSensitivity";
            this.txtMouseSensitivity.Size = new System.Drawing.Size(42, 20);
            this.txtMouseSensitivity.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Mouse Sensitivity";
            // 
            // OptionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 262);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMouseSensitivity);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.chkRestrictMovementXZ);
            this.Controls.Add(this.chkWireframe);
            this.Controls.Add(this.valFps);
            this.Controls.Add(this.lblFps);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.chkMSA);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "OptionsWindow";
            this.Text = "OptionsWindow";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OptionsWindow_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkMSA;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Label lblFps;
        private System.Windows.Forms.Label valFps;
        private System.Windows.Forms.CheckBox chkWireframe;
        private System.Windows.Forms.CheckBox chkRestrictMovementXZ;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMouseSensitivity;
        private System.Windows.Forms.Label label2;
    }
}