using System;

namespace WavFileHandler
{
    public class CartChunk
    {
        public string Version { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string CutID { get; set; }
        public string ClientID { get; set; }
        public string Category { get; set; }
        public string Classification { get; set; }
        public string OutCue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EndTime { get; set; }
        public string ProducerAppID { get; set; }
        public string ProducerAppVersion { get; set; }
        public string UserDef { get; set; }
        public long StartDatePosition { get; set; }
        public long EndDatePosition { get; set; }
        public long StartTimePosition { get; set; }
        public long EndTimePosition { get; set; }
        public int LevelReference { get; set; }
        public CartTimer[] PostTimer { get; set; } = new CartTimer[8];
        public string Url { get; set; }
        public string TagText { get; set; }

    }
    public class CartTimer
    {
        public string Usage { get; set; }
        public int Value { get; set; }
    }
}
