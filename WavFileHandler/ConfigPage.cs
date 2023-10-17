using System;
using System.Drawing.Text;
using System.Windows.Forms;
using WavFileHandler;
using WavFileHandler.Properties;

namespace WavFileHandlerGUI
{
    public partial class ConfigPage : Form
    {

        public ConfigPage()
        {
            InitializeComponent();

        }


        private void ConfigPage_Load(object sender, EventArgs e)
        {
            fromEmail_Textbox.Text = Settings.Default.fromEmail;
            mailServer_Textbox.Text = Settings.Default.mailServer;
            mailServerPort_Textbox.Text = Settings.Default.mailServerPort.ToString();
            toEmail1_Textbox.Text = Settings.Default.toEmail1;
            toEmail2_Textbox.Text = Settings.Default.toEmail2;
            toEmail3_Textbox.Text = Settings.Default.toEmail3;
            toEmail4_Textbox.Text = Settings.Default.toEmail4;
            updateStartDate.Checked = Settings.Default.UpdateStartDate;
            convertFiles.Checked = Settings.Default.convertFiles;
            testMode.Checked = MainForm.testMode;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Settings.Default.fromEmail = fromEmail_Textbox.Text;
            Settings.Default.mailServer = mailServer_Textbox.Text;
            Settings.Default.mailServerPort = int.Parse(mailServerPort_Textbox.Text);
            Settings.Default.toEmail1 = toEmail1_Textbox.Text;
            Settings.Default.toEmail2 = toEmail2_Textbox.Text;
            Settings.Default.toEmail3 = toEmail3_Textbox.Text;
            Settings.Default.toEmail4 = toEmail4_Textbox.Text;
            Settings.Default.UpdateStartDate = updateStartDate.Checked;
            Settings.Default.convertFiles = convertFiles.Checked;
            MainForm.mailServer = mailServer_Textbox.Text.TrimEnd('\0');
            MainForm.mailServerPort = int.Parse(mailServerPort_Textbox.Text);
            MainForm.fromEmailAddress = fromEmail_Textbox.Text.TrimEnd('\0');
            MainForm.toEmail1 = toEmail1_Textbox.Text.TrimEnd('\0');
            MainForm.toEmail2 = toEmail2_Textbox.Text.TrimEnd('\0');
            MainForm.toEmail3 = toEmail3_Textbox.Text.TrimEnd('\0');
            MainForm.toEmail4 = toEmail4_Textbox.Text.TrimEnd('\0');
            MainForm.updateStartDate = updateStartDate.Checked;
            MainForm.convertFiles = convertFiles.Checked;
            if (MainForm.testMode != testMode.Checked) {
                MainForm.testMode = testMode.Checked;
                mp3FileUtils.updateTestMode();
                WavFileUtils.updateTestMode();
                WavFileInfoForm.updateTestMode();
            }
            Settings.Default.Save();
            log.LogFunctions.loadEmailDetails();
            this.Close();
        }
        private void clearSettings_Click(object sender, EventArgs e)
        {
            Settings.Default.fromEmail = null;
            Settings.Default.mailServer = null;
            Settings.Default.toEmail1 = null;
            Settings.Default.toEmail2 = null;
            Settings.Default.toEmail3 = null;
            Settings.Default.toEmail4 = null;
            Settings.Default.UpdateStartDate = false;
            Settings.Default.SourcePath = null;
            Settings.Default.DestinationPath = null;
            Settings.Default.convertFiles = false;
            MainForm.sourcePath = null;
            MainForm.destinationPath = null;            
            Settings.Default.Save();
            this.Close();
        }
        private void aboutBox_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox1 = new AboutBox1();
            aboutBox1.Show();
        }
    }
}