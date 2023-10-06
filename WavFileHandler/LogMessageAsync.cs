using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using WavFileHandler.Properties;
using System.Diagnostics.Eventing.Reader;
using WavFileHandlerGUI;

namespace log
{

    public static class Logger
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static string _logFilePath = "log.txt";

        public static async Task LogMessageAsync(string message, bool isError = false)
        {
            await _semaphore.WaitAsync();
            try
            {
                string logMessage = $"{DateTime.Now}: {message}";
                using (StreamWriter logwriter = new StreamWriter(_logFilePath, append: true))
                {
                    await logwriter.WriteLineAsync(logMessage);
                }
                
                LogFunctions.UpdateLogDisplay(logMessage);

                if (isError)
                {
                    var sendMail = new log.LogFunctions();
                    sendMail.SendMail(logMessage);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions - be cautious about logging within a logger to avoid recursion.
                Console.WriteLine($"Error logging message: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    internal class LogFunctions
        {
        public static string fromEmailAddress;
        public static string mailServer;
        public static int mailServerPort;
        public static string toEmail1;
        public static string toEmail2;
        public static string toEmail3;
        public static string toEmail4;
        private static int emailLoadTries;

        public static void loadEmailDetails()
        {
            mailServer = Settings.Default.mailServer;
            mailServerPort = Settings.Default.mailServerPort;
            fromEmailAddress = Settings.Default.fromEmail; ;
            toEmail1 = Settings.Default.toEmail1;
            toEmail2 = Settings.Default.toEmail2;
            toEmail3 = Settings.Default.toEmail3;
            toEmail4 = Settings.Default.toEmail4;
        }

        public async void SendMail(string message)
        {
            Console.WriteLine($"{mailServer} {fromEmailAddress}");
            if (mailServer != string.Empty && fromEmailAddress != string.Empty )
            {
                emailLoadTries = 0;

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
                catch (Exception ex)
                {
                    await log.Logger.LogMessageAsync($"{ex}", false);
                }
            } else
            {

                if (emailLoadTries <= 2)
                {
                    loadEmailDetails();
                    await log.Logger.LogMessageAsync($"Loading Saved Email Values");
                    emailLoadTries = emailLoadTries + 1;
                    SendMail(message);
                } else
                {
                    await log.Logger.LogMessageAsync($"Could not load saved sendmail values");
                }

            }
        }

        public static void UpdateLogDisplay(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    MainForm.Instance?.UpdateLogDisplay(message);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}
