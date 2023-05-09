namespace WavFileHandlerGUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblSource = new System.Windows.Forms.Label();
            this.lblDestination = new System.Windows.Forms.Label();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.txtDestination = new System.Windows.Forms.TextBox();
            this.btnSourceBrowse = new System.Windows.Forms.Button();
            this.btnDestinationBrowse = new System.Windows.Forms.Button();
            this.btnStartWatching = new System.Windows.Forms.Button();
            this.btnStopWatching = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnShowWavInfo = new System.Windows.Forms.Button();
            this.txtLogDisplay = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(12, 15);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(44, 13);
            this.lblSource.TabIndex = 0;
            this.lblSource.Text = "Source:";
            // 
            // lblDestination
            // 
            this.lblDestination.AutoSize = true;
            this.lblDestination.Location = new System.Drawing.Point(12, 41);
            this.lblDestination.Name = "lblDestination";
            this.lblDestination.Size = new System.Drawing.Size(63, 13);
            this.lblDestination.TabIndex = 1;
            this.lblDestination.Text = "Destination:";
            // 
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(81, 12);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(474, 20);
            this.txtSource.TabIndex = 2;
            // 
            // txtDestination
            // 
            this.txtDestination.Location = new System.Drawing.Point(81, 38);
            this.txtDestination.Name = "txtDestination";
            this.txtDestination.Size = new System.Drawing.Size(474, 20);
            this.txtDestination.TabIndex = 3;
            // 
            // btnSourceBrowse
            // 
            this.btnSourceBrowse.Location = new System.Drawing.Point(572, 10);
            this.btnSourceBrowse.Name = "btnSourceBrowse";
            this.btnSourceBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnSourceBrowse.TabIndex = 4;
            this.btnSourceBrowse.Text = "Browse";
            this.btnSourceBrowse.UseVisualStyleBackColor = true;
            this.btnSourceBrowse.Click += new System.EventHandler(this.btnSourceBrowse_Click);
            // 
            // btnDestinationBrowse
            // 
            this.btnDestinationBrowse.Location = new System.Drawing.Point(572, 36);
            this.btnDestinationBrowse.Name = "btnDestinationBrowse";
            this.btnDestinationBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDestinationBrowse.TabIndex = 5;
            this.btnDestinationBrowse.Text = "Browse";
            this.btnDestinationBrowse.UseVisualStyleBackColor = true;
            this.btnDestinationBrowse.Click += new System.EventHandler(this.btnDestinationBrowse_Click);
            // 
            // btnStartWatching
            // 
            this.btnStartWatching.Location = new System.Drawing.Point(81, 64);
            this.btnStartWatching.Name = "btnStartWatching";
            this.btnStartWatching.Size = new System.Drawing.Size(100, 23);
            this.btnStartWatching.TabIndex = 6;
            this.btnStartWatching.Text = "Start Watching";
            this.btnStartWatching.UseVisualStyleBackColor = true;
            this.btnStartWatching.Click += new System.EventHandler(this.btnStartWatching_Click);
            // 
            // btnStopWatching
            // 
            this.btnStopWatching.Location = new System.Drawing.Point(187, 64);
            this.btnStopWatching.Name = "btnStopWatching";
            this.btnStopWatching.Size = new System.Drawing.Size(100, 23);
            this.btnStopWatching.TabIndex = 7;
            this.btnStopWatching.Text = "Stop Watching";
            this.btnStopWatching.UseVisualStyleBackColor = true;
            this.btnStopWatching.Click += new System.EventHandler(this.btnStopWatching_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 94);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 13);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Status:";
            // 
            // btnShowWavInfo
            // 
            this.btnShowWavInfo.Location = new System.Drawing.Point(572, 64);
            this.btnShowWavInfo.Name = "btnShowWavInfo";
            this.btnShowWavInfo.Size = new System.Drawing.Size(75, 23);
            this.btnShowWavInfo.TabIndex = 9;
            this.btnShowWavInfo.Text = "Show Info";
            this.btnShowWavInfo.UseVisualStyleBackColor = true;
            this.btnShowWavInfo.Click += new System.EventHandler(this.btnShowWavInfo_Click);
            // 
            // txtLogDisplay
            // 
            this.txtLogDisplay.Location = new System.Drawing.Point(15, 120);
            this.txtLogDisplay.Multiline = true;
            this.txtLogDisplay.Name = "txtLogDisplay";
            this.txtLogDisplay.ReadOnly = true;
            this.txtLogDisplay.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogDisplay.Size = new System.Drawing.Size(632, 261);
            this.txtLogDisplay.TabIndex = 10;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 393);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnStopWatching);
            this.Controls.Add(this.btnStartWatching);
            this.Controls.Add(this.btnDestinationBrowse);
            this.Controls.Add(this.btnSourceBrowse);
            this.Controls.Add(this.txtDestination);
            this.Controls.Add(this.txtSource);
            this.Controls.Add(this.lblDestination);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.btnShowWavInfo);
            this.Controls.Add(this.txtLogDisplay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WAV File Handler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblDestination;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.TextBox txtDestination;
        private System.Windows.Forms.Button btnSourceBrowse;
        private System.Windows.Forms.Button btnDestinationBrowse;
        private System.Windows.Forms.Button btnStartWatching;
        private System.Windows.Forms.Button btnStopWatching;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnShowWavInfo;
        private System.Windows.Forms.TextBox txtLogDisplay;
    }
}
