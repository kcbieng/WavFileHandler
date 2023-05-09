using System;
using System.Globalization;
using System.IO;
using System.Text;


namespace WavFileHandler
{
    public static class WavFileUtils
    {
        public static CartChunk ReadCartChunkData(FileStream stream)
        {
            stream.Position = 0;
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                // Read the RIFF header
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "RIFF")
                {
                    return null;
                }

                reader.ReadInt32(); // Skip chunk size
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "WAVE")
                {
                    return null;
                }

                // Search for the CART chunk
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    string chunkID = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    int chunkSize = reader.ReadInt32();

                    if (chunkID == "cart")
                    {
                        string[] allowedFormats = { "yyyy-MM-ddTHH:mm:ss", "yyyy/MM/ddTHH:mm:ss", "yyyy/MM/ddHH:mm:ss", "yyyy-MM-dd" };                        
                        // Read the CART chunk data
                        CartChunk cartChunk = new CartChunk();
                        cartChunk.Version = Encoding.ASCII.GetString(reader.ReadBytes(4));
                        cartChunk.Title = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.Artist = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.CutID = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.ClientID = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.Category = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.Classification = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.OutCue = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.StartDatePosition = reader.BaseStream.Position;
                        string startDateString = Encoding.ASCII.GetString(reader.ReadBytes(10));
                        //Console.WriteLine($"Trying to parse StartDate: '{startDateString}'");
                        cartChunk.StartDate = DateTime.ParseExact(startDateString, allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                        string startTimeString = Encoding.ASCII.GetString(reader.ReadBytes(8));
                        cartChunk.EndDatePosition = reader.BaseStream.Position;                        
                        string endDateString = Encoding.ASCII.GetString(reader.ReadBytes(10));
                        //Console.WriteLine($"Trying to parse EndDate: '{endDateString}'");
                        cartChunk.EndDate = DateTime.ParseExact(endDateString, allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                        string endTimeString = Encoding.ASCII.GetString(reader.ReadBytes(8));
                        //cartChunk.EndDate = DateTime.ParseExact(Encoding.ASCII.GetString(reader.ReadBytes(10)), "yyyy/MM/dd", null);                        
                        cartChunk.ProducerAppID = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.ProducerAppVersion = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        cartChunk.UserDef = Encoding.ASCII.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                        return cartChunk;
                    }
                    else
                    {
                        // Skip the chunk data
                        reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                    }
                }

            }

            return null;
        }
    }
}
