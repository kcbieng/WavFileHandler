using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using WavFileHandlerGUI;

namespace WavFileHandler
{
    public static class mp3FileUtils
    {
        private static bool testmode = false;
        
        public static async Task ProcessMp3FileAsync(string mp3FilePath, string outputWavPath, string destinationFilePath)
        {
            if (testmode == true)
            {
                Console.WriteLine($"MFP:{mp3FilePath}, OWP:{outputWavPath}, DFP: {destinationFilePath}");
            }

            try
            {
                //0. Remove Leftover TempFiles
                await removePossibleTempTrash(outputWavPath);

                // 1. Read ID3 Tag
                var cartChunk = ConvertId3ToCartChunk(mp3FilePath);
                if (testmode == true)
                {
                    await Task.Delay(1000);
                };

                // 2. Convert MP3 to 16-bit PCM WAV
                await ConvertMp3ToWavAsync(mp3FilePath, outputWavPath);
                if (testmode == true)
                {
                    await Task.Delay(1000);
                };

                // 3. Write CART Chunk to WAV
                await WriteCartChunkToWavAsync(outputWavPath, cartChunk);
                if (testmode == true)
                {
                    await Task.Delay(1000);
                };

                // 4. Move File to New Folder
                await moveWavFile(outputWavPath, destinationFilePath);
                if (testmode == true)
                {
                    await Task.Delay(1000);
                };

                // Log success message
                await log.Logger.LogMessageAsync($"Converted MP3 '{mp3FilePath}' to WAV '{outputWavPath}'.");
                
            }
            catch (Exception ex)
            {
                // Log error message
                await log.Logger.LogMessageAsync($"Error converting MP3 '{mp3FilePath}': {ex.Message}", true);
            }
        }

        private static CartChunk ConvertId3ToCartChunk(string mp3File)
        {
            var cartChunk = new CartChunk();

            try
            {
                var file = TagLib.File.Create(mp3File);

                cartChunk.Title = file.Tag.Title ?? "Unknown Title";
                cartChunk.Artist = file.Tag.FirstPerformer ?? "Unknown Artist";
                cartChunk.Category = file.Tag.FirstGenre ?? "Unknown Genre";

                // Handling dates might require parsing from ID3 format to DateTime
                cartChunk.StartDate = DateTime.Today;
                DateTime EndDate = MainForm.GetNextSunday(cartChunk.StartDate);
                TimeSpan EndTime = new TimeSpan(23, 59, 59);
                cartChunk.EndDate = EndDate.Date + EndTime;
                if (testmode == true)
                {
                    Console.WriteLine($"{cartChunk.EndDate}");
                }
                cartChunk.EndTime = DateTime.ParseExact("23:59:59", "HH:mm:ss", CultureInfo.InvariantCulture);
                cartChunk.ClientID = file.Tag.FirstComposer ?? "Unknown Client";
                cartChunk.CutID = file.Tag.Track.ToString("D2") ?? "00"; // Simplified, adjust as needed
                cartChunk.UserDef = file.Tag.Album ?? "No Key";
                //cartChunk.OutCue = file.Tag.OriginalFilename ?? "Unknown Cue";
                cartChunk.Version = "0101";
            }
            catch (Exception ex)
            {
                // Log error message
                log.Logger.LogMessageAsync($"Error converting ID3 to CART chunk for '{mp3File}': {ex.Message}", true);
            }

            return cartChunk;
        }

        private static async Task ConvertMp3ToWavAsync(string mp3File, string outputFile)
        {
            try
            {
                using (var reader = new MediaFoundationReader(mp3File))
                using (var writer = new WaveFileWriter(outputFile, reader.WaveFormat))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error message
                await log.Logger.LogMessageAsync($"Error converting MP3 '{mp3File}' to WAV: {ex.Message}", true);
                throw; // Re-throwing to prevent further processing in calling method
            }
        }

        private static async Task WriteCartChunkToWavAsync(string wavFile, CartChunk cartChunk)
        {

            
            var writeCartChunk = true;
            if (writeCartChunk == true)
            {
                try
                {
                    using (var stream = new FileStream(wavFile, FileMode.Open, FileAccess.ReadWrite))
                    {
                        // Read and validate RIFF header
                        byte[] riffHeader = new byte[12]; // RIFF header is 12 bytes
                        await stream.ReadAsync(riffHeader, 0, riffHeader.Length);

                        //Validate RIFF header...
                        //if (!await ValidateRiffHeaderAsync(stream))
                        //{
                        //    return;
                        //}

                        // Read fmt chunk header (8 bytes: 4 for "fmt " and 4 for chunk size)
                        byte[] fmtHeader = new byte[8];
                        await stream.ReadAsync(fmtHeader, 0, fmtHeader.Length);

                        // Get fmt chunk size
                        int fmtSize = BitConverter.ToInt32(fmtHeader, 4);
                        if (testmode == true)
                        {
                            Console.WriteLine($"fmtsize={fmtSize}");
                        }

                        // Move stream position to the end of fmt chunk
                        stream.Seek(fmtSize, SeekOrigin.Current);
                        var fmtend = stream.Position;
                        if (testmode == true)
                        {
                            Console.WriteLine($"{stream.Position}");
                        }

                        byte[] remainingData = new byte[stream.Length - stream.Position];
                        await stream.ReadAsync(remainingData, 0, remainingData.Length);

                        // Move stream position back to the end of fmt chunk
                        stream.Seek(fmtend, SeekOrigin.Begin);
                        if (testmode == true)
                        {
                            Console.WriteLine($"{stream.Position}");
                        }

                        if (testmode == true) { Console.WriteLine("Write CART chunk ID"); };
                        // Write CART chunk ID
                        byte[] cartID = Encoding.ASCII.GetBytes("cart");
                        await stream.WriteAsync(cartID, 0, cartID.Length);

                        // Write CART chunk size (size of the remaining data in the CART chunk)
                        // This should be the total size of all the fields written after this field

                        if (testmode == true) { Console.WriteLine("Write CART chunk size"); };
                        byte[] cartchunkSize = BitConverter.GetBytes(2050);                        
                        //stream.Seek(cartchunkSize.Length, SeekOrigin.Current);
                        await stream.WriteAsync(cartchunkSize, 0, cartchunkSize.Length);
                        var chunkSizeEndPosition = stream.Position;

                        if (testmode == true) { Console.WriteLine("Write CART version"); };
                        // Write CART version
                        byte[] version = GetNullTerminatedBytes(cartChunk.Version, 4); // 4 bytes
                        await stream.WriteAsync(version, 0, version.Length);

                        if (testmode == true) { Console.WriteLine("Write Title"); };
                        // Write Title
                        byte[] title = GetNullTerminatedBytes(cartChunk.Title, 64); // 64 bytes
                        await stream.WriteAsync(title, 0, title.Length);

                        if (testmode == true) { Console.WriteLine("Write Artist"); };
                        // Write Artist
                        byte[] artist = GetNullTerminatedBytes(cartChunk.Artist, 64); // 64 bytes
                        await stream.WriteAsync(artist, 0, artist.Length);

                        if (testmode == true) { Console.WriteLine("Write CutID"); };
                        // Write CutID
                        byte[] cutID = new byte[64]; // 64 bytes
                        await stream.WriteAsync(cutID, 0, cutID.Length);

                        if (testmode == true) { Console.WriteLine("Write ClientID"); };
                        // Write ClientID
                        byte[] clientID = new byte[64]; // 64 bytes
                        await stream.WriteAsync(clientID, 0, clientID.Length);

                        if (testmode == true) { Console.WriteLine("Write Category"); };
                        // Write Category
                        byte[] category = new byte[64]; // 64 bytes
                        await stream.WriteAsync(category, 0, category.Length);

                        if (testmode == true) { Console.WriteLine("Write Classification"); };
                        // Write Classification
                        byte[] classification = new byte[64]; // 64 bytes
                        await stream.WriteAsync(classification, 0, classification.Length);

                        if (testmode == true) { Console.WriteLine("Write Outcue"); };
                        // Write outcue
                        byte[] outcue = new byte[64]; // 64 bytes
                        await stream.WriteAsync(outcue, 0, outcue.Length);

                        if (testmode == true) { Console.WriteLine("StartDate"); };
                        // Write StartDate
                        byte[] startDate = GetNullTerminatedBytes(cartChunk.StartDate.ToString("yyyy-MM-dd"), 10); // 10 bytes
                        await stream.WriteAsync(startDate, 0, startDate.Length);

                        if (testmode == true) { Console.WriteLine("StartTime"); };
                        // Write StartTime
                        byte[] startTime = GetNullTerminatedBytes(cartChunk.StartTime.ToString("HH:mm:ss"), 8); // 8 bytes
                        await stream.WriteAsync(startTime, 0, startTime.Length);

                        if (testmode == true) { Console.WriteLine("Write EndDate"); };
                        // Write EndDate
                        byte[] endDate = GetNullTerminatedBytes(cartChunk.EndDate.ToString("yyyy-MM-dd"), 10); // 10 bytes
                        await stream.WriteAsync(endDate, 0, endDate.Length);

                        if (testmode == true) { Console.WriteLine("Write EndTime"); };
                        // Write EndTime
                        byte[] endTime = GetNullTerminatedBytes(cartChunk.EndTime.ToString("HH:mm:ss"), 8); // 8 bytes
                        await stream.WriteAsync(endTime, 0, endTime.Length);

                        if (testmode == true) { Console.WriteLine("Write ProducerAppID"); };
                        // Write ProducerAppID
                        byte[] producerAppID = GetNullTerminatedBytes("WavfileHandler by Solomon Bachman", 64); // 64 bytes
                        await stream.WriteAsync(producerAppID, 0, producerAppID.Length);

                        if (testmode == true) { Console.WriteLine("Write ProducerAppVersion"); };
                        // Write ProducerAppVersion
                        byte[] producerAppVersion = GetNullTerminatedBytes("2.0.1", 64); // 64 bytes
                        await stream.WriteAsync(producerAppVersion, 0, producerAppVersion.Length);

                        if (testmode == true) { Console.WriteLine("UserDef"); };
                        // Write UserDef
                        byte[] userDef = new byte[64]; // 64 bytes
                        await stream.WriteAsync(userDef, 0, userDef.Length);

                        if (testmode == true) { Console.WriteLine("dwLevelReference"); };
                        // Write dwLevelReference
                        byte[] levelReference = BitConverter.GetBytes(0000); // 4 bytes
                        await stream.WriteAsync(levelReference, 0, levelReference.Length);

                        if (testmode == true) { Console.WriteLine("Write PostTimer"); };
                        // Write PostTimer
                        for (int i = 0; i < 8; i++) // 8 timers, each 8 bytes (4 + 4)
                        {
                            byte[] usage = new byte[4]; // 4 bytes, initialized to 0
                            byte[] value = new byte[4]; // 4 bytes, initialized to 0
                            await stream.WriteAsync(usage, 0, usage.Length);
                            await stream.WriteAsync(value, 0, value.Length);
                        };

                        if (testmode == true) { Console.WriteLine("Write Reserved"); };
                        // Write Reserved
                        byte[] reserved = new byte[276]; // 276 bytes, initialized to 0
                        await stream.WriteAsync(reserved, 0, reserved.Length);

                        if (testmode == true) { Console.WriteLine("Write URL"); };
                        // Write URL
                        byte[] url = new byte[1024]; // 1024 bytes
                        await stream.WriteAsync(url, 0, url.Length);

                        if (testmode == true) { Console.WriteLine("Write TagText"); };
                        // Write TagText
                        byte[] tagText = new byte[2]; // Variable length
                        await stream.WriteAsync(tagText, 0, tagText.Length);
                        var endofCartChunk = stream.Position;


                        //Write the actual chunk size
                        //var calculatedCartChunkSize = BitConverter.GetBytes(stream.Position - chunkSizePosition);
                        //Console.WriteLine($"calculated chunk size {Encoding.ASCII.GetString(calculatedCartChunkSize)}");
                        //stream.Seek(chunkSizePosition, SeekOrigin.Begin);
                        //await stream.WriteAsync(calculatedCartChunkSize, 0, cartchunkSize.Length);

                        if (testmode == true) { Console.WriteLine("Write back the stored data"); };
                        // Write back the stored data
                        //stream.Seek(endofCartChunk, SeekOrigin.Begin);
                        await stream.WriteAsync(remainingData, 0, remainingData.Length);


                        // Update the RIFF header size
                        //int fileSize = (int)stream.Length;
                        //Console.WriteLine($"{stream.Length}");
                        int riffSize = BitConverter.ToInt32(riffHeader, 4) + BitConverter.ToInt32(cartchunkSize, 0) + 8;
                        if (testmode == true)
                        {
                            Console.WriteLine($"rheader:{BitConverter.ToInt32(riffHeader, 4)} rsize:{riffSize} ccsize:{BitConverter.ToInt32(cartchunkSize, 0)}");
                        }

                        // Update the size in the RIFF header
                        byte[] riffSizeBytes = BitConverter.GetBytes(riffSize);
                        stream.Seek(4, SeekOrigin.Begin);
                        await stream.WriteAsync(riffSizeBytes, 0, riffSizeBytes.Length);

                        // Ensure all data is written to the file
                        await stream.FlushAsync();

                        stream.Close();

                        if (testmode == true) { Console.WriteLine("Finished Writing Cart Chunk to File"); };
                        //bool isUpdated = await UpdateRiffHeaderSizeAsync(wavFile);
                        //if (!isUpdated)
                        //{
                        //    await log.Logger.LogMessageAsync($"Failed to update RIFF header size in WAV '{wavFile}'.", true);
                        //}
                    } 
                }
                catch (Exception ex)
                {
                    // Log or handle the error as per your use case
                    await log.Logger.LogMessageAsync($"Error writing CART chunk to WAV '{wavFile}': {ex.Message}", true);
                }
            } else {  await log.Logger.LogMessageAsync("Cart Chunk Writing Disabled"); }
        }

        private static DateTime? ParseId3Date(string dateString)
        {
            string[] allowedFormats = { "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd", "yyyy" };

            if (DateTime.TryParseExact(dateString, allowedFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }
            else
            {
                // Log or handle the error as per your use case
                log.Logger.LogMessageAsync($"Error parsing date '{dateString}': Format not recognized.", true);
                return null;
            }
        }
        private static async Task<bool> ValidateRiffHeaderAsync(FileStream stream)
        {
            byte[] riffHeader = new byte[12]; // RIFF header is 12 bytes
            await stream.ReadAsync(riffHeader, 0, riffHeader.Length);

            // Check for "RIFF" identifier
            if (Encoding.ASCII.GetString(riffHeader, 0, 4) != "RIFF")
            {
                await log.Logger.LogMessageAsync("Invalid WAV file: Missing RIFF identifier.", true);
                return false;
            }

            // Check for "WAVE" format
            if (Encoding.ASCII.GetString(riffHeader, 8, 4) != "WAVE")
            {
                await log.Logger.LogMessageAsync("Invalid WAV file: Missing WAVE format identifier.", true);
                return false;
            }

            return true;
        }
        private static async Task<bool> UpdateRiffHeaderSizeAsync(string wavFile)
        {
            try
            {
                using (var stream = new FileStream(wavFile, FileMode.Open, FileAccess.ReadWrite))
                {
                    // Read RIFF header
                    byte[] riffHeader = new byte[12];
                    await stream.ReadAsync(riffHeader, 0, riffHeader.Length);

                    // Validate RIFF header
                    if (Encoding.ASCII.GetString(riffHeader, 0, 4) != "RIFF" ||
                        Encoding.ASCII.GetString(riffHeader, 8, 4) != "WAVE")
                    {
                        await log.Logger.LogMessageAsync($"Invalid WAV file '{wavFile}': Missing RIFF or WAVE identifier.", true);
                        return false;
                    }

                    // Calculate the new size
                    int fileSize = (int)stream.Length;
                    int riffSize = fileSize - 8;
                    Console.WriteLine($"{riffSize}");

                    // Update the size in the RIFF header
                    byte[] riffSizeBytes = BitConverter.GetBytes(riffSize);
                    stream.Seek(4, SeekOrigin.Begin);
                    await stream.WriteAsync(riffSizeBytes, 0, riffSizeBytes.Length);

                    // Ensure all data is written to the file
                    await stream.FlushAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                await log.Logger.LogMessageAsync($"Error updating RIFF header size in WAV '{wavFile}': {ex.Message}", true);
                return false;
            }
        }
        private static byte[] GetNullTerminatedBytes(string input, int length)
        {
            // Ensure input is not null
            input = input ?? string.Empty;

            // Initialize the output byte array
            byte[] output = new byte[length];

            // If input is longer than the available length, truncate it
            if (input.Length >= length)
            {
                input = input.Substring(0, length); // No need to reserve space for NULL
            }
            else // If input is shorter than the length, ensure it is NULL-terminated
            {
                output[input.Length] = 0; // Explicitly set, for clarity
            }

            // Convert the truncated/padded string to ASCII bytes
            byte[] bytes = Encoding.ASCII.GetBytes(input);

            // Copy the ASCII bytes to the output array
            Array.Copy(bytes, output, bytes.Length);

            return output;
        }
        
        private async static Task removePossibleTempTrash(string trashPath)
        {
            if (testmode == true) { Console.WriteLine("Remove Possible Temp Trash"); };

            try
            {
                System.IO.File.Delete(trashPath);
            } catch (Exception ex) {
                await log.Logger.LogMessageAsync($"{ex}");
                throw;
            };
        }
        private async static Task moveWavFile(string outputWavPath, string destinationFilePath)
        {
            if(testmode == true) { 
            Console.WriteLine($"Move File to Destination: {outputWavPath} to {destinationFilePath}");
            };

            while (true)
            {
                try
                {
                    var lastWriteTime = System.IO.File.GetLastWriteTimeUtc(outputWavPath);

                    //Wait for a short delay before processing the file
                    await Task.Delay(1000);

                    DateTime newLastWriteTime = System.IO.File.GetLastWriteTimeUtc(outputWavPath);

                    // Check if the file has been modified during the delay
                    if (newLastWriteTime == lastWriteTime)
                    {
                        System.IO.File.Move(outputWavPath, destinationFilePath);
                        break;
                    }
                }
                catch (IOException ex)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (testmode == true)
                    {
                        Console.WriteLine($"Move IO Exception {ex}");
                    };
                }
            }
        }
    }
}