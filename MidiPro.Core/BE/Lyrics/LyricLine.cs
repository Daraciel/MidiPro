namespace MidiPro.Core.BE.Lyrics
{
    public class LyricLine
    {
        public int StartingMeasure { get; set; }
        public string Lyrics { get; set; }

        public LyricLine()
        {
            StartingMeasure = 1;
            Lyrics = string.Empty;
        }
    }
}