namespace GavelBackupGDrive.Ui
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.backupListBoxControl = new DevExpress.XtraEditors.ListBoxControl();
            this.serviceStatusLabel = new DevExpress.XtraEditors.LabelControl();
            this.stopButton = new DevExpress.XtraEditors.SimpleButton();
            this.startButton = new DevExpress.XtraEditors.SimpleButton();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.lblTimeRemaining = new DevExpress.XtraEditors.LabelControl();
            this.loginButton = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.backupListBoxControl)).BeginInit();
            this.SuspendLayout();
            // 
            // backupListBoxControl
            // 
            this.backupListBoxControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.backupListBoxControl.Location = new System.Drawing.Point(14, 63);
            this.backupListBoxControl.Name = "backupListBoxControl";
            this.backupListBoxControl.Size = new System.Drawing.Size(327, 227);
            this.backupListBoxControl.TabIndex = 7;
            // 
            // serviceStatusLabel
            // 
            this.serviceStatusLabel.Location = new System.Drawing.Point(14, 44);
            this.serviceStatusLabel.Name = "serviceStatusLabel";
            this.serviceStatusLabel.Size = new System.Drawing.Size(63, 13);
            this.serviceStatusLabel.TabIndex = 6;
            this.serviceStatusLabel.Text = "labelControl1";
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(95, 15);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 5;
            this.stopButton.Text = "Stop";
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(14, 15);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 4;
            this.startButton.Text = "Start";
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "GAVEL I.S Off Site";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // lblTimeRemaining
            // 
            this.lblTimeRemaining.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTimeRemaining.Location = new System.Drawing.Point(95, 44);
            this.lblTimeRemaining.Name = "lblTimeRemaining";
            this.lblTimeRemaining.Size = new System.Drawing.Size(81, 13);
            this.lblTimeRemaining.TabIndex = 8;
            this.lblTimeRemaining.Text = "lblTimeRemaining";
            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(176, 15);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(75, 23);
            this.loginButton.TabIndex = 9;
            this.loginButton.Text = "Login";
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 305);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.lblTimeRemaining);
            this.Controls.Add(this.backupListBoxControl);
            this.Controls.Add(this.serviceStatusLabel);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.IconOptions.Image = global::GavelBackupGDrive.Ui.Properties.Resources.gavelisback1;
            this.Name = "MainForm";
            this.Text = "GAVEL I.S Off Site";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.backupListBoxControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.ListBoxControl backupListBoxControl;
        private DevExpress.XtraEditors.LabelControl serviceStatusLabel;
        private DevExpress.XtraEditors.SimpleButton stopButton;
        private DevExpress.XtraEditors.SimpleButton startButton;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private DevExpress.XtraEditors.LabelControl lblTimeRemaining;
        private DevExpress.XtraEditors.SimpleButton loginButton;
    }
}

