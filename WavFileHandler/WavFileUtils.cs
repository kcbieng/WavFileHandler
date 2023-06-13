using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using WavFileHandlerGUI;

namespace WavFileHandler
{
    public static class WavFileUtils
    {
        public static CartChunk ReadCartChunkData(FileStream stream)
        {
            stream.Position = 0;
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                if (!TryReadBytes(reader, 4, out byte[] riffBytes) || Encoding.ASCII.GetString(riffBytes) != "RIFF")
                {
                    return null;
                }

                reader.ReadInt32(); // Skip chunk size

                if (!TryReadBytes(reader, 4, out byte[] waveBytes) || Encoding.ASCII.GetString(waveBytes) != "WAVE")
                {
                    return null;
                }

                // Search for the CART chunk
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    if (!TryReadBytes(reader, 4, out byte[] chunkIDBytes))
                    {
                        return null;
                    }

                    string chunkID = Encoding.ASCII.GetString(chunkIDBytes);
                    int chunkSize = reader.ReadInt32();

                    if (chunkID == "cart")
                    {
                        try
                        {
                            string[] allowedFormats = { "yyyy-MM-dd", "yyyy/MM/dd" };
                            CartChunk cartChunk = new CartChunk();

                            if (!TryReadBytes(reader, 4, out byte[] versionBytes)) return null;
                            cartChunk.Version = Encoding.ASCII.GetString(versionBytes);

                            if (!TryReadBytes(reader, 64, out byte[] titleBytes)) return null;
                            cartChunk.Title = Encoding.ASCII.GetString(titleBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] artistBytes)) return null;
                            cartChunk.Artist = Encoding.ASCII.GetString(artistBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] cutIDBytes)) return null;
                            cartChunk.CutID = Encoding.ASCII.GetString(cutIDBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] clientIDBytes)) return null;
                            cartChunk.ClientID = Encoding.ASCII.GetString(clientIDBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] categoryBytes)) return null;
                            cartChunk.Category = Encoding.ASCII.GetString(categoryBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] classificationBytes)) return null;
                            cartChunk.Classification = Encoding.ASCII.GetString(classificationBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] outCueBytes)) return null;
                            cartChunk.OutCue = Encoding.ASCII.GetString(outCueBytes).TrimEnd('\0');

                            cartChunk.StartDatePosition = reader.BaseStream.Position;
                            if (!TryReadBytes(reader, 10, out byte[] startDateBytes)) return null;
                            cartChunk.StartDate = DateTime.ParseExact(Encoding.ASCII.GetString(startDateBytes), allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

                            cartChunk.StartTimePosition = reader.BaseStream.Position;
                            if (!TryReadBytes(reader, 8, out byte[] startTimeBytes)) return null;

                            cartChunk.EndDatePosition = reader.BaseStream.Position;
                            if (!TryReadBytes(reader, 10, out byte[] endDateBytes)) return null;
                            cartChunk.EndDate = DateTime.ParseExact(Encoding.ASCII.GetString(endDateBytes), allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

                            cartChunk.EndTimePosition = reader.BaseStream.Position;
                            if (!TryReadBytes(reader, 8, out byte[] endTimeBytes)) return null;

                            if (!TryReadBytes(reader, 64, out byte[] producerAppIDBytes)) return null;
                            cartChunk.ProducerAppID = Encoding.ASCII.GetString(producerAppIDBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] producerAppVersionBytes)) return null;
                            cartChunk.ProducerAppVersion = Encoding.ASCII.GetString(producerAppVersionBytes).TrimEnd('\0');

                            if (!TryReadBytes(reader, 64, out byte[] userDefBytes)) return null;
                            cartChunk.UserDef = Encoding.ASCII.GetString(userDefBytes).TrimEnd('\0');

                            return cartChunk;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to Read CartChunk Data {ex}");
                        }
                    }
                    else if (chunkID == "data") { 
                        // We've hit the data chunk, so stop searching
                        break;
                    } else
                    {
                        // Skip the chunk data
                        reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                    }
                }
               
            }

            return null;
        }

        private static bool TryReadBytes(BinaryReader reader, int length, out byte[] result)
        {
            result = null;

            if (reader.BaseStream.Length - reader.BaseStream.Position < length)
            {
                // Not enough data left to read
                return false;
            }

            result = reader.ReadBytes(length);
            return true;
        }
    }
}

