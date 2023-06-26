using System;
using System.IO;
using System.Windows.Forms;
using WavFileHandler;

namespace WavFileHandlerGUI
{
    public partial class WavFileInfoForm : Form
    {
        public WavFileInfoForm()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "WAV files (*.wav)|*.wav";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    lblFileName.Text = Path.GetFileName(filePath);

                    using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            CartChunk cartChunk; // Define the cartChunk variable
                            cartChunk = WavFileUtils.ReadCartChunkData(stream);

                            if (cartChunk != null && cartChunk.StartDate != DateTime.Parse("0001/01/01") && cartChunk.EndDate != DateTime.Parse("0001/01/01"))
                            {
                            txtStartDate.Text = cartChunk.StartDate.ToString("yyyy-MM-dd");
                            txtEndDate.Text = cartChunk.EndDate.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                            MessageBox.Show("No CART chunk found in the selected WAV file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
