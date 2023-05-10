using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WavFileHandler; // Add this to reference WavFileUtils
using System.Collections.Concurrent; // Add this to use ConcurrentDictionary
using System.Windows.Forms.VisualStyles;

namespace WavFileHandlerGUI
{
    public partial class MainForm : Form
    {
        private FileSystemWatcher _watcher;
        private ConcurrentDictionary<string, DateTime> _processedFiles; // Add this line
        private int _processWavFileCounter = 0; // Count WAVs Processed
        private int _watcherFileCounter = 0; //Watcher File Counter
        private int _processMP3FileCounter = 0;  //Count MP3s Processed
        private static string _logFilePath = "log.txt";

        public static string LogFilePath
        {
            get
            {
                return _logFilePath;
            }

            private set
            {
                _logFilePath = value;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            SetStatusLabelText("Not Started");

            _processedFiles = new ConcurrentDictionary<string, DateTime>(); // Initialize the dictionary
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
            try
            {
                _watcher = new FileSystemWatcher
                {
                    Path = sourcePath,
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.FileName
                };
                _watcher.Created += (sender, e) => ProcessWavFile(sender, e, destinationPath);
                _watcher.EnableRaisingEvents = true;
                _watcherFileCounter++; // Increment the counter
                SetStatusLabelText("Watching for files...");
            } catch(Exception ex)
            {
                LogMessage($"Watcher not started: {ex}");
            }
        }

        private void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            SetStatusLabelText("Not Running");
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
            string filePath = e.FullPath;

            LogMessage($"Processing file:{Path.GetFileName(filePath)}");

            if (!File.Exists(filePath))
            {
                //Console.WriteLine($"File not Found '{filePath}'");
                //SetStatusLabelText($"File not Found '{filePath}'");
                LogMessage($"File '{filePath}' not found after processing began.");
                return;
            }

            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".wav")
            {
                // Check if the file has been processed recently
                if (_processedFiles.TryGetValue(filePath, out DateTime lastProcessedTime))
                {
                    TimeSpan timeSinceLastProcessed = DateTime.Now - lastProcessedTime;
                    const int debounceTimeInSeconds = 60; // You can adjust this value as needed
                    //Console.WriteLine($"{lastProcessedTime}");
                    if (timeSinceLastProcessed.TotalSeconds < debounceTimeInSeconds)
                    {
                        LogMessage($"'{Path.GetFileName(filePath)}' skipped because that same file was processed in the last 60 Seconds.");
                        return; // Ignore the file if it was processed recently
                    }
                }

                // Update the last processed time for the file
                _processedFiles.AddOrUpdate(filePath, DateTime.Now, (key, oldValue) => DateTime.Now);

                SetStatusLabelText("Processing WAV file...");

                await Task.Run(async () =>
                {
                    DateTime lastWriteTime;
                    while (true)
                    {
                        try
                        {
                            lastWriteTime = File.GetLastWriteTimeUtc(filePath);

                            // Wait for a short delay before processing the file
                            await Task.Delay(1000);

                            DateTime newLastWriteTime = File.GetLastWriteTimeUtc(filePath);

                            // Check if the file has been modified during the delay
                            if (newLastWriteTime == lastWriteTime)
                            {
                                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    UpdateCartChunkEndDate(stream);
                                    stream.Close();                                
                                }
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                    }

                    try
                    {
                        string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(filePath));
                        File.Move(filePath, destinationFilePath);
                        _processWavFileCounter++;
                        LogMessage($"Moved {Path.GetFileName(filePath)} to {Path.GetDirectoryName(destinationFilePath)} WAVs Processed:{_processWavFileCounter} Watcher Count:{_watcherFileCounter}");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Failed to copy '{Path.GetFileName(filePath)}': {ex.Message}");
                    }
                });

                SetStatusLabelText("Watching for files...");
            }
            else if (fileExtension == ".mp3")
            {
                // Check if the file has been processed recently
                if (_processedFiles.TryGetValue(filePath, out DateTime lastProcessedTime))
                {
                    TimeSpan timeSinceLastProcessed = DateTime.Now - lastProcessedTime;
                    const int debounceTimeInSeconds = 60; // You can adjust this value as needed
                    //Console.WriteLine($"{lastProcessedTime}");
                    if (timeSinceLastProcessed.TotalSeconds < debounceTimeInSeconds)
                    {
                        LogMessage($"'{Path.GetFileName(filePath)}' skipped because that same file was processed in the last 60 Seconds.");
                        return; // Ignore the file if it was processed recently
                    }
                }

                // Update the last processed time for the file
                _processedFiles.AddOrUpdate(filePath, DateTime.Now, (key, oldValue) => DateTime.Now);

                SetStatusLabelText("Transferring MP3 file...");

                // Move .mp3 files without processing
                await Task.Run(async () =>
                {
                    DateTime lastWriteTime;
                    while (true)
                    {
                        try
                        {
                            lastWriteTime = File.GetLastWriteTimeUtc(filePath);

                            // Wait for a short delay before moving the file
                            await Task.Delay(1000);

                            DateTime newLastWriteTime = File.GetLastWriteTimeUtc(filePath);

                            // Check if the file has been modified during the delay
                            if (newLastWriteTime == lastWriteTime)
                            {
                                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    stream.Close();
                                }
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    try
                    {
                        string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(filePath));
                        File.Move(filePath, destinationFilePath);
                        _processMP3FileCounter++;
                        LogMessage($"Moved {Path.GetFileName(filePath)} to {Path.GetDirectoryName(destinationFilePath)} MP3s Processed:{_processMP3FileCounter} Watcher Count:{_watcherFileCounter}");
                    } catch(Exception ex)
                    {
                        LogMessage($"Failed to copy '{Path.GetFileName(filePath)}': {ex.Message}");
                    }
                });
            }
            else
            {
                // Ignore any other file types
                LogMessage($"'{Path.GetFileName(filePath)}' ignored: isn't an allowed file type");
                return;
            }
        }


        void UpdateCartChunkEndDate(FileStream stream)
        {
            try
            {
                CartChunk cartChunk = WavFileUtils.ReadCartChunkData(stream);
                if (cartChunk != null)
                {
                    // Get the next Sunday date
                    DateTime nextSunday = GetNextSunday(cartChunk.StartDate);
                    string newEndDate = nextSunday.ToString("yyyy-MM-dd"); // Update the format to match the format used in ReadCartChunkData
                    string newEndTime = "23:59:59";

                    // Update the EndDate field
                    string originalEndDate = (cartChunk.EndDate).ToString("yyyy-MM-dd");
                    cartChunk.EndDate = nextSunday;

                    // Go back to the start position of the EndDate field in the file
                    long endDatePosition = cartChunk.EndDatePosition;
                    stream.Seek(endDatePosition, SeekOrigin.Begin);

                    // Write the updated EndDate back to the file
                    byte[] endDateBytes = Encoding.ASCII.GetBytes(newEndDate);
                    //Console.WriteLine($"Setting EndDate To: '{newEndDate}'");
                    stream.Write(endDateBytes, 0, endDateBytes.Length);

                    // Go back to the start position of the EndTime field in the file
                    long endTimePosition = cartChunk.EndTimePosition;
                    stream.Seek(endTimePosition, SeekOrigin.Begin);

                    //Write the Updated EndTime back to the file
                    byte[] endTimeBytes = Encoding.ASCII.GetBytes(newEndTime);
                    stream.Write(endTimeBytes, 0, endTimeBytes.Length);

                    //Log a Message about updating the EndDate
                    LogMessage($"Updated EndDate from {originalEndDate} to {newEndDate} {newEndTime}");

                    // Close the stream after the updated EndDate has been written
                    stream.Close();
                }
            } catch (Exception ex)
            {
                LogMessage($"Failed to update cartchunk: {ex.Message}");
            }
        }         

        static DateTime GetNextSunday(DateTime currentDate)
        {
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)currentDate.DayOfWeek + 7) % 7;
            return currentDate.AddDays(daysUntilSunday);
        }

        private void btnShowWavInfo_Click(object sender, EventArgs e)
        {
            WavFileInfoForm wavFileInfoForm = new WavFileInfoForm();
            wavFileInfoForm.Show();
        }

        public void LogMessage(string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now}: {message}";
                using (StreamWriter writer = File.AppendText(MainForm.LogFilePath))
                {
                    writer.WriteLine(logMessage);
                }
                UpdateLogDisplay(logMessage);
            }
             catch (Exception ex)
            {
                // Handle any errors that might occur while writing to the log file
                SetStatusLabelText($"Error writing to log file: {_logFilePath} {ex.Message}");
            }
        }

        private void UpdateLogDisplay(string message)
        {
            try
            {
                if (txtLogDisplay.InvokeRequired)
                {
                    txtLogDisplay.Invoke(new Action<string>(UpdateLogDisplay), message);
                }
                else
                {
                    txtLogDisplay.AppendText($"{message}{Environment.NewLine}");
                    txtLogDisplay.ScrollToCaret();
                }

            }
            catch (Exception ex)
            {
                // Handle any errors that might occur while reading from the log file
                SetStatusLabelText($"Error Updating Log Display: {ex.Message}");
            }
        }
    }
}
