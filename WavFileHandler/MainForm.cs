using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WavFileHandler; // Add this to reference WavFileUtils
using WavFileHandler.Properties;
using System.Collections.Concurrent; // Add this to use ConcurrentDictionary
using System.Windows.Forms.VisualStyles;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Net.Mail;
using System.Globalization;
using System.Runtime.CompilerServices;
using log;
using System.Threading;

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
        private Queue<string> _fileQueue = new Queue<string>(); // Queue to hold the files to be processed
        private bool _isProcessing = false; // Flag to indicate if a file is currently being processed
        public static string fromEmailAddress;
        public static string mailServer;
        public static int mailServerPort;
        public static string toEmail1;
        public static string toEmail2;
        public static string toEmail3;
        public static string toEmail4;
        public static bool updateStartDate;
        public static string sourcePath;
        public static string destinationPath;
        public string displayMessage;
        public static MainForm Instance { get; private set; }

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
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            SetStatusLabelText("Not Started");
            _processedFiles = new ConcurrentDictionary<string, DateTime>(); // Initialize the dictionary
            Instance = this;
        }

        private void btnSourceBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    sourcePath = dialog.SelectedPath;
                    txtSource.Text = sourcePath;
                }
            }
        }

        private void btnDestinationBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    destinationPath = dialog.SelectedPath;
                    txtDestination.Text = destinationPath;
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

        private async void StartWatching(string sourcePath, string destinationPath)
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
            }
            catch (Exception ex)
            {
                await Logger.LogMessageAsync($"Watcher failed to start: {ex}", true);
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

            // Check if the file has been queued recently
            if (_processedFiles.TryGetValue(filePath, out DateTime lastProcessedTime))
            {
                TimeSpan timeSinceLastProcessed = DateTime.Now - lastProcessedTime;
                const int debounceTimeInSeconds = 60; // You can adjust this value as needed
                                                      //Console.WriteLine($"{lastProcessedTime}");
                if (timeSinceLastProcessed.TotalSeconds < debounceTimeInSeconds)
                {
                    await Logger.LogMessageAsync($"'{Path.GetFileName(filePath)}' skipped because that same file was processed in the last 60 Seconds.", true);
                    return; // Ignore the file if it was processed recently
                }
            }

            // Add the file to the queue
            _fileQueue.Enqueue(filePath);

            // If a file is already being processed, return immediately
            if (_isProcessing)
            {
                return;
            }

            // Otherwise, start processing files from the queue
            _isProcessing = true;
            //LogMessage($"Processing file:{Path.GetFileName(filePath)}");

            try
            {
                while (_fileQueue.Count > 0)
                {
                    string fileToProcess = _fileQueue.Dequeue();
                    await Logger.LogMessageAsync($"Processing file:{Path.GetFileName(fileToProcess)}");
                    if (!File.Exists(fileToProcess))
                    {
                        //Console.WriteLine($"File not Found '{filePath}'");
                        //SetStatusLabelText($"File not Found '{filePath}'");
                        await Logger.LogMessageAsync($"File '{fileToProcess}' not found after processing began.", true);
                        return;
                    }

                    string fileExtension = Path.GetExtension(fileToProcess).ToLower();
                    Console.WriteLine($"'{fileToProcess}'");
                    if (fileExtension == ".wav")
                    {

                        // Update the last processed time for the file
                        _processedFiles.AddOrUpdate(fileToProcess, DateTime.Now, (key, oldValue) => DateTime.Now);

                        SetStatusLabelText("Processing WAV file...");

                        await Task.Run(async () =>
                        {
                            DateTime lastWriteTime;
                            while (true)
                            {
                                try
                                {
                                    lastWriteTime = File.GetLastWriteTimeUtc(fileToProcess);

                                    // Wait for a short delay before processing the file
                                    await Task.Delay(1000);

                                    DateTime newLastWriteTime = File.GetLastWriteTimeUtc(fileToProcess);

                                    // Check if the file has been modified during the delay
                                    if (newLastWriteTime == lastWriteTime)
                                    {
                                        using (FileStream stream = File.Open(fileToProcess, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
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
                                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(fileToProcess));
                                File.Move(fileToProcess, destinationFilePath);
                                _processWavFileCounter++;
                                await Logger.LogMessageAsync($"Moved '{Path.GetFileName(fileToProcess)}' to '{Path.GetDirectoryName(destinationFilePath)}' WAVs Processed:{_processWavFileCounter} Watcher Count:{_watcherFileCounter}");
                            }
                            catch (Exception ex)
                            {
                                await Logger.LogMessageAsync($"Failed to copy '{Path.GetFileName(fileToProcess)}': {ex.Message}", true);
                            }
                        });

                        SetStatusLabelText("Watching for files...");
                    }
                    else if (fileExtension == ".mp3")
                    {

                        // Update the last processed time for the file
                        _processedFiles.AddOrUpdate(fileToProcess, DateTime.Now, (key, oldValue) => DateTime.Now);

                        SetStatusLabelText("Transferring MP3 file...");

                        // Move .mp3 files without processing
                        await Task.Run(async () =>
                        {
                            DateTime lastWriteTime;
                            while (true)
                            {
                                try
                                {
                                    lastWriteTime = File.GetLastWriteTimeUtc(fileToProcess);

                                    // Wait for a short delay before moving the file
                                    await Task.Delay(1000);

                                    DateTime newLastWriteTime = File.GetLastWriteTimeUtc(fileToProcess);

                                    // Check if the file has been modified during the delay
                                    if (newLastWriteTime == lastWriteTime)
                                    {
                                        using (FileStream stream = File.Open(fileToProcess, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
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
                                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(fileToProcess));
                                File.Move(fileToProcess, destinationFilePath);
                                _processMP3FileCounter++;
                                await Logger.LogMessageAsync($"Moved '{Path.GetFileName(fileToProcess)}' to '{Path.GetDirectoryName(destinationFilePath)}' MP3s Processed:{_processMP3FileCounter} Watcher Count:{_watcherFileCounter}");
                            }
                            catch (Exception ex)
                            {
                                await Logger.LogMessageAsync($"Failed to copy '{Path.GetFileName(fileToProcess)}': {ex.Message}", true);
                            }
                        });
                    }
                    else
                    {
                        // Ignore any other file types
                        await Logger.LogMessageAsync($"'{Path.GetFileName(fileToProcess)}' ignored: isn't an allowed file type", true);
                        return;
                    }
                }
            }
            finally
            {
                _isProcessing = false;
                SetStatusLabelText("Watching for files...");
            }
        }


        void UpdateCartChunkEndDate(FileStream stream)
        {
            try
            {
                CartChunk cartChunk = WavFileUtils.ReadCartChunkData(stream);
                if (cartChunk == null || cartChunk.StartDate == DateTime.Parse("0001/01/01") || cartChunk.EndDate == DateTime.Parse("0001/01/01"))
                {
                    Logger.LogMessageAsync($"File does not contain CART chunk data.");
                    return;
                }
                {
                    DateTime oldStartDate = cartChunk.StartDate;
                    DateTime oldEndDate = cartChunk.EndDate;

                    // Get the next Sunday date
                    DateTime nextSunday = GetNextSunday(oldStartDate);
                    string newEndDate = nextSunday.ToString("yyyy-MM-dd"); // Update the format to match the format used in ReadCartChunkData
                    string newEndTime = "23:59:59";
                    string newStartDate = DateTime.Now.ToString("yyyy-MM-dd");
                    if (updateStartDate == true)
                    {
                        // Update the StartDate field
                        cartChunk.StartDate = DateTime.ParseExact(newStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);

                        // Go back to the start position of the StartDate field in the file
                        long startDatePosition = cartChunk.StartDatePosition;
                        stream.Seek(startDatePosition, SeekOrigin.Begin);

                        // Write the updated StartDate back to the file
                        byte[] startDateBytes = Encoding.ASCII.GetBytes(newStartDate);
                        //Console.WriteLine($"Setting EndDate To: '{newStartDate}'");
                        stream.Write(startDateBytes, 0, startDateBytes.Length);
                    }
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
                    if (updateStartDate != true)
                    {
                        //Log a Message about updating the EndDate
                        Logger.LogMessageAsync($"Updated EndDate from {originalEndDate} to {newEndDate} {newEndTime}");
                    }
                    else
                    {
                        //Log a Message about updating the EndDate
                        Logger.LogMessageAsync($"Updated StartDate from {oldStartDate.ToString("yyyy-MM-dd")} to {newStartDate} and EndDate from {originalEndDate} to {newEndDate} {newEndTime}");
                    }

                    // Close the stream after the updated EndDate has been written
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessageAsync($"Failed to update cartchunk: {ex.Message}", true);
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

        private void btnShowConfigPage_Click(object sender, EventArgs e)
        {
            ConfigPage configPage = new ConfigPage();
            configPage.Show();
        }

        public void LogMessage(string message, bool iserror = false)
        {
            try
            {
                string logMessage = $"{DateTime.Now}: {message}";
                using (StreamWriter writer = File.AppendText(MainForm.LogFilePath))
                {
                    writer.WriteLine(logMessage);
                }
                //UpdateLogDisplay(logMessage);
                if (iserror) { SendMail(logMessage); }
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur while writing to the log file
                SetStatusLabelText($"Error writing to log file: {_logFilePath} {ex.Message}");
            }
        }
        public void UpdateLogDisplay(string message)
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
        private void MainForm_Load(object sender, EventArgs e)
        {
            txtSource.Text = Settings.Default.SourcePath;
            txtDestination.Text = Settings.Default.DestinationPath;
            mailServer = Settings.Default.mailServer;
            mailServerPort = Settings.Default.mailServerPort;
            fromEmailAddress = Settings.Default.fromEmail; ;
            toEmail1 = Settings.Default.toEmail1;
            toEmail2 = Settings.Default.toEmail2;
            toEmail3 = Settings.Default.toEmail3;
            toEmail4 = Settings.Default.toEmail4;
            updateStartDate = Settings.Default.UpdateStartDate;
            log.LogFunctions.loadEmailDetails();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.SourcePath = sourcePath;
            Settings.Default.DestinationPath = destinationPath;
            Settings.Default.Save();
        }


        private void SendMail(string message)
        {
            try
            {
                var smtpClient = new SmtpClient($"{mailServer}")
                {
                    Port = mailServerPort,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress($"{fromEmailAddress}"),
                    Subject = "WavFile Handler Error",
                    Body = $"<h2>New Error Logged on WavFileHandler</h2></br> {message}",
                    IsBodyHtml = true,
                };
                if (!string.IsNullOrEmpty(toEmail1))
                {
                    mailMessage.To.Add($"{toEmail1}");
                }
                if (!string.IsNullOrEmpty(toEmail2))
                {
                    mailMessage.To.Add($"{toEmail2}");
                }
                if (!string.IsNullOrEmpty(toEmail3))
                {
                    mailMessage.To.Add($"{toEmail3}");
                }
                if (!string.IsNullOrEmpty(toEmail4))
                {
                    mailMessage.To.Add($"{toEmail4}");
                }
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex) { Logger.LogMessageAsync($"{ex}"); }
        }
    } 
}
