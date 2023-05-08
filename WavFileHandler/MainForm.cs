using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WavFileHandler; // Add this to reference WavFileUtils

namespace WavFileHandlerGUI
{
    public partial class MainForm : Form
    {
        private FileSystemWatcher _watcher;

        public MainForm()
        {
            InitializeComponent();
            SetStatusLabelText("Not Processing");
        }

        private void btnSourceBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSource.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnDestinationBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDestination.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnStartWatching_Click(object sender, EventArgs e)
        {
            if (_watcher != null)
            {
                StopWatching();
            }

            string sourcePath = txtSource.Text;
            string destinationPath = txtDestination.Text;
            StartWatching(sourcePath, destinationPath);
        }

        private void btnStopWatching_Click(object sender, EventArgs e)
        {
            StopWatching();
        }

        private void StartWatching(string sourcePath, string destinationPath)
        {
            _watcher = new FileSystemWatcher
            {
                Path = sourcePath,
                Filter = "*.wav",
                NotifyFilter = NotifyFilters.FileName
            };
            _watcher.Created += (sender, e) => ProcessWavFile(sender, e, destinationPath);
            _watcher.EnableRaisingEvents = true;

            SetStatusLabelText("Watching for files...");
        }

        private void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            SetStatusLabelText("Waiting for files...");
        }

        private void SetStatusLabelText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => lblStatus.Text = text));
            }
            else
            {
                lblStatus.Text = text;
            }
        }

        private async void ProcessWavFile(object sender, FileSystemEventArgs e, string destinationPath)
        {
            SetStatusLabelText("Processing file...");

            string filePath = e.FullPath;

            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                            UpdateCartChunkEndDate(stream);
                            //stream.Close();
                        }
                        break;
                    }
                    catch (IOException)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(filePath));
                File.Move(filePath, destinationFilePath);
            });

            SetStatusLabelText("Watching for files...");
        }

        static void UpdateCartChunkEndDate(FileStream stream)
        {
            CartChunk cartChunk = WavFileUtils.ReadCartChunkData(stream);
            if (cartChunk != null)
            {
                // Get the next Sunday date
                DateTime nextSunday = GetNextSunday();
                string newEndDate = nextSunday.ToString("yyyy-MM-dd"); // Update the format to match the format used in ReadCartChunkData

                // Update the EndDate field

                cartChunk.EndDate = nextSunday;

                // Go back to the start position of the EndDate field in the file
                long endDatePosition = cartChunk.EndDatePosition;
                stream.Seek(endDatePosition, SeekOrigin.Begin);

                // Write the updated EndDate back to the file
                byte[] endDateBytes = Encoding.ASCII.GetBytes(newEndDate);
                Console.WriteLine($"Setting EndDate To: '{newEndDate}'");
                stream.Write(endDateBytes, 0, endDateBytes.Length);

                // Close the stream after the updated EndDate has been written
                stream.Close();
            }
        }

        static DateTime GetNextSunday()
        {
            DateTime today = DateTime.Now;
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(daysUntilSunday);
        }

        private void btnShowWavInfo_Click(object sender, EventArgs e)
        {
            WavFileInfoForm wavFileInfoForm = new WavFileInfoForm();
            wavFileInfoForm.Show();
        }
    }
}
